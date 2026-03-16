using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class playerInteracting : IPlayerState
    {
        string name = "interacting";
        internal GameObject _lastInteractable;
        public void Enter(PlayerCore player)
        {

            if (!player.playerInput.interact && _lastInteractable == null)
            {
                Exit(player);
                player.GetStateMachine().SetCurrentPlayerState(player.idle);
                player.GetStateMachine().GetCurrentState().Enter(player);

            }
            else if (_lastInteractable != null)
            {
                player.GetAnimator().SetBool("interacting", true);

                // Get the actual player GameObject you're passing in
                GameObject playerGO = player.GetComponentInParent<Animator>().gameObject;
                PhotonView playerPV = playerGO.GetComponent<PhotonView>();

                // Call Interact locally as before, but also sync the target to the NPC
                Interact(_lastInteractable, playerGO);

                // Tell the NPC on all clients who the target is
                PhotonView npcPV = _lastInteractable.GetComponent<PhotonView>();
                if (npcPV != null && playerPV != null)
                    npcPV.RPC("SetTarget", RpcTarget.All, playerPV.ViewID);
            }
            else
            {
                Exit(player);
                player.GetStateMachine().SetCurrentPlayerState(player.idle);
                player.GetStateMachine().GetCurrentState().Enter(player);

            }


        }

        public void Exit(PlayerCore player)
        {
            player.GetAnimator().SetBool("interacting", false);
            player.playerInput.interact = false;
        }

        public string GetStateName()
        {
            return name;
        }

        internal void Interact(GameObject other, GameObject player)
        {
            IInteractable interactObj;
            interactObj = other.GetComponentInChildren<IInteractable>();
            interactObj.Interact(player);
            PlayerCore playerCore = player.GetComponentInChildren<PlayerCore>();
            Exit(playerCore);
            playerCore.GetStateMachine().SetCurrentPlayerState(playerCore.idle);
            playerCore.GetStateMachine().GetCurrentState().Enter(playerCore);
        }

        public void FixedUpdate(PlayerCore player)
        {
            //Debug.Log("Player Continuing to Consume");
            //player.RefillingPlayerOxygen(Time.deltaTime);
        }
    }
}
