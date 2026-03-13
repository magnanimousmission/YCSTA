using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpectatorController : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.5f, -4f);
    [SerializeField] private float followSmoothTime = 0.1f;
    [SerializeField] private KeyCode nextTargetKey = KeyCode.E;
    [SerializeField] private KeyCode prevTargetKey = KeyCode.Q;

    private List<PlayerDeathController> _livingPlayers = new List<PlayerDeathController>();
    private int _targetIndex = 0;
    private Transform _target;
    private Vector3 _velocity;
    
    private void OnEnable()
    {
        PlayerDeathController.OnAllPlayersDead += OnKickedFromSpectate;
    }

    private void OnDisable()
    {
        PlayerDeathController.OnAllPlayersDead -= OnKickedFromSpectate;
    }
    private void Update()
    {
        RefreshLivingPlayers(); // TODO: try to figure out how to not use this, cost too much, get all every frame

        if (Input.GetKeyDown(nextTargetKey)) CycleTarget(1);
        if (Input.GetKeyDown(prevTargetKey)) CycleTarget(-1);

        if (_target != null)
        {
            Vector3 desiredPos = _target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, followSmoothTime);
            
            transform.LookAt(_target.position);
        }
    }
    private void OnKickedFromSpectate()
    {
        FindFirstObjectByType<DeathScreenController>()?.QuitButtonClicked();
    }

    public void FindNextTarget()
    {
        RefreshLivingPlayers();
        if (_livingPlayers.Count == 0) return;

        _targetIndex = 0;
        _target = _livingPlayers[_targetIndex].transform;
    }

    private void CycleTarget(int direction)
    {
        if (_livingPlayers.Count == 0) return;

        _targetIndex = (_targetIndex + direction + _livingPlayers.Count) % _livingPlayers.Count;
        _target = _livingPlayers[_targetIndex].transform;
    }

    private void RefreshLivingPlayers()
    {
        _livingPlayers.Clear();
        foreach (var dc in FindObjectsByType<PlayerDeathController>(FindObjectsSortMode.None))
        {
            if (!dc.IsDead)
                _livingPlayers.Add(dc);
        }
    }
    
    public static bool AnyLivingPlayersExist()
    {
        foreach (var dc in FindObjectsByType<PlayerDeathController>(FindObjectsSortMode.None))
            if (!dc.IsDead) return true;
        return false;
    }
}