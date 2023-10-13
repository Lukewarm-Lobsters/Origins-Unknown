using Codice.CM.Client.Differences.Merge;
using MischievousByte.Silhouette.Utility;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Silhouette.Implementation
{
    [CreateAssetMenu(menuName = PackageInfo.AssetMenuPrefix + "Implementation/Arm Poser Module")]
    public class ArmPoserModule : BodyPoserModule
    {
        private static readonly Quaternion leftUpperArmCorrection = Quaternion.Euler(180f, 90f, 0);
        private static readonly Quaternion leftForearmCorrection = Quaternion.Euler(180f, 90f, 0);
        private static readonly Quaternion leftWristCorrection = Quaternion.Euler(-90f, 90f, 0);

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
            Pose p = side == LeftRight.Left ? input.leftHand : input.rightHand;

            SolveShoulder(side, input, constraints, ref pose);
            SolveArm(side, input, ref pose);

            //Debug.DrawLine(p.position, p.position + Vector3.down * 0.4f, Color.cyan);
        }

        private Matrix4x4 GetWorldMatrix(in BodyTree<Matrix4x4> tree, BodyNode node)
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
        private void SolveShoulder(LeftRight side, in BodyPoser.Input input, in BodyConstraints constraints, ref BodyTree<Matrix4x4> tree)
        {
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode wrist = side == LeftRight.Left ? BodyNode.LeftWrist : BodyNode.RightWrist;
            BodyNode hand = side == LeftRight.Left ? BodyNode.LeftHand : BodyNode.RightHand;

            Pose target = side == LeftRight.Left ? input.leftHand : input.rightHand;
            {
                Quaternion correction = Quaternion.Inverse(side == LeftRight.Left ? leftWristCorrection : rightWristCorrection);
                Pose desiredHand = new Pose(target.position, target.rotation * Quaternion.Inverse(correction));

                //Pose target;
                {
                    Matrix4x4 worldHand = GetWorldMatrix(tree, hand);
                    Matrix4x4 worldWrist = GetWorldMatrix(tree, wrist);

                    Matrix4x4 desiredHandMatrix = Matrix4x4.TRS(desiredHand.position, desiredHand.rotation, Vector3.one);

                    Matrix4x4 _localWrist = worldHand.inverse * worldWrist;

                    Matrix4x4 f = desiredHandMatrix * _localWrist;

                    target = new Pose(f.GetPosition(), f.rotation);
                }
            }


            BodyTree<Matrix4x4> worldTree = tree.ToWorld();

            Matrix4x4 matrix = Matrix4x4.TRS(
                worldTree[BodyNode.T7].GetPosition(),
                Quaternion.LookRotation(tree.GetForward()),
                Vector3.one
                );

            Bounds bounds = tree.GetReachBounds(side, matrix, constraints.MaxShoulderYRotation, constraints.MaxShoulderZRotation);

            Vector3 localWrist = matrix.inverse.MultiplyPoint(target.position);

            Vector3 clampedLocalWrist = new Vector3(
                Mathf.Clamp(localWrist.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(localWrist.y, bounds.min.y, bounds.max.y),
                Mathf.Clamp(localWrist.z, bounds.min.z, bounds.max.z));

            Vector3 clampedTargetPosition = matrix.MultiplyPoint(clampedLocalWrist);
            {
                Vector3 w = matrix.MultiplyPoint(clampedLocalWrist);
                Debug.DrawLine(w, w + Vector3.down * 0.4f, Color.yellow);
            }
            {
                Debug.DrawLine(clampedTargetPosition, clampedTargetPosition + Vector3.down * 0.6f, Color.black);
            }


            Vector3 normalizedLocalWrist = new Vector3(
                Mathf.InverseLerp(bounds.min.x, bounds.max.x, clampedLocalWrist.x),
                Mathf.InverseLerp(bounds.min.y, bounds.max.y, clampedLocalWrist.y),
                Mathf.InverseLerp(bounds.min.z, bounds.max.z, clampedLocalWrist.z));

            Vector2 projectedY = new Vector2(normalizedLocalWrist.x, normalizedLocalWrist.z) * 2 - Vector2.one;

            float yFactor = Mathf.Clamp01(projectedY.magnitude);
            float zFactor = Mathf.Clamp01(normalizedLocalWrist.y * 2 - 1);
            (float y, float z) angles = tree.CalculateOptimalShoulderAngles(
                side,
                clampedTargetPosition - worldTree[clavicle].GetPosition(), 
                constraints.MaxShoulderYRotation, 
                constraints.MaxShoulderZRotation);

            Quaternion localRotation = Quaternion.Euler(0, angles.y * yFactor, angles.z * zFactor);

            Quaternion finalRotation = matrix.rotation* localRotation;

            tree[clavicle] = Matrix4x4.TRS(
                tree[clavicle].GetPosition(),
                worldTree[clavicle.GetParent()].inverse.rotation * finalRotation,
                Vector3.one);
        }

        private void SolveArm(LeftRight side, in BodyPoser.Input input, ref BodyTree<Matrix4x4> tree)
        {
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArm = side == LeftRight.Left ? BodyNode.LeftUpperArm : BodyNode.RightUpperArm;
            BodyNode forearm = side == LeftRight.Left ? BodyNode.LeftForearm : BodyNode.RightForearm;
            BodyNode wrist = side == LeftRight.Left ? BodyNode.LeftWrist : BodyNode.RightWrist;
            BodyNode hand = side == LeftRight.Left ? BodyNode.LeftHand : BodyNode.RightHand;

            Matrix4x4 worldClavicle = GetWorldMatrix(in tree, clavicle);

            //Calculate target
            Pose target;
            {
                Pose handInput = side == LeftRight.Left ? input.leftHand : input.rightHand;

                Quaternion correction = Quaternion.Inverse(side == LeftRight.Left ? leftWristCorrection : rightWristCorrection);
                Pose desiredHand = new Pose(handInput.position, handInput.rotation * Quaternion.Inverse(correction));

                //Pose target;
                {
                    Matrix4x4 worldHand = GetWorldMatrix(tree, hand);
                    Matrix4x4 worldWrist = GetWorldMatrix(tree, wrist);

                    Matrix4x4 desiredHandMatrix = Matrix4x4.TRS(desiredHand.position, desiredHand.rotation, Vector3.one);

                    Matrix4x4 localWrist = worldHand.inverse * worldWrist;

                    Matrix4x4 f = desiredHandMatrix * localWrist;

                    target = new Pose(f.GetPosition(), f.rotation);
                }
            }

            float segmentALength = tree[forearm].GetPosition().magnitude;
            float segmentBLength = tree[wrist].GetPosition().magnitude;

            float totalLength = segmentALength + segmentBLength;

            Matrix4x4 worldUpperArm = worldClavicle * tree[upperArm];


            target.position = worldUpperArm.GetPosition() + Vector3.ClampMagnitude(target.position - worldUpperArm.GetPosition(), totalLength * 0.999f);

            float distance = (target.position - worldUpperArm.GetPosition()).magnitude;

            Vector3 worldHint = CalculateSimpleHint(side, in tree);//CalculateHint(side, target, in tree);

            Vector3 normal = Vector3.Cross(worldUpperArm.GetPosition() - worldHint, target.position - worldHint).normalized;

            tree[upperArm] = Matrix4x4.TRS(
                tree[upperArm].GetPosition(), 
                worldClavicle.inverse.rotation 
                * Quaternion.LookRotation(target.position - worldUpperArm.GetPosition(), normal), 
                Vector3.one);

            worldUpperArm = worldClavicle * tree[upperArm];

            float cos = (segmentBLength * segmentBLength - segmentALength * segmentALength - distance * distance) /
                    (-2 * distance * segmentALength);
            cos = Mathf.Clamp(cos, -1, 1); //Clamp because of float precision errors


            float angle = -Mathf.Acos(cos) * Mathf.Rad2Deg;
            
            tree[upperArm] = Matrix4x4.TRS(tree[upperArm].GetPosition(), worldClavicle.inverse.rotation * (Quaternion.AngleAxis(-angle, normal) * worldUpperArm.rotation), Vector3.one);
            tree[upperArm] = Matrix4x4.TRS(tree[upperArm].GetPosition(), tree[upperArm].rotation * (side == LeftRight.Left ? leftUpperArmCorrection : rightUpperArmCorrection), Vector3.one);

            worldUpperArm = worldClavicle * tree[upperArm];


            Matrix4x4 worldForearm = worldUpperArm * tree[forearm];
            tree[forearm] = Matrix4x4.TRS(tree[forearm].GetPosition(), worldUpperArm.inverse.rotation * Quaternion.LookRotation(target.position - worldForearm.GetPosition(), normal), Vector3.one);

            tree[forearm] = Matrix4x4.TRS(tree[forearm].GetPosition(), tree[forearm].rotation * (side == LeftRight.Left ? leftForearmCorrection : rightForearmCorrection), Vector3.one);

            worldForearm = worldUpperArm * tree[forearm];
            tree[wrist] = Matrix4x4.TRS(tree[wrist].GetPosition(), worldForearm.inverse.rotation * (target.rotation), Vector3.one);
        }

        private Vector3 CalculateSimpleHint(LeftRight side, in BodyTree<Matrix4x4> pose)
        {
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArm = side == LeftRight.Left ? BodyNode.LeftUpperArm : BodyNode.RightUpperArm;
            BodyNode forearm = side == LeftRight.Left ? BodyNode.LeftForearm : BodyNode.RightForearm;
            BodyNode wrist = side == LeftRight.Left ? BodyNode.LeftWrist : BodyNode.RightWrist;
            BodyNode hand = side == LeftRight.Left ? BodyNode.LeftHand : BodyNode.RightHand;

            float l = pose[upperArm].GetPosition().magnitude + pose[forearm].GetPosition().magnitude + pose[wrist].GetPosition().magnitude;

            BodyTree<Matrix4x4> worldTree = pose.ToWorld();
            Matrix4x4 m = Matrix4x4.TRS(worldTree[BodyNode.T7].GetPosition(), Quaternion.LookRotation(pose.GetForward()), Vector3.one);

            return m.MultiplyPoint((side == LeftRight.Left ? Vector3.left : Vector3.right) + Vector3.back  + Vector3.down * 0.5f * l * 2);
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