using MischievousByte.Silhouette;
using MischievousByte.Silhouette.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Masquerade.Character
{
    [CreateAssetMenu(menuName = Core_PackageInfo.AssetMenuPrefix + "Standard Body Remapper")]
    public class StandardBodyRemapper : BodyRemapper
    {
        public bool remapShoulders;

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

        private static void DrawMarker(Vector3 position, float size, Color color)
        {
            Debug.DrawLine(position - Vector3.right * size, position + Vector3.right * size, color);
            Debug.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size, color);
            Debug.DrawLine(position - Vector3.up * size, position + Vector3.up * size, color);
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

            Debug.Log(scale);

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

            DrawMarker(globalInput[wristNode].GetPosition(), 0.1f, Color.green);
            DrawMarker(remappedPose.position, 0.05f, Color.red);

            if (remapShoulders)
                SolveShoulder(side, ref output, constraints, remappedPose.position);
            else
                output[clavicleNode] = Matrix4x4.TRS(output[clavicleNode].GetPosition(), Quaternion.identity, Vector3.one);

            Vector3 hint = GenerateHint(side, input, output, remappedPose.position);

            SolveArm(side, ref output, remappedPose, hint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="side"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="constraints"></param>
        /// <returns>Remapped wrist point</returns>
        private Vector3 CalculateRemappedEndpoint(LeftRight side, in BodyTree<Matrix4x4> input, in BodyTree<Matrix4x4> output, in BodyConstraints constraints)
        {
            BodyNode clavicleNode = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArmNode = clavicleNode + 1;
            BodyNode forearmNode = upperArmNode + 1;
            BodyNode wristNode = forearmNode + 1;

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
            Bounds outputBounds;
            if(remapShoulders)
                outputBounds = output.GetReachBounds(side, outputMatrix, constraints.MaxShoulderYRotation, constraints.MaxShoulderZRotation);
            else
                outputBounds = output.GetReachBounds(side, outputMatrix, 0, 0);

            DrawBounds(inputMatrix, inputBounds, Color.red);
            DrawBounds(outputMatrix, outputBounds, Color.yellow);

            Debug.DrawLine(inputMatrix.GetPosition(), inputMatrix.MultiplyPoint(Vector3.forward), Color.black);
            //Vector3 inputLocalHand = inputMatrix.inverse.MultiplyPoint(globalInput[handNode].GetPosition());
            Vector3 inputLocalWrist = inputMatrix.inverse.MultiplyPoint(globalInput[wristNode].GetPosition());

            /*Vector3 normalizedLocalHand = new Vector3(
                Mathf.InverseLerp(inputBounds.min.x, inputBounds.max.x, inputLocalHand.x),
                Mathf.InverseLerp(inputBounds.min.y, inputBounds.max.y, inputLocalHand.y),
                Mathf.InverseLerp(inputBounds.min.z, inputBounds.max.z, inputLocalHand.z));*/

            Vector3 normalizedLocalWrist = new Vector3(
                Mathf.InverseLerp(inputBounds.min.x, inputBounds.max.x, inputLocalWrist.x),
                Mathf.InverseLerp(inputBounds.min.y, inputBounds.max.y, inputLocalWrist.y),
                Mathf.InverseLerp(inputBounds.min.z, inputBounds.max.z, inputLocalWrist.z));

            /*Vector3 outputLocalHand = new Vector3(
                Mathf.Lerp(outputBounds.min.x, outputBounds.max.x, normalizedLocalHand.x),
                Mathf.Lerp(outputBounds.min.y, outputBounds.max.y, normalizedLocalHand.y),
                Mathf.Lerp(outputBounds.min.z, outputBounds.max.z, normalizedLocalHand.z));*/

            Vector3 outputLocalWrist = new Vector3(
                Mathf.Lerp(outputBounds.min.x, outputBounds.max.x, normalizedLocalWrist.x),
                Mathf.Lerp(outputBounds.min.y, outputBounds.max.y, normalizedLocalWrist.y),
                Mathf.Lerp(outputBounds.min.z, outputBounds.max.z, normalizedLocalWrist.z));

            {
                float CalculateArmMaxReach(in BodyTree<Matrix4x4> tree)
                {
                    float armLength = tree[forearmNode].GetPosition().magnitude
                        + tree[wristNode].GetPosition().magnitude;
                        //+ tree[handNode].GetPosition().magnitude;

                    float clavicleX = tree[clavicleNode].GetPosition().x;
                    float upperArmX = tree[upperArmNode].GetPosition().x;

                    float torsoX = clavicleX + upperArmX;

                    return Mathf.Abs(torsoX) + armLength;
                }

                float inputMax = CalculateArmMaxReach(in input);
                float outputMax = CalculateArmMaxReach(in output);

                outputLocalWrist.x = inputLocalWrist.x / inputMax * outputMax;
            }

            return outputMatrix.MultiplyPoint(outputLocalWrist);
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

            Vector3 localWrist = matrix.inverse.MultiplyPoint(targetPosition);

            Vector3 clampedLocalWrist = new Vector3(
                Mathf.Clamp(localWrist.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(localWrist.y, bounds.min.y, bounds.max.y),
                Mathf.Clamp(localWrist.z, bounds.min.z, bounds.max.z)
                );

            {
                Vector3 w = matrix.MultiplyPoint(clampedLocalWrist);
                Debug.DrawLine(w, w + Vector3.down * 0.4f, Color.yellow);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="side"></param>
        /// <param name="tree"></param>
        /// <param name="target">Wrist</param>
        /// <param name="worldHint"></param>
        private void SolveArm(LeftRight side, ref BodyTree<Matrix4x4> tree, Pose target, Vector3 worldHint)
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

            Matrix4x4 worldClavicle = GetWorldMatrix(in tree, clavicle);

            float segmentALength = tree[forearm].GetPosition().magnitude;
            float segmentBLength = tree[wrist].GetPosition().magnitude;

            float totalLength = segmentALength + segmentBLength;

            Matrix4x4 worldUpperArm = worldClavicle * tree[upperArm];


            target.position = worldUpperArm.GetPosition() + Vector3.ClampMagnitude(target.position - worldUpperArm.GetPosition(), totalLength * 0.999f);

            float distance = (target.position - worldUpperArm.GetPosition()).magnitude;

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
