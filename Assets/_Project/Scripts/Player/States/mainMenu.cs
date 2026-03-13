using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    /// <summary>
    /// State when the main menu is active
    /// </summary>
    internal class mainMenu : IPlayerState
    {
        string name = "main menu";
        public void Enter(PlayerCore player)
        {
            Debug.Log("Main Menu Open");
        }

        public void Exit(PlayerCore player)
        {
            Debug.Log("Main Menu Close");
        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            //Player Character does nothing when the main menu is active
        }
    }
}
