using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPCs
{

    internal class npcInteracting : INPCState
    {
        bool interacting;
        string name = "interacting";
        public void Enter(NPCCore npc)
        {
            npc.GetAnimator().SetBool("talking", true);

            interacting = true;
        }

        public void Exit(NPCCore npc)
        {

            npc.GetAnimator().SetBool("talking", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(NPCCore npc)
        {
            if (interacting)
            {
                // Get info for the state currently playing on Layer 0
                AnimatorStateInfo stateInfo = npc.GetAnimator().GetCurrentAnimatorStateInfo(0);

                float progress = stateInfo.normalizedTime;
                //Debug.Log(duration);
                if (progress > 1 && stateInfo.IsName("Talking"))
                {
                    Exit(npc);
                    npc.GetStateMachine().SetCurrentNPCState(npc.idle);
                    interacting = false;
                }
                
            }

        }
    }
}
