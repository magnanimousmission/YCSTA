using UnityEngine;

/// <summary>
/// Example integration script showing how to use the ObjectiveManager.
/// Attach this to a GameObject with a GameManager or similar component.
/// </summary>
public class ObjectiveIntegrationExample : MonoBehaviour
{
    private ObjectiveManager _objectiveManager;

    private void Start()
    {
        _objectiveManager = ObjectiveManager.Instance;

        if (_objectiveManager != null)
        {
            // Subscribe to objective events
            _objectiveManager.OnObjectiveChanged += OnObjectiveChanged;
            _objectiveManager.OnObjectiveCompleted += OnObjectiveCompleted;
            _objectiveManager.OnAllObjectivesCompleted += OnAllObjectivesCompleted;

            Debug.Log($"Started with {_objectiveManager.GetObjectiveCount()} objectives");
        }
    }

    private void Update()
    {
        // Example: Complete objective with a key press (for testing)
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (_objectiveManager?.CurrentObjective != null)
            {
                Debug.Log($"Completing objective: {_objectiveManager.CurrentObjective.Description}");
                _objectiveManager.CompleteCurrentObjective();
            }
        }

        // Example: Add a new objective at runtime
        if (Input.GetKeyDown(KeyCode.A))
        {
            _objectiveManager?.AddObjective($"New Objective {_objectiveManager.GetObjectiveCount() + 1}");
        }
    }

    private void OnObjectiveChanged(Objective objective)
    {
        Debug.Log($"Objective changed to: {objective.Description}");
        Debug.Log($"Progress: {_objectiveManager.GetProgress() * 100f:F1}%");
    }

    private void OnObjectiveCompleted(Objective objective)
    {
        Debug.Log($"Objective completed: {objective.Description}");
    }

    private void OnAllObjectivesCompleted()
    {
        Debug.Log("All objectives completed! Game finished!");
    }

    private void OnDestroy()
    {
        if (_objectiveManager != null)
        {
            _objectiveManager.OnObjectiveChanged -= OnObjectiveChanged;
            _objectiveManager.OnObjectiveCompleted -= OnObjectiveCompleted;
            _objectiveManager.OnAllObjectivesCompleted -= OnAllObjectivesCompleted;
        }
    }
}
