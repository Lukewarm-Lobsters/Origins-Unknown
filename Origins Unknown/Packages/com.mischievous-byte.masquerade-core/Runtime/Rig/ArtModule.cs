using CodiceApp.Gravatar;
using JetBrains.Annotations;
using MischievousByte.Masquerade.Character;
using MischievousByte.RigBuilder;
using MischievousByte.Silhouette;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace MischievousByte.Masquerade.Rig
{
    public class ArtModule : MasqueradeModule
    {
        [SerializeField] private MasqueradeModule referenceModule;

        Dictionary<HumanBodyBones, Matrix4x4> globalAnimatorMatrices = new();

        public bool hasLegs;

        private Animator animator;

        protected override void OnContextChanged(ModularRig.Context context)
        {
            base.OnContextChanged(context);

            context.onUpdate.Bind(Execute);

            //UpdateTPose();
        }

        private void Execute()
        {
            HandleLegs();
            Pose();
        }


        private void OnAvatarChanged(AvatarChangeContext context)
        {
            Action<UnityEngine.Object> destroy = Application.isPlaying ? Destroy : DestroyImmediate;


            if (IsPrefabOnDisk())
                return;

            foreach (var a in GetComponentsInChildren<Animator>())
                destroy(a.gameObject);

            animator = Instantiate(context.avatar.Animator, transform);
            animator.gameObject.SetActive(true);

            animator.transform.localPosition = Vector3.zero;
            animator.transform.localRotation = Quaternion.identity;

            destroy(animator.GetComponent<Avatar>());

            UpdateTPose();

            Pose();
            HandleLegs();
        }

        private void HandleLegs()
        {
            if (!isActiveAndEnabled)
                return;

            if (!animator)
                return;

            animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localScale = hasLegs ? Vector3.one : Vector3.zero;
            animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).localScale = hasLegs ? Vector3.one : Vector3.zero;
        }

        private void UpdateTPose()
        {
            {
                IEnumerable<HumanBodyBones> bones =
                System.Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>().Where(b => b != HumanBodyBones.LastBone);

                Dictionary<HumanBodyBones, Matrix4x4> tposeMatrices = new Dictionary<HumanBodyBones, Matrix4x4>();

                foreach (var bone in bones)
                {
                    if (animator.GetBoneTransform(bone) == null)
                        continue;

                    IEnumerable<SkeletonBone> candidates = animator.avatar.humanDescription.skeleton.Where(s => s.name == animator.GetBoneTransform(bone).name);

                    if (candidates.Count() == 0)
                        continue;

                    SkeletonBone b = candidates.First();

                    if (bone == HumanBodyBones.Hips)
                    {
                        List<Transform> chain = new();


                        Transform current = animator.GetBoneTransform(HumanBodyBones.Hips);

                        while (current != animator.avatarRoot)

                        {
                            current = current.parent;

                            if (current == null)
                                break;

                            chain.Add(current);
                        }

                        Matrix4x4 m = Matrix4x4.identity;

                        foreach (var t in ((IEnumerable<Transform>)chain).Reverse())
                            m *= Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);

                        tposeMatrices[bone] = m * Matrix4x4.TRS(b.position, b.rotation, b.scale);
                    }
                    else
                    {
                        tposeMatrices[bone] = Matrix4x4.TRS(b.position, b.rotation, b.scale);
                    }
                }


                foreach (var bone in bones)
                {
                    List<HumanBodyBones> chain = new List<HumanBodyBones>();

                    HumanBodyBones currentBone = bone;
                    while (currentBone != HumanBodyBones.LastBone)
                    {
                        chain.Add(currentBone);
                        currentBone = currentBone.GetParent();
                    }
                    chain.Reverse();


                    Matrix4x4 matrix = Matrix4x4.identity;

                    foreach (var link in chain)
                        if (tposeMatrices.ContainsKey(link))
                            matrix *= tposeMatrices[link];

                    globalAnimatorMatrices[bone] = matrix;
                }
            }

        }
        private void Pose()
        {
            BodyTree<Matrix4x4> globalInput = referenceModule.Tree.ToWorld();

            foreach (BodyNode node in BodyNodeUtility.All)
            {
                if (!node.IsHumanoid())
                    continue;

                HumanBodyBones bone = node.ToHumanoid();

                Transform boneTransform = animator.GetBoneTransform(bone);

                if (boneTransform == null)
                    continue;

                boneTransform.position = globalInput[node].GetPosition();
                Quaternion fixedRotation = globalInput[node].rotation * globalAnimatorMatrices[bone].rotation;

                boneTransform.rotation = transform.rotation * fixedRotation;
            }
        }

        private bool IsPrefabOnDisk()
        {
#if UNITY_EDITOR
            return UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null || UnityEditor.EditorUtility.IsPersistent(this);
#else
            return false;
#endif
        }
    }
}
