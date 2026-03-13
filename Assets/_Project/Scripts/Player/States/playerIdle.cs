using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerIdle : IPlayerState
    {
        string name = "idle";
        public void Enter(PlayerCore player)
        {
            player.GetAnimator().SetBool("Idle", true);
        }

        public void Exit(PlayerCore player)
        {
            player.GetAnimator().SetBool("Idle", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            //Debug.Log("Player is idling");
        }
    }
}
