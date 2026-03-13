using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcIdle : INPCState
    {
        string name = "idle";
        public void Enter(NPCCore npc)
        {
            npc.GetAnimator().SetBool("Idle", true);
        }

        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("Idle", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(NPCCore npc)
        {
            //Debug.Log("Player is idling");
        }
    }
}
