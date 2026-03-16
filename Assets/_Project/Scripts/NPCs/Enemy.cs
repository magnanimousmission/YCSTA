using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    List<GameObject> targets = new();
    GameObject targetToAttack;
    NavMeshAgent agent;
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("idle", true);
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("NPC") && !targets.Contains(other.gameObject))
        {
            targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == targetToAttack)
        {
            targets.Remove(other.gameObject);
            targetToAttack = null;
        }

    }

    private void Update()
    {

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if(animator == null)
            animator = GetComponent<Animator>();

        if(targets.Count > 0)
        {
            float closestDistance = 99999f;
            foreach (GameObject enemy in targets)
            {
                float distance = Vector3.Distance(gameObject.transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetToAttack = enemy;
                }

            }

            if (targetToAttack != null)
                agent.SetDestination(targetToAttack.transform.position);
            else
            {
                ResetAnimations();
                animator.SetBool("idle", true);
                
            }


            if (Vector3.Distance(transform.position, targetToAttack.transform.position) < agent.stoppingDistance)
            {
                ResetAnimations();
                animator.SetBool("attack", true);
            }
            else
            {
                ResetAnimations();
                animator.SetBool("run", true);
            }

            if (agent != null && agent.isOnNavMesh)
                agent.SetDestination(targetToAttack.transform.position);

            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                
                // Calculate direction to the next corner (steeringTarget)
                Vector3 lookDirection = agent.steeringTarget - gameObject.transform.position;
                lookDirection.y = 0; // Keep the agent upright

                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    // Smoothly interpolate to the target rotation
                    gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }



    }

    private void ResetAnimations()
    {
        animator.SetBool("run", false);
        animator.SetBool("attack", false);
        animator.SetBool("idle", false);
    }
}
