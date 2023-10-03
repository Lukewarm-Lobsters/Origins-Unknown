using MischievousByte.Silhouette.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace MischievousByte.Silhouette.Builtin
{
    [CreateAssetMenu]
    public class ArmPoserModule : BodyPoserModule
    {
        private static readonly Quaternion leftUpperArmCorrection = Quaternion.Euler(180f, 90f, 0);
        private static readonly Quaternion leftForearmCorrection = Quaternion.Euler(180f, 90f, 0);
        private static readonly Quaternion leftWristCorrection = Quaternion.Euler(-90f, 90f, 0);

        public Vector3 offset;
        private static readonly Quaternion rightUpperArmCorrection = Quaternion.Euler(0, -90f, 0);
        private static readonly Quaternion rightForearmCorrection = Quaternion.Euler(0, -90f, 0);
        private static readonly Quaternion rightWristCorrection = Quaternion.Euler(-90f, -90f, 0);

        public override void Pose(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
            Solve(LeftRight.Left, input, measurements, constraints, ref pose);
            Solve(LeftRight.Right, input, measurements, constraints, ref pose);
        }



        private void Solve(LeftRight side, in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
            SolveShoulder(side, input, constraints, ref pose);
            SolveArm(side, input, ref pose);
        }

        private void SolveShoulder(LeftRight side, in BodyPoser.Input input, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
            Pose target = side == LeftRight.Left ? input.leftHand : input.rightHand;
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;

            BodyTree<Matrix4x4> worldTree = pose.ToWorld();

            Matrix4x4 matrix = Matrix4x4.TRS(
                worldTree[BodyNode.T7].GetPosition(),
                Quaternion.LookRotation(pose.GetForward()),
                Vector3.one
                );

            Bounds bounds = pose.GetReachBounds(side, matrix, constraints.MaxShoulderYRotation, constraints.MaxShoulderZRotation);

            Vector3 localHand = matrix.inverse.MultiplyPoint(target.position);

            Vector3 normalizedLocalHand = new Vector3(
                Mathf.InverseLerp(bounds.min.x, bounds.max.x, localHand.x),
                Mathf.InverseLerp(bounds.min.y, bounds.max.y, localHand.y),
                Mathf.InverseLerp(bounds.min.z, bounds.max.z, localHand.z));

            Vector2 projectedY = new Vector2(normalizedLocalHand.x, normalizedLocalHand.z) * 2 - Vector2.one;

            float yFactor = Mathf.Clamp01(projectedY.magnitude);
            float zFactor = Mathf.Clamp01(normalizedLocalHand.y * 2 - 1);
            (float y, float z) angles = pose.CalculateOptimalShoulderAngles(
                side, 
                target.position - worldTree[clavicle].GetPosition(), 
                constraints.MaxShoulderYRotation, 
                constraints.MaxShoulderZRotation);

            Quaternion localRotation = Quaternion.Euler(0, angles.y * yFactor, angles.z * zFactor);

            Quaternion finalRotation = matrix.rotation* localRotation;

            pose[clavicle] = Matrix4x4.TRS(
                pose[clavicle].GetPosition(),
                worldTree[clavicle.GetParent()].inverse.rotation * finalRotation,
                Vector3.one);
        }

        private void SolveArm(LeftRight side, in BodyPoser.Input input, ref BodyTree<Matrix4x4> pose)
        {
            Matrix4x4 GetWorldMatrix(in BodyTree<Matrix4x4> tree, BodyNode node)
            {
                Matrix4x4 m = tree[BodyNode.Pelvis];
                BodyNode[] chain = node.GetParentChain().Reverse().ToArray();

                for (int i = 1; i < chain.Length; i++)
                {
                    m *= tree[chain[i]];
                }

                m *= tree[node];

                return m;
            }

            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArm = side == LeftRight.Left ? BodyNode.LeftUpperArm : BodyNode.RightUpperArm;
            BodyNode forearm = side == LeftRight.Left ? BodyNode.LeftForearm : BodyNode.RightForearm;
            BodyNode wrist = side == LeftRight.Left ? BodyNode.LeftWrist : BodyNode.RightWrist;
            BodyNode hand = side == LeftRight.Left ? BodyNode.LeftHand : BodyNode.RightHand;

            
            Pose handInput = side == LeftRight.Left ? input.leftHand : input.rightHand;

            Pose target;
            {
                Quaternion correction = Quaternion.Inverse(side == LeftRight.Left ? leftWristCorrection : rightWristCorrection);
                Pose desiredHand = new Pose(handInput.position, handInput.rotation * Quaternion.Inverse(correction));

                //Pose target;
                {
                    Matrix4x4 worldHand = GetWorldMatrix(pose, hand);
                    Matrix4x4 worldWrist = GetWorldMatrix(pose, wrist);

                    Matrix4x4 desiredHandMatrix = Matrix4x4.TRS(desiredHand.position, desiredHand.rotation, Vector3.one);

                    Matrix4x4 localWrist = worldHand.inverse * worldWrist;

                    Matrix4x4 f = desiredHandMatrix * localWrist;

                    target = new Pose(f.GetPosition(), f.rotation);
                }
            }

            Vector3 worldHint = CalculateHint(side, target, in pose);

            {
                float segmentALength = pose[forearm].GetPosition().magnitude;
                float segmentBLength = pose[wrist].GetPosition().magnitude;

                float totalLength = segmentALength + segmentBLength;

                Vector3 targetPosition;
                Vector3 normal;
                float distance;
                {
                    
                    Matrix4x4 worldUpperArm = GetWorldMatrix(in pose, upperArm);
                    //Debug.Log($"Target to upper: {Vector3.Distance(target.position, worldUpperArm.GetPosition())}");
                    //Makes sure that the target is within range;
                    targetPosition = worldUpperArm.GetPosition() + Vector3.ClampMagnitude(target.position - worldUpperArm.GetPosition(), totalLength * 0.999f);

                    distance = (targetPosition - worldUpperArm.GetPosition()).magnitude;

                    

                    normal = Vector3.Cross(worldUpperArm.GetPosition() - worldHint, targetPosition - worldHint);

                    normal.Normalize();
                }


                //root.LookAt(targetPosition, normal);
                {
                    Matrix4x4 worldUpperArm = GetWorldMatrix(in pose, upperArm);
                    Matrix4x4 worldClavicle = GetWorldMatrix(in pose, clavicle);

                    Quaternion q = Quaternion.LookRotation(targetPosition - worldUpperArm.GetPosition(), normal);

                    pose[upperArm] = Matrix4x4.TRS(pose[upperArm].GetPosition(), worldClavicle.inverse.rotation * q, Vector3.one);
                }
                
                //Use cosine rule to determine the angle of the triangle's corner (located at the root)
                float cos = (segmentBLength * segmentBLength - segmentALength * segmentALength - distance * distance) /
                    (-2 * distance * segmentALength);
                cos = Mathf.Clamp(cos, -1, 1); //Clamp because of float precision errors


                float angle = -Mathf.Acos(cos) * Mathf.Rad2Deg;


                //root.RotateAround(root.position, normal, -angle);
                {
                    Matrix4x4 worldShoulder = GetWorldMatrix(in pose, clavicle);
                    Matrix4x4 worldUpperArm = GetWorldMatrix(in pose, upperArm);

                    Quaternion q = Quaternion.AngleAxis(-angle, normal) * worldUpperArm.rotation;
                    Quaternion lq = worldShoulder.inverse.rotation * q;

                    pose[upperArm] = Matrix4x4.TRS(pose[upperArm].GetPosition(), lq, Vector3.one);
                }

                
                //root.localRotation *= Quaternion.Euler(rootCorrection);
                {
                    Quaternion correction = side == LeftRight.Left ? leftUpperArmCorrection : rightUpperArmCorrection;
                    pose[upperArm] = Matrix4x4.TRS(pose[upperArm].GetPosition(), pose[upperArm].rotation * correction, Vector3.one);
                }
                
                
                //middle.LookAt(targetPosition, normal);
                {
                    Matrix4x4 worldForearm = GetWorldMatrix(in pose, forearm);
                    Matrix4x4 worldUpperArm = GetWorldMatrix(in pose, upperArm);
                    
                    Quaternion q = Quaternion.LookRotation(targetPosition - worldForearm.GetPosition(), normal);

                    pose[forearm] = Matrix4x4.TRS(pose[forearm].GetPosition(),worldUpperArm.inverse.rotation * q, Vector3.one);
                }
                
                //middle.localRotation *= Quaternion.Euler(middleCorrection);
                {
                    Quaternion correction = side == LeftRight.Left ? leftForearmCorrection : rightForearmCorrection;
                    pose[forearm] = Matrix4x4.TRS(pose[forearm].GetPosition(), pose[forearm].rotation * correction, Vector3.one);
                }


                //end.rotation = target.rotation;
                {
                    Matrix4x4 worldForearm = GetWorldMatrix(in pose, forearm);
                    //Quaternion correction = side == LeftRight.Left ? leftWristCorrection : rightWristCorrection;
                    pose[wrist] = Matrix4x4.TRS(pose[wrist].GetPosition(), worldForearm.inverse.rotation * (target.rotation /* * correction */), Vector3.one);
                }
            }
        }

        private Vector3 CalculateHint(LeftRight side, Pose target, in BodyTree<Matrix4x4> pose)
        {
            float correction = side == LeftRight.Left ? 1 : -1;

            BodyNode upperArm = side == LeftRight.Left ? BodyNode.LeftUpperArm : BodyNode.RightUpperArm;
            BodyNode forearm = side == LeftRight.Left ? BodyNode.LeftForearm : BodyNode.RightForearm;
            BodyNode wrist = side == LeftRight.Left ? BodyNode.LeftWrist : BodyNode.RightWrist;

            float upperArmLength = pose[forearm].GetPosition().magnitude;
            float forearmLength = pose[wrist].GetPosition().magnitude;
            float totalLength = upperArmLength + forearmLength;

            Matrix4x4 worldAnchor;
            {
                worldAnchor = pose[BodyNode.Pelvis];
                BodyNode[] chain = upperArm.GetParentChain().Reverse().ToArray();

                for (int i = 1; i < chain.Length; i++)
                {
                    worldAnchor *= pose[chain[i]];
                }

                worldAnchor *= pose[upperArm];
            }

            Vector3 anchorToTarget = target.position - worldAnchor.GetPosition();
            Vector3 anchorToTargetProjected = Vector3.ProjectOnPlane(anchorToTarget, Vector3.up);

            Vector3 hint = Vector3.Lerp(worldAnchor.GetPosition(), target.position, 0.5f);

            Quaternion rot = Quaternion.Euler(0, 40f * correction, 0) * Quaternion.LookRotation(anchorToTargetProjected);

            Vector3 defaultRotOffset = rot * (Vector3.left * upperArmLength * correction);

            float closeFactor = Mathf.Clamp01(Mathf.InverseLerp(0, upperArmLength, anchorToTargetProjected.magnitude));


            hint += Vector3.Lerp(Vector3.left * correction * upperArmLength, defaultRotOffset, closeFactor);

            hint += Vector3.down * upperArmLength;

            Vector3 targetBack = target.rotation * Vector3.back;
            Vector3 targetSide = target.rotation * Vector3.left * correction * 0.3f;
            Vector3 targetOffset = targetBack + targetSide;// + rot * Vector3.forward * Mathf.Abs(targetBack.y) * outerOffset;

            targetOffset.Normalize();

            Vector3 lerpTarget = target.position + targetOffset * forearmLength;

            hint = Vector3.Lerp(hint, lerpTarget, 0.4f);

            Vector3 targetToHint = hint - target.position;

            hint = target.position + targetToHint.normalized * totalLength * 0.5f;

            Quaternion otherRot = Quaternion.Euler(0, -40f * correction, 0) * Quaternion.LookRotation(anchorToTargetProjected);

            hint += otherRot * Vector3.forward * totalLength * 0.1f;

            return hint;
        }
    }
}