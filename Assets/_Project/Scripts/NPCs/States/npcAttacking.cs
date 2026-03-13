using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPCs
{
    internal class npcAttacking : INPCState
    {
        string name = "attacking";
        public void Enter(NPCCore npc)
        {
            Debug.Log("Player is starting to attack");
        }

        public void Exit(NPCCore npc)
        {
            Debug.Log("Player is ending their attack");
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(NPCCore npc)
        {
            Debug.Log("Player is attacking");
        }
    }
}
