using System;
using UnityEngine;
using Photon.Pun;

public class NPCInputHandler : MonoBehaviour
{
    public event EventHandler<NPCInputEventArgs> npcInput;

    [SerializeField] private float followStopDistance = 1.5f;
    [SerializeField] private float sprintDistance = 5f;

    private NPCInputEventArgs _currentInput = new();
    private bool _shouldProcessInput  = true;
    [SerializeField]private Collider myCollider;

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
        if (myCollider.bounds.Contains(other.transform.position)) return;

        if (other.CompareTag("Player") && other.gameObject == _currentInput.target)
        {
            ClearInput();
            OnTargetLost?.Invoke();
        }
    }

    public void SetTarget(GameObject target)
    {
        if(target != null)
            _currentInput.target = target;
    
    }

    private void Update()
    {
        if (!HasSimulationAuthority())
        {
            ClearMotionInput();
            return;
        }

        if (_currentInput.target == null)
            return;

        Vector3 toPlayer = _currentInput.target.transform.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > followStopDistance)
        {
            Vector3 localDir = transform.InverseTransformDirection(toPlayer.normalized);
            _currentInput.move = new Vector2(localDir.x, localDir.z);
            _currentInput.sprint = distance > sprintDistance;
        }
        else
        {
            _currentInput.move = Vector2.zero;
            _currentInput.sprint = false;
        }

        _currentInput.jump = false;
        _currentInput.roll = false;
        _currentInput.interacting = false;
        npcInput?.Invoke(this, _currentInput);
    }

    private void ClearMotionInput()
    {
        _currentInput.move = Vector2.zero;
        _currentInput.sprint = false;
        _currentInput.jump = false;
        _currentInput.roll = false;
        _currentInput.interacting = false;
        _currentInput.oxygen = false;
    }

    private void ClearInput()
    {
        ClearMotionInput();
        _currentInput.target = null;
        _currentInput.reset = true;
        npcInput?.Invoke(this, _currentInput);
    }
}