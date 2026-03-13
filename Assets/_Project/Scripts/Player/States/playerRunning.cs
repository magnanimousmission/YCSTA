using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerRunning : IPlayerState
    {
        string name = "running";
        public void Enter(PlayerCore player)
        {
            if (player.GetPlayerInput().move.x < 0)
            {
                player.GetAnimator().SetBool("RunningStrafeLeft", true);
                player.GetAnimator().SetBool("RunningStrafeRight", false);
                player.GetAnimator().SetBool("running", false);
                player.GetAnimator().SetBool("runningBack", false);
            }
            else if (player.GetPlayerInput().move.x > 0)
            {
                player.GetAnimator().SetBool("RunningStrafeLeft", false);
                player.GetAnimator().SetBool("RunningStrafeRight", true);
                player.GetAnimator().SetBool("running", false);
                player.GetAnimator().SetBool("runningBack", false);
            }
            else
            {
                if (player.GetPlayerInput().move.y < 0)
                {
                    player.GetAnimator().SetBool("runningBack", true);
                    player.GetAnimator().SetBool("running", false);
                }

                else if (player.GetPlayerInput().move.y > 0)
                {
                    player.GetAnimator().SetBool("running", true);
                    player.GetAnimator().SetBool("runningBack", false);
                }
                else
                {
                    player.GetAnimator().SetBool("running", false);
                    player.GetAnimator().SetBool("runningBack", false);
                }

                player.GetAnimator().SetBool("RunningStrafeLeft", false);
                player.GetAnimator().SetBool("RunningStrafeRight", false);
            }

        }

        public void Exit(PlayerCore player)
        {
            player.gameObject.GetComponentInParent<LookAtMouse>().enabled = true;
            player.GetAnimator().SetBool("running", false);
            player.GetAnimator().SetBool("runningBack", false);
            player.GetAnimator().SetBool("RunningStrafeLeft", false);
            player.GetAnimator().SetBool("RunningStrafeRight", false);

        }

        public string GetStateName()
        {
            return name;
        }

        public void FixedUpdate(PlayerCore player)
        {
            if (player.GetInputHandler() == null) return;

            if (player.GetPlayerInput().sprint)
                player.EvaluateInput(player.GetPlayerInput());


            if (player.GetPlayerInput().move != Vector2.zero)
            {

                Vector2 move = player.GetInputHandler().MoveDirection.normalized;
                //Debug.Log(move);

                Vector2 moveInput = player.GetInputHandler().MoveDirection;
                
                player.DecreasePlayerEnergyPassively(Time.deltaTime);
                
                if (moveInput.x < 0)
                {

                    player.GetAnimator().SetBool("RunningStrafeLeft", true);
                    player.GetAnimator().SetBool("RunningStrafeRight", false);
                    player.GetAnimator().SetBool("running", false);
                    player.GetAnimator().SetBool("runningBack", false);
                }
                else if(moveInput.x > 0)
                {

                    player.GetAnimator().SetBool("RunningStrafeLeft", false);
                    player.GetAnimator().SetBool("RunningStrafeRight", true);
                    player.GetAnimator().SetBool("running", false);
                    player.GetAnimator().SetBool("runningBack", false);
                }
                else if(moveInput.x == 0 && (moveInput.y > 0 || moveInput.y < 0))
                {

                    player.GetAnimator().SetBool("RunningStrafeLeft", false);
                    player.GetAnimator().SetBool("RunningStrafeRight", false);
                    if (player.GetPlayerInput().move.y < 0)
                    {
                        player.GetAnimator().SetBool("runningBack", true);
                        player.GetAnimator().SetBool("running", false);
                    }

                    else if (player.GetPlayerInput().move.y > 0)
                    {
                        player.GetAnimator().SetBool("running", true);
                        player.GetAnimator().SetBool("runningBack", false);
                    }
                    else
                    {
                        player.GetAnimator().SetBool("running", false);
                        player.GetAnimator().SetBool("runningBack", false);
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
