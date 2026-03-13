using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    private Outline _outline;

    private void Start()
    {
        _outline = GetComponent<Outline>();
        DisableOutline();
    }

    public void Interact()
    {
        Debug.Log("Interact");
    }

    public void DisableOutline()
    {
        _outline.enabled = false;
    }

    public void EnableOutline()
    {
        _outline.enabled = true;
    }
    
    
}
