using UnityEngine;

public class AuraTriggerController : MonoBehaviour
{
    [SerializeField] private AuraController ownerAuraController;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("AuraObject")) return;
        if (ownerAuraController.OwnerType != AuraOwnerType.Player) return;
        var otherAura = other.GetComponentInParent<AuraController>();
        if (otherAura == null || otherAura == ownerAuraController) return;
        
        if(otherAura.OwnerType != AuraOwnerType.Player) return;
        ownerAuraController.AddPeer(otherAura);
        otherAura.AddPeer(ownerAuraController);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("AuraObject")) return;
        if (ownerAuraController.OwnerType != AuraOwnerType.Player) return;

        var otherAura = other.GetComponentInParent<AuraController>();
        if (otherAura == null || otherAura == ownerAuraController) return;

        ownerAuraController.RemovePeer(otherAura);
        otherAura.RemovePeer(ownerAuraController);
    }
    
}