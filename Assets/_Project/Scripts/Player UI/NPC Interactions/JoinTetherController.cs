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

        gameObject.GetComponentInParent<Animator>().SetBool("talking", true);
        gameObject.GetComponentInParent<InitialState>().RemoveParameters();

        var otherAura = interactorSource.GetComponentInChildren<AuraController>();
        if (otherAura == null || otherAura == ownerAuraController)
        {
            gameObject.GetComponentInParent<Animator>().SetBool("talking", false);
            return;
        }

        ownerAuraController.AddPeer(otherAura);
        otherAura.AddPeer(ownerAuraController);
        InteractUI.SetActive(false);
        
        npcInputHandler.SetTarget(interactorSource);
        InteractUI.SetActive(false);
        gameObject.GetComponentInParent<Animator>().SetBool("talking", false);
    }
    
    private void OnNPCTargetLost()
    {
        _hasInteracted = false;
        InteractUI.SetActive(true);
    }
}
