using System;
using UnityEngine;

public class KeyPickUp : MonoBehaviour
{
    [SerializeField] private string objectiveId = "pickup_key_01";

    private bool _isActive;
    private bool _hasPickedUp;
    private MeshRenderer _meshRenderer;
    
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false;
    }

    private void OnEnable()
    {
        ObjectiveManager.OnSubObjectiveActivated += HandleSubObjectiveActivated;
    }

    private void OnDisable()
    {
        ObjectiveManager.OnSubObjectiveActivated -= HandleSubObjectiveActivated;
    }

    private void HandleSubObjectiveActivated(string id)
    {
        _isActive = id == objectiveId;
        if (_isActive)
        {
            _meshRenderer.enabled = true;
        }
        else
        {
            _meshRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive || _hasPickedUp) return;
        PlayerCore player = other.GetComponentInChildren<PlayerCore>();
        if (player == null) return;

        _hasPickedUp = true;
        Debug.Log("Picked up key");
        ObjectiveManager.Instance.CompleteSubObjective();
    }
}