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
    [SerializeField] private float playerHealth;
    [SerializeField] private float playerEnergy;
    [SerializeField] private float playerOxygen;

    internal void SetJumpCooldown(float duration)
    {
        jumpCooldown = duration;
    }

    internal PlayerInputEventArgs playerInput;

    internal playerIdle idle = new();

    internal bool GetIsJumping()
    {
        if(GetStateMachine().currentState == jump)
            return true;
        else return false;
    }

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
    playerConsuming oxygen = new();
    internal playerJumping jump = new();

    bool refillingOxygen = false;

    float jumpCooldown = 2;
    float jumpCooldownClock = 0;

    private bool rolling = false;
    private bool rollLock = false;
    float rollCooldown = 3;
    float rollCooldownClock = 0;
    private bool _oxygenDepletedNotified;


    internal void SetIsLocal(bool v)
    {
        isLocalPlayer = true;
    }

    private PlayerStateMachine stateMachine;
    private PhotonView ownerPhotonView;
    bool initialized = false;


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
        input.playerInput += PlayerInputHandler_playerInput;
        Cursor.visible = false;
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

    private void PlayerInputHandler_playerInput(object sender, PlayerInputEventArgs playerInput)
    {

        EvaluateInput(playerInput);

    }

    internal void EvaluateInput(PlayerInputEventArgs playerInput )
    {
        if (playerInput == null || stateMachine.GetCurrentState() == fallen || stateMachine.GetCurrentState() == dead || stateMachine.GetCurrentState() == oxygen || GetIsJumping())
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

        if (playerInput.roll && !rollLock && playerEnergy > 0)
        {
            if (stateMachine.currentState != roll)
                stateMachine.currentState.Exit(this);

            rollLock = true;
            rolling = true;
            stateMachine.SetCurrentPlayerState(roll);
            stateMachine.GetCurrentState().Enter(this);

        }

        if (playerInput.oxygen) {
            if (stateMachine.currentState != oxygen)
                stateMachine.currentState.Exit(this);

            refillingOxygen = true;
            stateMachine.SetCurrentPlayerState(oxygen);
            stateMachine.GetCurrentState().Enter(this);
 
        }

        if (playerInput.move != Vector2.zero && !playerInput.sprint)
        {

            this.playerInput = playerInput;


            if(stateMachine.currentState != walking)
            {
                stateMachine.currentState.Exit(this);
                stateMachine.SetCurrentPlayerState(walking);
                stateMachine.GetCurrentState().Enter(this);

                if (playerInput.jump && playerEnergy > playerData.jumpingEnergyDrain)
                {
                    if (stateMachine.currentState != jump)
                        stateMachine.currentState.Exit(this);

                        stateMachine.SetCurrentPlayerState(jump);
                        stateMachine.GetCurrentState().Enter(this);

                }
                else if(playerInput.jump && playerEnergy < playerData.jumpingEnergyDrain)
                {
                    playerInput.jump = false;
                }

                if (playerInput.roll)
                {
                    if (stateMachine.currentState != roll)
                        stateMachine.currentState.Exit(this);

                    rollLock = true;
                    rolling = true;
                    stateMachine.SetCurrentPlayerState(roll);
                    stateMachine.GetCurrentState().Enter(this);
                }
            }


        }
        else if (playerInput.move != Vector2.zero && playerInput.sprint && playerEnergy > 0)
        {

            this.playerInput = playerInput;

            if (stateMachine.currentState != running)
            {
                stateMachine.currentState.Exit(this);
                stateMachine.SetCurrentPlayerState(running);
                stateMachine.GetCurrentState().Enter(this);

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

                if (playerInput.roll)
                {
                    if (stateMachine.currentState != roll)
                        stateMachine.currentState.Exit(this);

                    rollLock = true;
                    rolling = true;
                    stateMachine.SetCurrentPlayerState(roll);
                    stateMachine.GetCurrentState().Enter(this);
                }


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
        OnOxygenChanged?.Invoke(playerOxygen, playerData.oxygen);
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
        if (playerOxygen <= 0 && !_oxygenDepletedNotified)
        {
            _oxygenDepletedNotified = true;
            OnOxygenDepleted?.Invoke();
        }

        if (playerOxygen > 0)
            _oxygenDepletedNotified = false;
        //Debug.Log(GetStateMachine().GetCurrentState());


        if (GetIsJumping())
        {
            jumpCooldownClock += Time.fixedDeltaTime;

            if (jumpCooldownClock > jumpCooldown)
            {
                //Debug.Log("Cooldown reached");

                playerInput.jump = false;

                stateMachine.GetCurrentState().Exit(this);
                stateMachine.SetCurrentPlayerState(idle);
                stateMachine.GetCurrentState().Enter(this);
                jumpCooldownClock = 0;
                _energyRecoveryTimer = 0f;
            }

        }

        if (rolling)
        {
            if (rollLock)
            {
                rollCooldownClock += Time.fixedDeltaTime;

            }

            if (rollCooldownClock > rollCooldown)
            {
                if (playerInput == null)
                    return;
                playerInput.roll = false;
                rolling = false;
                stateMachine.GetCurrentState().Exit(this);
                stateMachine.SetCurrentPlayerState(idle);
                stateMachine.GetCurrentState().Enter(this);
                rollLock = false;
                rollCooldownClock = 0;
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
            
            DrainPlayerOxygen(playerOxygen - (playerData.oxygenPassiveDrainRate * Time.deltaTime));
            
            if (refillingOxygen && !playerInput.oxygen)
            {
                refillingOxygen = false;
                stateMachine.GetCurrentState().Exit(this);
                stateMachine.SetCurrentPlayerState(idle);
                stateMachine.GetCurrentState().Enter(this);
            }

            if (stateMachine.currentState == running && playerInput.sprint)
            {
                if (playerInput.move != Vector2.zero && !rolling && !GetIsJumping())
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
        
    }
}
