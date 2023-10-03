using Codice.Client.BaseCommands.Download;
using MischievousByte.Silhouette;
using MischievousByte.Silhouette.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using static MischievousByte.RigBuilder.ModularRig;
using static UnityEngine.GraphicsBuffer;

namespace MischievousByte.Masquerade.Character
{
    [CreateAssetMenu()]
    public class BuiltinBodyRemapper : BodyRemapper
    {
        private static void DrawBounds(Matrix4x4 matrix, Bounds bounds, Color color)
        {
            Vector3[] points = new Vector3[]
            {
                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),

                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)
            };


            points = points.Select(p => matrix.MultiplyPoint(p)).ToArray();

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[3], points[0], color);

            Debug.DrawLine(points[4], points[5], color);
            Debug.DrawLine(points[5], points[6], color);
            Debug.DrawLine(points[6], points[7], color);
            Debug.DrawLine(points[7], points[4], color);

            Debug.DrawLine(points[0], points[4], color);
            Debug.DrawLine(points[1], points[5], color);
            Debug.DrawLine(points[2], points[6], color);
            Debug.DrawLine(points[3], points[7], color);
        }


        private static readonly Quaternion leftUpperArmCorrection = Quaternion.Euler(180f, 90f, 0);
        private static readonly Quaternion leftForearmCorrection = Quaternion.Euler(180f, 90f, 0);
        private static readonly Quaternion leftWristCorrection = Quaternion.Euler(-90f, 90f, 0);

        private static readonly Quaternion rightUpperArmCorrection = Quaternion.Euler(0, -90f, 0);
        private static readonly Quaternion rightForearmCorrection = Quaternion.Euler(0, -90f, 0);
        private static readonly Quaternion rightWristCorrection = Quaternion.Euler(-90f, -90f, 0);

        public override void Remap(in BodyTree<Matrix4x4> input, ref BodyTree<Matrix4x4> output, float inputHeight, float outputHeight, in BodyConstraints constraints)
        {
            RemapSpine(in input, ref output, inputHeight, outputHeight);
            RemapArm(LeftRight.Left, input, ref output, constraints);
            RemapArm(LeftRight.Right, input, ref output, constraints);
        }

        private readonly BodyNode[] spineChain = new BodyNode[] { BodyNode.Pelvis, BodyNode.L3, BodyNode.T12, BodyNode.T7, BodyNode.C7, BodyNode.Head};
        
        private void RemapSpine(in BodyTree<Matrix4x4> input, ref BodyTree<Matrix4x4> output, float inputHeight, float outputHeight)
        {
            BodyTree<Matrix4x4> globalInput = input.ToWorld();

            for (ushort i = 0; i < spineChain.Length; i++)
            {
                BodyNode node = spineChain[i];

                output[node] = Matrix4x4.TRS(output[node].GetPosition(), input[node].rotation, Vector3.one);
            }

            BodyTree<Matrix4x4> globalOutput = output.ToWorld();

            Vector3 realtimeEyePosition = globalInput[BodyNode.Eyes].GetPosition();

            //float avatarEyeHeight = 1.675753f; //TODO: REMOVE TMP HEIGHT
            //float avatarEyeHeight = 1.695753f; //TODO: REMOVE TMP HEIGHT
            
            float playerEyeHeight, avatarEyeHeight;
            {
                float top = input[BodyNode.HeadTop].GetPosition().y;
                float eyes = input[BodyNode.Eyes].GetPosition().y;

                float dir = eyes - top;

                playerEyeHeight = inputHeight + dir;
            }
            {
                float top = output[BodyNode.HeadTop].GetPosition().y;
                float eyes = output[BodyNode.Eyes].GetPosition().y;

                float dir = eyes - top;

                avatarEyeHeight = outputHeight + dir;
            }

            float scale = avatarEyeHeight / playerEyeHeight;

            Vector3 virtualEyePosition = realtimeEyePosition * scale;

            Vector3 error = virtualEyePosition - globalOutput[BodyNode.Eyes].GetPosition();

            output[BodyNode.Pelvis] = Matrix4x4.TRS(output[BodyNode.Pelvis].GetPosition() + error, output[BodyNode.Pelvis].rotation, Vector3.one); //.position += error;

        }

        private void RemapArm(LeftRight side, in BodyTree<Matrix4x4> input, ref BodyTree<Matrix4x4> output,  in BodyConstraints constraints)
        {
            BodyTree<Matrix4x4> globalInput = input.ToWorld();

            BodyNode clavicleNode = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArmNode = clavicleNode + 1;
            BodyNode forearmNode = upperArmNode + 1;
            BodyNode wristNode = forearmNode + 1;
            BodyNode handNode = wristNode + 1;
            
            Pose remappedPose = new Pose(CalculateRemappedEndpoint(side, input, output, constraints), globalInput[handNode].rotation);

            
            SolveShoulder(side, ref output, constraints, remappedPose.position);

            Vector3 hint = GenerateHint(side, input, output, remappedPose.position);

            SolveArm(side, ref output, remappedPose, hint);
        }

        private Vector3 CalculateRemappedEndpoint(LeftRight side, in BodyTree<Matrix4x4> input, in BodyTree<Matrix4x4> output, in BodyConstraints constraints)
        {
            BodyNode clavicleNode = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArmNode = clavicleNode + 1;
            BodyNode forearmNode = upperArmNode + 1;
            BodyNode wristNode = forearmNode + 1;
            BodyNode handNode = wristNode + 1;

            BodyTree<Matrix4x4> globalInput = input.ToWorld();
            BodyTree<Matrix4x4> globalOutput = output.ToWorld();

            Matrix4x4 inputMatrix = Matrix4x4.TRS(
                globalInput[BodyNode.T7].GetPosition(),
                Quaternion.LookRotation(input.GetForward()),
                Vector3.one);

            
            Matrix4x4 outputMatrix = Matrix4x4.TRS(
                globalOutput[BodyNode.T7].GetPosition(),
                Quaternion.LookRotation(output.GetForward()),
                Vector3.one);

            Bounds inputBounds = input.GetReachBounds(side, inputMatrix, constraints.MaxShoulderYRotation, constraints.MaxShoulderZRotation);
            Bounds outputBounds = output.GetReachBounds(side, outputMatrix, constraints.MaxShoulderYRotation, constraints.MaxShoulderZRotation);

            DrawBounds(inputMatrix, inputBounds, Color.red);
            DrawBounds(outputMatrix, outputBounds, Color.yellow);

            Debug.DrawLine(inputMatrix.GetPosition(), inputMatrix.MultiplyPoint(Vector3.forward), Color.black);
            Vector3 inputLocalHand = inputMatrix.inverse.MultiplyPoint(globalInput[handNode].GetPosition());

            Vector3 normalizedLocalHand = new Vector3(
                Mathf.InverseLerp(inputBounds.min.x, inputBounds.max.x, inputLocalHand.x),
                Mathf.InverseLerp(inputBounds.min.y, inputBounds.max.y, inputLocalHand.y),
                Mathf.InverseLerp(inputBounds.min.z, inputBounds.max.z, inputLocalHand.z));

            Vector3 outputLocalHand = new Vector3(
                Mathf.Lerp(outputBounds.min.x, outputBounds.max.x, normalizedLocalHand.x),
                Mathf.Lerp(outputBounds.min.y, outputBounds.max.y, normalizedLocalHand.y),
                Mathf.Lerp(outputBounds.min.z, outputBounds.max.z, normalizedLocalHand.z)); ;

            {
                float CalculateArmMaxReach(in BodyTree<Matrix4x4> tree)
                {
                    float armLength = tree[forearmNode].GetPosition().magnitude
                        + tree[wristNode].GetPosition().magnitude
                        + tree[handNode].GetPosition().magnitude;

                    float clavicleX = tree[clavicleNode].GetPosition().x;
                    float upperArmX = tree[upperArmNode].GetPosition().x;

                    float torsoX = clavicleX + upperArmX;

                    return Mathf.Abs(torsoX) + armLength;
                }

                float inputMax = CalculateArmMaxReach(in input);
                float outputMax = CalculateArmMaxReach(in output);

                outputLocalHand.x = inputLocalHand.x / inputMax * outputMax;
            }

            return outputMatrix.MultiplyPoint(outputLocalHand);

        }

        private void SolveShoulder(LeftRight side, ref BodyTree<Matrix4x4> tree, in BodyConstraints constraints, Vector3 targetPosition)
        {
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;

            BodyTree<Matrix4x4> worldTree = tree.ToWorld();

            Matrix4x4 matrix = Matrix4x4.TRS(
                worldTree[BodyNode.T7].GetPosition(),
                Quaternion.LookRotation(tree.GetForward()),
                Vector3.one
                );

            Bounds bounds = tree.GetReachBounds(side, matrix, constraints.MaxShoulderYRotation, constraints.MaxShoulderZRotation);

            Vector3 localHand = matrix.inverse.MultiplyPoint(targetPosition);

            Vector3 normalizedLocalHand = new Vector3(
                Mathf.InverseLerp(bounds.min.x, bounds.max.x, localHand.x),
                Mathf.InverseLerp(bounds.min.y, bounds.max.y, localHand.y),
                Mathf.InverseLerp(bounds.min.z, bounds.max.z, localHand.z));

            Vector2 projectedY = new Vector2(normalizedLocalHand.x, normalizedLocalHand.z) * 2 - Vector2.one;

            float yFactor = Mathf.Clamp01(projectedY.magnitude);
            float zFactor = Mathf.Clamp01(normalizedLocalHand.y * 2 - 1);
            (float y, float z) angles = tree.CalculateOptimalShoulderAngles(
                side,
                targetPosition - worldTree[clavicle].GetPosition(),
                constraints.MaxShoulderYRotation,
                constraints.MaxShoulderZRotation);

            Quaternion localRotation = Quaternion.Euler(0, angles.y * yFactor, angles.z * zFactor);

            Quaternion finalRotation = matrix.rotation * localRotation;

            tree[clavicle] = Matrix4x4.TRS(
                tree[clavicle].GetPosition(),
                worldTree[clavicle.GetParent()].inverse.rotation * finalRotation,
                Vector3.one);
        }

        private void SolveArm(LeftRight side, ref BodyTree<Matrix4x4> tree, Pose target, Vector3 hint)
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

            {
                Matrix4x4 worldHand = GetWorldMatrix(tree, hand);
                Matrix4x4 worldWrist= GetWorldMatrix(tree, wrist);

                Matrix4x4 targetMatrix = Matrix4x4.TRS(target.position, target.rotation, Vector3.one);

                Matrix4x4 localWrist = worldHand.inverse * worldWrist;
                
                Matrix4x4 f = targetMatrix * localWrist;

                target = new Pose(f.GetPosition(), f.rotation);
            }

            float segmentALength = tree[forearm].GetPosition().magnitude;
            float segmentBLength = tree[wrist].GetPosition().magnitude;

            float totalLength = segmentALength + segmentBLength;

            Vector3 targetPosition;
            Vector3 normal;
            float distance;

            Matrix4x4 worldClavicle;
            {
                worldClavicle = Matrix4x4.identity;
                BodyNode[] chain = clavicle.GetParentChain().Reverse().ToArray();

                for (int i = 0; i < chain.Length; i++)
                {
                    worldClavicle *= tree[chain[i]];
                }

                worldClavicle *= tree[clavicle];
            }

            {
                Matrix4x4 worldUpperArm = worldClavicle * tree[upperArm];

                //Makes sure that the target is within range;
                targetPosition = worldUpperArm.GetPosition() + Vector3.ClampMagnitude(target.position - worldUpperArm.GetPosition(), totalLength * 0.999f);

                distance = (targetPosition - worldUpperArm.GetPosition()).magnitude;

                normal = Vector3.Cross(worldUpperArm.GetPosition() - hint, targetPosition - hint);

                normal.Normalize();
            }


            //root.LookAt(targetPosition, normal);
            {
                Matrix4x4 worldUpperArm = GetWorldMatrix(in tree, upperArm);
                Matrix4x4 worldShoulder = GetWorldMatrix(in tree, clavicle);

                Quaternion q = Quaternion.LookRotation(targetPosition - worldUpperArm.GetPosition(), normal);

                tree[upperArm] = Matrix4x4.TRS(tree[upperArm].GetPosition(), worldShoulder.inverse.rotation * q, Vector3.one);
            }

            //Use cosine rule to determine the angle of the triangle's corner (located at the root)
            float cos = (segmentBLength * segmentBLength - segmentALength * segmentALength - distance * distance) /
                (-2 * distance * segmentALength);
            cos = Mathf.Clamp(cos, -1, 1); //Clamp because of float precision errors


            float angle = -Mathf.Acos(cos) * Mathf.Rad2Deg;


            //root.RotateAround(root.position, normal, -angle);
            {
                Matrix4x4 worldShoulder = GetWorldMatrix(in tree, clavicle);
                Matrix4x4 worldUpperArm = GetWorldMatrix(in tree, upperArm);

                Quaternion q = Quaternion.AngleAxis(-angle, normal) * worldUpperArm.rotation;
                Quaternion lq = worldShoulder.inverse.rotation * q;

                tree[upperArm] = Matrix4x4.TRS(tree[upperArm].GetPosition(), lq, Vector3.one);
            }


            //root.localRotation *= Quaternion.Euler(rootCorrection);
            {
                Quaternion correction = side == LeftRight.Left ? leftUpperArmCorrection : rightUpperArmCorrection;
                tree[upperArm] = Matrix4x4.TRS(tree[upperArm].GetPosition(), tree[upperArm].rotation * correction, Vector3.one);
            }


            //middle.LookAt(targetPosition, normal);
            {
                Matrix4x4 worldForearm = GetWorldMatrix(in tree, forearm);
                Matrix4x4 worldUpperArm = GetWorldMatrix(in tree, upperArm);

                Quaternion q = Quaternion.LookRotation(targetPosition - worldForearm.GetPosition(), normal);

                tree[forearm] = Matrix4x4.TRS(tree[forearm].GetPosition(), worldUpperArm.inverse.rotation * q, Vector3.one);
            }

            //middle.localRotation *= Quaternion.Euler(middleCorrection);
            {
                Quaternion correction = side == LeftRight.Left ? leftForearmCorrection : rightForearmCorrection;
                tree[forearm] = Matrix4x4.TRS(tree[forearm].GetPosition(), tree[forearm].rotation * correction, Vector3.one);
            }


            //end.rotation = target.rotation;
            {
                Matrix4x4 worldForearm = GetWorldMatrix(in tree, forearm);

                tree[wrist] = Matrix4x4.TRS(tree[wrist].GetPosition(), worldForearm.inverse.rotation * target.rotation, Vector3.one);
            }
        }

        private Vector3 GenerateHint(LeftRight side, in BodyTree<Matrix4x4> input, in BodyTree<Matrix4x4> output, Vector3 targetPosition)
        {
            BodyNode clavicleNode = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArmNode = clavicleNode + 1;
            BodyNode forearmNode = upperArmNode + 1;
            BodyNode wristNode = forearmNode + 1;
            BodyNode handNode = wristNode + 1;

            BodyTree<Matrix4x4> globalInput = input.ToWorld();
            BodyTree<Matrix4x4> globalOutput = output.ToWorld();

            Vector3 GetPlaneNormal()
            {
                Vector3 localRoot = globalInput[upperArmNode].GetPosition();
                Vector3 localMiddle = globalInput[forearmNode].GetPosition();
                Vector3 localEnd = globalInput[wristNode].GetPosition();

                Vector3 a = localMiddle - localRoot;
                Vector3 b = localEnd - localMiddle;

                Vector3 normal = Vector3.Cross(a, b).normalized;

                return normal;
            }

            Vector3 localRoot = globalOutput[upperArmNode].GetPosition();

            Vector3 localTarget = targetPosition;

            Vector3 localNormal = GetPlaneNormal();

            Vector3 localRootToTarget = localTarget - localRoot;

            Vector3 hintDirection = Vector3.Cross(localNormal, localRootToTarget).normalized;

            Vector3 halfway = Vector3.Lerp(localRoot, localTarget, 0.5f);

            return halfway - hintDirection;
        }
    }
}
