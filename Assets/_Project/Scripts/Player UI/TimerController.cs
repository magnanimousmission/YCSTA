using UnityEngine;
using System;

public class TimerController : MonoBehaviour
{
    [SerializeField] private float timerDuration = 8f;

    private float _currentTime;
    
    public event Action OnTimerFinished;
    public event Action<float> OnTimerTick;

    public float CurrentTime => _currentTime;


    private void Start()
    {
        _currentTime = timerDuration;
    }

    private void Update()
    {
        _currentTime -= Time.deltaTime;
        OnTimerTick?.Invoke(_currentTime / timerDuration);

        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            OnTimerFinished?.Invoke();
        }
    }
    
}