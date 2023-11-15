using MischievousByte.Masquerade.Character;
using MischievousByte.RigBuilder;
using MischievousByte.Silhouette;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Rig
{
    [AddComponentMenu(Core_PackageInfo.ComponentMenuPrefix + "Skeleton Module")]
    public class SkeletonModule : MasqueradeModule
    {
        [SerializeField] private HumanBody body;
        [SerializeField] private BodyRemapper remapper;
        [SerializeField] private BodyNode center = BodyNode.T12;

        [Space(10)]
        [SerializeField] private BodyNodeBindings bindings;
        
        protected override void OnContextChanged(ModularRig.Context context)
        {
            base.OnContextChanged(context);

            context.onUpdate.Bind(Execute);
        }


        private void OnAvatarChanged(AvatarChangeContext context)
        {
            tree = BodyTreeExtensions.From(context.avatar.Animator, context.avatar.EyeOverride);

            Matrix4x4 worldHead = tree[BodyNode.Pelvis] * tree[BodyNode.L3] * tree[BodyNode.T12] * tree[BodyNode.T7] * tree[BodyNode.C7] * tree[BodyNode.Head];

            Matrix4x4 worldTop = Matrix4x4.TRS(
                new Vector3(worldHead.GetPosition().x, context.avatar.Height, worldHead.GetPosition().z),
                worldHead.rotation, Vector3.one);

            tree[BodyNode.HeadTop] = worldHead.inverse * worldTop;

            bindings?.Apply(tree);
        }

        private bool cycledOnce;
        private Vector3 previousCenterPosition;
        public Vector3 Velocity { get; private set; }

        private void Execute()
        {
            BodyTree<Matrix4x4> input = body.Pose;

            remapper.Remap(in input, ref tree, body.measurements.Height, AvatarContainer.Avatar.Height, body.Poser.BodyConstraints);

            var worldTree = tree.ToWorld();
            Vector3 centerPosition = worldTree[center].GetPosition();
            
            if (cycledOnce)
            {
                Vector3 delta = Vector3.ProjectOnPlane(centerPosition - previousCenterPosition, Vector3.up);
                Velocity = delta / Time.deltaTime;
            }

            tree[BodyNode.Pelvis] = Matrix4x4.TRS(
                tree[BodyNode.Pelvis].GetPosition() - Vector3.ProjectOnPlane(centerPosition, Vector3.up),
                tree[BodyNode.Pelvis].rotation,
                Vector3.one);

            previousCenterPosition = centerPosition;
            bindings?.Apply(tree);
            cycledOnce = true;
        }
    }
}
