using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Silhouette
{
    [AddComponentMenu(PackageInfo.ComponentMenuPrefix + "Body Renderer")]
    public class BodyRenderer : MonoBehaviour
    {
        public HumanBody body;


        private void OnDrawGizmos()
        {
            if (body == null)
                return;

            Matrix4x4 mat = Gizmos.matrix;

            Gizmos.matrix = transform.localToWorldMatrix;


            Gizmos.color = Color.blue;

            BodyTree<Matrix4x4> p = body.Pose;

            foreach (var node in BodyNodeUtility.All.Where(x => x != BodyNode.Pelvis))
                p[node] = p[node.GetParent()] * p[node];

            Gizmos.DrawLine(p[BodyNode.Pelvis].GetPosition(), p[BodyNode.L3].GetPosition());
            Gizmos.DrawLine(p[BodyNode.L3].GetPosition(), p[BodyNode.T12].GetPosition());
            Gizmos.DrawLine(p[BodyNode.T12].GetPosition(), p[BodyNode.T7].GetPosition());
            Gizmos.DrawLine(p[BodyNode.T7].GetPosition(), p[BodyNode.C7].GetPosition());

            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(p[BodyNode.C7].GetPosition(), p[BodyNode.Head].GetPosition());
            Gizmos.DrawLine(p[BodyNode.Head].GetPosition(), p[BodyNode.Eyes].GetPosition());
            Gizmos.DrawLine(p[BodyNode.Head].GetPosition(), p[BodyNode.HeadTop].GetPosition());

            Gizmos.color = Color.green;

            //Gizmos.DrawLine(p[BodyNode.LeftClavicle].GetPosition(), p[BodyNode.LeftShoulder].GetPosition());
            Gizmos.DrawLine(p[BodyNode.LeftClavicle].GetPosition(), p[BodyNode.LeftUpperArm].GetPosition());
            Gizmos.DrawLine(p[BodyNode.LeftUpperArm].GetPosition(), p[BodyNode.LeftForearm].GetPosition());
            Gizmos.DrawLine(p[BodyNode.LeftForearm].GetPosition(), p[BodyNode.LeftWrist].GetPosition());
            Gizmos.DrawLine(p[BodyNode.LeftWrist].GetPosition(), p[BodyNode.LeftHand].GetPosition());

            //Gizmos.DrawLine(p[BodyNode.RightClavicle].GetPosition(), p[BodyNode.RightShoulder].GetPosition());
            Gizmos.DrawLine(p[BodyNode.RightClavicle].GetPosition(), p[BodyNode.RightUpperArm].GetPosition());
            Gizmos.DrawLine(p[BodyNode.RightUpperArm].GetPosition(), p[BodyNode.RightForearm].GetPosition());
            Gizmos.DrawLine(p[BodyNode.RightForearm].GetPosition(), p[BodyNode.RightWrist].GetPosition());
            Gizmos.DrawLine(p[BodyNode.RightWrist].GetPosition(), p[BodyNode.RightHand].GetPosition());

            Gizmos.matrix = mat;
        }
    }
}
