using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LukewarmLobsters.OriginsUnknown.Entity.Living
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyController : EntityController
    {
        private new Rigidbody rigidbody;

        
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        private void Update()
        {
            
        }
    }
}
