using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcRolling : INPCState
    {
        string name = "roll";

        // Small upward impulse applied once when entering the jump state
        private float jumpImpulse = .035f;   // ← tune this value (5–9 is typical range)

        public void Enter(NPCCore npc)
        {
            npc.GetAnimator().SetBool("roll", true);

        }

        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("roll", false);
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