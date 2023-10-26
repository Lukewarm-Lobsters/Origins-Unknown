using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukewarmLobsters.OriginsUnknown
{
    public class ChaseStateBehavour : StateMachineBehaviour
    {
        float timer;
        UnityEngine.AI.NavMeshAgent navMeshAgent;

        [SerializeField] private float minChaseTime = 15;
        [SerializeField] private float maxChaseTime = 15;

        Transform playerTransform;

        [SerializeField] private float ChaseDistance = 10;
        [SerializeField] private float attackDistance = 3;
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            timer = 0;

            navMeshAgent = animator.GetComponent<UnityEngine.AI.NavMeshAgent>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            timer += Time.deltaTime;
            if (timer > Random.Range(minChaseTime, maxChaseTime))
            {
                animator.SetBool("isChasing", false);
            }

            navMeshAgent.SetDestination(playerTransform.position);

            float distanceFromPlayer = Vector3.Distance(playerTransform.position, animator.transform.position);
            if (distanceFromPlayer > ChaseDistance)
            {
                animator.SetBool("isChasing", false);
            }

            Debug.Log("Chasing distance from player: " + distanceFromPlayer);
            if (distanceFromPlayer < attackDistance)
            {
                animator.SetBool("isAttacking", true);
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
