using MischievousByte.Silhouette;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

namespace MischievousByte.Masquerade
{
    public class BodyNodeBindings : MonoBehaviour
    {
        public BodyTree<Transform> tree;


        public void Apply(BodyTree<Matrix4x4> t)
        {
            foreach(BodyNode node in BodyNodeUtility.All)
            {
                if (tree[node] == null)
                    continue;

                tree[node].localPosition = t[node].GetPosition();
                //Debug.Log($"{node}: {t[node].GetPosition()}, {t[node].rotation.eulerAngles}");
                tree[node].localRotation = t[node].rotation;
            }
        }
    }
}
