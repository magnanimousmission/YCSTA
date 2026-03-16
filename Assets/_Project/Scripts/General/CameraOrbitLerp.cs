using UnityEngine;
using UnityEngine.Splines;

[AddComponentMenu("Camera/Camera Spline Lerp")]
public class CameraOrbitLerp: MonoBehaviour
{
    public bool UsesTimedStop => stopAfterDuration;
    public bool HasStopped { get; private set; }

    [Header("Spline")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField, Min(0f)] private float travelDuration = 10f;
    [SerializeField] private bool snapToSplineOnStart = true;

    [Header("Look Target")]
    [SerializeField] private Transform lookTarget;
    [SerializeField] private Vector3 lookTargetOffset = Vector3.zero;

    [Header("Smoothing")]
    [SerializeField, Min(0f)] private float positionLerpSpeed = 6f;
    [SerializeField, Min(0f)] private float rotationLerpSpeed = 8f;

    [Header("Duration")]
    [SerializeField] private bool stopAfterDuration = true;
    [SerializeField, Min(0f)] private float stopAfterSeconds = 10f;

    [Header("Post Stop")]
    [SerializeField] private bool keepLookingAtTargetAfterStop = true;

    private float _travelT;        // 0–1 progress along spline
    private float _elapsedTime;
    public bool HasSettled = false;

    private void Start()
    {
        if (splineContainer == null)
        {
            Debug.LogWarning("[CameraSplineLerp] No SplineContainer assigned.", this);
            return;
        }
        RenderSettings.fog = false;
        if (snapToSplineOnStart)
        {
            var startPos = EvaluateSplinePosition(0f);
            transform.position = startPos;
            transform.rotation = GetLookRotation(startPos);
            _travelT = 0f;
        }
        else
        {
            // Start from the closest point on the spline to current position
            SplineUtility.GetNearestPoint(
                splineContainer.Spline,
                transform.InverseTransformPoint(transform.position),
                out _,
                out _travelT
            );
        }
        
    }

    private void LateUpdate()
    {
        if (splineContainer == null) return;

        var isStopped = stopAfterDuration && _elapsedTime >= stopAfterSeconds;

        if (!isStopped)
        {
            _elapsedTime += Time.deltaTime;
            isStopped = stopAfterDuration && _elapsedTime >= stopAfterSeconds;

            // Advance along spline based on elapsed time vs travel duration
            _travelT = Mathf.Clamp01(_elapsedTime / travelDuration);
        }

        HasStopped = isStopped;

        // Position
        var desiredPosition = EvaluateSplinePosition(_travelT);
        var positionT = 1f - Mathf.Exp(-positionLerpSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionT);

        // Rotation — always face the look target if assigned
        if (lookTarget != null || keepLookingAtTargetAfterStop)
        {
            var desiredRotation = GetLookRotation(transform.position);
            var rotationT = 1f - Mathf.Exp(-rotationLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationT);
        }
    }

    public void DisableOrbit()
    {
        HasSettled = true;
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.enabled = false;

            var audioListener = cam.GetComponent<AudioListener>();
            if (audioListener != null)
                audioListener.enabled = false;
        }

        RenderSettings.fog = true;
    }

    private Vector3 EvaluateSplinePosition(float t)
    {
        // SplineUtility.Evaluate gives position, tangent, up in local spline space
        SplineUtility.Evaluate(splineContainer.Spline, t, out var localPos, out _, out _);
        return splineContainer.transform.TransformPoint(localPos);
    }

    private Quaternion GetLookRotation(Vector3 fromPosition)
    {
        if (lookTarget == null)
            return transform.rotation;

        var targetPoint = lookTarget.position + lookTargetOffset;
        var direction = targetPoint - fromPosition;

        if (direction.sqrMagnitude < 0.0001f)
            return transform.rotation;

        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}