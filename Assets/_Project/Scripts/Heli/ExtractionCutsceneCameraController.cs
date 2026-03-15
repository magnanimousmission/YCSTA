using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class ExtractionCutsceneCameraController  : MonoBehaviour
{
    [SerializeField] private SplineContainer spline;
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private Camera _camera;
    private float _t = 0f;
    private bool _moving = false;
    public static ExtractionCutsceneCameraController _instance;

    private void Awake()
    {
        _instance = this;
        _camera.enabled = false;
    }
    
    private void Update()
    {
        if (!_moving) return;

        _t = Mathf.Clamp01(_t + speed * Time.deltaTime);
        transform.position = spline.EvaluatePosition(_t);
    }

    public void Begin()
    {
        _camera.enabled = true;
        _t = 0f;
        _moving = true;
    }
}