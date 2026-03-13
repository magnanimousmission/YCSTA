using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerAttacking : IPlayerState
    {
        string name = "attacking";
        public void Enter(PlayerCore player)
        {
            Debug.Log("Player is starting to attack");
        }

        public void Exit(PlayerCore player)
        {
            Debug.Log("Player is ending their attack");
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            Debug.Log("Player is attacking");
        }
    }
}
