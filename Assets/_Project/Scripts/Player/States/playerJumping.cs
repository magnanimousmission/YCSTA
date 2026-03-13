using UnityEngine;

namespace Assets._Project.Scripts.Player.States
{
    internal class playerJumping : IPlayerState
    {
        string name = "jumping";


        public void Enter(PlayerCore player)
        {
            player.GetAnimator().SetBool("jump", true);

            Animator animator = player.gameObject.transform.parent.gameObject.GetComponentInParent<Animator>();

            // Get info for the state currently playing on Layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            float duration = stateInfo.length;
            //Debug.Log(duration);
            player.SetJumpCooldown(duration);

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