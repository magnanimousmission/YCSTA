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
    [SerializeField] Collider myCollider;
    [SerializeField]
    private PlayerCore player;
    
    private void Update()
    {


    }


    private void TryInteract(GameObject other)
    {

        other = other.GetComponentInParent<Animator>().gameObject;

        IInteractable interactObj;
        interactObj = other.GetComponentInChildren<IInteractable>();

        if (interactObj != null)
        {
            //Debug.Log("IInt found");
            interactObj.IsLookingAt = true;
            interactObj.RotateUI(player.GetComponentInParent<Animator>().gameObject.transform);

        }
        else
        {
            //Debug.Log("IInt not found");

        }

        if (interactObj != null)
            player.InteractWithObject(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!myCollider.bounds.Contains(other.transform.position)) return;

        //Debug.Log(other.gameObject.transform.tag);
        if(!other.gameObject.CompareTag("Terrain"))
            TryInteract(other.gameObject);
    }
}

