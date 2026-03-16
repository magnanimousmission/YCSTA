using UnityEngine;
using UnityEngine.Splines;

[AddComponentMenu("Camera/Extraction Cutscene Camera Controller")]
public class ExtractionCutsceneCameraController : MonoBehaviour
{
    public bool HasFinished { get; private set; }

    [Header("Spline")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField, Min(0f)] private float travelDuration = 8f;
    [SerializeField] private bool snapToSplineOnStart = false;

    [Header("Look Target")]
    [SerializeField] private Transform lookTarget;
    [SerializeField] private Vector3 lookTargetOffset = Vector3.zero;

    [Header("Smoothing")]
    [SerializeField, Min(0f)] private float positionLerpSpeed = 6f;
    [SerializeField, Min(0f)] private float rotationLerpSpeed = 8f;

    [Header("On Finish")]
    [SerializeField] private bool keepLookingAtTargetAfterFinish = true;

    public static ExtractionCutsceneCameraController Instance { get; private set; }

    private Camera _camera;
    private float _travelT;
    private float _elapsedTime;
    private bool _moving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[ExtractionCutsceneCameraController] Duplicate instance destroyed.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _camera = GetComponent<Camera>();
        if (_camera != null)
            _camera.enabled = false;
    }

    private void Start()
    {
        if (splineContainer == null)
        {
            Debug.LogWarning("[ExtractionCutsceneCameraController] No SplineContainer assigned.", this);
            return;
        }

        if (snapToSplineOnStart)
        {
            var startPos = EvaluateSplinePosition(0f);
            transform.position = startPos;
            transform.rotation = GetLookRotation(startPos);
            _travelT = 0f;
        }
    }

    private void LateUpdate()
    {
        if (!_moving || splineContainer == null) return;

        if (!HasFinished)
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= travelDuration)
                HasFinished = true;

            _travelT = Mathf.Clamp01(_elapsedTime / travelDuration);
        }

        // Position
        var desiredPosition = EvaluateSplinePosition(_travelT);
        var positionT = 1f - Mathf.Exp(-positionLerpSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionT);

        // Rotation
        if (lookTarget != null && (!HasFinished || keepLookingAtTargetAfterFinish))
        {
            var desiredRotation = GetLookRotation(transform.position);
            var rotationT = 1f - Mathf.Exp(-rotationLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationT);
        }
    }

    public void Begin()
    {
        if (splineContainer == null)
        {
            Debug.LogWarning("[ExtractionCutsceneCameraController] Cannot begin — no SplineContainer assigned.", this);
            return;
        }

        if (_camera != null)
            _camera.enabled = true;
        
        RenderSettings.fog = false;
        _travelT = 0f;
        _elapsedTime = 0f;
        HasFinished = false;
        _moving = true;
    }

    public void Stop()
    {
        _moving = false;

        if (_camera != null)
            _camera.enabled = false;
    }

    private Vector3 EvaluateSplinePosition(float t)
    {
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