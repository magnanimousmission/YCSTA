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

    private Transform   _localPlayer;
    private Transform[] _survivors;

    private LineRenderer[] _coreWires;
    private LineRenderer[] _innerGlows;
    private LineRenderer[] _outerHalos;

    private AuraController   _localAura;
    private AuraController[] _survivorAuras;

    private bool _tetherVisible = true;
    
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
        if (playersChanged || _coreWires == null || _coreWires.Length != _survivors.Length)
        {
            RebuildTethers();
            _localAura = null;
        }

        CacheLocalAura();

        if (_localPlayer == null || _survivors == null || _coreWires == null)
            return;

        for (int i = 0; i < _survivors.Length; i++)
        {
            if (_survivors[i] == null) continue;

            bool isPeer = _localAura != null
                       && _survivorAuras != null
                       && _survivorAuras[i] != null
                       && _localAura.HasPeer(_survivorAuras[i]);

            SetTetherLayerActive(i, isPeer && _tetherVisible);
            if (!isPeer) continue;

            Vector3 start = GetTetherAnchor(_localPlayer, isLocal: true);
            Vector3 end   = GetTetherAnchor(_survivors[i]);

            UpdateTetherPositions(_coreWires[i],  start, end);
            UpdateTetherPositions(_innerGlows[i], start, end);
            UpdateTetherPositions(_outerHalos[i], start, end);
            
            float  auraRatio   = _localAura.CurrentAura / 100f;
            Color  tetherColor = Color.Lerp(Color.red, Color.white, auraRatio);

            ApplyColor(_coreWires[i],  tetherColor);
            ApplyColor(_innerGlows[i], tetherColor);
            ApplyColor(_outerHalos[i], tetherColor);
            
            float widthPulse = 1f + 0.15f * Mathf.Sin(Time.time * 3f);
            _coreWires[i].startWidth = CoreWidth * widthPulse;
            _coreWires[i].endWidth   = CoreWidth * widthPulse;
        }
    }
    
    private Vector3 GetTetherAnchor(Transform t, bool isLocal = false)
    {
        if (isLocal && _localAnchor != null) return _localAnchor.position;
        if (t == null) return Vector3.zero;
        return t.position + Vector3.up * 1.0f;
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

        int       previousPlayerCount   = Players.Count;
        Transform previousLocalPlayer   = _localPlayer;
        int       previousSurvivorCount = _survivors != null ? _survivors.Length : 0;

        Players.Clear();
        _localPlayer = null;
        List<Transform> remotePlayers = new();

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
            else
                remotePlayers.Add(view.transform);
        }

        _survivors = remotePlayers.ToArray();

        return Players.Count          != previousPlayerCount
            || _localPlayer           != previousLocalPlayer
            || _survivors.Length      != previousSurvivorCount;
    }
    
    private void RebuildTethers()
    {
        DestroyAllLayers();

        int count = _survivors != null ? _survivors.Length : 0;

        _coreWires     = new LineRenderer[count];
        _innerGlows    = new LineRenderer[count];
        _outerHalos    = new LineRenderer[count];
        _survivorAuras = new AuraController[count];

        for (int i = 0; i < count; i++)
        {
            _coreWires[i]  = SpawnLayer(CorePrefab, CoreWidth);
            _innerGlows[i] = SpawnLayer(GlowPrefab, GlowWidth);
            _outerHalos[i] = SpawnLayer(HaloPrefab, HaloWidth);

            if (_survivors[i] != null)
                _survivorAuras[i] = _survivors[i].GetComponentInChildren<AuraController>()
                                 ?? _survivors[i].GetComponentInParent<AuraController>();
        }
    }

    private LineRenderer SpawnLayer(LineRenderer prefab, float width)
    {
        if (prefab == null) return null;
        LineRenderer lr   = Instantiate(prefab, transform);
        lr.positionCount  = PointsPerTether;
        lr.useWorldSpace  = true;
        lr.startWidth     = width;
        lr.endWidth       = width;
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
    
    private void UpdateTetherPositions(LineRenderer lr, Vector3 start, Vector3 end)
    {
        if (lr == null) return;
        for (int p = 0; p < PointsPerTether; p++)
        {
            float   t           = p / (float)(PointsPerTether - 1);
            Vector3 pos         = Vector3.Lerp(start, end, t);
            Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.up);
            pos += perpendicular * Mathf.Sin(Time.time * WaveSpeed + t * Mathf.PI * 2f) * WaveAmplitude;
            lr.SetPosition(p, pos);
        }
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