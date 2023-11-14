using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukewarmLobsters.OriginsUnknown
{
    public class ChaseStateBehavour : StateMachineBehaviour
    {
        UnityEngine.AI.NavMeshAgent navMeshAgent;

        float PlayerInSightTimer;

        Transform playerTransform;

        [SerializeField] private float attackDistance;

        [SerializeField] private float MaxOutOfSightTime;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            navMeshAgent = animator.GetComponent<UnityEngine.AI.NavMeshAgent>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            PlayerInSightTimer = 0;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            navMeshAgent.SetDestination(playerTransform.position);

            float distanceFromPlayer = Vector3.Distance(playerTransform.position, animator.transform.position);
            Debug.Log("Chasing distance from player: " + distanceFromPlayer);
            if (distanceFromPlayer < attackDistance)
            {
                animator.SetBool("isAttacking", true);
            }

            if (!animator.GetBool("canSeePlayer"))
            {
                PlayerInSightTimer += Time.deltaTime;

                if (PlayerInSightTimer > MaxOutOfSightTime)
                {
                    animator.SetBool("isChasing", false);
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
    }
}
