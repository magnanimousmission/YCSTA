using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ExtractionStartingController : MonoBehaviour
{
    [SerializeField] private GameObject heli;
    
    [Header("References")]
    [SerializeField] private SplineAnimate splineAnimate;
    [SerializeField] private Animation heliAnimation;

    [Header("Splines")]
    [SerializeField] private SplineContainer reEnteringSpline;
    [SerializeField] private SplineContainer extractingSpline;

    [Header("Settings")]
    [SerializeField] private string rotorClipName = "Rotation+No Wheels";
    [SerializeField] private float reEnteringDuration = 8f;
    [SerializeField] private float extractingDuration = 8f;
    [Header("Audio")]
    [SerializeField] private AudioClip extractingClip;
    [SerializeField] private AudioClip helicopterLoopClip;
    
    private AudioSource _audioSource;

    private enum HeliPhase { Idle, ReEntering, Grounded, Extracting, Done }
    private HeliPhase _phase = HeliPhase.Idle;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        TimerController.OnExtractingStarting += HandleExtractionStarting;
        TimerController.OnTimerFinished += HandleTimerFinished;
    }

    private void OnDisable()
    {
        TimerController.OnExtractingStarting -= HandleExtractionStarting;
        TimerController.OnTimerFinished -= HandleTimerFinished;
        _audioSource.Stop();
    }

    private void HandleExtractionStarting()
    {
        if (_phase != HeliPhase.Idle) return;

        heli.SetActive(true);
        _phase = HeliPhase.ReEntering;

        _audioSource.PlayOneShot(extractingClip);
        _audioSource.clip = helicopterLoopClip;
        _audioSource.loop = true;
        _audioSource.Play();

        heliAnimation[rotorClipName].wrapMode = WrapMode.Loop;
        heliAnimation[rotorClipName].speed = 1f;
        heliAnimation.Play(rotorClipName);

        splineAnimate.Container = reEnteringSpline;
        splineAnimate.Duration = reEnteringDuration;
        splineAnimate.Restart(true);
    }

    private void HandleTimerFinished()
    {
        // Only depart if we're sitting on the pad waiting
        if (_phase != HeliPhase.Grounded) return;

        StartCoroutine(Depart());
    }

    private void Update()
    {
        if (_phase == HeliPhase.ReEntering && splineAnimate.NormalizedTime >= 1f)
        {
            _phase = HeliPhase.Grounded;
            splineAnimate.Pause();
            _audioSource.Stop();
            StartCoroutine(SpinDown());
        }
    }

    private IEnumerator Depart()
    {
        _phase = HeliPhase.Extracting;

        yield return StartCoroutine(SpinUp());

        _audioSource.clip = helicopterLoopClip;
        _audioSource.loop = true;
        _audioSource.Play();

        splineAnimate.Container = extractingSpline;
        splineAnimate.Duration = extractingDuration;
        splineAnimate.Restart(true);

        yield return new WaitUntil(() => splineAnimate.NormalizedTime >= 1f);

        _phase = HeliPhase.Done;
        splineAnimate.Pause();
        _audioSource.Stop();
        StartCoroutine(SpinDown());
    }

    private IEnumerator SpinDown()
    {
        float t = heliAnimation[rotorClipName].speed;
        while (t > 0f)
        {
            t -= Time.deltaTime * 0.3f;
            heliAnimation[rotorClipName].speed = Mathf.Max(t, 0f);
            yield return null;
        }
        heliAnimation.Stop();
    }

    private IEnumerator SpinUp()
    {
        heliAnimation[rotorClipName].wrapMode = WrapMode.Loop;
        heliAnimation.Play(rotorClipName);

        float t = heliAnimation[rotorClipName].speed;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.3f;
            heliAnimation[rotorClipName].speed = Mathf.Min(t, 1f);
            yield return null;
        }
    }
}