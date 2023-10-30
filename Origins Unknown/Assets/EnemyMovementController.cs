using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukewarmLobsters.OriginsUnknown.Entity.Living
{
    public class EnemyMovementController : MonoBehaviour
    {
        [SerializeField] private float friction;
        [SerializeField] private float castLength;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float stabilizer;
        
        private new Rigidbody rigidbody;
        private new CapsuleCollider collider;

        public Vector3 targetVelocity;
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();
        }

        private void FixedUpdate()
        {
            Vector3 origin = transform.TransformPoint(collider.center + Vector3.down * (collider.height / 2f - collider.radius));

            if(Physics.SphereCast(origin, collider.radius * 0.95f, Vector3.down, out RaycastHit hit, castLength))
            {
                float t = hit.distance / castLength;
                var g = Physics.gravity * rigidbody.mass;

                var force = curve.Evaluate(t) * -g;
                rigidbody.AddForce(force);
            }


            rigidbody.AddForce(Vector3.down * rigidbody.velocity.y * stabilizer);
        }
    }
}
