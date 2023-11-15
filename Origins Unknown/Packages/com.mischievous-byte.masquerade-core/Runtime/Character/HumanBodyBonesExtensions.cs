using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade
{
    public static class HumanBodyBonesExtensions
    {
        public static HumanBodyBones GetParent(this HumanBodyBones bone)
        {
            switch (bone)
            {
                case HumanBodyBones.Hips: return HumanBodyBones.LastBone;
                case HumanBodyBones.Spine: return HumanBodyBones.Hips;
                case HumanBodyBones.Chest: return HumanBodyBones.Spine;
                case HumanBodyBones.UpperChest: return HumanBodyBones.Chest;
                case HumanBodyBones.Neck: return HumanBodyBones.UpperChest;
                case HumanBodyBones.Head: return HumanBodyBones.Neck;
                case HumanBodyBones.Jaw: return HumanBodyBones.Head;
                case HumanBodyBones.LeftEye: return HumanBodyBones.Head;
                case HumanBodyBones.RightEye: return HumanBodyBones.Head;

                case HumanBodyBones.LeftShoulder: return HumanBodyBones.UpperChest;
                case HumanBodyBones.LeftUpperArm: return HumanBodyBones.LeftShoulder;
                case HumanBodyBones.LeftLowerArm: return HumanBodyBones.LeftUpperArm;
                case HumanBodyBones.LeftHand: return HumanBodyBones.LeftLowerArm;

                case HumanBodyBones.LeftThumbProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftThumbIntermediate: return HumanBodyBones.LeftThumbProximal;
                case HumanBodyBones.LeftThumbDistal: return HumanBodyBones.LeftThumbIntermediate;

                case HumanBodyBones.LeftIndexProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftIndexIntermediate: return HumanBodyBones.LeftIndexProximal;
                case HumanBodyBones.LeftIndexDistal: return HumanBodyBones.LeftIndexIntermediate;

                case HumanBodyBones.LeftMiddleProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftMiddleIntermediate: return HumanBodyBones.LeftMiddleProximal;
                case HumanBodyBones.LeftMiddleDistal: return HumanBodyBones.LeftMiddleIntermediate;

                case HumanBodyBones.LeftRingProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftRingIntermediate: return HumanBodyBones.LeftRingProximal;
                case HumanBodyBones.LeftRingDistal: return HumanBodyBones.LeftRingIntermediate;

                case HumanBodyBones.LeftLittleProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftLittleIntermediate: return HumanBodyBones.LeftLittleProximal;
                case HumanBodyBones.LeftLittleDistal: return HumanBodyBones.LeftLittleIntermediate;

                case HumanBodyBones.RightShoulder: return HumanBodyBones.UpperChest;
                case HumanBodyBones.RightUpperArm: return HumanBodyBones.RightShoulder;
                case HumanBodyBones.RightLowerArm: return HumanBodyBones.RightUpperArm;
                case HumanBodyBones.RightHand: return HumanBodyBones.RightLowerArm;

                case HumanBodyBones.RightThumbProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightThumbIntermediate: return HumanBodyBones.RightThumbProximal;
                case HumanBodyBones.RightThumbDistal: return HumanBodyBones.RightThumbIntermediate;

                case HumanBodyBones.RightIndexProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightIndexIntermediate: return HumanBodyBones.RightIndexProximal;
                case HumanBodyBones.RightIndexDistal: return HumanBodyBones.RightIndexIntermediate;

                case HumanBodyBones.RightMiddleProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightMiddleIntermediate: return HumanBodyBones.RightMiddleProximal;
                case HumanBodyBones.RightMiddleDistal: return HumanBodyBones.RightMiddleIntermediate;

                case HumanBodyBones.RightRingProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightRingIntermediate: return HumanBodyBones.RightRingProximal;
                case HumanBodyBones.RightRingDistal: return HumanBodyBones.RightRingIntermediate;

                case HumanBodyBones.RightLittleProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightLittleIntermediate: return HumanBodyBones.RightLittleProximal;
                case HumanBodyBones.RightLittleDistal: return HumanBodyBones.RightLittleIntermediate;

                case HumanBodyBones.LeftUpperLeg: return HumanBodyBones.Hips;
                case HumanBodyBones.LeftLowerLeg: return HumanBodyBones.LeftUpperLeg;
                case HumanBodyBones.LeftFoot: return HumanBodyBones.LeftLowerLeg;
                case HumanBodyBones.LeftToes: return HumanBodyBones.LeftFoot;

                case HumanBodyBones.RightUpperLeg: return HumanBodyBones.Hips;
                case HumanBodyBones.RightLowerLeg: return HumanBodyBones.RightUpperLeg;
                case HumanBodyBones.RightFoot: return HumanBodyBones.RightLowerLeg;
                case HumanBodyBones.RightToes: return HumanBodyBones.RightFoot;

                default: return HumanBodyBones.LastBone;

            }
        }
    }
}
