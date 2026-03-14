using UnityEngine;
using System.Collections.Generic;

public class ExtractionController : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private string extractableTag = "Player";
    private readonly HashSet<PlayerCore> _playersInZone = new();
    private bool _extractionTriggered = false;
    
    private PlayerCore _playerCore;
    
    [SerializeField]
    private DeathScreenController deathScreen;
    [SerializeField]
    private ExtractionUIController extractionUI;

    private void OnEnable()
    {
        TimerController.OnTimerFinished += HandleTimerFinished;
    }

    private void OnDisable()
    {
        TimerController.OnTimerFinished -= HandleTimerFinished;
    }
    
    public void Bind(PlayerCore playerCore)
    {
        _playerCore = playerCore;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_extractionTriggered) return;

        PlayerCore player = other.GetComponentInParent<PlayerCore>();
        if (player == null) player = other.GetComponentInChildren<PlayerCore>();
        if (player == null) return;

        _playersInZone.Add(player);
        Debug.Log($"[Extraction] {player.name} entered zone. ({_playersInZone.Count} inside)");
    }

    private void OnTriggerExit(Collider other)
    {
        if (_extractionTriggered) return;

        PlayerCore player = other.GetComponentInParent<PlayerCore>();
        if (player == null) player = other.GetComponentInChildren<PlayerCore>();
        if (player == null) return;

        _playersInZone.Remove(player);
        Debug.Log($"[Extraction] {player.name} left zone. ({_playersInZone.Count} inside)");
    }

    private void HandleTimerFinished()
    {
        if (_extractionTriggered) return;
        _extractionTriggered = true;

        if (_playerCore == null) return;

        if (_playersInZone.Contains(_playerCore))
        {
            Debug.Log($"[Extraction] {_playerCore.name} extracted successfully.");
            // TODO: show extraction success UI
            extractionUI.ActivateExtractionUI();
        }
        else
        {
            Debug.Log($"[Extraction] {_playerCore.name} failed to extract.");
            deathScreen.ActivateDeathPanel();
        }
    }
}