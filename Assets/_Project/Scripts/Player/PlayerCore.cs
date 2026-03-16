using System;
using Photon.Pun;
using UnityEngine;
using Assets._Project.Scripts.Player.States;
using Assets.Scripts.Player;
using Unity.VisualScripting;

public class PlayerCore : MonoBehaviour
{
    [SerializeField] private bool isLocalPlayer = true;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInputHandler input;
    internal PlayerInputEventArgs playerInput;

    [SerializeField] private float playerHealth;
    [SerializeField] private float playerEnergy;
    [SerializeField] private float playerOxygen;
    [Header("Audio")]
    public float walkingSoundInterval = 0.45f;
    public float runningSoundInterval = 0.3f;


    internal playerIdle idle = new();
    playerWalking walking = new();
    playerRunning running = new();
    playerFallen fallen = new();
    internal playerRoll roll = new();

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnEnergyChanged;
    public event Action<float, float> OnOxygenChanged;
    public event Action OnOxygenDepleted;
    
    private float _energyRecoveryTimer = 0f;

    playerDead dead = new();
    internal playerInteracting interacting = new();
    internal playerJumping jump = new();

    bool refillingOxygen = false;

    float jumpCooldown = 2;
    float jumpCooldownClock = 0;

    float rollCooldown = 3;
    float rollCooldownClock = 0;
    private bool _oxygenDepletedNotified;
    private float _footstepTimer;

    private PlayerStateMachine stateMachine;
    private PhotonView ownerPhotonView;
    bool initialized = false;

    internal void DecrementHealth(float damage)
    {
        playerHealth -= damage;
    }

    internal PlayerStateMachine GetStateMachine()
    {
        return stateMachine;
    }

    private void OnEnable()
    {
        initialize();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        ownerPhotonView = GetComponentInParent<PhotonView>();
        if (ownerPhotonView != null)
        {
            isLocalPlayer = ownerPhotonView.IsMine;
        }


    }

    private void Start()
    {
        if (ownerPhotonView != null && !ownerPhotonView.IsMine) return;
        FindFirstObjectByType<EnergyUIController>()?.Bind(this);
        FindFirstObjectByType<OxygenUIController>()?.Bind(this);
        FindFirstObjectByType<DeathScreenController>(FindObjectsInactive.Include)?.Bind(this);
        FindFirstObjectByType<ExtractionController>(FindObjectsInactive.Include)?.Bind(this);
    }

    internal bool GetIsLocal()
    {
        return isLocalPlayer;
    }

    public void initialize()
    {
        stateMachine = new PlayerStateMachine();
        stateMachine.SetCurrentPlayerState(idle);
        stateMachine.GetCurrentState().Enter(this);
        playerHealth = playerData.health;
        playerEnergy = playerData.energy;
        playerOxygen = playerData.oxygen;
        playerInput = new();
        input.playerInput += PlayerInputHandler_playerInput;
        Cursor.visible = false;
        _footstepTimer = 0f;
        initialized = true;
    }

    private void OnDestroy()
    {
        input.playerInput -= PlayerInputHandler_playerInput;
    }

    internal PlayerInputEventArgs GetPlayerInput()
    {
        return playerInput;
    }

    internal PlayerInputHandler GetInputHandler()
    {
        return input;
    }

    internal Rigidbody GetRB()
    {
        return rb;
    }

    internal PlayerData GetPlayerData()
    {
        return playerData;
    }

    internal void InteractWithObject(GameObject interactableObj)
    {
        if(stateMachine == null)
            initialize();

        if (stateMachine.currentState != interacting)
        {
            stateMachine.currentState.Exit(this);
            stateMachine.SetCurrentPlayerState(interacting);
            interacting._lastInteractable = interactableObj;
            stateMachine.GetCurrentState().Enter(this);
        }



    }

    private void PlayerInputHandler_playerInput(object sender, PlayerInputEventArgs playerInput)
    {
        if (stateMachine.GetCurrentState() == fallen ||
            stateMachine.GetCurrentState() == dead ||
            stateMachine.GetCurrentState() == interacting ||
            GetIsJumping() ||
            GetIsRolling())  // ? add this
            return;
        EvaluateInput(playerInput);

    }

    internal void EvaluateInput(PlayerInputEventArgs playerInput )
    {
        if (stateMachine.GetCurrentState() == fallen || stateMachine.GetCurrentState() == dead || stateMachine.GetCurrentState() == interacting)
            return;

        //Debug.Log("Evaluating");
        if (playerInput.jump && playerEnergy > playerData.jumpingEnergyDrain)
        {
            if (stateMachine.currentState != jump)         
                stateMachine.currentState.Exit(this);

                stateMachine.SetCurrentPlayerState(jump);
                stateMachine.GetCurrentState().Enter(this);


        }
        else if (playerInput.jump && playerEnergy < playerData.jumpingEnergyDrain)
        {
            playerInput.jump = false;
        }

        if (playerInput.roll && !GetIsRolling() && playerEnergy > 0)
        {
            if (stateMachine.currentState != roll)
                stateMachine.currentState.Exit(this);


            stateMachine.SetCurrentPlayerState(roll);
            stateMachine.GetCurrentState().Enter(this);

        }
        else if (playerInput.roll)
        {
            playerInput.roll = false;
        }


        if (playerInput.move != Vector2.zero && !playerInput.sprint)
        {
            this.playerInput = playerInput;

            if (!GetIsRolling() && !GetIsJumping()) // ? add guards
            {
                if(stateMachine.currentState != walking)
                    stateMachine.currentState.Exit(this);

                stateMachine.SetCurrentPlayerState(walking);
                stateMachine.GetCurrentState().Enter(this);
            }
        }
        if (playerInput.move != Vector2.zero && playerInput.sprint && playerEnergy > 0)
        {
            this.playerInput = playerInput;

            if (!GetIsRolling() && !GetIsJumping()) // ? add guards
            {
                if(stateMachine.currentState != running)
                    stateMachine.currentState.Exit(this);
                
                stateMachine.SetCurrentPlayerState(running);
                stateMachine.GetCurrentState().Enter(this);
            }
        }
    }

    internal Animator GetAnimator()
    {
        return animator;
    }
    
    // internal float GetPlayerHealth() => playerHealth;
    // internal void SetPlayerHealth(float value)
    // {
    //     playerHealth = Mathf.Clamp(value, 0, playerData.health);
    //     OnHealthChanged?.Invoke(playerHealth, playerData.health);
    // }

    internal float GetPlayerEnergy() => playerEnergy;
    internal void SetPlayerEnergy(float value)
    {
        playerEnergy = Mathf.Clamp(value, 0, playerData.energy);
        OnEnergyChanged?.Invoke(playerEnergy, playerData.energy);
    }


    internal void DecreasePlayerEnergyPassively(float value)
    {
        _energyRecoveryTimer = 0f;
        playerEnergy -= playerData.energyDrainRate * value;
        playerEnergy = Mathf.Clamp(playerEnergy, 0, playerData.energy);
        OnEnergyChanged?.Invoke(playerEnergy, playerData.energy);
    }
    
    internal void DecreasePlayerEnergyInstantly()
    {
        _energyRecoveryTimer = 0f;
        playerEnergy -= playerData.jumpingEnergyDrain;
        playerEnergy = Mathf.Clamp(playerEnergy, 0, playerData.energy);
        OnEnergyChanged?.Invoke(playerEnergy, playerData.energy);
    }
    
    internal float GetPlayerOxygen() => playerOxygen;

    internal void DrainPlayerOxygen(float value)
    {
        playerOxygen = Mathf.Clamp(value, 0, playerData.oxygen);
        //OnOxygenChanged?.Invoke(playerOxygen, playerData.oxygen);
    }

    internal void RefillingPlayerOxygen(float value)
    {
        refillingOxygen = true;
        playerOxygen += playerData.oxygenRecoveryRate * value;
        playerOxygen = Mathf.Clamp(playerOxygen, 0, playerData.oxygen);
        OnOxygenChanged?.Invoke(playerOxygen, playerData.oxygen);
    }

    internal void TriggerOxygenRefill()
    {
        refillingOxygen = true;
    }
    
    void Update()
    {
       // Debug.Log(stateMachine.GetCurrentState());
        //if (playerOxygen <= 0 && !_oxygenDepletedNotified)
        //{
            //_oxygenDepletedNotified = true;
            //OnOxygenDepleted?.Invoke();
        //}

        //if (playerOxygen > 0)
            //_oxygenDepletedNotified = false;


        if (GetIsJumping())
        {

            // Get info for the state currently playing on Layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            float progress = stateInfo.normalizedTime;
            //Debug.Log(duration);


            if (progress >= 1 && (stateInfo.IsName("Stand To Roll") || stateInfo.IsName("Jump Walking") || stateInfo.IsName("Jump Running") || stateInfo.IsName("Jumping")))
            {

                playerInput.jump = false;

                stateMachine.GetCurrentState().Exit(this);

                IPlayerState nextState = idle;
                if (playerInput.sprint && playerInput.move != Vector2.zero)
                    nextState = running;
                else if (!playerInput.sprint && playerInput.move != Vector2.zero)
                    nextState = walking;

                stateMachine.SetCurrentPlayerState(nextState);
                stateMachine.GetCurrentState().Enter(this);
                _energyRecoveryTimer = 0f;
            }

        }

        if (GetIsRolling())
        {
            // Get info for the state currently playing on Layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            float progress = stateInfo.normalizedTime;

            //Debug.Log(progress);

            if (progress >= 1 && (stateInfo.IsName("Stand To Roll") || stateInfo.IsName("Jumping") || stateInfo.IsName("Jump Running") || stateInfo.IsName("Jump Walking")))
            {
                stateMachine.GetCurrentState().Exit(this);

                IPlayerState nextState = idle;
                if(playerInput.sprint)
                    nextState = running;
                else if (!playerInput.sprint && playerInput.move != Vector2.zero)
                    nextState = walking;


                stateMachine.SetCurrentPlayerState(nextState);
                stateMachine.GetCurrentState().Enter(this);
                _energyRecoveryTimer = 0f;
            }
        }


            if (playerHealth <= 5 && playerHealth > 0)
            {
                if (stateMachine.currentState != fallen)
                    stateMachine.currentState.Exit(this);

                stateMachine.SetCurrentPlayerState(fallen);
                stateMachine.GetCurrentState().Enter(this);
            }
            else if(playerHealth <= 0)
            {
                if (stateMachine.currentState != dead)
                    stateMachine.currentState.Exit(this);

                stateMachine.SetCurrentPlayerState(dead);
                stateMachine.GetCurrentState().Enter(this);
            }
            else if (playerHealth > 5 && (stateMachine.currentState == fallen || stateMachine.currentState == dead))
            {

                stateMachine.currentState.Exit(this);
                stateMachine.SetCurrentPlayerState(idle);
                stateMachine.GetCurrentState().Enter(this);
            }
            
            //DrainPlayerOxygen(playerOxygen - (playerData.oxygenPassiveDrainRate * Time.deltaTime));
            
            //if (refillingOxygen && !playerInput.interact)
            //{
                //refillingOxygen = false;
                //stateMachine.GetCurrentState().Exit(this);
                //stateMachine.SetCurrentPlayerState(idle);
                //stateMachine.GetCurrentState().Enter(this);
            //}

            if (stateMachine.currentState == running && playerInput.sprint)
            {
                if (playerInput.move != Vector2.zero && !GetIsRolling() && !GetIsJumping())
                {
                    _energyRecoveryTimer = 0f;
                }
                
                if (playerEnergy <= 0)
                {
                    stateMachine.currentState.Exit(this);
                    stateMachine.SetCurrentPlayerState(walking);
                    stateMachine.GetCurrentState().Enter(this);
                }
            }
            else
            {
                _energyRecoveryTimer += Time.deltaTime;

                if (_energyRecoveryTimer >= playerData.energyRecoveryDelay)
                {
                    SetPlayerEnergy(playerEnergy + (playerData.energyRecoveryRate * Time.deltaTime));
                }
            }


            stateMachine.GetCurrentState().FixedUpdate(this);
            HandleFootstepAudio();
        
    }


    internal bool GetIsJumping()
    {
        if (GetStateMachine().currentState == jump)
            return true;
        else return false;
    }

    internal void SetJumpCooldown(float duration)
    {
        jumpCooldown = duration;
    }

    internal void SetRollingCooldown(float duration)
    {
        rollCooldown = duration;
    }

    internal bool GetIsRolling()
    {
        if (stateMachine.GetCurrentState() == roll)
            return true;
        else
            return false;

    }
    private void HandleFootstepAudio()
    {
        if (!isLocalPlayer || AudioManager.Instance == null)
            return;

    
    
        Vector2 moveInput = input != null ? input.MoveDirection : Vector2.zero;
        bool hasInput = moveInput.sqrMagnitude > 0.0001f;
        bool isRunningByInput = playerInput != null && playerInput.sprint && playerEnergy > 0f;
        bool blockedByState = stateMachine != null && (stateMachine.currentState == dead || stateMachine.currentState == fallen);

        if (!hasInput || GetIsRolling() || GetIsJumping() || blockedByState)
        {
            _footstepTimer = 0f;
            return;
        }

        float interval = isRunningByInput ? runningSoundInterval : walkingSoundInterval;
        interval = Mathf.Max(0.05f, interval);
        _footstepTimer += Time.deltaTime;

        if (_footstepTimer >= interval)
        {
            AudioManager.Instance.PlaySfx(AudioManager.SfxClip.PlayerWalkSound);
            _footstepTimer = 0f;
        }
    }
}
