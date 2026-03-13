using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcReviving : INPCState
    {
        string name = "reviving";
        public void Enter(NPCCore npc)
        {
            Debug.Log("npc has begun reviving someone");
        }

        public void Exit(NPCCore npc)
        {
            Debug.Log("npc is done or no longer reviving someone");
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(NPCCore npc)
        {
            Debug.Log("npc is reviving someone");
        }
    }
}
