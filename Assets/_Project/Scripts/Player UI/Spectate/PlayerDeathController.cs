using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerDeathController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject spectatorCameraPrefab;
    [SerializeField] private AuraController auraController;
    [SerializeField] private List<GameObject> playerVisuals;
    [SerializeField] private Camera playerCamera;
    
    private PhotonView _photonView;

    private bool _isDead = false;
    public bool IsDead => _isDead;
    
    public event Action OnNoPlayersLeftToSpectate;
    public static event Action OnAllPlayersDead;

    private void Awake()
    {
        _photonView = GetComponentInParent<PhotonView>();
    }
    
    public void Spectate()
    {
        if (!SpectatorController.AnyLivingPlayersExist())
        {
            OnNoPlayersLeftToSpectate?.Invoke();
            return;
        }
        
        if (!photonView.IsMine) return;
        
        auraController.OnPlayerDied();
        playerCamera.gameObject.SetActive(false);
        
        var go = Instantiate(spectatorCameraPrefab, transform.position, Quaternion.identity);
        var spectator = go.GetComponent<SpectatorController>();
        spectator.FindNextTarget();
    }
    
    public void DisablePlayerPresence()
    {
        _isDead = true;
        
        foreach (var visual in playerVisuals)
            visual.SetActive(false);
        
        GetComponent<PlayerInputHandler>().enabled = false;
        
        if (!SpectatorController.AnyLivingPlayersExist())
            OnAllPlayersDead?.Invoke();
    }
    
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != _photonView.Owner) return;

        if (changedProps.ContainsKey("presenceDisabled") && (bool)changedProps["presenceDisabled"])
            DisablePlayerPresence();
    }
    
    public void BroadcastDisablePlayerPresence()
    {
        if (_isDead)
            return;

        DisablePlayerPresence();

        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
            return;

        var props = new ExitGames.Client.Photon.Hashtable { { "presenceDisabled", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}