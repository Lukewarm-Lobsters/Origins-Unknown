using MischievousByte.Silhouette.Utility;
using System;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Silhouette.Implementation
{
    [CreateAssetMenu(menuName = PackageInfo.AssetMenuPrefix + "Implementation/Spine Poser Module")]
    public class SpinePoserModule : BodyPoserModule
    {
        [System.Serializable]
        public struct BendSettings
        {
            public AnimationCurve heightCurve;
            
            [Range(0, 90)]
            public float angle;
        }

        [Serializable]
        public struct DirectionSettings
        {
            public AnimationCurve reachMultiplier;
            [Range(0, 180)]
            public float reachRange;
            [Range(0, 1)]
            public float reachBlend;
        }

        [SerializeField] private BendSettings bendSettings = new BendSettings()
        {
            heightCurve = AnimationCurve.Linear(0, 1, 1, 0),
            angle = 45f
        };

        [SerializeField] private DirectionSettings directionSettings = new DirectionSettings()
        {
            reachMultiplier = AnimationCurve.Linear(0, 0, 1, 1),
            reachRange = 180,
            reachBlend = 0.5f
        };

        [Space(10)]
        [SerializeField] private float neckFollowFactor;
        public override void Pose(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
            PoseSpine(input, measurements, constraints, ref pose);
            PoseNeck(input, measurements, constraints, ref pose);
            CorrectTranslationError(input, ref pose);
        }

        private void PoseSpine(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
            float normalizedHeight = CalculateNormalizedHeight(in input, in measurements, in pose);
            float bend = bendSettings.heightCurve.Evaluate(normalizedHeight) * bendSettings.angle;

            pose[BodyNode.Pelvis] = Matrix4x4.TRS(
                pose[BodyNode.Pelvis].GetPosition(),
                CalculateForward(in pose, in input, in measurements) * Quaternion.Euler(bend, 0, 0),
                Vector3.one);
        }

        private void PoseNeck(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose)
        {
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


        private Quaternion CalculateForward(in BodyTree<Matrix4x4> tree, in BodyPoser.Input input, in BodyMeasurements measurements)
        {
            Quaternion headRotation;
            {
                Vector3 headForward = input.eyes.forward;
                Vector3 headUp = input.eyes.up;
                Vector3 bodyForward = Vector3.Slerp(headForward, headUp * -Mathf.Sign(headForward.y), Mathf.Abs(headForward.y));

                bodyForward = Vector3.ProjectOnPlane(bodyForward, Vector3.up);

                headRotation = Quaternion.LookRotation(bodyForward);
            }



            (float offset, float bias) CalculateHandOffset(LeftRight side, in BodyTree<Matrix4x4> tree, in BodyPoser.Input input, in BodyMeasurements measurements)
            {
                BodyNode upperArmNode = side == LeftRight.Left ? BodyNode.LeftUpperArm : BodyNode.RightUpperArm;
                BodyNode forearmNode = upperArmNode + 1;
                BodyNode wristNode = forearmNode + 1;
                BodyNode handNode = wristNode + 1;

                Vector3 wristPosition;
                {
                    Pose target = side == LeftRight.Left ? input.leftHand : input.rightHand;
                    Quaternion correction = Quaternion.Inverse(side == LeftRight.Left ? Quaternion.Euler(-90f, 90f, 0) : Quaternion.Euler(-90f, -90f, 0));
                    Pose desiredHand = new Pose(target.position, target.rotation * Quaternion.Inverse(correction));
                    
                    Matrix4x4 desiredHandMatrix = Matrix4x4.TRS(desiredHand.position, desiredHand.rotation, Vector3.one);

                    Matrix4x4 handToWristMatrix = tree[handNode].inverse;

                    Matrix4x4 currentWrist = handToWristMatrix * desiredHandMatrix;

                    wristPosition = currentWrist.GetPosition();
                }

                Matrix4x4 headLocalToEyes = tree[BodyNode.Eyes].inverse;

                Matrix4x4 eyesMatrix = Matrix4x4.TRS(input.eyes.position, input.eyes.rotation, Vector3.one);

                Vector3 headPosition = (headLocalToEyes * eyesMatrix).GetPosition();
                Vector3 headToWrist = wristPosition - headPosition;
                Vector3 headToWristProjected = Vector3.ProjectOnPlane(headToWrist, Vector3.up);


                Quaternion headToWristRotation = Quaternion.LookRotation(headToWristProjected);

                float angle = Vector3.SignedAngle(headRotation * Vector3.forward, headToWristRotation * Vector3.forward, Vector3.up);

                float t = angle / 180f;

                float offset;
                if (side == LeftRight.Left)
                    offset = directionSettings.reachRange / 2f + t * directionSettings.reachRange;
                else
                    offset = -directionSettings.reachRange / 2f + t * directionSettings.reachRange;

                float armLength = tree[forearmNode].GetPosition().magnitude + tree[wristNode].GetPosition().magnitude;

                float distance = headToWristProjected.magnitude;

                float bias = Mathf.Clamp01(directionSettings.reachMultiplier.Evaluate(Mathf.Clamp01(distance / armLength)));

                return (offset, bias);
            }


            var leftHandOffsetData = CalculateHandOffset(LeftRight.Left, in tree, input, measurements);
            
            var rightHandOffsetData = CalculateHandOffset(LeftRight.Right, in tree, input, measurements);

            float bias = Mathf.Clamp01(0.5f + leftHandOffsetData.bias * 0.5f - rightHandOffsetData.bias * 0.5f);

            float offset = Mathf.Lerp(rightHandOffsetData.offset, leftHandOffsetData.offset, bias);

            return Quaternion.Euler(0, offset * directionSettings.reachBlend, 0f) * headRotation;
        }
    }
}