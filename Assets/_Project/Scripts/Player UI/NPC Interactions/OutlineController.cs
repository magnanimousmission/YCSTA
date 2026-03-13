using System;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    [SerializeField]
    private Outline outline;

    private void Awake()
    {
        outline.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("AuraObject")) return;
        outline.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("AuraObject")) return;
        outline.enabled = false;
    }
}
