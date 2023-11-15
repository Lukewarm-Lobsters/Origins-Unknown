using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace LukewarmLobsters.OriginsUnknown
{
    public class PatrolStateBehaviour : StateMachineBehaviour
    {
        float timer;
        float PlayerInSightTimer;

        List<Transform> waypoints = new List<Transform>();
        NavMeshAgent navMeshAgent;

        Transform playerTransform;

        [SerializeField] private float minPatrolTime;
        [SerializeField] private float maxPatrolTime;

        [SerializeField] private float minInSightTime;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navMeshAgent = animator.GetComponent<NavMeshAgent>();
            timer = 0;
            PlayerInSightTimer = 0;

            GameObject waypointsObj = GameObject.FindGameObjectWithTag("Waypoints");
            foreach (Transform transform in waypointsObj.transform)
            {
                waypoints.Add(transform);
            }

            navMeshAgent.SetDestination(waypoints[Random.Range(0, waypoints.Count)].position);

            playerTransform = GameObject.FindGameObjectWithTag("GameController").transform;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            timer += Time.deltaTime;
            if (timer > Random.Range(minPatrolTime, maxPatrolTime))
            {
                animator.SetBool("isPatrolling", false);
            }

            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                navMeshAgent.SetDestination(waypoints[Random.Range(0, waypoints.Count)].position);
            }

            if (animator.GetBool("canSeePlayer"))
            {
                PlayerInSightTimer += Time.deltaTime;

                if (PlayerInSightTimer > minInSightTime)
                {
                    animator.SetBool("isChasing", true);
                }
            }
            else
            {
                PlayerInSightTimer = 0;
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
