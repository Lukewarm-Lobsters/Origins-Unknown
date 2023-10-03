using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Character
{
    public class AvatarContainer : MonoBehaviour, IAvatarContainer
    {
        [SerializeField] private Avatar avatar;

        public Avatar Avatar
        {
            get => avatar;
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                avatar = value;

                IAvatarHandler.EmitChange(this, avatar);
            }
        }

        private void Start()
        {
            if (avatar == null)
                return;

            IAvatarHandler.EmitChange(this, avatar, false);
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            if (avatar == null)
                throw new ArgumentNullException();

            IAvatarHandler.EmitChange(this, avatar, true);
        }

        [ContextMenu("Emit Change")]
        private void EmitChange() => IAvatarHandler.EmitChange(this, avatar, true);
    }
}
