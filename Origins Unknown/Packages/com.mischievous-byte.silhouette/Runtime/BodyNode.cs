using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MischievousByte.Silhouette
{
    public enum BodyNode : byte
    {
        /// <summary>
        /// The base of the spine (comparable to <see cref="HumanBodyBones.Hips"/>
        /// </summary>
        [NodeProperties(HumanBodyBones.Hips)] Pelvis,
        /// <summary>
        /// The 3rd lumbar vertebrae of the spine (comparable to <see cref="HumanBodyBones.Spine"/>
        /// </summary>
        [NodeProperties(HumanBodyBones.Spine, Pelvis)] L3,
        /// <summary>
        /// The 12th thoracic vertebrae of the spine (comparable to <see cref="HumanBodyBones.Chest"/>
        /// </summary>
        [NodeProperties(HumanBodyBones.Chest, L3)] T12,
        /// <summary>
        /// The 7th thoracic vertebrae of the spine (comparable to <see cref="HumanBodyBones.UpperChest"/>
        /// </summary>
        [NodeProperties(HumanBodyBones.UpperChest, T12)] T7,
        /// <summary>
        /// The 7th cervical vertebrae of the spine (comparable to <see cref="HumanBodyBones.Neck" />)
        /// </summary>
        [NodeProperties(HumanBodyBones.Neck, T7)] C7,
        [NodeProperties(HumanBodyBones.Head, C7)] Head,

        [NodeProperties(Head)] Eyes,
        [NodeProperties(Head)] HeadTop,
        [NodeProperties(HumanBodyBones.LeftShoulder, T7)] LeftClavicle,
        //[NodeProperties(LeftClavicle)] LeftShoulder,
        //[NodeProperties(HumanBodyBones.LeftUpperArm, LeftShoulder)] LeftUpperArm,
        [NodeProperties(HumanBodyBones.LeftUpperArm, LeftClavicle)] LeftUpperArm,
        [NodeProperties(HumanBodyBones.LeftLowerArm, LeftUpperArm)] LeftForearm,
        [NodeProperties(HumanBodyBones.LeftHand, LeftForearm)] LeftWrist,
        [NodeProperties(LeftWrist)] LeftHand,

        [NodeProperties(HumanBodyBones.RightShoulder, T7)] RightClavicle,
        //[NodeProperties(RightClavicle)] RightShoulder,
        //[NodeProperties(HumanBodyBones.RightUpperArm, RightShoulder)] RightUpperArm,
        [NodeProperties(HumanBodyBones.RightUpperArm, RightClavicle)] RightUpperArm,
        [NodeProperties(HumanBodyBones.RightLowerArm, RightUpperArm)] RightForearm,
        [NodeProperties(HumanBodyBones.RightHand, RightForearm)] RightWrist,
        [NodeProperties(RightWrist)] RightHand,

        [NodeProperties(HumanBodyBones.LeftUpperLeg, Pelvis)] LeftUpperLeg,
        [NodeProperties(HumanBodyBones.LeftLowerLeg, LeftUpperLeg)] LeftLowerLeg,
        [NodeProperties(HumanBodyBones.LeftFoot, LeftLowerLeg)] LeftFoot,
        [NodeProperties(HumanBodyBones.LeftToes, LeftFoot)] LeftToes,
        [NodeProperties(HumanBodyBones.RightUpperLeg, Pelvis)] RightUpperLeg,
        [NodeProperties(HumanBodyBones.RightLowerLeg, RightUpperLeg)] RightLowerLeg,
        [NodeProperties(HumanBodyBones.RightFoot, RightLowerLeg)] RightFoot,
        [NodeProperties(HumanBodyBones.RightToes, RightFoot)] RightToes,
        [NodeProperties(HumanBodyBones.LastBone)] Invalid
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class NodePropertiesAttribute : Attribute
    {
        public readonly HumanBodyBones Reference = HumanBodyBones.LastBone;
        public readonly BodyNode Parent = BodyNode.Invalid;

        public NodePropertiesAttribute(HumanBodyBones reference)
        {
            Reference = reference;
        }

        public NodePropertiesAttribute(BodyNode parent)
        {
            Parent = parent;
        }

        public NodePropertiesAttribute(HumanBodyBones reference, BodyNode parent)
        {
            Reference = reference;
            Parent = parent;
        }
    }

    public static class BodyNodeUtility
    {
        private struct NodeProperties
        {
            public HumanBodyBones reference;
            public BodyNode parent;
        }

        private static Dictionary<BodyNode, NodeProperties> data = new();

        public static readonly IReadOnlyCollection<BodyNode> All = new ReadOnlyCollection<BodyNode>(Enum.GetValues(typeof(BodyNode)).Cast<BodyNode>().Where(x => x != BodyNode.Invalid).ToArray());
        static BodyNodeUtility()
        {
            foreach(var node in Enum.GetValues(typeof(BodyNode)).Cast<BodyNode>())
            {
                string name = Enum.GetName(typeof(BodyNode), node);

                var attr = typeof(BodyNode).GetMember(name).First().GetCustomAttribute<NodePropertiesAttribute>();
                byte id = (byte)Array.IndexOf(All.ToArray(), node);

                NodeProperties properties = new NodeProperties()
                {
                    reference = attr?.Reference ?? HumanBodyBones.LastBone,
                    parent = attr?.Parent ?? BodyNode.Invalid
                };


                data.Add(node, properties);
            }
        }

        public static bool IsHumanoid(this BodyNode node) => data[node].reference != HumanBodyBones.LastBone;
        public static HumanBodyBones ToHumanoid(this BodyNode node) => data[node].reference;

        public static bool HasParent(this BodyNode node) => data[node].parent != BodyNode.Invalid;
        public static BodyNode GetParent(this BodyNode node) => data[node].parent;

        public static BodyNode[] GetParentChain(this BodyNode node)
        {
            if (node == BodyNode.Invalid)
                throw new ArgumentException();

            List<BodyNode> r = new List<BodyNode>();

            while ((node = node.GetParent()) != BodyNode.Invalid)
            {
                r.Add(node);
            }

            return r.ToArray();
        }
    }
}
