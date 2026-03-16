using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class JoinTetherController : BaseInteractable, IOnEventCallback
{
    private const byte NpcInteractEventCode = 41;

    [Header("Tether")]
    [SerializeField] private AuraController ownerAuraController;
    [SerializeField] private NPCInputHandler npcInputHandler;
    public PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponentInParent<PhotonView>();
    }
    
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        npcInputHandler.OnTargetLost += OnNPCTargetLost;
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        npcInputHandler.OnTargetLost -= OnNPCTargetLost;
    }
    
    public override void Interact(GameObject interactorSource)
    {
        if (_hasInteracted) return;
        _hasInteracted = true;
        PlayerCore player;
        var otherAura = interactorSource.GetComponentInChildren<AuraController>();
        if (otherAura == null || otherAura == ownerAuraController)
        {
            player = interactorSource.GetComponentInChildren<PlayerCore>();
            player.GetStateMachine().SetCurrentPlayerState(player.idle);
            return;
        }

        if (interactorSource == null)
            return;

        ApplyInteraction(interactorSource);

        if (PhotonNetwork.InRoom && _photonView != null)
        {
            var interactorView = interactorSource.GetComponentInParent<PhotonView>();
            if (interactorView == null)
            {
                return;
            }

            object[] eventData = { _photonView.ViewID, interactorView.ViewID };
            var raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(NpcInteractEventCode, eventData, raiseOptions, SendOptions.SendReliable);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != NpcInteractEventCode)
            return;

        if (_photonView == null)
            return;

        if (photonEvent.CustomData is not object[] eventData || eventData.Length < 2)
            return;

        int targetNpcViewId = (int)eventData[0];
        if (targetNpcViewId != _photonView.ViewID)
            return;

        int interactorViewId = (int)eventData[1];
        var interactorView = PhotonView.Find(interactorViewId);
        if (interactorView == null)
            return;

        ApplyInteraction(interactorView.transform.root.gameObject);
    }

    private void ApplyInteraction(GameObject interactorSource)
    {


        if (interactorSource == null)
            return;

        var otherAura = interactorSource.GetComponentInChildren<AuraController>();
        if (otherAura == null || otherAura == ownerAuraController)
            return;

        _hasInteracted = true;
        ownerAuraController.AddPeer(otherAura);
        otherAura.AddPeer(ownerAuraController);
        npcInputHandler.SetTarget(interactorSource);

        InteractUI.SetActive(false);

        PlayerCore player  = interactorSource.GetComponentInChildren<PlayerCore>();
        InitialState isController = gameObject.GetComponentInParent<InitialState>();
        isController.RemoveParameters();
        NPCCore npc = isController.gameObject.GetComponentInChildren<NPCCore>();
        npc.GetStateMachine().GetCurrentState().Exit(npc);
        npc.GetStateMachine().SetCurrentNPCState(npc.interacting);
        npc.GetStateMachine().GetCurrentState().Enter(npc);
        player.GetStateMachine().SetCurrentPlayerState(player.idle);
    }
    
    private void OnNPCTargetLost()
    {
        _hasInteracted = false;
        InteractUI.SetActive(true);
    }
}