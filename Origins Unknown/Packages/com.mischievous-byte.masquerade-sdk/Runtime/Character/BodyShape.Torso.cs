using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Character
{
    public partial class BodyShape
    {
        [System.Serializable]
        public struct Torso
        {
            public SoftEllipse hipsEllipse;
            public SoftEllipse waistEllipse;
            public float waistY;
            public SoftEllipse chestEllipse;
            public float chestY;
            public SoftEllipse sternumEllipse;
            public SoftEllipse neckEllipse;
            public SoftEllipse neckEndEllipse;
        }



        public Torso torso;
    }
}
