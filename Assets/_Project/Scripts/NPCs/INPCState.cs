using UnityEngine;

public interface INPCState
{
    void Exit(NPCCore npc);
    void Enter(NPCCore npc);
    void FixedUpdate(NPCCore npc);
    string GetStateName();
}
