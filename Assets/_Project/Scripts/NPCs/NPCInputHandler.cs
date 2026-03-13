using System;
using UnityEngine;
using Photon.Pun;

public class NPCInputHandler : MonoBehaviour
{
    public event EventHandler<NPCInputEventArgs> npcInput;

    [SerializeField] private float followStopDistance = 1.5f;
    
    private NPCInputEventArgs _currentInput = new();
    private bool shouldProcessInput = true;
    Transform target;

    private void Awake()
    {
        var photonView = GetComponentInParent<PhotonView>();
        if (photonView != null)
            shouldProcessInput = photonView.IsMine;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(target);
            target = other.transform;
            _currentInput.target = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = null;
            _currentInput.target = null;
            ClearInput();
        }
    }

    private void Update()
    {
        if (!shouldProcessInput )
        {
            ClearInput();
            return;
        }

        if (target == null)
            return;
        Vector3 toPlayer = target.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > followStopDistance)
        {
            // Convert world direction to local 2D movement input
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