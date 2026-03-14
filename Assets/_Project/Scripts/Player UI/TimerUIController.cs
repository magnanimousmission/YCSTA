using UnityEngine;
using TMPro;

public class TimerUIController : MonoBehaviour
{ 
    private TextMeshProUGUI _timerText;

    private void Awake()
    {
        _timerText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        TimerController.OnTimerTick += HandleTimerTick;
        TimerController.OnTimerFinished += HandleTimerFinished;
    }

    private void OnDisable()
    {
        TimerController.OnTimerTick -= HandleTimerTick;
        TimerController.OnTimerFinished -= HandleTimerFinished;
    }

    private void HandleTimerTick(float normalizedTime)
    {
        var seconds = TimerController.Instance.CurrentTime;
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