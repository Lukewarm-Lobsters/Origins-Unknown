using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Silhouette.Builtin
{
    [CreateAssetMenu]
    public class ModularBodyPoser : BodyPoser
    {
        [SerializeField] private BodyConstraints bodyConstraints;
        [SerializeField] private BodyPoserModule[] modules;


        public override BodyConstraints BodyConstraints => bodyConstraints;


        public override void Pose(in Input input, in BodyMeasurements measurements, ref BodyTree<Matrix4x4> pose)
        {
            foreach (var module in modules)
                module.Pose(in input, in measurements, in bodyConstraints, ref pose);
        }
    }
}
