using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveDirection { get; private set; }
    public event EventHandler<PlayerInputEventArgs> playerInput;
    PlayerInputEventArgs newInput = new();
    [SerializeField] InputActionReference sprint;
    [SerializeField] InputActionReference interact;
    [SerializeField] InputActionReference jump;
    [SerializeField] InputActionReference roll;
    private bool shouldProcessInput = true;
    PlayerCore player;

    private bool _isPauseMenuToggled = false;

    private void Awake()
    {
        var ownerPhotonView = GetComponentInParent<PhotonView>();
        if (ownerPhotonView != null)
        {
            shouldProcessInput = ownerPhotonView.IsMine;
        }
        player = gameObject.GetComponent<PlayerCore>();
    }
    
    void OnEnable()  => PauseMenuController.OnPauseToggled += HandlePause;
    void OnDisable() => PauseMenuController.OnPauseToggled -= HandlePause;

    void HandlePause(bool paused)
    {
        _isPauseMenuToggled = paused;

        if (paused)
        {
            MoveDirection = Vector2.zero;
            newInput.move = Vector2.zero;
            playerInput?.Invoke(this, newInput);
        }
    }

    public void OnMove(InputValue value)
    {
        if (_isPauseMenuToggled) return;
        if (!gameObject.GetComponent<PlayerCore>().GetIsLocal())
            return;

        MoveDirection = value.Get<Vector2>();
        newInput.move = MoveDirection.normalized;
        //Debug.Log(MoveDirection);
        playerInput?.Invoke(this, newInput);
    }
    private void Update()
    {
        if (_isPauseMenuToggled) return;

        bool sprintPressed = sprint.action.IsPressed();
        if (sprintPressed != newInput.sprint)
        {
            newInput.sprint = sprintPressed;
            playerInput?.Invoke(this, newInput);
        }

        bool interactPressed = interact.action.IsPressed();
        if (interactPressed != newInput.interact)
        {
            newInput.interact = interactPressed;
            playerInput?.Invoke(this, newInput);
        }

        if (jump.action.WasPressedThisFrame() && !player.GetIsJumping())
        {
            newInput.jump = true;
            playerInput?.Invoke(this, newInput);
            newInput.jump = false;
        }

        if (roll.action.WasPressedThisFrame() && !player.GetIsRolling())
        {
            newInput.roll = true;
            playerInput?.Invoke(this, newInput);
            newInput.roll = false;
        }
    }
}
