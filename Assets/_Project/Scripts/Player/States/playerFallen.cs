using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerFallen : IPlayerState
    {
        string name = "fallen";
        public void Enter(PlayerCore player)
        {
            player.GetAnimator().SetBool("fallen", true);
        }

        public void Exit(PlayerCore player)
        {
            player.GetAnimator().SetBool("fallen", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            Debug.Log("Player is dying");
        }
    }
}
