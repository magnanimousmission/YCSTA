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
            agent.updateRotation = false;
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
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                GameObject par = npc.gameObject.GetComponentInParent<Animator>().gameObject;
                // Calculate direction to the next corner (steeringTarget)
                Vector3 lookDirection = agent.steeringTarget - par.transform.position;
                lookDirection.y = 0; // Keep the agent upright

                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    // Smoothly interpolate to the target rotation
                    par.transform.rotation = Quaternion.Slerp(par.transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }
    }
}