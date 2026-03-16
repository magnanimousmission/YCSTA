using UnityEngine;

namespace Assets._Project.Scripts.Player.States
{
    internal class playerJumping : IPlayerState
    {
        string name = "jumping";
        Animator animator;

        public void Enter(PlayerCore player)
        {
            animator = player.GetAnimator();
            animator.SetBool("jump", true);





            player.DecreasePlayerEnergyInstantly();

        }

        public void Exit(PlayerCore player)
        {
            player.GetAnimator().SetBool("jump", false);

        }

        public void FixedUpdate(PlayerCore player)
        {

        }

        public string GetStateName()
        {
            return name;
        }
    }
}