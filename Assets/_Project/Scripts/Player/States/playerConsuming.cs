using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerConsuming : IPlayerState
    {
        string name = "consuming";
        public void Enter(PlayerCore player)
        {
            player.GetAnimator().SetBool("oxygen", true);

        }

        public void Exit(PlayerCore player)
        {
            player.GetAnimator().SetBool("oxygen", false);
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            Debug.Log("Player Continuing to Consume");
            player.RefillingPlayerOxygen(Time.deltaTime);
        }
    }
}
