using System;
using UnityEngine;
using Photon.Pun;

public class NPCInputHandler : MonoBehaviour
{
    public event EventHandler<NPCInputEventArgs> npcInput;

    [SerializeField] private float followStopDistance = 1.5f;
    
    private NPCInputEventArgs _currentInput = new();
    private bool _shouldProcessInput  = true;
    private Transform _target;
    
    public event Action OnTargetLost;

    private void Awake()
    {
        var photonView = GetComponentInParent<PhotonView>();
        if (photonView != null)
            _shouldProcessInput  = photonView.IsMine;
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
        if (!_shouldProcessInput)
        {
            ClearInput();
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

    private void ClearInput()
    {
        _currentInput.move = Vector2.zero;
        _currentInput.sprint = false;
        _currentInput.jump = false;
        _currentInput.roll = false;
        _currentInput.oxygen = false;
        _currentInput.target = null;
        npcInput?.Invoke(this, _currentInput);
    }
}