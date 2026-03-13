using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class pausedGame : IPlayerState
    {
        string name = "paused";
        public void Enter(PlayerCore player)
        {
            Debug.Log("Pause");
        }

        public void Exit(PlayerCore player)
        {
            Debug.Log("Exit Pause");
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            //Player Character does nothing when paused
        }
    }
}
