using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LukewarmLobsters.OriginsUnknown
{
    [RequireComponent(typeof(Camera))]
    public class PreventClipping : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Color color;
        [SerializeField] private float offset;
        [SerializeField] private float radius;
        [SerializeField] private GameObject player;
        private new Camera camera;

        private void Awake()
        {
            camera = GetComponent<Camera>();
        }

        private void Update()
        {
            Color c = color;
            c.a = Mathf.Lerp(image.color.a, IsClipping() ? 1.0f : 0.0f, Time.deltaTime * 30);
            image.color = c;
        }


        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Vector3.forward * offset, radius);
        }
        private bool IsClipping()
        {
            /*Plane plane = GeometryUtility.CalculateFrustumPlanes(camera)[0];


            Vector3 position = plane.ClosestPointOnPlane(transform.position);
            Quaternion rotation = transform.rotation;

            Vector3 a = transform.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane)));
            Vector3 b = transform.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane)));

            Vector3 size = b - a;

            size.z = camera.nearClipPlane;

            Physics.OverlapBox(Vector3.Distance)*/


            foreach(var collider in Physics.OverlapSphere(transform.position + transform.forward * offset, radius))
            {
                if (collider.gameObject != player)
                    return true;
            }

            return false;
        }

    }
}