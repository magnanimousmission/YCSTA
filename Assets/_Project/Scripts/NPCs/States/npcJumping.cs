using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcJumping : INPCState
    {
        string name = "jumping";


        public void Enter(NPCCore npc)
        {
            npc.GetAnimator().SetBool("jump", true);

        }

        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("jump", false);
        }

        public void FixedUpdate(NPCCore npc)
        {
            // Intentionally empty — no continuous force, no air control, nothing
        }

        public string GetStateName()
        {
            return name;
        }
    }
}