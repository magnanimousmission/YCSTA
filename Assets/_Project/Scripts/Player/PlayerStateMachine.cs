using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public IPlayerState currentState;

    internal IPlayerState GetCurrentState()
    {
        return currentState;
    }
    internal void SetCurrentPlayerState(IPlayerState State)
    {
        currentState = State;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
