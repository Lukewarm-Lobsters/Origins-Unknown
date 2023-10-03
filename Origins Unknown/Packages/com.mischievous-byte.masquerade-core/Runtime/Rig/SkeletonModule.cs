using MischievousByte.Masquerade.Character;
using MischievousByte.RigBuilder;
using MischievousByte.Silhouette;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Rig
{
    public class SkeletonModule : MasqueradeModule
    {
        [SerializeField] private BodyRemapper remapper;
        [SerializeField] private HumanBody body;

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
        }

        private void Execute()
        {
            BodyTree<Matrix4x4> input = body.Pose;
            remapper.Remap(in input, ref tree, body.measurements.Height, AvatarContainer.Avatar.Height, body.Poser.BodyConstraints);
        }
    }
}
