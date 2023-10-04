using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukewarmLobsters.OriginsUnknown.Entity
{
    public interface IKilleable
    {
        public bool IsAlive { get; }
        public event Action onKill;
        public void Kill();
    }
}
