using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerDead : IPlayerState
    {
        string name = "dead";
        public void Enter(PlayerCore player)
        {
            player.GetAnimator().SetBool("dead", true);
        }

        public void Exit(PlayerCore player)
        {
            player.GetAnimator().SetBool("dead", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            //Player Character does nothing while dead
        }
    }
}
