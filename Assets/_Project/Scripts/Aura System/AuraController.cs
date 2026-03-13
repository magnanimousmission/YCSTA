using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public enum AuraOwnerType { Player, Npc }

public class AuraController : MonoBehaviour
{
    [SerializeField] private AuraOwnerType ownerType = AuraOwnerType.Player;
    [SerializeField] private float auraContributionPercentage = 12f;
    [SerializeField] private bool startsWithNoAura = false;
    
    private readonly HashSet<AuraController> _peers = new HashSet<AuraController>();
    private const float TotalAura = 100f;
    private static int _playerCount = 0;
    private PhotonView _ownerPhotonView;
    
    public event Action<float> OnAuraChanged;
    public AuraOwnerType OwnerType => ownerType;
    
    private float _currentAura = -1f; 
    public IReadOnlyCollection<AuraController> GetPeers() => _peers;
    public float CurrentAura => _currentAura < 0f ? TotalAura : _currentAura;
    public bool HasPeer(AuraController other) => _peers.Contains(other);

    
    private void Awake()
    {
        _ownerPhotonView = GetComponentInParent<PhotonView>();
        if (ownerType == AuraOwnerType.Player)
            _playerCount++;
    }

    private void Start()
    {
        if (_ownerPhotonView != null && !_ownerPhotonView.IsMine)
        {
            return;
        }
        if (ownerType == AuraOwnerType.Player)
            FindFirstObjectByType<AuraUIController>()?.Bind(this);
        
        if (startsWithNoAura)
        {
            OnAuraChanged?.Invoke(0f);
        }
        else
            BroadcastGroupAura();
    }
    
    public void RequestCurrentAura()
    {
        BroadcastGroupAura();
    }

    public void AddPeer(AuraController other)
    {
        if (_peers.Add(other))
            BroadcastGroupAura();
    }

    public void RemovePeer(AuraController other)
    {
        if (_peers.Remove(other))
            BroadcastGroupAura();
    }

    private HashSet<AuraController> GetConnectedGroup()
    {
        var visited = new HashSet<AuraController>();
        var queue = new Queue<AuraController>();

        queue.Enqueue(this);
        visited.Add(this);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var peer in current._peers)
            {
                if (visited.Add(peer))
                    queue.Enqueue(peer);
            }
        }

        return visited;
    }

    private void BroadcastGroupAura()
    {
        var group = GetConnectedGroup();
        var totalAura = CalculateGroupAura(group);

        foreach (var member in group)
        {
            member._currentAura = totalAura;
            member.OnAuraChanged?.Invoke(totalAura);
        }
    }

    private float CalculateGroupAura(HashSet<AuraController> group)
    {
        var playersInGroup = 0;
        var totalPenalty = 0f;

        foreach (var member in group)
        {
            if (member.ownerType == AuraOwnerType.Player)
            {
                if (member.startsWithNoAura)
                {
                    member.OnAuraChanged?.Invoke(0f);
                    return 0f;
                }
                playersInGroup++;
            }
            else
                totalPenalty += member.auraContributionPercentage;
        }

        if (playersInGroup == 0)
            return 0f;

        if (_playerCount == 1)
        {
            var multiplier = Mathf.Max(0f, 1f - (totalPenalty / 100f));
            return TotalAura * multiplier;
        }

        int playersOutsideGroup = _playerCount - playersInGroup;
        if (playersInGroup == 1 && playersOutsideGroup > 0)
            return 0f;

        totalPenalty += (playersInGroup - 1) * auraContributionPercentage;
        var groupMultiplier = Mathf.Max(0f, 1f - (totalPenalty / 100f));
        return TotalAura * groupMultiplier;
    }

    private void OnDestroy()
    {
        if (ownerType == AuraOwnerType.Player)
            _playerCount--;

        foreach (var peer in _peers)
            peer.RemovePeer(this);

        _peers.Clear();
        BroadcastGroupAura();
    }
    
    public void OnPlayerDied()
    {
        foreach (var peer in _peers)
            peer.RemovePeer(this);
        _peers.Clear();
        _playerCount--;
        BroadcastGroupAura();
    }
    
}