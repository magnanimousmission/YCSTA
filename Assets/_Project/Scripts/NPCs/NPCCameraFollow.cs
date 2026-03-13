using UnityEngine;

public class NPCCameraFollow : MonoBehaviour
{
    [Header("Target & Offset")]
    [SerializeField] private Transform target; // Drag your player/character Transform here in Inspector

    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); // Relative to TARGET: X=side, Y=height, Z=behind (negative = behind player)

    [Header("Smooth Follow")]
    [SerializeField, Range(0.01f, 1f)] private float smoothSpeed = 0.125f; // Lerp speed for position & rotation (lower = smoother)

    [Header("Camera Orientation")]
    [SerializeField] private bool matchPlayerPitch = false; // If true, camera tilts with player (usually false for level horizon)

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: No target assigned!");
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go == null)
                return;
            else
                target = go.transform;
        }

        // Desired position: always behind player relative to their forward
        Vector3 desiredPosition = target.position
            + target.right * offset.x
            + target.up * offset.y
            + target.forward * offset.z;

        // Desired rotation: face same direction as player.forward (level horizon by default)
        Vector3 upDirection = matchPlayerPitch ? target.up : Vector3.up;
        Quaternion desiredRotation = Quaternion.LookRotation(target.forward, upDirection);

        // Smoothly interpolate position & rotation
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed);

        transform.position = smoothedPosition;
        transform.rotation = smoothedRotation;
    }
}