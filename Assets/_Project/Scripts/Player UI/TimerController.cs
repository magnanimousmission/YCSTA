using UnityEngine;
using System;

public class TimerController : MonoBehaviour
{
    public static TimerController Instance { get; private set; }
    
    [SerializeField] private float timerDuration = 8f;
    [SerializeField] private float extractingStartTime = 60f;
    private float _currentTime;
    private bool _finished = false;
    private bool _extractingStarted = false;
    
    public static event Action OnTimerFinished;
    public static event Action<float> OnTimerTick;

    public static event Action onExtractingStarting;

    public float CurrentTime => _currentTime;
    public float NormalizedTime => _currentTime / timerDuration;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        _currentTime = timerDuration;
    }

    private void Update()
    {
        if (_finished) return;

        if (!_extractingStarted && _currentTime <= extractingStartTime)
        {
            _extractingStarted = true;
            onExtractingStarting?.Invoke();
            Debug.Log("Extracting starting");
        }
        
        _currentTime -= Time.deltaTime;

        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            _finished = true;
            OnTimerTick?.Invoke(0f);
            OnTimerFinished?.Invoke();
            return;
        }

        OnTimerTick?.Invoke(_currentTime / timerDuration);
    }
}