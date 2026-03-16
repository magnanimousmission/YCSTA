using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ExtractingNPCController : MonoBehaviour
{
    [SerializeField] private NPCInputHandler npcInputHandler;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Extraction"))
        {
            var x = other.GetComponent<IdlePoints>();
            
            int randomIndex = Random.Range(0, x.points.Count);
            GameObject randomLocation = x.points[randomIndex];

            //on hitting extraction collider -> set target to point given -> go toward 
            
            npcInputHandler.SetTarget(randomLocation);
            
        }
    }
}
