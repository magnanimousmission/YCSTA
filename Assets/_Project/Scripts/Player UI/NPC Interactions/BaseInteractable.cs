using System;
using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Interactable UI")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private GameObject interactUICanvas;

    public bool IsLookingAt { get; set; }
    public GameObject InteractUI => interactUI;
    
    [HideInInspector]
    public bool _hasInteracted = false;

    private void Awake()
    {
        interactUI.SetActive(true);
    }

    protected virtual void Update()
    {
        if (_hasInteracted)
        {
            interactUI.SetActive(false);
        }
        else
        {
            interactUI.SetActive(true);
        }
    }

    public virtual void RotateUI(Transform playerTransform)
    {
        if (!IsLookingAt || interactUICanvas == null) return;

        var direction = interactUICanvas.transform.position - playerTransform.position;
        direction.y = 0f;
        interactUICanvas.transform.rotation = 
            Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);
    }

    public abstract void Interact(GameObject interactorSource);
}