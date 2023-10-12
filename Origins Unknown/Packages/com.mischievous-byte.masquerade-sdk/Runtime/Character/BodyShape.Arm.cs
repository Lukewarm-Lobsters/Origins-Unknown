using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Character
{
    public partial class BodyShape
    {
        [System.Serializable]
        public struct Arm
        {
            public float upperArmOffset;
            public SoftEllipse upperArmEllipse;
            public SoftEllipse forearmEllipse;
            public SoftEllipse wristEllipse;
        }

        public Arm arm;
    }
}
