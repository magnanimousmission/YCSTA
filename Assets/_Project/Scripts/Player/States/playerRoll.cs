using UnityEngine;

namespace Assets._Project.Scripts.Player.States
{
    internal class playerRoll : IPlayerState
    {
        string name = "roll";

        // Small upward impulse applied once when entering the jump state
        private float jumpImpulse = .035f;   // ← tune this value (5–9 is typical range)

        public void Enter(PlayerCore player)
        {
            player.GetAnimator().SetBool("roll", true);
            if (player.GetIsLocal())
                AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.PlayerRollSound);

        }

        public void Exit(PlayerCore player)
        {
            player.GetAnimator().SetBool("roll", false);
        }

        public void FixedUpdate(PlayerCore player)
        {
            // Intentionally empty — no continuous force, no air control, nothing
        }

        public string GetStateName()
        {
            return name;
        }
    }
}