using LukewarmLobsters.OriginsUnknown.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukewarmLobsters.OriginsUnknown.Entity
{
    public interface IDamageable
    {
        public event Action<Damage> onDamage;
        public void Damage(Damage damage);
    }
}
