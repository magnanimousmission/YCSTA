using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcRunning : INPCState
    {
        string name = "running";

        public void Enter(NPCCore npc)
        {
            if (npc.GetTarget() == null) return;

            npc.GetAnimator().SetBool("running", true);
            npc.GetAnimator().SetBool("runningBack", false);
            npc.GetAnimator().SetBool("RunningStrafeLeft", false);
            npc.GetAnimator().SetBool("RunningStrafeRight", false);
        }

        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("running", false);
            npc.GetAnimator().SetBool("runningBack", false);
            npc.GetAnimator().SetBool("RunningStrafeLeft", false);
            npc.GetAnimator().SetBool("RunningStrafeRight", false);
        }

        public string GetStateName() => name;

        public void FixedUpdate(NPCCore npc)
        {
            GameObject target = npc.GetTarget();

            if (target == null)
            {
                Exit(npc);
                npc.GetStateMachine().SetCurrentNPCState(npc.idle);
                npc.GetStateMachine().GetCurrentState().Enter(npc);
                return;
            }

            if (npc.GetNPCInput().move == Vector2.zero)
            {
                Exit(npc);
                npc.GetStateMachine().SetCurrentNPCState(npc.idle);
                npc.GetStateMachine().GetCurrentState().Enter(npc);
                return;
            }

            // Face the target
            Vector3 direction = (target.transform.position - npc.transform.position);
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                npc.transform.rotation = Quaternion.Slerp(
                    npc.transform.rotation,
                    targetRotation,
                    Time.deltaTime * npc.GetNPCData().rotationSpeed
                );
            }
        }
    }
}