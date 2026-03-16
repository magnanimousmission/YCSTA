using UnityEngine;
using System.Collections.Generic;

public class IdlePoints : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> points = new List<GameObject>();

    void Awake()
    {
        foreach (Transform child in transform)
        {
            points.Add(child.gameObject);
        }
    }
}