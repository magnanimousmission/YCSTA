using UnityEngine;

public class JoinTetherController : BaseInteractable
{
    [Header("Tether")]
    [SerializeField] private AuraController ownerAuraController;
    

    public override void Interact(GameObject interactorSource)
    {
        if (_hasInteracted) return;
        _hasInteracted = true;
        var otherAura = interactorSource.GetComponentInChildren<AuraController>();
        if (otherAura == null || otherAura == ownerAuraController) return;

        ownerAuraController.AddPeer(otherAura);
        otherAura.AddPeer(ownerAuraController);
        InteractUI.SetActive(false);
    }
}
