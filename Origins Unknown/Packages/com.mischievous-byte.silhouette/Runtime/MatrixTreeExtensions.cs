using MischievousByte.Silhouette.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Silhouette
{
    public static class MatrixTreeExtensions
    {
        public static BodyTree<Matrix4x4> ToWorld(this in BodyTree<Matrix4x4> tree)
        {
            BodyTree<Matrix4x4> p = tree;

            foreach (var node in BodyNodeUtility.All.Where(x => x != BodyNode.Pelvis))
                p[node] = p[node.GetParent()] * p[node];

            return p;
        }

        public static Vector3 GetForward(this in BodyTree<Matrix4x4> tree)
        {
            var worldTree = tree.ToWorld();
            Vector3 forward = worldTree[BodyNode.T7].rotation * Vector3.forward;
            Vector3 up = worldTree[BodyNode.T7].rotation * Vector3.up;
            Vector3 bodyForward = Vector3.Slerp(forward, up * -Mathf.Sign(forward.y), Mathf.Abs(forward.y));

            bodyForward = Vector3.ProjectOnPlane(bodyForward, Vector3.up);

            return bodyForward;
        }


        public static Bounds GetReachBounds(this in BodyTree<Matrix4x4> tree, LeftRight side, Matrix4x4 matrix, float maxShoulderYAngle, float maxShoulderZAngle)
        {
            BodyTree<Matrix4x4> worldTree = tree.ToWorld();

            BodyNode clavicleNode = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArmNode = clavicleNode + 1;
            BodyNode forearmNode = upperArmNode + 1;
            BodyNode wristNode = forearmNode + 1;
            BodyNode handNode = wristNode + 1;


            float armLength = tree[forearmNode].GetPosition().magnitude + tree[wristNode].GetPosition().magnitude;
            armLength += tree[handNode].GetPosition().magnitude;


            Vector3 GetUpperArmPosition(in BodyTree<Matrix4x4> tree, Vector3 direction)
            {
                var angles = tree.CalculateOptimalShoulderAngles(side, direction, maxShoulderYAngle, maxShoulderZAngle);
                Matrix4x4 rotatedClavicle = Matrix4x4.TRS(tree[clavicleNode].GetPosition(), Quaternion.Euler(0, angles.y, angles.z), Vector3.one);


                return rotatedClavicle.MultiplyPoint(tree[upperArmNode].GetPosition());
            }

            float xMin = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.left)).x - armLength;
            float xMax = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.right)).x + armLength;
            float yMin = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.down)).y - armLength;
            float yMax = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.up)).y + armLength;
            float zMin = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.back)).z - armLength;
            float zMax = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.forward)).z + armLength;
            
            Bounds bounds = new Bounds();

            bounds.SetMinMax(
                new Vector3(xMin, yMin, zMin),
                new Vector3(xMax, yMax, zMax));

            return bounds;
        }

        
        public static (float y, float z) CalculateOptimalShoulderAngles(this in BodyTree<Matrix4x4> tree, LeftRight side, Vector3 direction, float maxY, float maxZ)
        {
            BodyTree<Matrix4x4> globalTree = tree.ToWorld();

            BodyNode clavicleNode = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;

            Matrix4x4 matrix = Matrix4x4.TRS(globalTree[clavicleNode].GetPosition(), globalTree[BodyNode.T7].rotation, Vector3.one);

            Vector3 localDirection = matrix.inverse.MultiplyVector(direction).normalized;

            float yAngle, zAngle;
            {
                float phi = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg; //x and z are swapped here
                float theta = Mathf.Acos(
                        localDirection.y /
                        (localDirection.x * localDirection.x
                        + localDirection.y * localDirection.y
                        + localDirection.z * localDirection.z)) * Mathf.Rad2Deg - 90f;

                Vector3 phiVector = Quaternion.Euler(0, phi, 0) * Vector3.forward;

                Vector3 halfwayYAngle = Quaternion.Euler(
                    0,
                    side == LeftRight.Left ? -90 + maxY / 2f : 90 - maxY / 2f, 0)
                    * Vector3.forward;

                float deltaPhi = Vector3.SignedAngle(halfwayYAngle, phiVector, Vector3.up);

                float clampedDeltaPhi = Mathf.Clamp(deltaPhi, -maxY / 2f, maxY / 2f);

                yAngle = clampedDeltaPhi + (side == LeftRight.Left ? maxY / 2f : -maxY / 2f);

                Vector3 thetaVector = Quaternion.Euler(theta, 0, 0) * Vector3.forward;

                Vector3 halfwayZAngle = Quaternion.Euler(
                    -maxZ / 2f,
                    0, 0)
                    * Vector3.forward;

                float deltaTheta = Vector3.SignedAngle(halfwayZAngle, thetaVector, Vector3.right);

                float clampedDeltaTheta = Mathf.Clamp(deltaTheta, -maxZ / 2f, maxZ / 2f);

                zAngle = side == LeftRight.Left ? clampedDeltaTheta - maxZ / 2f : -clampedDeltaTheta + maxZ / 2f;
            }

            return (yAngle, zAngle);
        }
    }
}
