using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Character
{
    public partial class Avatar
    {

        private void OnDrawGizmos()
        {
            DrawHeight();

            DrawTorso();
            DrawArm(true);
        }


        private void DrawHeight() => Gizmos.DrawWireCube(transform.position + transform.up * height, (Vector3.right + Vector3.forward) * 0.1f);

        private void DrawTorso()
        {
            Matrix4x4 mat = Gizmos.matrix;

            Matrix4x4 torsoMatrix =
                Matrix4x4.TRS(
                    animator.GetBoneTransform(HumanBodyBones.Hips).position,
                    animator.transform.rotation,
                    Vector3.one);

            Gizmos.matrix = torsoMatrix;
            Gizmos.color = Color.blue;



            Vector3 localHips = Vector3.zero;

            Vector3 localSternum = torsoMatrix.inverse.MultiplyPoint(
                Vector3.Lerp(
                    animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position,
                    animator.GetBoneTransform(HumanBodyBones.RightShoulder).position,
                    0.5f));


            //Vector3 groinLower = torsoMatrix.inverse.MultiplyPoint(
            //animator.transform.TransformPoint(BodyShape.TorsoShape.GroinBounds.getBoundaryA(animator)));

            Vector3 localWaist = Vector3.Lerp(localHips, localSternum, shape.torso.waistY);
            Vector3 localChest = Vector3.Lerp(localHips, localSternum, shape.torso.chestY);
            Vector3 localNeck = torsoMatrix.inverse.MultiplyPoint(animator.GetBoneTransform(HumanBodyBones.Neck).position);
            Vector3 localHead = torsoMatrix.inverse.MultiplyPoint(animator.GetBoneTransform(HumanBodyBones.Head).position);
            /*
            Vector3 groinHigher = torsoMatrix.inverse.MultiplyPoint(
                animator.transform.TransformPoint(BodyShape.TorsoShape.GroinBounds.getBoundaryB(animator)));

            Vector3 waistLower = torsoMatrix.inverse.MultiplyPoint(
                animator.transform.TransformPoint(BodyShape.TorsoShape.WaistBounds.getBoundaryA(animator)));

            Vector3 waistHigher = torsoMatrix.inverse.MultiplyPoint(
                animator.transform.TransformPoint(BodyShape.TorsoShape.WaistBounds.getBoundaryB(animator)));

            Vector3 chestLower = torsoMatrix.inverse.MultiplyPoint(
                animator.transform.TransformPoint(BodyShape.TorsoShape.ChestBounds.getBoundaryA(animator)));

            Vector3 chestHigher = torsoMatrix.inverse.MultiplyPoint(
                animator.transform.TransformPoint(BodyShape.TorsoShape.ChestBounds.getBoundaryB(animator)));

            */

            /*Vector3 localGroin = Vector3.Lerp(groinLower, groinHigher, shape.Torso.GroinY);
            Vector3 localWaist = Vector3.Lerp(waistLower, waistHigher, shape.Torso.WaistY);
            Vector3 localChest = Vector3.Lerp(chestLower, chestHigher, shape.Torso.ChestY);
            Vector3 localNeck = torsoMatrix.inverse.MultiplyPoint(animator.GetBoneTransform(HumanBodyBones.Neck).position);
            Vector3 localHead = torsoMatrix.inverse.MultiplyPoint(animator.GetBoneTransform(HumanBodyBones.Head).position);
            */

            //float scaleFactor = (localSternum.y - localHips.y) * BodyShape.TorsoShape.EllipseScaleFactor;

            Quaternion rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

            DrawRing(localHips, shape.torso.hipsEllipse, rotation, 1, 24);
            DrawRing(localWaist, shape.torso.waistEllipse, rotation, 1, 24);
            DrawRing(localChest, shape.torso.chestEllipse, rotation, 1, 24);
            DrawRing(localSternum, shape.torso.sternumEllipse, rotation, 1, 24);
            DrawRing(localNeck, shape.torso.neckEllipse, rotation, 1, 24);
            DrawRing(localHead, shape.torso.neckEndEllipse, rotation, 1, 24);


            DrawConnection(localHips, rotation, shape.torso.hipsEllipse, localWaist, rotation, shape.torso.waistEllipse, 1f, 24);
            DrawConnection(localWaist, rotation, shape.torso.waistEllipse, localChest, rotation, shape.torso.chestEllipse, 1f, 24);
            DrawConnection(localChest, rotation, shape.torso.chestEllipse, localSternum, rotation, shape.torso.sternumEllipse, 1f, 24);
            DrawConnection(localSternum, rotation, shape.torso.sternumEllipse, localNeck, rotation, shape.torso.neckEllipse, 1f, 24);
            DrawConnection(localNeck, rotation, shape.torso.neckEllipse, localHead, rotation, shape.torso.neckEndEllipse, 1f, 24);
            

            Gizmos.matrix = mat;
        }

        private void DrawArm(bool isLeft)
        {
            bool flipX = !isLeft;

            HumanBodyBones upperBone;
            HumanBodyBones middleBone;
            HumanBodyBones lowerBone;

                upperBone = isLeft ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm;
                middleBone = isLeft ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm;
                lowerBone = isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;

            Transform upperTransform = animator.GetBoneTransform(upperBone);
            Transform middleTransform = animator.GetBoneTransform(middleBone);
            Transform lowerTransform = animator.GetBoneTransform(lowerBone);

            float segmentALength = Vector3.Distance(upperTransform.position, middleTransform.position);
            float segmentBLength = Vector3.Distance(middleTransform.position, lowerTransform.position);

            float totalLength = segmentALength + segmentBLength;


            Quaternion upperToMiddle = Quaternion.LookRotation(middleTransform.position - upperTransform.position,animator.transform.up);
            Quaternion middleToLower = Quaternion.LookRotation(lowerTransform.position - middleTransform.position, animator.transform.up);

            Matrix4x4 upperMatrix = Matrix4x4.TRS(upperTransform.position, upperToMiddle, Vector3.one);
            Matrix4x4 middleMatrix = Matrix4x4.TRS(middleTransform.position, Quaternion.Slerp(upperToMiddle, middleToLower, 0.5f), Vector3.one);
            Matrix4x4 lowerMatrix = Matrix4x4.TRS(lowerTransform.position, middleToLower, Vector3.one);

            Matrix4x4 cachedMatrix = Gizmos.matrix;

            Gizmos.color = Color.green;

            SoftEllipse upperEllipse = shape.arm.upperArmEllipse;
            SoftEllipse middleEllipse = shape.arm.forearmEllipse;
            SoftEllipse lowerEllipse = shape.arm.wristEllipse;

            Gizmos.matrix = upperMatrix;
            Vector3 upperPosition = Vector3.forward * segmentALength * shape.arm.upperArmOffset;

            float scaleFactor = 1f;
            //float scaleFactor = totalLength * BodyShape.LimbShape.EllipseScaleFactor;
            DrawRing(
                upperPosition,
                upperEllipse, Quaternion.identity, scaleFactor, flipX: flipX);

            DrawConnection(
                upperPosition,
                Quaternion.identity,
                upperEllipse,

                Vector3.forward * segmentALength,
                Quaternion.Inverse(upperMatrix.rotation) * middleMatrix.rotation,
                middleEllipse,

                scaleFactor,
                flipX: flipX);


            Gizmos.matrix = middleMatrix;
            DrawRing(
                Vector3.zero,
                middleEllipse, Quaternion.identity, scaleFactor, flipX: flipX);





            Gizmos.matrix = lowerMatrix;
            DrawRing(
                Vector3.zero,
                lowerEllipse, Quaternion.identity, scaleFactor, flipX: flipX);

            DrawConnection(
                Vector3.zero,
                Quaternion.identity,
                lowerEllipse,

                -Vector3.forward * segmentBLength,
                Quaternion.Inverse(lowerMatrix.rotation) * middleMatrix.rotation,
                middleEllipse,

                scaleFactor,
                flipX: flipX);

            Gizmos.matrix = cachedMatrix;
        }
        private void DrawRing(Vector3 position, SoftEllipse ellipse, Quaternion rotation, float scaleFactor, int segments = 12, bool flipX = false, bool flipY = false)
        {
            for (int i = 0; i < segments; i++)
            {
                Vector2 sampleA = ellipse.Sample((float)i / segments) * scaleFactor;
                Vector2 sampleB = ellipse.Sample((float)(i + 1) / segments) * scaleFactor;
                Vector3 from = position + rotation * new Vector3(flipX ? -sampleA.x : sampleA.x, flipY ? -sampleA.y : sampleA.y);
                Vector3 to = position + rotation * new Vector3(flipX ? -sampleB.x : sampleB.x, flipY ? -sampleB.y : sampleB.y);

                Gizmos.DrawLine(from, to);
            }
        }

        private void DrawConnection(Vector3 positionA, Quaternion rotationA, SoftEllipse ellipseA, Vector3 positionB, Quaternion rotationB, SoftEllipse ellipseB, float scaleFactor, int segments = 12, bool flipX = false, bool flipY = false)
        {
            for (int i = 0; i < segments; i++)
            {
                Vector2 sampleA = ellipseA.Sample((float)i / segments) * scaleFactor;
                Vector2 sampleB = ellipseB.Sample((float)i / segments) * scaleFactor;
                Vector3 from = positionA + rotationA * new Vector3(flipX ? -sampleA.x : sampleA.x, flipY ? -sampleA.y : sampleA.y);
                Vector3 to = positionB + rotationB * new Vector3(flipX ? -sampleB.x : sampleB.x, flipY ? -sampleB.y : sampleB.y);

                Gizmos.DrawLine(from, to);
            }
        }

    }
}
