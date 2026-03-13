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
        string name = "interacting";
        public void Enter(NPCCore npc)
        {
            npc.GetAnimator().SetBool("oxygen", true);

        }

        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("oxygen", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(NPCCore npc)
        {
            Debug.Log("Player Continuing to Consume");
        }
    }
}
