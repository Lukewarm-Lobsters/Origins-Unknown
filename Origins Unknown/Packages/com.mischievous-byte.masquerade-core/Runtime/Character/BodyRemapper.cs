using MischievousByte.Silhouette;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Character
{
    public abstract class BodyRemapper : ScriptableObject
    {
        public abstract void Remap(in BodyTree<Matrix4x4> input, ref BodyTree<Matrix4x4> output, float inputHeight, float outputHeight, in BodyConstraints constraints);
    }
}