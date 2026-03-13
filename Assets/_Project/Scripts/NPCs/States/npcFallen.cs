using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcFallen : INPCState
    {
        string name = "fallen";
        public void Enter(NPCCore npc)
        {
            npc.GetAnimator().SetBool("fallen", true);
        }

        public void Exit(NPCCore npc)
        {
            npc.GetAnimator().SetBool("fallen", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(NPCCore npc)
        {
            Debug.Log("npc is dying");
        }
    }
}
