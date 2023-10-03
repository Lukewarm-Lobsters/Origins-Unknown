using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Silhouette.Builtin
{
    public abstract class BodyPoserModule : ScriptableObject
    {
        public abstract void Pose(in BodyPoser.Input input, in BodyMeasurements measurements, in BodyConstraints constraints, ref BodyTree<Matrix4x4> pose);
    }
}
