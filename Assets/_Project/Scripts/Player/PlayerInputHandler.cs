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
    [SerializeField] InputActionReference oxygen;
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

        if (sprint.action.IsPressed())
        {
            newInput.sprint = true;
            playerInput?.Invoke(this, newInput);

        }
        else
        {
            newInput.sprint = false;
            playerInput?.Invoke(this, newInput);
        }

        if (oxygen.action.IsPressed())
        {
            newInput.oxygen = true;
            playerInput?.Invoke(this, newInput);


        }
        else
        {
            newInput.oxygen = false;
            playerInput?.Invoke(this, newInput);
        }

        if (jump.action.WasPressedThisFrame() && !player.GetIsJumping())
        {
            newInput.jump = true;
            playerInput?.Invoke(this, newInput);
        }


        if (roll.action.IsPressed())
        {
            newInput.roll = true;
            playerInput?.Invoke(this, newInput);


        }
        else
        {
            newInput.roll = false;
            playerInput?.Invoke(this, newInput);
        }

    }
}
