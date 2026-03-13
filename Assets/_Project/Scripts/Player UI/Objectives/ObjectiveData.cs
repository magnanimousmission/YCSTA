using UnityEngine;

[CreateAssetMenu(fileName = "NewObjective", menuName = "The Tether/Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Main Objective")]
    [TextArea(2, 4)]
    public string mainObjectiveText = "New Objective";

    [Header("Sub Objectives")]
    public SubObjectiveData[] subObjectives;

    [Header("Chain")]
    [Tooltip("Objective to load automatically after this one is completed. Leave null to end chain.")]
    public ObjectiveData nextObjective;

    [Header("Timing")]
    [Tooltip("How long the main objective banner displays before fading (seconds)")]
    public float mainDisplayDuration = 4f;

    [System.NonSerialized] public bool IsComplete;
    [System.NonSerialized] public int CurrentSubIndex;

    public bool HasSubObjectives => subObjectives != null && subObjectives.Length > 0;

    public bool AllSubObjectivesComplete =>
        !HasSubObjectives || CurrentSubIndex >= subObjectives.Length;
    
    public string CurrentSubObjectiveText =>
        HasSubObjectives && CurrentSubIndex < subObjectives.Length
            ? subObjectives[CurrentSubIndex].displayText
            : string.Empty;
    
    public string CurrentSubObjectiveId => 
        HasSubObjectives && CurrentSubIndex < subObjectives.Length
            ? subObjectives[CurrentSubIndex].id
            : string.Empty;

    public void ResetState()
    {
        IsComplete = false;
        CurrentSubIndex = 0;
    }
}