using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialState : MonoBehaviour
{
    [SerializeField] Animator animator;

    [Header("Scripted Play")]
    [SerializeField] bool triggerColliderActivatedAnime;
    [SerializeField] string triggerActivatedParameter;
    [SerializeField] string parameterToActivateFirst;
    [SerializeField] Collider myCollider;
    bool triggerActivated = false;



    void Start()
    {
        animator.SetBool("Idle", false);
        animator.SetBool(parameterToActivateFirst, true);

    }

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerActivated) return;
        if (!myCollider.bounds.Contains(other.transform.position)) return;
        if(other.gameObject.GetComponentInParent<Animator>() == null) return;

        if (triggerColliderActivatedAnime && other.gameObject.GetComponentInParent<Animator>().gameObject.CompareTag("Player"))
        {
            animator.SetBool(parameterToActivateFirst, false);
            animator.SetBool(triggerActivatedParameter, true);
            triggerActivated = true;
        }
    }

    internal void RemoveParameters()
    {
        animator.SetBool(parameterToActivateFirst, false);
        animator.SetBool(triggerActivatedParameter, false);
        this.enabled = false;
    }
}