using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.NPCs
{
    internal class npcWalking : INPCState
    {
        string name = "walking";
        public void Enter(NPCCore npc)
        {
            if (npc.GetNPCInput().target == null) return;
            npc.GetAnimator().SetBool("walking", true);
            npc.GetAnimator().SetBool("walkBack", false);
            npc.GetAnimator().SetBool("WalkingStrafeLeft", false);
            npc.GetAnimator().SetBool("WalkingStrafeRight", false);

            NavMeshAgent agent = npc.GetAgent();
            if (agent != null)
                agent.SetDestination(npc.GetNPCInput().target.transform.position);
        }
        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("walking", false);
            npc.GetAnimator().SetBool("walkBack", false);
            npc.GetAnimator().SetBool("WalkingStrafeLeft", false);
            npc.GetAnimator().SetBool("WalkingStrafeRight", false);

            // Clear destination when exiting walking state
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.ResetPath();
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

            if (agent != null)
                agent.SetDestination(target.transform.position);

            Vector3 direction = (target.transform.position - npc.transform.position);
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion smoothed = Quaternion.Slerp(
                    npc.GetRB().rotation,
                    targetRotation,
                    Time.deltaTime * npc.GetNPCData().rotationSpeed
                );
                npc.GetRB().MoveRotation(smoothed);
            }
        }
    }
}