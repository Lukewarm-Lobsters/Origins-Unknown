using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace LukewarmLobsters.OriginsUnknown
{
    [ExecuteAlways]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(SplineExtrude))]
    public class TentacleController : MonoBehaviour
    {
        private SplineContainer splineContainer;
        private SplineExtrude splineExtrude;
        // Start is called before the first frame update

        [SerializeField] private float curveFactor = 1f;

        [SerializeField] private Transform target;

        void Awake()
        {
            splineContainer = GetComponent<SplineContainer>();
            splineExtrude = GetComponent<SplineExtrude>();
        }

        private void LateUpdate()
        {
            HandleTarget();
            UpdateSpline();
            UpdateMaterial();
        }

        private void HandleTarget()
        {

        }

        private void UpdateSpline()
        {
            if (target == null)
                return;

            if (splineContainer.Spline == null)
                splineContainer.AddSpline();

            splineContainer.Spline.Clear();

            var start = new BezierKnot();
            var end = new BezierKnot();


            Vector3 localTarget = transform.InverseTransformPoint(target.position);
            Vector3 localNormal = transform.InverseTransformVector(target.forward.normalized);

            float distance = localTarget.magnitude;

            float curve = Mathf.Sqrt(curveFactor * distance);
            start.Position = Vector3.zero;
            start.Rotation = Quaternion.identity;
            start.TangentIn = Vector3.back * curve;
            start.TangentOut = Vector3.forward * curve;

            end.Position = localTarget;
            end.Rotation = Quaternion.identity;
            end.TangentIn = localNormal * curve;
            end.TangentOut = localNormal * curve;

            splineContainer.Spline.Add(start);
            splineContainer.Spline.Add(end);

            splineExtrude.Rebuild();
        }

        private void UpdateMaterial()
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetFloat("_Length", splineContainer.Spline.GetLength());
            propertyBlock.SetFloat("_Circumference", splineExtrude.Radius * Mathf.PI * 2f);

            GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);


        }
    }
}
