using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukewarmLobsters.OriginsUnknown
{
    public class IdleStateBehaviour : StateMachineBehaviour
    {
        float timer;
        float PlayerInSightTimer;
        Transform playerTransform;

        [SerializeField] private float minIdleTime;
        [SerializeField] private float maxIdleTime;

        [SerializeField] private float minInSightTime;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            playerTransform = GameObject.FindGameObjectWithTag("GameController").transform;
            timer = 0;
            PlayerInSightTimer = 0;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            timer += Time.deltaTime;

            if (timer > Random.Range(minIdleTime, maxIdleTime))
            {
                animator.SetBool("isPatrolling", true);
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
