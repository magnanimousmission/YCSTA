using System;
using Photon.Pun;
using UnityEngine;
using Assets._Project.Scripts.Player.States;
using Assets.Scripts.Player;
using Unity.VisualScripting;
using Assets.Scripts.NPCs;
using UnityEngine.AI;

public class NPCCore : MonoBehaviour
{
    [SerializeField] private NPCData npcData;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private NPCInputHandler input;
    [SerializeField] private float npcHealth;
    [SerializeField] private float npcEnergy;
    [SerializeField] private float npcOxygen;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] bool teamMember = false;

    internal NPCInputEventArgs npcInput;

    internal npcIdle idle = new();

    internal bool GetIsJumping()
    {
        if (GetStateMachine().currentState == jump)
            return true;
        else return false;
    }

    internal bool GetIsRolling()
    {
        if (stateMachine.GetCurrentState() == roll)
            return true;
        else
            return false;
    }

    npcWalking walking = new();
    npcRunning running = new();
    npcFallen fallen = new();
    internal npcRolling roll = new();

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnEnergyChanged;
    public event Action<float, float> OnOxygenChanged;

    internal void SetAnimatorBool(string v, bool YesNo)
    {
        animator.SetBool(v, YesNo);
    }

    private float _energyRecoveryTimer = 0f;

    npcDead dead = new();
    npcInteracting interacting = new();
    internal npcJumping jump = new();

    bool refillingOxygen = false;

    private bool _oxygenDepletedNotified;
    private NPCStateMachine stateMachine;
    private PhotonView ownerPhotonView;
    bool initialized = false;

    internal NPCStateMachine GetStateMachine()
    {
        return stateMachine;
    }

    private void OnEnable()
    {
        initialize();
    }

    void Awake()
    {
        ownerPhotonView = GetComponentInParent<PhotonView>();
    }

    private void Start()
    {
        if (teamMember)
        {
            //if (ownerPhotonView != null && !ownerPhotonView.IsMine) return;
            //FindFirstObjectByType<EnergyUIController>()?.Bind(this);
            //FindFirstObjectByType<OxygenUIController>()?.Bind(this);
        }
    }

    internal GameObject GetTarget()
    {
        return npcInput?.target;
    }

    public void initialize()
    {
        stateMachine = new NPCStateMachine();
        stateMachine.SetCurrentNPCState(idle);
        stateMachine.GetCurrentState().Enter(this);
        npcHealth = npcData.health;
        npcEnergy = npcData.energy;
        npcOxygen = npcData.oxygen;
        npcInput = new NPCInputEventArgs();
        input.npcInput += NPCInputHandler_playerInput;
        Cursor.visible = false;
        initialized = true;
    }

    internal NavMeshAgent GetAgent()
    {
        return agent;
    }

    private void OnDestroy()
    {
        input.npcInput -= NPCInputHandler_playerInput;
    }

    internal NPCInputEventArgs GetNPCInput()
    {
        return npcInput;
    }

    internal NPCInputHandler GetInputHandler()
    {
        return input;
    }

    internal Rigidbody GetRB()
    {
        return rb;
    }

    internal NPCData GetNPCData()
    {
        return npcData;
    }

    private void NPCInputHandler_playerInput(object sender, NPCInputEventArgs NpcInput)
    {
        if (stateMachine.GetCurrentState() == fallen ||
            stateMachine.GetCurrentState() == dead ||
            stateMachine.GetCurrentState() == interacting ||
            GetIsJumping() ||
            GetIsRolling())
            return;

        npcInput = NpcInput;
        EvaluateInput(npcInput);
    }

    internal void EvaluateInput(NPCInputEventArgs NPCInput)
    {
        if (NPCInput == null || stateMachine.GetCurrentState() == fallen || stateMachine.GetCurrentState() == dead || stateMachine.GetCurrentState() == interacting)
            return;

        if (NPCInput.jump && npcEnergy > npcData.jumpingEnergyDrain)
        {
            if (stateMachine.currentState != jump)
                stateMachine.currentState.Exit(this);

            stateMachine.SetCurrentNPCState(jump);
            stateMachine.GetCurrentState().Enter(this);
        }
        else if (NPCInput.jump && npcEnergy < npcData.jumpingEnergyDrain)
        {
            NPCInput.jump = false;
        }

        if (NPCInput.roll && !GetIsRolling() && npcEnergy > 0)
        {
            if (stateMachine.currentState != roll)
                stateMachine.currentState.Exit(this);

            stateMachine.SetCurrentNPCState(roll);
            stateMachine.GetCurrentState().Enter(this);
        }
        else if (NPCInput.roll)
        {
            NPCInput.roll = false;
        }

        if (NPCInput.oxygen)
        {
            if (stateMachine.currentState != interacting)
                stateMachine.currentState.Exit(this);

            refillingOxygen = true;
            stateMachine.SetCurrentNPCState(interacting);
            stateMachine.GetCurrentState().Enter(this);
        }

        if (NPCInput.move != Vector2.zero && !NPCInput.sprint)
        {
            if (stateMachine.currentState != walking && !GetIsRolling() && !GetIsJumping())
            {
                stateMachine.currentState.Exit(this);
                stateMachine.SetCurrentNPCState(walking);
                stateMachine.GetCurrentState().Enter(this);
            }
        }
        else if (NPCInput.move != Vector2.zero && NPCInput.sprint && npcEnergy > 0)
        {
            if (stateMachine.currentState != running && !GetIsRolling() && !GetIsJumping())
            {
                stateMachine.currentState.Exit(this);
                stateMachine.SetCurrentNPCState(running);
                stateMachine.GetCurrentState().Enter(this);
            }
        }
        else if (NPCInput.move == Vector2.zero)
        {
            if (stateMachine.currentState != idle && !GetIsRolling() && !GetIsJumping())
            {
                stateMachine.currentState.Exit(this);
                stateMachine.SetCurrentNPCState(idle);
                stateMachine.GetCurrentState().Enter(this);
            }
        }
    }

    internal Animator GetAnimator()
    {
        return animator;
    }

    internal float GetPlayerEnergy() => npcEnergy;
    internal void SetPlayerEnergy(float value)
    {
        npcEnergy = Mathf.Clamp(value, 0, npcData.energy);
        OnEnergyChanged?.Invoke(npcEnergy, npcData.energy);
    }

    internal void DecreasePlayerEnergyPassively(float value)
    {
        _energyRecoveryTimer = 0f;
        npcEnergy -= npcData.energyDrainRate * value;
        npcEnergy = Mathf.Clamp(npcEnergy, 0, npcData.energy);
        OnEnergyChanged?.Invoke(npcEnergy, npcData.energy);
    }

    internal void DecreasePlayerEnergyInstantly()
    {
        _energyRecoveryTimer = 0f;
        npcEnergy -= npcData.jumpingEnergyDrain;
        npcEnergy = Mathf.Clamp(npcEnergy, 0, npcData.energy);
        OnEnergyChanged?.Invoke(npcEnergy, npcData.energy);
    }

    internal float GetPlayerOxygen() => npcOxygen;

    internal void DrainPlayerOxygen(float value)
    {
        npcOxygen = Mathf.Clamp(value, 0, npcData.oxygen);
        OnOxygenChanged?.Invoke(npcOxygen, npcData.oxygen);
    }

    internal void RefillingPlayerOxygen(float value)
    {
        refillingOxygen = true;
        npcOxygen += npcData.oxygenRecoveryRate * value;
        npcOxygen = Mathf.Clamp(npcOxygen, 0, npcData.oxygen);
        OnOxygenChanged?.Invoke(npcOxygen, npcData.oxygen);
    }

    internal void TriggerOxygenRefill()
    {
        refillingOxygen = true;
    }

    void Update()
    {
        if (GetIsJumping())
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float progress = stateInfo.normalizedTime;

            if (progress >= 1 && (stateInfo.IsName("Stand To Roll") || stateInfo.IsName("Jump Walking") || stateInfo.IsName("Jump Running") || stateInfo.IsName("Jumping")))
            {
                npcInput.jump = false;
                stateMachine.GetCurrentState().Exit(this);

                INPCState nextState = idle;
                if (npcInput.sprint && npcInput.move != Vector2.zero)
                    nextState = running;
                else if (!npcInput.sprint && npcInput.move != Vector2.zero)
                    nextState = walking;

                stateMachine.SetCurrentNPCState(nextState);
                stateMachine.GetCurrentState().Enter(this);
                _energyRecoveryTimer = 0f;
            }
        }

        if (GetIsRolling())
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float progress = stateInfo.normalizedTime;

            if (progress >= 1 && (stateInfo.IsName("Stand To Roll") || stateInfo.IsName("Jumping") || stateInfo.IsName("Jump Running") || stateInfo.IsName("Jump Walking")))
            {
                npcInput.roll = false;
                stateMachine.GetCurrentState().Exit(this);

                INPCState nextState = idle;
                if (npcInput.sprint && npcInput.move != Vector2.zero)
                    nextState = running;
                else if (!npcInput.sprint && npcInput.move != Vector2.zero)
                    nextState = walking;

                stateMachine.SetCurrentNPCState(nextState);
                stateMachine.GetCurrentState().Enter(this);
                _energyRecoveryTimer = 0f;
            }
        }

        if (npcHealth <= 5 && npcHealth > 0)
        {
            if (stateMachine.currentState != fallen)
                stateMachine.currentState.Exit(this);

            stateMachine.SetCurrentNPCState(fallen);
            stateMachine.GetCurrentState().Enter(this);
        }
        else if (npcHealth <= 0)
        {
            if (stateMachine.currentState != dead)
                stateMachine.currentState.Exit(this);

            stateMachine.SetCurrentNPCState(dead);
            stateMachine.GetCurrentState().Enter(this);
        }
        else if (npcHealth > 5 && (stateMachine.currentState == fallen || stateMachine.currentState == dead))
        {
            stateMachine.currentState.Exit(this);
            stateMachine.SetCurrentNPCState(idle);
            stateMachine.GetCurrentState().Enter(this);
        }

        DrainPlayerOxygen(npcOxygen - (npcData.oxygenPassiveDrainRate * Time.deltaTime));

        if (refillingOxygen && npcInput != null && !npcInput.oxygen)
        {
            refillingOxygen = false;
            stateMachine.GetCurrentState().Exit(this);
            stateMachine.SetCurrentNPCState(idle);
            stateMachine.GetCurrentState().Enter(this);
        }

        if (stateMachine.currentState == running && npcInput.sprint)
        {
            if (npcInput.move != Vector2.zero && !GetIsRolling() && !GetIsJumping())
            {
                _energyRecoveryTimer = 0f;
            }

            if (npcEnergy <= 0)
            {
                stateMachine.currentState.Exit(this);
                stateMachine.SetCurrentNPCState(walking);
                stateMachine.GetCurrentState().Enter(this);
            }
        }
        else
        {
            _energyRecoveryTimer += Time.deltaTime;

            if (_energyRecoveryTimer >= npcData.energyRecoveryDelay)
            {
                SetPlayerEnergy(npcEnergy + (npcData.energyRecoveryRate * Time.deltaTime));
            }
        }

        stateMachine.GetCurrentState().FixedUpdate(this);
    }
}