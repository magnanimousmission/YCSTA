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
            Debug.Log("Enter Roll");
            animator = player.GetAnimator();
            animator.SetBool("roll", true);

            // Get info for the state currently playing on Layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            float duration = stateInfo.length;
            //Debug.Log(duration);
            player.SetRollingCooldown(duration);

            if (player.GetIsLocal())
                AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.PlayerRollSound);

        }

        public void Exit(PlayerCore player)
        {
            Debug.Log("Exit Roll");
            player.GetAnimator().SetBool("roll", false);
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