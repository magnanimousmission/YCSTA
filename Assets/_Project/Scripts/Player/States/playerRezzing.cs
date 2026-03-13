using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerRezzing : IPlayerState
    {
        string name = "rezzing";
        public void Enter(PlayerCore player)
        {
            Debug.Log("Player has begun reviving someone");
        }

        public void Exit(PlayerCore player)
        {
            Debug.Log("Player is done or no longer reviving someone");
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            Debug.Log("Player is reviving someone");
        }
    }
}
