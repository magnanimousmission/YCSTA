using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerWalking : IPlayerState
    {
        string name = "walking";

        public void Enter(PlayerCore player)
        {
            if (player.GetPlayerInput().move.x < 0)
            {
                player.GetAnimator().SetBool("WalkingStrafeLeft", true);
                player.GetAnimator().SetBool("WalkingStrafeRight", false);
                player.GetAnimator().SetBool("walking", false);
                player.GetAnimator().SetBool("walkBack", false);
            }
            else if (player.GetPlayerInput().move.x > 0)
            {
                player.GetAnimator().SetBool("WalkingStrafeLeft", false);
                player.GetAnimator().SetBool("WalkingStrafeRight", true);
                player.GetAnimator().SetBool("walking", false);
                player.GetAnimator().SetBool("walkBack", false);
            }
            else
            {
                if (player.GetPlayerInput().move.y < 0)
                {
                    player.GetAnimator().SetBool("walkBack", true);
                    player.GetAnimator().SetBool("walking", false);
                }

                else if (player.GetPlayerInput().move.y > 0)
                {
                    player.GetAnimator().SetBool("walking", true);
                    player.GetAnimator().SetBool("walkBack", false);
                }
                else
                {
                    player.GetAnimator().SetBool("walking", false);
                    player.GetAnimator().SetBool("walkBack", false);
                }

                player.GetAnimator().SetBool("WalkingStrafeLeft", false);
                player.GetAnimator().SetBool("WalkingStrafeRight", false);
            }
        }

        public void Exit(PlayerCore player) {
            player.GetAnimator().SetBool("walking", false);
            player.GetAnimator().SetBool("walkBack", false);
            player.GetAnimator().SetBool("WalkingStrafeLeft", false);
            player.GetAnimator().SetBool("WalkingStrafeRight", false);

        }

        public string GetStateName() => name;

        public void FixedUpdate(PlayerCore player)
        {
            if (player.GetInputHandler() == null) return;


            if (player.GetPlayerInput().move != Vector2.zero)
            {
                Vector2 moveInput = player.GetInputHandler().MoveDirection;

                if (moveInput.x < 0)
                {
                    player.GetAnimator().SetBool("WalkingStrafeLeft", true);
                    player.GetAnimator().SetBool("WalkingStrafeRight", false);
                    player.GetAnimator().SetBool("walking", false);
                    player.GetAnimator().SetBool("walkBack", false);
                }
                else if (moveInput.x > 0)
                {
                    player.GetAnimator().SetBool("WalkingStrafeLeft", false);
                    player.GetAnimator().SetBool("WalkingStrafeRight", true);
                    player.GetAnimator().SetBool("walking", false);
                    player.GetAnimator().SetBool("walkBack", false);
                }


                else if (moveInput.x == 0 && (moveInput.y > 0 || moveInput.y < 0))
                {
                    player.GetAnimator().SetBool("WalkingStrafeLeft", false);
                    player.GetAnimator().SetBool("WalkingStrafeRight", false);

                    if(moveInput.y < 0)
                    {
                        player.GetAnimator().SetBool("walkBack", true);
                        player.GetAnimator().SetBool("walking", false);
                    }

                    else if (player.GetPlayerInput().move.y > 0)
                    {
                        player.GetAnimator().SetBool("walking", true);
                        player.GetAnimator().SetBool("walkBack", false);
                    }
                    else
                    {
                        player.GetAnimator().SetBool("walking", false);
                        player.GetAnimator().SetBool("walkBack", false);
                    }

                }

}
            else
            {
                Exit(player);
                player.GetStateMachine().SetCurrentPlayerState(player.idle);
                player.GetStateMachine().GetCurrentState().Enter(player);
            }



        }
    }
}