using UnityEngine;
using UnityEngine.AI;

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
            GameObject target = npc.GetNPCInput().target;
            NavMeshAgent agent = npc.GetAgent();

            if (target == null)
            {
                // Clear destination when target is lost
                if (agent != null)
                    agent.ResetPath();

                Exit(npc);
                npc.GetStateMachine().SetCurrentNPCState(npc.idle);
                npc.GetStateMachine().GetCurrentState().Enter(npc);
                return;
            }
            if (npc.GetNPCInput().move == Vector2.zero)
            {
                if (agent != null)
                    agent.ResetPath();

                Exit(npc);
                npc.GetStateMachine().SetCurrentNPCState(npc.idle);
                npc.GetStateMachine().GetCurrentState().Enter(npc);
                return;
            }

            if (agent != null && agent.isOnNavMesh)
                agent.SetDestination(target.transform.position);

            Vector3 direction = (target.transform.position - npc.transform.position);
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion smoothed = Quaternion.Slerp(
                    npc.GetRB().rotation,
                    targetRotation,
                    Time.fixedDeltaTime * npc.GetNPCData().rotationSpeed
                );
                npc.GetRB().MoveRotation(smoothed);
            }
        }
    }
}