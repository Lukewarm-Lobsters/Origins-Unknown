using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Silhouette
{
    [System.Serializable]
    public struct BodyMeasurements
    {
        public const float MinHeight = 1f;
        public const float MaxHeight = 3f;
        public const float MinWingspan = 1f;
        public const float MaxWingspan = 3f;


        [SerializeField, Range(MinHeight, MaxHeight)]
        private float height;
        public float Height { get { return height = Mathf.Clamp(height, MinHeight, MaxHeight); } set { height = Mathf.Clamp(value, MinHeight, MaxHeight); } }

        [SerializeField, Range(MinHeight, MaxHeight)]
        private float wingspan;
        public float Wingspan { get { return wingspan = Mathf.Clamp(wingspan, MinWingspan, MaxWingspan); } set { wingspan = Mathf.Clamp(value, MinWingspan, MaxWingspan); } }

    }
}
