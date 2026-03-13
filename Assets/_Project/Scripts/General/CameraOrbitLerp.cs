using UnityEngine;

[AddComponentMenu("Camera/Camera Orbit Lerp")]
public class CameraOrbitLerp : MonoBehaviour
{
    public bool UsesTimedStop => stopAfterDuration;
    public bool HasStopped { get; private set; }

    [Header("Orbit Center")]
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private Vector3 centerOffset = Vector3.zero;

    [Header("Orbit Shape")]
    [SerializeField, Min(0.1f)] private float orbitRadius = 18f;
    [SerializeField] private float orbitHeight = 6f;
    [SerializeField, Min(0.1f)] private float secondsPerRevolution = 10f;
    [SerializeField] private bool clockwise = true;

    [Header("Smoothing")]
    [SerializeField, Min(0f)] private float positionLerpSpeed = 6f;
    [SerializeField, Min(0f)] private float rotationLerpSpeed = 8f;

    [Header("Startup")]
    [SerializeField] private bool snapToOrbitOnStart = true;

    [Header("Duration")]
    [SerializeField] private bool stopAfterDuration = true;
    [SerializeField, Min(0f)] private float stopAfterSeconds = 10f;

    [Header("Post Stop")]
    [SerializeField] private bool lerpCenterOffsetAfterStop = true;
    [SerializeField] private Vector3 postStopCenterOffset = new Vector3(15f, 8f, 35f);
    [SerializeField, Min(0f)] private float postStopCenterLerpSpeed = 1.5f;

    private float _angleDegrees;
    private float _elapsedTime;

    private void Start()
    {
        if (snapToOrbitOnStart)
        {
            var center = GetCenterPoint();
            var startPos = center + new Vector3(orbitRadius, orbitHeight, 0f);
            transform.position = startPos;
            transform.rotation = Quaternion.LookRotation(center - startPos, Vector3.up);
            _angleDegrees = 0f;
            return;
        }

        var offset = transform.position - GetCenterPoint();
        offset.y = 0f;

        if (offset.sqrMagnitude > 0.0001f)
            _angleDegrees = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
    }

    private void LateUpdate()
    {
        var isStopped = stopAfterDuration && _elapsedTime >= stopAfterSeconds;

        if (!isStopped)
        {
            _elapsedTime += Time.deltaTime;
            isStopped = stopAfterDuration && _elapsedTime >= stopAfterSeconds;
        }
        else if (lerpCenterOffsetAfterStop)
        {
            var centerOffsetT = 1f - Mathf.Exp(-postStopCenterLerpSpeed * Time.deltaTime);
            centerOffset = Vector3.Lerp(centerOffset, postStopCenterOffset, centerOffsetT);
        }

        HasStopped = isStopped;

        var center = GetCenterPoint();

        if (!isStopped)
        {
            var direction = clockwise ? -1f : 1f;
            var degreesPerSecond = 360f / secondsPerRevolution;
            _angleDegrees += direction * degreesPerSecond * Time.deltaTime;
        }

        var radians = _angleDegrees * Mathf.Deg2Rad;
        var desiredPosition = center + new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            orbitHeight,
            Mathf.Sin(radians) * orbitRadius);

        var lookDirection = center - desiredPosition;
        if (lookDirection.sqrMagnitude < 0.0001f)
            return;

        var desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        var positionT = 1f - Mathf.Exp(-positionLerpSpeed * Time.deltaTime);
        var rotationT = 1f - Mathf.Exp(-rotationLerpSpeed * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionT);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationT);
    }

    public void DisableOrbit()
    {
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.enabled = false;

            var audioListener = cam.GetComponent<AudioListener>();
            if (audioListener != null)
                audioListener.enabled = false;
        }
    }

    private Vector3 GetCenterPoint()
    {
        if (orbitCenter == null)
            return centerOffset;

        return orbitCenter.position + centerOffset;
    }
}
