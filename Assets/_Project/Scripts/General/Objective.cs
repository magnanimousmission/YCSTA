using System;

/// <summary>
/// Represents a single objective in the game.
/// </summary>
public class Objective
{
    public string Description { get; set; }
    public int Index { get; set; }
    public bool IsCompleted { get; private set; }

    public event Action<Objective> OnCompleted;

    /// <summary>
    /// Creates a new objective with the given description.
    /// </summary>
    public Objective(string description, int index = 0)
    {
        Description = description;
        Index = index;
        IsCompleted = false;
    }

    /// <summary>
    /// Marks this objective as completed and invokes the OnCompleted event.
    /// </summary>
    public void Complete()
    {
        if (IsCompleted) return;

        IsCompleted = true;
        OnCompleted?.Invoke(this);
    }
}
