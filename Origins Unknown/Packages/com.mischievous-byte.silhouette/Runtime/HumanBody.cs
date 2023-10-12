using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MischievousByte.Silhouette
{
    [CreateAssetMenu(menuName = PackageInfo.AssetMenuPrefix + "Human Body")]
    public class HumanBody : ScriptableObject
    {
        [System.Serializable]
        private struct PoseInput
        {
            public InputActionReference positionAction;
            public InputActionReference rotationAction;

            public Pose Read() => new Pose(positionAction.action.ReadValue<Vector3>(), rotationAction.action.ReadValue<Quaternion>());

            public bool IsValid()
            {
                if (positionAction == null && rotationAction == null)
                    return false;

                if (positionAction.action.controls.Count == 0 || rotationAction.action.controls.Count == 0)
                    return false;

                return true;
            }
        }
        internal static List<HumanBody> instances = new List<HumanBody>();

        [SerializeField] private BodyBuilder builder;
        [SerializeField] private BodyPoser poser;

        [Space(10)]
        public BodyMeasurements measurements;

        [Header("Input")]
        [SerializeField] private PoseInput eyesInput;
        [SerializeField] private PoseInput leftHandInput;
        [SerializeField] private PoseInput rightHandInput;


        public bool @override = false;
        public float rotation;
        public Pose eyesPose;
        public Pose leftHandPose;
        public Pose rightHandPose;

        public BodyBuilder Builder => builder;
        public BodyPoser Poser => poser;

        [SerializeField] private bool onlyPoseWhenRequired = true;

        private BodyTree<Matrix4x4> pose = new BodyTree<Matrix4x4>();
        public BodyTree<Matrix4x4> Pose => GetLatestPose();

        private int lastPoseFrame = -1;


        private void OnValidate()
        {
            Rebuild();
        }

        private void OnEnable()
        {
            Rebuild();
            
            if(!instances.Contains(this))
                instances.Add(this);
        }

        private void OnDisable()
        {
            instances.Remove(this);
        }

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            if (builder == null)
                return;

            builder.Build(ref pose, in measurements);
        }

        private BodyTree<Matrix4x4> GetLatestPose()
        {
            TryUpdate();

            return pose;
        }

        internal void Update_Internal()
        {
            TryUpdate();
        }

        private void TryUpdate()
        {
            bool shouldPose = !onlyPoseWhenRequired || (onlyPoseWhenRequired && lastPoseFrame != Time.frameCount);

            if (!shouldPose)
                return;


            lastPoseFrame = Time.frameCount;

            BodyPoser.Input input;

            Quaternion q = Quaternion.Euler(0, rotation, 0);
            if (@override)
                input = new BodyPoser.Input()
                {
                    eyes = new Pose(q * eyesPose.position, q * eyesPose.rotation),
                    leftHand = new Pose(q * leftHandPose.position, q * leftHandPose.rotation),
                    rightHand = new Pose(q * rightHandPose.position, q * rightHandPose.rotation)
                };
            else
                input = new BodyPoser.Input()
                {
                    eyes  = eyesInput.IsValid() ? eyesInput.Read() : new Pose(Vector3.zero, Quaternion.identity),
                    leftHand = leftHandInput.IsValid() ? leftHandInput.Read() : new Pose(Vector3.zero, Quaternion.identity),
                    rightHand = rightHandInput.IsValid() ? rightHandInput.Read() : new Pose(Vector3.zero, Quaternion.identity)
                };

            
            poser.Pose(in input, in measurements, ref pose);

            BodyTree<Matrix4x4> globalPose = pose.ToWorld();

            //Debug.Log(Vector3.Distance(globalPose[BodyNode.LeftUpperArm].GetPosition(), globalPose[BodyNode.LeftHand].GetPosition()));
        }
    }
}