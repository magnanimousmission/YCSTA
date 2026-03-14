using System.Collections.Generic;
using UnityEngine;

interface IInteractable
{
    public bool IsLookingAt { get; set; }
    public void RotateUI(Transform playerTransform);
    public void Interact(GameObject interactorSource);
}

public class Interactor : MonoBehaviour
{
    [SerializeField]
    private GameObject playerRoot;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private int rayCount = 10;

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
            if (!Physics.Raycast(r, out RaycastHit hitInfo, interactorRange))
                continue;
            if (!hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                continue;

            if (_lastInteractable != null && _lastInteractable != interactObj)
                _lastInteractable.IsLookingAt = false;

            interactObj.IsLookingAt = true;
            interactObj.RotateUI(interactorSource);
            _lastInteractable = interactObj;

            if (Input.GetKeyDown(KeyCode.E))
                interactObj.Interact(playerRoot);

            return true;
        }
        return false;
    }
    
    private IEnumerable<Vector3> GetRayDirections()
    {
        yield return interactorSource.forward;

        for (var i = 0; i < rayCount; i++)
        {
            var angle = Mathf.Lerp(-spreadAngle, spreadAngle, i / (float)(rayCount - 1));
            Quaternion rotation = Quaternion.AngleAxis(angle, interactorSource.up);
            yield return rotation * interactorSource.forward;
        }
    }
}

