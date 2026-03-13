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
    
    public Transform interactorSource;
    public float interactorRange;

    private IInteractable _lastInteractable;

    private void Update()
    {
        Ray r = new Ray(interactorSource.position, interactorSource.forward);
        if (Physics.Raycast(r, out RaycastHit hitInfo, interactorRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
            {
                if (_lastInteractable != null && _lastInteractable != interactObj)
                    _lastInteractable.IsLookingAt = false;

                interactObj.IsLookingAt = true;
                interactObj.RotateUI(interactorSource);
                _lastInteractable = interactObj;

                if (Input.GetKeyDown(KeyCode.E))
                    interactObj.Interact(playerRoot);

                return;
            }
        }
        
        if (_lastInteractable != null)
        {
            _lastInteractable.IsLookingAt = false;
            _lastInteractable = null;
        }
    }
}