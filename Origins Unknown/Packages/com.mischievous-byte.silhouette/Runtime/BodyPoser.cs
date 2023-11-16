using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MischievousByte.Silhouette
{
    public abstract class BodyPoser : ScriptableObject, IBodyConstraintsContainer
    {
        [System.Serializable]
        public struct Input
        {
            public Pose eyes;
            public Pose leftHand;
            public Pose rightHand;
        }

        public abstract BodyConstraints BodyConstraints { get; }

        public abstract void Pose(in Input input, in BodyMeasurements measurements, ref BodyTree<Matrix4x4> pose);
    }

}
