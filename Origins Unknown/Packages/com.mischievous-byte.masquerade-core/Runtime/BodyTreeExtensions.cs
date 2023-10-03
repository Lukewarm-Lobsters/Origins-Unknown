using MischievousByte.Silhouette;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

namespace MischievousByte.Masquerade
{
    public static class BodyTreeExtensions
    {
        public static BodyTree<Matrix4x4> ToMatrix(this in BodyTree<Transform> bodyTree)
        {
            BodyTree<Matrix4x4> r = new BodyTree<Matrix4x4>();
            foreach (BodyNode node in BodyNodeUtility.All)
                if (bodyTree[node] != null)
                    r[node] = Matrix4x4.TRS(bodyTree[node].localPosition, bodyTree[node].localRotation, Vector3.one);

            return r;
        }

        public static BodyTree<Matrix4x4> From(Animator animator, Transform eyeOverride = null)
        {
            BodyTree<Matrix4x4> output = new BodyTree<Matrix4x4>();

            Dictionary<HumanBodyBones, Matrix4x4> globalAnimatorMatrices = new();
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

                        while(current != animator.avatarRoot)

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

            BodyNode[] nope = new BodyNode[] { BodyNode.Pelvis, BodyNode.Eyes, BodyNode.LeftHand, BodyNode.RightHand };

            output[BodyNode.Pelvis] = Matrix4x4.TRS(globalAnimatorMatrices[HumanBodyBones.Hips].GetPosition(), Quaternion.identity, Vector3.one);

            foreach(BodyNode node in BodyNodeUtility.All.Where(x => !nope.Contains(x)))
            {
                output[node] = Matrix4x4.identity;
                if (!node.IsHumanoid())
                    continue;

                Matrix4x4 globalParent;
                {
                    globalParent = Matrix4x4.identity;

                    var chain = node.GetParentChain().Reverse();

                    foreach (var link in chain)
                        globalParent *= output[link];

                    globalParent *= output[node];
                }

                HumanBodyBones bone = node.ToHumanoid();

                Vector3 localPosition = globalParent.inverse.MultiplyPoint(globalAnimatorMatrices[bone].GetPosition());
                
                output[node] = Matrix4x4.TRS(localPosition, Quaternion.identity, Vector3.one);
            }

            {
                Vector3 a = Vector3.Lerp(
                    Vector3.Lerp(
                        globalAnimatorMatrices[HumanBodyBones.LeftHand].GetPosition(),
                        globalAnimatorMatrices[HumanBodyBones.LeftThumbProximal].GetPosition(),
                        0.45f),
                    globalAnimatorMatrices[HumanBodyBones.LeftRingProximal].GetPosition(),
                    0.45f);

                Vector3 planeNormal = (globalAnimatorMatrices[HumanBodyBones.LeftHand].GetPosition() - globalAnimatorMatrices[HumanBodyBones.LeftLowerArm].GetPosition()).normalized;

                Plane plane = new Plane(planeNormal, globalAnimatorMatrices[HumanBodyBones.LeftHand].GetPosition());

                float distance = Mathf.Lerp(plane.GetDistanceToPoint(globalAnimatorMatrices[HumanBodyBones.LeftRingProximal].GetPosition()),
                    plane.GetDistanceToPoint(globalAnimatorMatrices[HumanBodyBones.LeftIndexProximal].GetPosition()), 0.5f);

                Vector3 n = Vector3.Cross(
                    globalAnimatorMatrices[HumanBodyBones.LeftIndexProximal].GetPosition() - globalAnimatorMatrices[HumanBodyBones.LeftHand].GetPosition(),
                    globalAnimatorMatrices[HumanBodyBones.LeftRingProximal].GetPosition() - globalAnimatorMatrices[HumanBodyBones.LeftHand].GetPosition()).normalized;

                a += n * distance * 0.2f;

                Vector3 superLocalPosition = globalAnimatorMatrices[HumanBodyBones.LeftHand].inverse.MultiplyPoint(a);

                output[BodyNode.LeftHand] = Matrix4x4.TRS(globalAnimatorMatrices[HumanBodyBones.LeftHand].rotation * superLocalPosition, Quaternion.identity, Vector3.one);
            }

            {
                Vector3 a = Vector3.Lerp(
                    Vector3.Lerp(
                        globalAnimatorMatrices[HumanBodyBones.RightHand].GetPosition(),
                        globalAnimatorMatrices[HumanBodyBones.RightThumbProximal].GetPosition(),
                        0.45f),
                    globalAnimatorMatrices[HumanBodyBones.RightRingProximal].GetPosition(),
                    0.45f);

                Vector3 planeNormal = (globalAnimatorMatrices[HumanBodyBones.RightHand].GetPosition() - globalAnimatorMatrices[HumanBodyBones.RightLowerArm].GetPosition()).normalized;

                Plane plane = new Plane(planeNormal, globalAnimatorMatrices[HumanBodyBones.RightHand].GetPosition());

                float distance = Mathf.Lerp(plane.GetDistanceToPoint(globalAnimatorMatrices[HumanBodyBones.RightRingProximal].GetPosition()),
                    plane.GetDistanceToPoint(globalAnimatorMatrices[HumanBodyBones.RightIndexProximal].GetPosition()), 0.5f);

                Vector3 n = Vector3.Cross(
                    globalAnimatorMatrices[HumanBodyBones.RightIndexProximal].GetPosition() - globalAnimatorMatrices[HumanBodyBones.RightHand].GetPosition(),
                    globalAnimatorMatrices[HumanBodyBones.RightRingProximal].GetPosition() - globalAnimatorMatrices[HumanBodyBones.RightHand].GetPosition()).normalized;

                a += n * -distance * 0.2f;

                Vector3 superLocalPosition = globalAnimatorMatrices[HumanBodyBones.RightHand].inverse.MultiplyPoint(a);

                output[BodyNode.RightHand] = Matrix4x4.TRS(globalAnimatorMatrices[HumanBodyBones.RightHand].rotation * superLocalPosition, Quaternion.identity, Vector3.one);
            }

            //TODO: Use eyes reference
            {
                //output[BodyNode.Eyes] = Matrix4x4.TRS(Vector3.forward * 0.12f + Vector3.up * 0.075f, Quaternion.identity, Vector3.one); //YBot
                //output[BodyNode.Eyes] = Matrix4x4.TRS(Vector3.forward * 0.14f + Vector3.up * 0.12f, Quaternion.identity, Vector3.one); //Scifi dude

                Vector3 animatorLocalEyes;
                if (eyeOverride)
                {
                    animatorLocalEyes = animator.GetBoneTransform(HumanBodyBones.Head).InverseTransformPoint(eyeOverride.transform.position);
                }
                else
                {
                    animatorLocalEyes = Vector3.Lerp(
                    (globalAnimatorMatrices[HumanBodyBones.Head].inverse * globalAnimatorMatrices[HumanBodyBones.LeftEye]).GetPosition(),
                    (globalAnimatorMatrices[HumanBodyBones.Head].inverse * globalAnimatorMatrices[HumanBodyBones.RightEye]).GetPosition(), 
                    0.5f);
                }


                output[BodyNode.Eyes] = Matrix4x4.TRS(animatorLocalEyes, Quaternion.identity, Vector3.one);
            }


            return output;
        }
    }
}