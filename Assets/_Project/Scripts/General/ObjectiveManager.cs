using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all objectives for the game.
/// Tracks the current objective and handles progression to the next one.
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [SerializeField] private List<string> objectiveDescriptions = new List<string>();

    private List<Objective> _objectives = new List<Objective>();
    private int _currentObjectiveIndex = 0;

    public Objective CurrentObjective { get; private set; }

    /// <summary>
    /// Invoked when the current objective changes.
    /// </summary>
    public event Action<Objective> OnObjectiveChanged;

    /// <summary>
    /// Invoked when an objective is completed.
    /// </summary>
    public event Action<Objective> OnObjectiveCompleted;

    /// <summary>
    /// Invoked when all objectives are completed.
    /// </summary>
    public event Action OnAllObjectivesCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeObjectives();
    }

    /// <summary>
    /// Initialize objectives from the serialized list.
    /// </summary>
    private void InitializeObjectives()
    {
        _objectives.Clear();
        _currentObjectiveIndex = 0;

        for (int i = 0; i < objectiveDescriptions.Count; i++)
        {
            var objective = new Objective(objectiveDescriptions[i], i);
            objective.OnCompleted += HandleObjectiveCompleted;
            _objectives.Add(objective);
        }

        if (_objectives.Count > 0)
        {
            SetCurrentObjective(0);
        }
    }

    /// <summary>
    /// Sets the current objective by index.
    /// </summary>
    private void SetCurrentObjective(int index)
    {
        if (index < 0 || index >= _objectives.Count)
            return;

        _currentObjectiveIndex = index;
        CurrentObjective = _objectives[index];
        OnObjectiveChanged?.Invoke(CurrentObjective);
    }

    /// <summary>
    /// Completes the current objective and moves to the next one.
    /// </summary>
    public void CompleteCurrentObjective()
    {
        if (CurrentObjective == null) return;

        CurrentObjective.Complete();
    }

    /// <summary>
    /// Handles objective completion and progression.
    /// </summary>
    private void HandleObjectiveCompleted(Objective objective)
    {
        OnObjectiveCompleted?.Invoke(objective);

        int nextIndex = _currentObjectiveIndex + 1;

        if (nextIndex < _objectives.Count)
        {
            SetCurrentObjective(nextIndex);
        }
        else
        {
            CurrentObjective = null;
            OnAllObjectivesCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Adds a new objective at runtime.
    /// </summary>
    public void AddObjective(string description)
    {
        var objective = new Objective(description, _objectives.Count);
        objective.OnCompleted += HandleObjectiveCompleted;
        _objectives.Add(objective);

        if (CurrentObjective == null)
        {
            SetCurrentObjective(_objectives.Count - 1);
        }
    }

    /// <summary>
    /// Gets the total number of objectives.
    /// </summary>
    public int GetObjectiveCount()
    {
        return _objectives.Count;
    }

    /// <summary>
    /// Gets the current objective index.
    /// </summary>
    public int GetCurrentObjectiveIndex()
    {
        return _currentObjectiveIndex;
    }

    /// <summary>
    /// Gets the progress as a fraction (0 to 1).
    /// </summary>
    public float GetProgress()
    {
        if (_objectives.Count == 0) return 0f;
        return (_currentObjectiveIndex + 1f) / _objectives.Count;
    }

    /// <summary>
    /// Resets all objectives.
    /// </summary>
    public void Reset()
    {
        foreach (var objective in _objectives)
        {
            objective.OnCompleted -= HandleObjectiveCompleted;
        }
        InitializeObjectives();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
