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
            // Get info for the state currently playing on Layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            float duration = stateInfo.length;
            //Debug.Log(duration);
            player.SetJumpCooldown(duration);
        }

        public string GetStateName()
        {
            return name;
        }
    }
}