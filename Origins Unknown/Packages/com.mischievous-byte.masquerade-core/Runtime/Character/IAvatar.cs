using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Avatar = MischievousByte.Masquerade.Character.Avatar;

namespace MischievousByte.Masquerade.Character
{
    public interface IAvatarHandler
    {
        protected static void EmitChange(Component component, Avatar avatar, bool calledFromEditor = false)
        {
            void Emit()
            {
                if (component == null)
                    return;


                if (!component.gameObject.scene.IsValid())
                    return;

                AvatarChangeContext context = new AvatarChangeContext(avatar);


                if (Application.isPlaying)
                    foreach (Transform child in component.transform)
                        child.SendMessage("OnAvatarChanged", context, SendMessageOptions.DontRequireReceiver);
                else
                {

                    IEnumerable<(MonoBehaviour behaviour, MethodInfo methodInfo)> targets =
                        component.transform.GetComponentsInChildren<MonoBehaviour>()
                        .Where(x => x != null)
                        .Select(
                            behaviour => (behaviour, behaviour.GetType().GetMethod(
                                "OnAvatarChanged",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                null, CallingConventions.Any, new Type[] { typeof(AvatarChangeContext) }, null)))
                        .Where(t => t.Item2 != null);

                    foreach (var target in targets)
                    {
                        try
                        {
                            target.methodInfo.Invoke(target.behaviour, new object[] { context });
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }

    #if UNITY_EDITOR
                if (calledFromEditor)
                    UnityEditor.EditorApplication.delayCall += () => { Emit(); };
                else
                    Emit();
    #else
                Emit();
    #endif
            }
        }

    public interface IAvatarContainer : IAvatarHandler
    {
        public Avatar Avatar { get; set; }
    }

    public struct AvatarChangeContext
    {
        public readonly Avatar avatar;
        public AvatarChangeContext(Avatar avatar)
        {
            this.avatar = avatar;
        }
    }
}
