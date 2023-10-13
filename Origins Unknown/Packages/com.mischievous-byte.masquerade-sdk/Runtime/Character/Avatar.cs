using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Character
{
    [AddComponentMenu(SDK_PackageInfo.ComponentMenuPrefix + "Avatar")]
    public sealed partial class Avatar : MonoBehaviour
    {
        public enum Error
        {
            MissingAnimator,
            NotHumanoid,
            MissingEyes
        }

        [SerializeField] private Animator animator;
        public Animator Animator => animator;

        [SerializeField] private Transform eyeOverride;
        [SerializeField] private float height;

        public float Height => height;

        public Transform EyeOverride => eyeOverride;

        public BodyShape shape;
        public float EyeHeight
        {
            get
            {
                if (eyeOverride != null)
                    return transform.InverseTransformPoint(eyeOverride.position).y;

                return Vector3.Lerp(
                    transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftEye).position),
                    transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightEye).position), 
                    0.5f).y;
            }
        }
        public bool IsValid
        {
            get
            {
                return Errors.Count == 0;
            }
        }

        public List<Error> Errors
        {
            get
            {
                var r = new List<Error>();

                if(animator == null)
                    r.Add(Error.MissingAnimator);

                if(!r.Contains(Error.MissingAnimator) && !animator.isHuman)
                    r.Add(Error.NotHumanoid);

                return r;
            }
        }


        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
}
