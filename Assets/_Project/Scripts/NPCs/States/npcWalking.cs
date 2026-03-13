using UnityEngine;
namespace Assets.Scripts.NPCs
{
    internal class npcWalking : INPCState
    {
        string name = "walking";

        public void Enter(NPCCore npc)
        {
            if (npc.GetTarget() == null) return;

            npc.GetAnimator().SetBool("walking", true);
            npc.GetAnimator().SetBool("walkBack", false);
            npc.GetAnimator().SetBool("WalkingStrafeLeft", false);
            npc.GetAnimator().SetBool("WalkingStrafeRight", false);
        }

        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("walking", false);
            npc.GetAnimator().SetBool("walkBack", false);
            npc.GetAnimator().SetBool("WalkingStrafeLeft", false);
            npc.GetAnimator().SetBool("WalkingStrafeRight", false);
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