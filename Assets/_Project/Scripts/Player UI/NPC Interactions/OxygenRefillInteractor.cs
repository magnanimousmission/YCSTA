using UnityEngine;

public class OxygenRefillInteractor : BaseInteractable
{
    public override void Interact(GameObject interactorSource)
    {
        var playerCore = interactorSource.transform.root.GetComponentInChildren<PlayerCore>();
        if (playerCore == null) return;

        playerCore.TriggerOxygenRefill();
    }
}
