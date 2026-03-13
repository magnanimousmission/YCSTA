using UnityEngine;

public enum ConsumableType
{
    Health, 
    Energy, 
    Aura
}

[CreateAssetMenu(fileName = "ConsumableData", menuName = "Consumables/Data")]
public class ConsumableData : ScriptableObject
{
    public string consumableName;
    public ConsumableType type;
    public float amount;
}