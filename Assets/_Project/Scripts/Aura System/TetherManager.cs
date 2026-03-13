using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TetherManager : MonoBehaviourPunCallbacks
{
    [Header("Tether Settings")]
    public LineRenderer TetherPrefab;
    
    [Header("Player References")]
    public Transform LocalPlayer;
    public Transform[] Survivors;
    public List<GameObject> Players = new();
    public int PointsPerTether = 20;
    public float WaveAmplitude = 0.1f;
    public float WaveSpeed = 3f;

    private LineRenderer[] activeTethers;
    private AuraController _localAura;
    private AuraController[] _survivorAuras;
    private bool _tetherVisible = true;

    void Start()
    {
        RefreshPlayersFromPhoton();
        RebuildTethers();
    }
    
    private void CacheLocalAura()
    {
        if (_localAura != null) return;
        if (LocalPlayer == null) return;
        _localAura = LocalPlayer.GetComponentInChildren<AuraController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _tetherVisible = !_tetherVisible;
            if (activeTethers != null)
            {
                foreach (var lr in activeTethers)
                {
                    if (lr != null) lr.enabled = _tetherVisible;
                }
            }
        }

        bool playersChanged = RefreshPlayersFromPhoton();
        if (playersChanged || activeTethers == null || activeTethers.Length != Survivors.Length)
        {
            RebuildTethers();
            _localAura = null;
        }

        CacheLocalAura();

        if (LocalPlayer == null || Survivors == null || activeTethers == null)
            return;

        for (int i = 0; i < Survivors.Length; i++)
        {
            if (Survivors[i] == null || activeTethers[i] == null) continue;

            bool isPeer = _localAura != null
                          && _survivorAuras != null
                          && _survivorAuras[i] != null
                          && _localAura.HasPeer(_survivorAuras[i]);
            
            activeTethers[i].enabled = isPeer && _tetherVisible;
            if (!isPeer) continue;

            var start = GetTetherWorldPosition(LocalPlayer);
            var end   = GetTetherWorldPosition(Survivors[i]);
            UpdateTether(activeTethers[i], start, end);

            float auraRatio = _localAura.CurrentAura / 100f;
            Color tetherColor = Color.Lerp(Color.red, Color.white, auraRatio);
            activeTethers[i].startColor = tetherColor;
            activeTethers[i].endColor   = tetherColor;
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

        int previousPlayerCount = Players.Count;
        Transform previousLocalPlayer = LocalPlayer;
        int previousSurvivorCount = Survivors != null ? Survivors.Length : 0;

        Players.Clear();
        LocalPlayer = null;
        List<Transform> remotePlayers = new();
        HashSet<int> seenActorNumbers = new();
        var playerViews = FindObjectsByType<PhotonView>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        for (int i = 0; i < playerViews.Length; i++)
        {
            var view = playerViews[i];
            if (view == null || view.Owner == null)
                continue;
            
            var go = view.gameObject;
    
            if (go.GetComponentInChildren<PlayerCore>() == null &&
                go.GetComponentInParent<PlayerCore>() == null)
                continue;
            
            var root = view.transform.root.gameObject;
            if (Players.Contains(root))
                continue;

            Players.Add(root);

            if (view.IsMine)
                LocalPlayer = view.transform;
            else
                remotePlayers.Add(view.transform);
        }
        
        Survivors = remotePlayers.ToArray();

        bool playerCountChanged = Players.Count != previousPlayerCount;
        bool localPlayerChanged = LocalPlayer != previousLocalPlayer;
        bool tetherTargetCountChanged = Survivors.Length != previousSurvivorCount;
        return playerCountChanged || localPlayerChanged || tetherTargetCountChanged;
    }
    
    private void RebuildTethers()
    {
        if (TetherPrefab == null)
            return;

        if (activeTethers != null)
        {
            for (int i = 0; i < activeTethers.Length; i++)
            {
                if (activeTethers[i] != null)
                    Destroy(activeTethers[i].gameObject);
            }
        }

        int survivorCount = Survivors != null ? Survivors.Length : 0;
        activeTethers = new LineRenderer[survivorCount];
        _survivorAuras = new AuraController[survivorCount];

        for (int i = 0; i < survivorCount; i++)
        {
            LineRenderer lr = Instantiate(TetherPrefab, transform);
            lr.positionCount = PointsPerTether;
            lr.useWorldSpace = true;
            activeTethers[i] = lr;
            
            if (Survivors[i] != null)
                _survivorAuras[i] = Survivors[i].GetComponentInChildren<AuraController>()
                                    ?? Survivors[i].GetComponentInParent<AuraController>();
        }
    }
    
    private Vector3 GetTetherWorldPosition(Transform root)
    {
        if (root == null) return Vector3.zero;
        return root.position;
    }

    void UpdateTether(LineRenderer lr, Vector3 start, Vector3 end)
    {
        for (int p = 0; p < PointsPerTether; p++)
        {
            float t = p / (float)(PointsPerTether - 1);
            Vector3 pos = Vector3.Lerp(start, end, t);

            Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.up);
            pos += perpendicular * Mathf.Sin(Time.time * WaveSpeed + t * Mathf.PI * 2f) * WaveAmplitude;

            lr.SetPosition(p, pos);
        }
    }
}