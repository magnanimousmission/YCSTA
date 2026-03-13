using System;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }
    
    public static event Action<ObjectiveData> OnMainObjectiveSet;
    
    public static event Action<string> OnSubObjectiveChanged;

    public static event Action<ObjectiveData> OnObjectiveComplete;
    
    public static event Action<string> OnSubObjectiveActivated;


    [Header("Starting Objective")]
    [SerializeField] private ObjectiveData startingObjective;
    
    public ObjectiveData CurrentObjective { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        if (startingObjective != null)
            SetObjective(startingObjective);
    }
    
    public void SetObjective(ObjectiveData data)
    {
        if (data == null)
        {
            Debug.Log("[ObjectiveManager] SetObjective called with null data.");
            return;
        }

        data.ResetState();
        CurrentObjective = data;

        OnMainObjectiveSet?.Invoke(CurrentObjective);
        
        BroadcastCurrentSub();
    }

    public void CompleteSubObjective()
    {
        if (CurrentObjective == null) return;

        if (CurrentObjective.AllSubObjectivesComplete)
        {
            Debug.Log("[ObjectiveManager] All sub-objectives already complete.");
            return;
        }

        CurrentObjective.CurrentSubIndex++;

        BroadcastCurrentSub();

        // If that was the last sub-objective, you can optionally auto-complete:
        // if (CurrentObjective.AllSubObjectivesComplete) CompleteMainObjective();
    }

    public void CompleteMainObjective()
    {
        if (CurrentObjective == null) return;

        CurrentObjective.IsComplete = true;
        OnObjectiveComplete?.Invoke(CurrentObjective);

        ObjectiveData next = CurrentObjective.nextObjective;

        if (next != null)
        {
            SetObjective(next);
        }
        else
        {
            // Chain ended -> clear UI -> make sure to change to make panel transparent 
            OnSubObjectiveChanged?.Invoke(string.Empty);
        }
    }

    private void BroadcastCurrentSub()
    {
        var sub = CurrentObjective.subObjectives[CurrentObjective.CurrentSubIndex];
        OnSubObjectiveChanged?.Invoke(sub.displayText);
        OnSubObjectiveActivated?.Invoke(sub.id);
    }
}