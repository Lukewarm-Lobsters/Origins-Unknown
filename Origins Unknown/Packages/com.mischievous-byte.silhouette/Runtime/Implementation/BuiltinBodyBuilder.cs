using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Silhouette.Implementation
{
    [CreateAssetMenu(menuName = PackageInfo.AssetMenuPrefix + "Implementation/Built-in Body Builder")]
    public class BuiltinBodyBuilder : BodyBuilder
    {
        [System.Serializable]
        public struct SpineSettings
        {
            public AnimationCurve lengthCurve;
            public float pelvisZ;
            public Vector2 l3;
            public Vector2 t12;
            public Vector2 t7;
            public Vector2 c7;
            public float headZ;
        }

        [System.Serializable]
        public struct HeadSettings
        {
            public AnimationCurve lengthCurve;
            public float depth;
            public Vector2 attachPoint;
        }

        [System.Serializable]
        public struct ArmSettings
        {
            public AnimationCurve handLengthCurve;
            public Vector3 claviclePosition;
            public Vector3 upperArmPosition;
            public Vector3 palm;
        }
        
        [SerializeField] private SpineSettings spineSettings;
        [SerializeField] private HeadSettings headSettings;
        [SerializeField] private ArmSettings armSettings;


        public override void Build(ref BodyTree<Matrix4x4> pose, in BodyMeasurements measurements)
        {
            foreach (BodyNode node in BodyNodeUtility.All)
                pose[node] = Matrix4x4.identity;

            BuildSpineAndHead(ref pose, in measurements);
            BuildArms(ref pose, in measurements);


            MakeLocal(ref pose);
        }


        private void MakeLocal(ref BodyTree<Matrix4x4> pose)
        {
            BodyTree<Matrix4x4> copy = pose;
            foreach(BodyNode node in BodyNodeUtility.All.Where(x => x != BodyNode.Pelvis))
            {
                pose[node] = copy[node.GetParent()].inverse * pose[node];
            }
                
        }

        private void BuildSpineAndHead(ref BodyTree<Matrix4x4> pose, in BodyMeasurements measurements)
        {
            float spineLength = spineSettings.lengthCurve.Evaluate(measurements.Height);
            float headLength = headSettings.lengthCurve.Evaluate(measurements.Height);
            float headDepth = headSettings.depth * headLength;


            float headY = measurements.Height + headLength * (-0.5f + headSettings.attachPoint.y);

            Vector3 pelvisPosition = new Vector3(0, headY - spineLength, spineSettings.pelvisZ);
            Vector3 l3Postition = new Vector3(0, Mathf.Lerp(pelvisPosition.y, headY, spineSettings.l3.y), spineLength * spineSettings.l3.x);
            Vector3 t12Position = new Vector3(0, Mathf.Lerp(pelvisPosition.y, headY, spineSettings.t12.y), spineLength * spineSettings.t12.x);
            Vector3 t7Position = new Vector3(0, Mathf.Lerp(pelvisPosition.y, headY, spineSettings.t7.y), spineLength * spineSettings.t7.x);
            Vector3 c7Position = new Vector3(0, Mathf.Lerp(pelvisPosition.y, headY, spineSettings.c7.y), spineLength * spineSettings.c7.x);
            Vector3 headPosition = new Vector3(0, headY, spineLength * spineSettings.headZ);
            Vector3 eyesPosition = new Vector3(0, measurements.Height - headLength * 0.5f, headPosition.z - headSettings.attachPoint.x * headDepth + 0.5f * headDepth);
            Vector3 headTopPosition = new Vector3(0, measurements.Height, eyesPosition.z - headDepth * 0.5f);

            pose[BodyNode.Pelvis] = Matrix4x4.TRS(pelvisPosition, Quaternion.identity, Vector3.one);
            pose[BodyNode.L3] = Matrix4x4.TRS(l3Postition, Quaternion.identity, Vector3.one);
            pose[BodyNode.T12] = Matrix4x4.TRS(t12Position, Quaternion.identity, Vector3.one);
            pose[BodyNode.T7] = Matrix4x4.TRS(t7Position, Quaternion.identity, Vector3.one);
            pose[BodyNode.C7] = Matrix4x4.TRS(c7Position, Quaternion.identity, Vector3.one);
            pose[BodyNode.Head] = Matrix4x4.TRS(headPosition, Quaternion.identity, Vector3.one);
            pose[BodyNode.Eyes] = Matrix4x4.TRS(eyesPosition, Quaternion.identity, Vector3.one);
            pose[BodyNode.HeadTop] = Matrix4x4.TRS(headTopPosition, Quaternion.identity, Vector3.one);
            
        }

        private void BuildArms(ref BodyTree<Matrix4x4> pose, in BodyMeasurements measurements)
        {
            float spineLength = spineSettings.lengthCurve.Evaluate(measurements.Height);
            float headLength = headSettings.lengthCurve.Evaluate(measurements.Height);

            float headY = measurements.Height + headLength * (-0.5f + headSettings.attachPoint.y);

            float pelvisY = headY - spineLength;

            Vector3 claviclePosition = Vector3.up * pelvisY + armSettings.claviclePosition * spineLength;
            Vector3 upperArmPosition = Vector3.up * pelvisY + armSettings.upperArmPosition * spineLength;

            pose[BodyNode.RightClavicle] = Matrix4x4.TRS(claviclePosition, Quaternion.identity, Vector3.one);
            pose[BodyNode.RightUpperArm] = Matrix4x4.TRS(upperArmPosition, Quaternion.identity, Vector3.one);

            float handLength = armSettings.handLengthCurve.Evaluate(measurements.Wingspan);
            float armLength = measurements.Wingspan * 0.5f - upperArmPosition.x;
            float segmentLength = (armLength - handLength) * 0.5f;

            Vector3 forearmPosition = upperArmPosition + Vector3.right * segmentLength;
            Vector3 wristPosition = forearmPosition + Vector3.right * segmentLength;
            Vector3 handPosition = wristPosition + armSettings.palm * handLength;

            pose[BodyNode.RightForearm] = Matrix4x4.TRS(forearmPosition, Quaternion.identity, Vector3.one);
            pose[BodyNode.RightWrist] = Matrix4x4.TRS(wristPosition, Quaternion.identity, Vector3.one);
            pose[BodyNode.RightHand] = Matrix4x4.TRS(handPosition, Quaternion.identity, Vector3.one);


            Vector3 Mirror(Vector3 v) => new Vector3(-v.x, v.y, v.z);

            pose[BodyNode.LeftClavicle] = Matrix4x4.TRS(Mirror(claviclePosition), Quaternion.identity, Vector3.one);
            pose[BodyNode.LeftUpperArm] = Matrix4x4.TRS(Mirror(upperArmPosition), Quaternion.identity, Vector3.one);
            pose[BodyNode.LeftForearm] = Matrix4x4.TRS(Mirror(forearmPosition), Quaternion.identity, Vector3.one);
            pose[BodyNode.LeftWrist] = Matrix4x4.TRS(Mirror(wristPosition), Quaternion.identity, Vector3.one);
            pose[BodyNode.LeftHand] = Matrix4x4.TRS(Mirror(handPosition), Quaternion.identity, Vector3.one);
        }
        
        private float CalculateHeadY(in BodyMeasurements measurements) => measurements.Height + headSettings.lengthCurve.Evaluate(measurements.Height)  * (-0.5f + headSettings.attachPoint.y);
    }
}
