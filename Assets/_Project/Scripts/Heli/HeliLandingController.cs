using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class HeliLandingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineAnimate splineAnimate;
    [SerializeField] private Animation heliAnimation;

    [Header("Splines")]
    [SerializeField] private SplineContainer landingSpline;
    [SerializeField] private SplineContainer departureSpline;

    [Header("Settings")]
    [SerializeField] private string rotorClipName = "rotation+No Wheels";
    [SerializeField] private float landingDuration = 8f;
    [SerializeField] private float departureDuration = 8f;
    [SerializeField] private float groundedDelay = 5f;
    [Range(0f, 1f)] [SerializeField] private float helicopterSfxVolume = 1f;

    private enum HeliPhase { Landing, Grounded, Departing, Done }
    private HeliPhase _phase = HeliPhase.Landing;

    private void Start()
    {
        heliAnimation[rotorClipName].wrapMode = WrapMode.Loop;
        heliAnimation.Play(rotorClipName);

        StartLanding();
    }

    public void StartLanding()
    {
        _phase = HeliPhase.Landing;

        splineAnimate.Container = landingSpline;
        splineAnimate.Duration = landingDuration;
        splineAnimate.Restart(true); // rewind to t=0 and play

        AudioManager.Instance?.StartLoopingSfx(AudioManager.SfxClip.HelicopterSound, helicopterSfxVolume);
    }

    private void Update()
    {
        if (_phase == HeliPhase.Landing && splineAnimate.NormalizedTime >= 1f)
        {
            _phase = HeliPhase.Grounded;
            splineAnimate.Pause();
            AudioManager.Instance?.StopLoopingSfx(AudioManager.SfxClip.HelicopterSound);

            StartCoroutine(SpinDown());
            StartCoroutine(WaitThenDepart());
        }
    }

    private IEnumerator WaitThenDepart()
    {
        yield return new WaitForSeconds(groundedDelay);

        _phase = HeliPhase.Departing;

        // Spin back up to full speed before swapping splines
        yield return StartCoroutine(SpinUp());

        splineAnimate.Container = departureSpline;
        splineAnimate.Duration = departureDuration;
        splineAnimate.Restart(true);

        AudioManager.Instance?.StartLoopingSfx(AudioManager.SfxClip.HelicopterSound, helicopterSfxVolume);
        
        yield return new WaitUntil(() => splineAnimate.NormalizedTime >= 1f);

        _phase = HeliPhase.Done;
        splineAnimate.Pause();
        yield return StartCoroutine(SpinDown());
        gameObject.SetActive(false);
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

    private void OnDisable()
    {
        AudioManager.Instance?.StopLoopingSfx(AudioManager.SfxClip.HelicopterSound);
    }
}