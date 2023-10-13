using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Silhouette
{

    public abstract class BodyBuilder : ScriptableObject
    {
        public abstract void Build(ref BodyTree<Matrix4x4> pose, in BodyMeasurements measurements);
    }
}
