using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcPaused : INPCState
    {
        string name = "paused";
        public void Enter(NPCCore npc)
        {
            Debug.Log("Pause");
        }

        public void Exit(NPCCore npc)
        {
            Debug.Log("Exit Pause");
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(NPCCore npc)
        {
            //Player Character does nothing when paused
        }
    }
}
