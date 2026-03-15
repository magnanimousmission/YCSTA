using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class HeliLandingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineAnimate splineAnimate;
    [SerializeField] private Animation heliAnimation;   // the Animation component on the heli mesh

    [Header("Settings")]
    [SerializeField] private string rotorClipName = "rotation+No WHeels";
    [SerializeField] private float landingDuration = 8f;
    [Range(0f, 1f)] [SerializeField] private float helicopterSfxVolume = 1f;

    private bool _landed = false;

    private void Start()
    {
        // Start rotor animation immediately, loop forever
        heliAnimation[rotorClipName].wrapMode = WrapMode.Loop;
        heliAnimation.Play(rotorClipName);

        // Don't move yet — wait for cutscene to call this
        splineAnimate.Duration = landingDuration;

        StartLanding();
    }

    /// <summary>
    /// Call this from your cutscene coordinator when the camera is ready.
    /// </summary>
    public void StartLanding()
    {
        _landed = false;
        AudioManager.Instance?.StartLoopingSfx(AudioManager.SfxClip.HelicopterSound, helicopterSfxVolume);
        splineAnimate.Play();
    }

    private void Update()
    {
        if (_landed) return;

        if (splineAnimate.NormalizedTime >= 1f)
        {
            _landed = true;
            splineAnimate.Pause();
            AudioManager.Instance?.StopLoopingSfx(AudioManager.SfxClip.HelicopterSound);
            
            StartCoroutine(SpinDown());
        }
    }

    private void OnDisable()
    {
        AudioManager.Instance?.StopLoopingSfx(AudioManager.SfxClip.HelicopterSound);
    }

    private IEnumerator SpinDown()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 0.3f;
            heliAnimation[rotorClipName].speed = t;
            yield return null;
        }
        heliAnimation.Stop();
    }
}