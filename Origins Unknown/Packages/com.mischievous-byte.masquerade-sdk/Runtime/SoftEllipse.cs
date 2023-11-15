using Codice.CM.Common.Tree.Partial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade
{
    [System.Serializable]
    public struct SoftEllipse : ISerializationCallbackReceiver
    {
        [SerializeField] private float xPositive;
        [SerializeField] private float xNegative;
        [SerializeField] private float yPositive;
        [SerializeField] private float yNegative;

        public float XPositive { get { return xPositive; } set { xPositive = Mathf.Max(xPositive, 0); } }
        public float XNegative { get { return xNegative; } set { xNegative = Mathf.Max(xNegative, 0); } }
        public float YPositive { get { return yPositive; } set { yPositive = Mathf.Max(yPositive, 0); } }
        public float YNegative { get { return yNegative; } set {  yNegative = Mathf.Max(yNegative, 0); } }

        public SoftEllipse(float xPositive, float xNegative, float yPositive, float yNegative)
        {
            this.xPositive = xPositive;
            this.xNegative = xNegative;
            this.yPositive = yPositive;
            this.yNegative = yNegative;
        }

        public Vector2 Sample(float t)
        {
            Vector2 center = (new Vector2(xPositive, yPositive) - new Vector2(xNegative, yNegative)) / 2f;

            float angle = t * Mathf.PI * 2;

            float xRadius = (xPositive + xNegative) / 2f;
            float yRadius = (yPositive + yNegative) / 2f;

            return center + new Vector2(Mathf.Sin(angle) * xRadius, Mathf.Cos(angle) * yRadius);
        }

        public void OnBeforeSerialize()
        {
            Fix();
        }

        public void OnAfterDeserialize()
        {
            Fix();
        }

        private void Fix()
        {
            xPositive = Mathf.Max(xPositive, 0);
            xNegative = Mathf.Max(xNegative, 0);
            yPositive = Mathf.Max(yPositive, 0);
            yNegative = Mathf.Max(yNegative, 0);
        }
    }
}
