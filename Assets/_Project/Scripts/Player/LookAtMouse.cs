using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class LookAtMouse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [Header("Rotation Feel")]
    [SerializeField, Range(0f, 40f)] private float deadZonePercent = 8f;
    [SerializeField, Range(30f, 180f)] private float minTurnSpeed = 80f;
    [SerializeField, Range(180f, 1080f)] private float maxTurnSpeed = 480f;
    [SerializeField, Range(0.5f, 3f)] private float rampPower = 1.4f;
    private Rigidbody _rb;
    private Animator _animator;
    private Transform _tr;
    private PhotonView _photonView;
    private bool _ready;
    private Quaternion _desiredRotation;
    private void Awake()
    {
        _tr = transform;
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        if (mainCamera == null)
            mainCamera = Camera.main;
        _photonView = GetComponentInParent<PhotonView>();
        if (_photonView != null && !_photonView.IsMine)
        {
            enabled = false;
            return;
        }
        _rb.freezeRotation = true;
        _desiredRotation = _tr.rotation;
        _ready = true;
    }
    private void LateUpdate()
    {
        if (!_ready || Mouse.current == null || mainCamera == null) return;
        float mouseX = Mouse.current.position.ReadValue().x;
        float screenCenter = Screen.width * 0.5f;
        float distFromCenter = mouseX - screenCenter;
        float deadZonePixels = screenCenter * (deadZonePercent / 50f);
        // Dead zone — do nothing while mouse is near center
        if (Mathf.Abs(distFromCenter) < deadZonePixels) return;
        // How far past the dead zone (0=just outside, 1=screen edge)
        float t = Mathf.Clamp01(
            Mathf.InverseLerp(deadZonePixels, screenCenter, Mathf.Abs(distFromCenter))
        );
        t = Mathf.Pow(t, rampPower);
        float speed = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, t);
        // Left of center = rotate left, right of center = rotate right
        float direction = Mathf.Sign(distFromCenter);
        float currentY = _rb.rotation.eulerAngles.y;
        _desiredRotation = Quaternion.Euler(0f, currentY + direction * speed * Time.deltaTime, 0f);
    }
    private void OnAnimatorMove()
    {
        if (!_ready) return;
        _rb.MovePosition(_rb.position + _animator.deltaPosition);
        _rb.MoveRotation(_desiredRotation);
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_tr == null) _tr = transform;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(_tr.position, 0.3f);
    }
#endif
}