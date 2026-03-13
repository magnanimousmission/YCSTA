using UnityEngine;
using UnityEngine.UI;

public class AuraDeathSliderUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject textPanel;

    private AuraDeathTimer _auraDeathTimer;

    private void Awake()
    {
        slider.gameObject.SetActive(false);
        textPanel.SetActive(false);
    }

    public void Bind(AuraDeathTimer timer)
    {
        _auraDeathTimer = timer;
        _auraDeathTimer.OnTimerTick += HandleTimerTick;
        _auraDeathTimer.OnTimerCancelled += HandleTimerCancelled;
        _auraDeathTimer.OnTimerExpired += HandleTimerExpired;
    }
    
    private void OnDisable()
    {
        if (_auraDeathTimer == null) return;
        _auraDeathTimer.OnTimerTick -= HandleTimerTick;
        _auraDeathTimer.OnTimerCancelled -= HandleTimerCancelled;
        _auraDeathTimer.OnTimerExpired -= HandleTimerExpired;
    }

    private void HandleTimerTick(float remaining)
    {
        slider.gameObject.SetActive(true);
        textPanel.SetActive(true);
        slider.value = remaining / _auraDeathTimer.Duration;
    }

    private void HandleTimerCancelled()
    {
        slider.value = 1f;
        slider.gameObject.SetActive(false);
        textPanel.SetActive(false);
    }

    private void HandleTimerExpired()
    {
        slider.value = 0f;
        slider.gameObject.SetActive(false);
        textPanel.SetActive(false);
    }
}