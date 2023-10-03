using MischievousByte.Silhouette.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Silhouette.Builtin
{
    [CreateAssetMenu]
    public class SpinePoserModule : BodyPoserModule
    {
        [Header("Bend")]
        [SerializeField] private AnimationCurve bendCurve;
        [SerializeField] private float bendAngle;
        [SerializeField] private float neckFollowFactor;

        [Header("Forward")]
        [SerializeField] private AnimationCurve handCurve;
        [SerializeField] private float maxAngle;

        public override void Pose(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
            PoseSpine(input, measurements, constraints, ref pose);
            PoseNeck(input, measurements, constraints, ref pose);
            CorrectTranslationError(input, ref pose);
        }

        private void PoseSpine(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
            float normalizedHeight = CalculateNormalizedHeight(in input, in measurements, in pose);
            float bend = bendCurve.Evaluate(normalizedHeight) * bendAngle;

            pose[BodyNode.Pelvis] = Matrix4x4.TRS(
                pose[BodyNode.Pelvis].GetPosition(),
                Quaternion.LookRotation(CalculateForward(in input, in measurements, in pose)) * Quaternion.Euler(bend, 0, 0),
                Vector3.one);
        }

        private void PoseNeck(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
            /*BodyNode[] c7Chain = BodyNode.C7.GetParentChain().Reverse().ToArray();

            Matrix4x4 globalNeckMatrix = pose[BodyNode.Pelvis];
            Matrix4x4 globalHeadMatrix;


            for (int i = 1; i < c7Chain.Length; i++)
                globalNeckMatrix *= pose[c7Chain[i]];

            globalNeckMatrix *= pose[BodyNode.C7];

            globalHeadMatrix = globalNeckMatrix * pose[BodyNode.Head];


            globalHeadMatrix = Matrix4x4.TRS(globalHeadMatrix.GetPosition(), input.eyes.rotation, Vector3.one);
            pose[BodyNode.Head] = globalNeckMatrix.inverse * globalHeadMatrix;*/

            BodyNode[] t7Chain = BodyNode.C7.GetParentChain().Reverse().ToArray();

            Matrix4x4 globalT7 = Matrix4x4.identity;
            for (int i = 0; i < t7Chain.Length; i++)
                globalT7 *= pose[t7Chain[i]];

            globalT7 *= pose[BodyNode.T7];

            Quaternion localT7TargetRotation = globalT7.inverse.rotation * input.eyes.rotation;

            Quaternion neckRotation = Quaternion.Slerp(Quaternion.identity, localT7TargetRotation, neckFollowFactor);

            pose[BodyNode.C7] = Matrix4x4.TRS(pose[BodyNode.C7].GetPosition(), neckRotation, Vector3.one);

            Matrix4x4 globalC7 = globalT7 * pose[BodyNode.C7];
            Quaternion localHeadRotation = globalC7.inverse.rotation * input.eyes.rotation;

            pose[BodyNode.Head] = Matrix4x4.TRS(pose[BodyNode.Head].GetPosition(), localHeadRotation, Vector3.one);


        }

        private void CorrectTranslationError(in BodyPoser.Input input, ref BodyTree<Matrix4x4> pose)
        {
            BodyNode[] eyeChain = BodyNode.Eyes.GetParentChain().Reverse().ToArray();

            Matrix4x4 globalEyesMatrix = pose[BodyNode.Pelvis];
            for (int i = 1; i < eyeChain.Length; i++)
            {
                globalEyesMatrix *= pose[eyeChain[i]];
            }

            globalEyesMatrix *= pose[BodyNode.Eyes];

            Vector3 eyeDifference = input.eyes.position - globalEyesMatrix.GetPosition();

            pose[BodyNode.Pelvis] = Matrix4x4.TRS(
                pose[BodyNode.Pelvis].GetPosition() + eyeDifference,
                pose[BodyNode.Pelvis].rotation,
                Vector3.one);
        }

        private float CalculateNormalizedHeight(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyTree<Matrix4x4> pose)
        {
            float headToTopOffset = pose[BodyNode.HeadTop].GetPosition().y;

            Matrix4x4 eyesMatrix = pose[BodyNode.Eyes];

            Vector3 eyesToHeadOffset = -eyesMatrix.GetPosition();

            Matrix4x4 currentEyesMatrix = Matrix4x4.TRS(input.eyes.position, input.eyes.rotation, Vector3.one);
            Vector3 currentHeadPosition = currentEyesMatrix.MultiplyPoint(eyesToHeadOffset);

            float maxHeadY = measurements.Height - headToTopOffset;
            float height = currentHeadPosition.y;

            float normalizedHeight = Mathf.Clamp01(height / maxHeadY);
            return normalizedHeight;
        }

        private Vector3 CalculateForward(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyTree<Matrix4x4> pose)
        {
            Vector3 CalculateHeadForward(in BodyPoser.Input input)
            {
                Vector3 headForward = input.eyes.forward;
                Vector3 headUp = input.eyes.up;
                Vector3 bodyForward = Vector3.Slerp(headForward, headUp * -Mathf.Sign(headForward.y), Mathf.Abs(headForward.y));

                bodyForward = Vector3.ProjectOnPlane(bodyForward, Vector3.up);

                return bodyForward;
            }

            float CalculateHandForwardOffset(in BodyPoser.Input input, in BodyMeasurements measurements, BodyTree<Matrix4x4> pose, LeftRight side, Vector3 headForward)
            {
                Matrix4x4 eyesMatrix = pose[BodyNode.Eyes];

                Vector3 eyesToHeadOffset = -eyesMatrix.GetPosition();

                Matrix4x4 currentEyesMatrix = Matrix4x4.TRS(input.eyes.position, input.eyes.rotation, Vector3.one);  
                Vector3 currentHeadPosition = currentEyesMatrix.MultiplyPoint(eyesToHeadOffset);

                Pose handPose = side == LeftRight.Left ? input.leftHand : input.rightHand;

                Matrix4x4 mat = Matrix4x4.TRS(currentHeadPosition, Quaternion.LookRotation(headForward), Vector3.one);

                Vector3 localPoint = mat.inverse.MultiplyPoint(handPose.position);

                Vector3 projectedLocalPoint = Vector3.ProjectOnPlane(localPoint, Vector3.up);

                float angle = Vector3.SignedAngle(Vector3.forward, projectedLocalPoint, Vector3.up) + (side == LeftRight.Left ? 90f : -90f);

                angle = Mathf.Clamp(angle, -maxAngle, maxAngle);

                float distance = Vector3.Distance(currentHeadPosition, handPose.position);

                return handCurve.Evaluate(distance / (measurements.Wingspan / 2f)) * angle;
            }

            Vector3 headForward = CalculateHeadForward(input);

            float leftOffset = CalculateHandForwardOffset(input, measurements, pose, LeftRight.Left, headForward);
            float rightOffset = CalculateHandForwardOffset(input, measurements, pose, LeftRight.Right, headForward);

            float offset = Mathf.Clamp(leftOffset + rightOffset, -maxAngle, maxAngle);

            Quaternion rotationOffset = Quaternion.Euler(0, offset, 0);

            return rotationOffset * headForward;
        }
    }
}