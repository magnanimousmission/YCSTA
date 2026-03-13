using UnityEngine;
using TMPro;

public class TimerUIController : MonoBehaviour
{ 
    private TimerController _timerController;
    private TextMeshProUGUI _timerText;

    private void Awake()
    {
        _timerController = GetComponent<TimerController>();
        _timerText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        _timerController.OnTimerTick += HandleTimerTick;
        _timerController.OnTimerFinished += HandleTimerFinished;
    }

    private void OnDisable()
    {
        _timerController.OnTimerTick -= HandleTimerTick;
        _timerController.OnTimerFinished -= HandleTimerFinished;
    }

    private void HandleTimerTick(float normalizedTime)
    {
        var seconds = _timerController.CurrentTime;
        _timerText.text = FormatTime(seconds);
    }

    private void HandleTimerFinished()
    {
        _timerText.text = "00:00";
    }

    private string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}