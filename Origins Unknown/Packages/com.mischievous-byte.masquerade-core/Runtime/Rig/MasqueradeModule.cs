using MischievousByte.Masquerade.Character;
using MischievousByte.RigBuilder;
using MischievousByte.Silhouette;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Rig
{
    public abstract class MasqueradeModule : RigModule
    {
        private IAvatarContainer avatarContainer;
        public IAvatarContainer AvatarContainer => avatarContainer;

        [SerializeField, HideInInspector] protected BodyTree<Matrix4x4> tree;
        public BodyTree<Matrix4x4> Tree => tree;

        protected virtual void OnContextChanged(ModularRig.Context context)
        {
            avatarContainer = Rig.GetComponent<IAvatarContainer>();
        }
    }
}
