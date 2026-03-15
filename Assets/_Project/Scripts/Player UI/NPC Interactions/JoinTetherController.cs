using UnityEngine;

public class JoinTetherController : BaseInteractable
{
    [Header("Tether")]
    [SerializeField] private AuraController ownerAuraController;
    [SerializeField] private NPCInputHandler npcInputHandler;
    
    private void OnEnable()
    {
        npcInputHandler.OnTargetLost += OnNPCTargetLost;
    }

    private void OnDisable()
    {
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

        ownerAuraController.AddPeer(otherAura);
        otherAura.AddPeer(ownerAuraController);

        npcInputHandler.SetTarget(interactorSource);

        InteractUI.SetActive(false);

        player = interactorSource.GetComponentInChildren<PlayerCore>();
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