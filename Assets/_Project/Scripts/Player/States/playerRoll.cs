using UnityEngine;

namespace Assets._Project.Scripts.Player.States
{
    internal class playerRoll : IPlayerState
    {
        string name = "roll";
        Animator animator;
        // Small upward impulse applied once when entering the jump state
        private float jumpImpulse = .035f;   // ← tune this value (5–9 is typical range)

        public void Enter(PlayerCore player)
        {

            animator = player.GetAnimator();
            animator.SetBool("roll", true);

        }

        public void Exit(PlayerCore player)
        {
            player.GetAnimator().SetBool("roll", false);
        }

        public void FixedUpdate(PlayerCore player)
        {

            // Get info for the state currently playing on Layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            float duration = stateInfo.length;
            //Debug.Log(duration);
            player.SetRollingCooldown(duration);
        }

        public string GetStateName()
        {
            return name;
        }
    }
}