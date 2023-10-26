using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukewarmLobsters.OriginsUnknown
{
    public class AttackStateBehavour : StateMachineBehaviour
    {

        UnityEngine.AI.NavMeshAgent navMeshAgent;


        Transform playerTransform;

        [SerializeField] private float attackDistance = 2;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navMeshAgent = animator.GetComponent<UnityEngine.AI.NavMeshAgent>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;


        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.Log("Attacking");
            animator.transform.LookAt(playerTransform);

            float distanceFromPlayer = Vector3.Distance(playerTransform.position, animator.transform.position);

            if (distanceFromPlayer > attackDistance)
            {
                animator.SetBool("isAttacking", false);
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
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
