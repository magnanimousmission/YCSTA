using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcDead : INPCState
    {
        string name = "dead";
        public void Enter(NPCCore npc)
        {
            npc.GetAnimator().SetBool("dead", true);
        }

        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("dead", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(NPCCore player)
        {
            //Player Character does nothing while dead
        }
    }
}
