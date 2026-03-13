using UnityEngine;

public class NPCStateMachine : MonoBehaviour
{
    public INPCState currentState;

    internal INPCState GetCurrentState()
    {
        return currentState;
    }
    internal void SetCurrentNPCState(INPCState State)
    {
        currentState = State;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
