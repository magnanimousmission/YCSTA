using System;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    private Outline[] _outlines;

    private void Awake()
    {
        var found = new List<Outline>();
        foreach (var t in transform.root.GetComponentsInChildren<Transform>(includeInactive: true))
        {
            if (!t.gameObject.activeSelf) continue;
            if (t.TryGetComponent<Outline>(out var o))
                found.Add(o);
        }
        _outlines = found.ToArray();

        foreach (var outline in _outlines)
            outline.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("AuraObject")) return;
        SetOutlines(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("AuraObject")) return;
        SetOutlines(false);
    }

    private void SetOutlines(bool state)
    {
        foreach (var outline in _outlines)
            outline.enabled = state;
    }
}
