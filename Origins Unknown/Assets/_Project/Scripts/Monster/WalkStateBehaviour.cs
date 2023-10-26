using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace LukewarmLobsters.OriginsUnknown
{
    public class WalkStateBehaviour : StateMachineBehaviour
    {
        float timer;
        List<Transform> waypoints = new List<Transform>();
        NavMeshAgent navMeshAgent;

        Transform playerTransform;

        [SerializeField] private float minWalkTime = 6;
        [SerializeField] private float maxWalkTime = 12;

        [SerializeField] private float chaseDistance = 5;
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navMeshAgent = animator.GetComponent<NavMeshAgent>();
            timer = 0;

            GameObject waypointsObj = GameObject.FindGameObjectWithTag("Waypoints");
            foreach (Transform transform in waypointsObj.transform)
            {
                waypoints.Add(transform);
            }

            navMeshAgent.SetDestination(waypoints[Random.Range(0, waypoints.Count)].position);

            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            timer += Time.deltaTime;
            if (timer > Random.Range(minWalkTime, maxWalkTime))
            {
                animator.SetBool("isPatrolling", false);
            }

            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                navMeshAgent.SetDestination(waypoints[Random.Range(0, waypoints.Count)].position);
            }

            float distanceFromPlayer = Vector3.Distance(playerTransform.position, animator.transform.position);
            if (distanceFromPlayer < chaseDistance)
            {
                animator.SetBool("isChasing", true);
            }

        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navMeshAgent.SetDestination(navMeshAgent.transform.position);
        }


        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
    }
