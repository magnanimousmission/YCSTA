using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TetherManager2 : MonoBehaviourPunCallbacks
{
    [Header("Tether Prefabs")]
    public LineRenderer CorePrefab;
    public LineRenderer GlowPrefab;
    public LineRenderer HaloPrefab;

    [Header("Tether Settings")]
    public int PointsPerTether = 20;
    public float WaveAmplitude = 0.1f;
    public float WaveSpeed = 3f;

    [Header("Width Settings")]
    public float CoreWidth  = 0.03f;
    public float GlowWidth  = 0.08f;
    public float HaloWidth  = 0.18f;

    [Header("Player References")]
    public List<GameObject> Players = new();

    private Transform _localAnchor;
    private Transform _localPlayer;

    private struct TetherEdge
    {
        public Transform From;
        public Transform To;
    }

    private TetherEdge[] _edges= Array.Empty<TetherEdge>();
    private LineRenderer[] _coreWires;
    private LineRenderer[] _innerGlows;
    private LineRenderer[] _outerHalos;

    private AuraController _localAura;

    private bool _tetherVisible = true;
    
    private Dictionary<Transform, Transform> _anchorCache = new();

    
    private void Start()
    {
        RefreshPlayersFromPhoton();
        RebuildTethers();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _tetherVisible = !_tetherVisible;
            SetAllLayersActive(_tetherVisible);
        }

        bool playersChanged = RefreshPlayersFromPhoton();
        var currentEdges = BuildEdges();
        
        if (playersChanged || currentEdges.Count != _edges.Length)
        {
            _edges = currentEdges.ToArray();
            RebuildTethers();
            _localAura = null;
        }

        CacheLocalAura();

        if (_coreWires == null || _coreWires.Length == 0)
            return;

        if (_localPlayer == null || _coreWires == null)
            return;

        for (int i = 0; i < _edges.Length; i++)
        {
            //Debug.Log($"Edge {i}: From={_edges[i].From?.position} To={_edges[i].To?.position}");

            var edge = _edges[i];
            if (edge.From == null || edge.To == null) continue;

            SetTetherLayerActive(i, _tetherVisible);
            if (!_tetherVisible) continue;

            Vector3 start = GetTetherAnchor(edge.From, edge.From == _localPlayer);
            Vector3 end   = GetTetherAnchor(edge.To,   edge.To   == _localPlayer);

            UpdateTetherPositions(_coreWires[i],  start, end);
            UpdateTetherPositions(_innerGlows[i], start, end);
            UpdateTetherPositions(_outerHalos[i], start, end);

            float auraRatio   = _localAura != null ? _localAura.CurrentAura / 100f : 1f;
            Color tetherColor = Color.Lerp(Color.red, Color.white, auraRatio);

            ApplyColor(_coreWires[i],  tetherColor);
            ApplyColor(_innerGlows[i], tetherColor);
            ApplyColor(_outerHalos[i], tetherColor);

            float widthPulse = 1f + 0.15f * Mathf.Sin(Time.time * 3f);
            _coreWires[i].startWidth = CoreWidth * widthPulse;
            _coreWires[i].endWidth   = CoreWidth * widthPulse;
        }
    }
    
    public override void OnJoinedRoom()
    {
        RefreshPlayersFromPhoton();
        RebuildTethers();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        RefreshPlayersFromPhoton();
        RebuildTethers();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        RefreshPlayersFromPhoton();
        RebuildTethers();
    }
    
    private bool RefreshPlayersFromPhoton()
    {
        if (!PhotonNetwork.InRoom)
            return false;

        int       previousPlayerCount = Players.Count;
        Transform previousLocalPlayer = _localPlayer;

        Players.Clear();
        _localPlayer = null;

        var playerViews = FindObjectsByType<PhotonView>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var view in playerViews)
        {
            if (view == null || view.Owner == null) continue;

            var go = view.gameObject;
            if (go.GetComponentInChildren<PlayerCore>() == null &&
                go.GetComponentInParent<PlayerCore>()   == null)
                continue;

            var root = view.transform.root.gameObject;
            if (Players.Contains(root)) continue;

            Players.Add(root);

            if (view.IsMine)
                _localPlayer = view.transform;
        }

        return Players.Count != previousPlayerCount
               || _localPlayer  != previousLocalPlayer;
    }

    private void CacheLocalAura()
    {
        if (_localPlayer == null) return;

        if (_localAura == null)
            _localAura = _localPlayer.GetComponentInChildren<AuraController>();

        if (_localAnchor == null)
        {
            foreach (Transform child in _localPlayer.GetComponentsInChildren<Transform>())
            {
                if (child.CompareTag("TetherAnchor"))
                {
                    _localAnchor = child;
                    break;
                }
            }
        }
    }

    private List<TetherEdge> BuildEdges()
    {
        var edges = new List<TetherEdge>();
        
        var auraToTransform = new Dictionary<AuraController, Transform>();
        
        foreach (var go in Players)
        {
            if (go == null) continue;
            var aura = go.GetComponentInChildren<AuraController>();
            var view = go.GetComponentInChildren<PhotonView>()
                       ?? go.GetComponentInParent<PhotonView>();
            if (aura != null && view != null)
                auraToTransform[aura] = view.transform;
        }
        
        var allAuras = FindObjectsByType<AuraController>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var aura in allAuras)
        {
            if (aura.OwnerType != AuraOwnerType.Npc) continue;
            if (!auraToTransform.ContainsKey(aura))
                auraToTransform[aura] = aura.transform;
        }
        
        foreach (var (auraA, transformA) in auraToTransform)
        {
            foreach (var auraB in auraA.GetPeers())
            {
                if (!auraToTransform.TryGetValue(auraB, out Transform transformB))
                    continue;

                if (auraA.GetInstanceID() < auraB.GetInstanceID())
                    edges.Add(new TetherEdge { From = transformA, To = transformB });
            }
        }

        return edges;
    }
    
    private void RebuildTethers()
    {
        DestroyAllLayers();
        _anchorCache.Clear();

        int count = _edges != null ? _edges.Length : 0;

        _coreWires  = new LineRenderer[count];
        _innerGlows = new LineRenderer[count];
        _outerHalos = new LineRenderer[count];

        for (int i = 0; i < count; i++)
        {
            _coreWires[i]  = SpawnLayer(CorePrefab, CoreWidth);
            _innerGlows[i] = SpawnLayer(GlowPrefab, GlowWidth);
            _outerHalos[i] = SpawnLayer(HaloPrefab, HaloWidth);
        }
    }


    private LineRenderer SpawnLayer(LineRenderer prefab, float width)
    {
        if (prefab == null) return null;
        LineRenderer lr  = Instantiate(prefab, transform);
        lr.positionCount = PointsPerTether;
        lr.useWorldSpace = true;
        lr.startWidth    = width;
        lr.endWidth      = width;
        return lr;
    }

    private void DestroyAllLayers()
    {
        DestroyLayer(_coreWires);
        DestroyLayer(_innerGlows);
        DestroyLayer(_outerHalos);
    }

    private void DestroyLayer(LineRenderer[] layer)
    {
        if (layer == null) return;
        foreach (var lr in layer)
            if (lr != null) Destroy(lr.gameObject);
    }
    
    private Vector3 GetTetherAnchor(Transform t, bool isLocal = false)
    {
        if (t == null) return Vector3.zero;
        Transform anchor = GetAnchorTransform(t);
        if (anchor == t)
            return t.position + Vector3.up * 1.0f;
        return anchor.position;
    }
    
    private void UpdateTetherPositions(LineRenderer lr, Vector3 start, Vector3 end)
    {
        if (lr == null) return;
        for (int p = 0; p < PointsPerTether; p++)
        {
            float   t             = p / (float)(PointsPerTether - 1);
            Vector3 pos           = Vector3.Lerp(start, end, t);
            Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.up);
            pos += perpendicular * Mathf.Sin(Time.time * WaveSpeed + t * Mathf.PI * 2f) * WaveAmplitude;
            lr.SetPosition(p, pos);
        }
    }
    
    private Transform GetAnchorTransform(Transform root)
    {
        if (_anchorCache.TryGetValue(root, out Transform cached))
            return cached;

        foreach (Transform child in root.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("TetherAnchor"))
            {
                _anchorCache[root] = child;
                return child;
            }
        }
        
        _anchorCache[root] = root;
        return root;
    }

    private void ApplyColor(LineRenderer lr, Color color)
    {
        if (lr == null) return;
        lr.startColor = color;
        lr.endColor   = color;
    }

    private void SetTetherLayerActive(int i, bool active)
    {
        if (_coreWires[i]  != null) _coreWires[i].enabled  = active;
        if (_innerGlows[i] != null) _innerGlows[i].enabled = active;
        if (_outerHalos[i] != null) _outerHalos[i].enabled = active;
    }

    private void SetAllLayersActive(bool active)
    {
        for (int i = 0; i < (_coreWires?.Length ?? 0); i++)
            SetTetherLayerActive(i, active);
    }
}