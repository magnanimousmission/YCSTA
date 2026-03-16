using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

interface IInteractable
{
    public bool IsLookingAt { get; set; }
    public void RotateUI(Transform playerTransform);
    public void Interact(GameObject interactorSource);
}

public class Interactor : MonoBehaviour
{
    [SerializeField] private GameObject playerRoot;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private int horizontalRayCount = 6;
    [SerializeField] private int verticalRayCount = 3;
    [SerializeField] private float verticalSpreadAngle = 20f;
    [SerializeField] [Range(0f, 1f)] private float verticalBias = 0.25f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] InputActionReference interact;

    public Transform interactorSource;
    public float interactorRange;

    private IInteractable _lastInteractable;

    private void Update()
    {
        if (TryInteract())
            return;

        if (_lastInteractable != null)
        {
            _lastInteractable.IsLookingAt = false;
            _lastInteractable = null;
        }
    }

    private bool TryInteract()
    {
        foreach (var direction in GetRayDirections())
        {
            Ray r = new Ray(interactorSource.position, direction);
            if (!Physics.Raycast(r, out RaycastHit hitInfo, interactorRange, interactableMask))
                continue;
            if (!hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                continue;

            if (_lastInteractable != null && _lastInteractable != interactObj)
                _lastInteractable.IsLookingAt = false;

            interactObj.IsLookingAt = true;
            interactObj.RotateUI(interactorSource);
            _lastInteractable = interactObj;

            bool interactPressed = interact.action.WasPressedThisFrame();
            if (interactPressed)
            {
                interactObj.Interact(playerRoot);
            }


            return true;
        }
        return false;
    }

    private IEnumerable<Vector3> GetRayDirections()
    {
        yield return interactorSource.forward;

        int hCount = Mathf.Max(horizontalRayCount, 1);
        int vCount = Mathf.Max(verticalRayCount, 1);

        for (int v = 0; v < vCount; v++)
        {
            float vAngle = vCount == 1
                ? 0f
                : Mathf.Lerp(verticalSpreadAngle * verticalBias, -verticalSpreadAngle, v / (float)(vCount - 1));

            Quaternion pitchRot = Quaternion.AngleAxis(vAngle, interactorSource.right);

            for (int h = 0; h < hCount; h++)
            {
                float hAngle = hCount == 1
                    ? 0f
                    : Mathf.Lerp(-spreadAngle, spreadAngle, h / (float)(hCount - 1));

                Quaternion yawRot = Quaternion.AngleAxis(hAngle, interactorSource.up);
                yield return pitchRot * yawRot * interactorSource.forward;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (interactorSource == null) return;

        foreach (var direction in GetRayDirections())
        {
            Ray r = new Ray(interactorSource.position, direction);

            // Mirror TryInteract exactly — same mask, same range
            bool hit = Physics.Raycast(r, out RaycastHit hitInfo, interactorRange, interactableMask);

            if (hit)
            {
                // Yellow if hit but not an IInteractable (blocked by something else on the mask)
                bool isInteractable = hitInfo.collider.gameObject.TryGetComponent(out IInteractable _);

                Gizmos.color = isInteractable ? Color.green : Color.yellow;
                Gizmos.DrawLine(interactorSource.position, hitInfo.point);

                Gizmos.color = isInteractable
                    ? new Color(0f, 1f, 0f, 0.4f)
                    : new Color(1f, 1f, 0f, 0.4f);
                Gizmos.DrawSphere(hitInfo.point, 0.05f);
            }
            else
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
                Gizmos.DrawLine(
                    interactorSource.position,
                    interactorSource.position + direction * interactorRange
                );
            }
        }

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(interactorSource.position, 0.08f);
    }
#endif
}