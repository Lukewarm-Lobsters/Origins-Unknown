using LukewarmLobsters.OriginsUnknown.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LukewarmLobsters.OriginsUnknown
{
    public class EntityController : MonoBehaviour, IDamageable, IKilleable
    {
        [SerializeField] private float maxHealth;
        [SerializeField] private float health;

        public float MaxHealth => maxHealth;
        public float Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                AliveCheck();
            }
        }
        

        public bool IsAlive { get; private set; }

        public event Action onKill;
        public event Action<Damage> onDamage;
        
        public void Damage(Damage damage)
        {
            if (!IsAlive)
                return;

            health = Mathf.Clamp(health - damage.physical, 0, maxHealth);
            onDamage?.Invoke(damage);
            AliveCheck();
        }

        [ContextMenu("Kill")]
        public void Kill()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            onKill?.Invoke();
        }

        private void AliveCheck()
        {
            if (!IsAlive)
                return;

            if (health <= 0)
                Kill();
        }
    }
}
