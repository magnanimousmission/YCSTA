using System;
using UnityEngine;
using Photon.Pun;

public class NPCInputHandler : MonoBehaviour
{
    public event EventHandler<NPCInputEventArgs> npcInput;

    [SerializeField] private float followStopDistance = 1.5f;
    
    private NPCInputEventArgs _currentInput = new();
    private Transform _target;
    private PhotonView _photonView;
    
    public event Action OnTargetLost;

    private void Awake()
    {
        _photonView = GetComponentInParent<PhotonView>();
    }

    private bool HasSimulationAuthority()
    {
        if (_photonView == null)
            return true;

        if (_photonView.IsMine)
            return true;

        // Scene/room-owned NPCs should be simulated by MasterClient.
        if (_photonView.IsRoomView)
            return PhotonNetwork.IsMasterClient;

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("Player"))
        // {
        //     Debug.Log(_target);
        //     _target = other.transform;
        //     _currentInput.target = other.gameObject;
        // }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.transform == _target)
        {
            _target = null;
            ClearInput();
            OnTargetLost?.Invoke();
        }
    }

    public void SetTarget(GameObject target)
    {
        _target = target != null ? target.transform : null;
        _currentInput.target = target;
    }
    
    private void Update()
    {
        if (!HasSimulationAuthority())
        {
            ClearMotionInput();
            return;
        }
        
        if (_target == null)
            return;
        
        Vector3 toPlayer = _target.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > followStopDistance)
        {
            Vector3 localDir = transform.InverseTransformDirection(toPlayer.normalized);
            _currentInput.move = new Vector2(localDir.x, localDir.z);
        }
        else
        {
            _currentInput.move = Vector2.zero;
        }

        _currentInput.sprint = false;
        _currentInput.jump = false;
        _currentInput.roll = false;
        _currentInput.oxygen = false;

        npcInput?.Invoke(this, _currentInput);
    }

    private void ClearMotionInput()
    {
        _currentInput.move = Vector2.zero;
        _currentInput.sprint = false;
        _currentInput.jump = false;
        _currentInput.roll = false;
        _currentInput.oxygen = false;
    }

    private void ClearInput()
    {
        ClearMotionInput();
        _currentInput.target = null;
        npcInput?.Invoke(this, _currentInput);
    }
}