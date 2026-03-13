using UnityEngine;

[CreateAssetMenu(fileName = "NPPData", menuName = "NPC/Data")]
public class NPCData : ScriptableObject
{
    [Header("Movement")] public float walkSpeed = 5f, runSpeed = 8f, jumpHeight = 2f;
    [Header("Combat")] public float health = 100f, energy = 100f, aura = 100f, oxygen = 100f;
    [Header("Energy")] public float energyDrainRate = 10f, energyRecoveryRate = 5f, energyRecoveryDelay = 2f, jumpingEnergyDrain = 7f;
    [Header("Oxygen")] public float oxygenPassiveDrainRate = 10f, oxygenRecoveryRate = 5f;
    internal float rotationSpeed = 10f;
}
