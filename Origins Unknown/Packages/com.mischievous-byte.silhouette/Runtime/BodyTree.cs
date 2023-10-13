using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Silhouette
{
    [System.Serializable]
    public struct BodyTree<T>
    {
        [Header("Spine")]
        [SerializeField] private T pelvis;
        [SerializeField] private T l3;
        [SerializeField] private T t12;
        [SerializeField] private T t7;
        [SerializeField] private T c7;
        [SerializeField] private T head;
        [SerializeField] private T eyes;
        [SerializeField] private T headTop;

        [Header("Left Arm")]
        [SerializeField] private T leftClavicle;
        //[SerializeField] private T leftShoulder;
        [SerializeField] private T leftUpperArm;
        [SerializeField] private T leftForearm;
        [SerializeField] private T leftWrist;
        [SerializeField] private T leftHand;

        [Header("Right Arm")]
        [SerializeField] private T rightClavicle;
        //[SerializeField] private T rightShoulder;
        [SerializeField] private T rightUpperArm;
        [SerializeField] private T rightForearm;
        [SerializeField] private T rightWrist;
        [SerializeField] private T rightHand;

        [Header("Left Leg")]
        [SerializeField] private T leftUpperLeg;
        [SerializeField] private T leftLowerLeg;
        [SerializeField] private T leftFoot;
        [SerializeField] private T leftToes;

        [Header("Right Leg")]
        [SerializeField] private T rightUpperLeg;
        [SerializeField] private T rightLowerLeg;
        [SerializeField] private T rightFoot;
        [SerializeField] private T rightToes;

        public T this[BodyNode node]
        {
            get
            {
                switch(node)
                {
                    case BodyNode.Pelvis: return pelvis;
                    case BodyNode.L3: return l3;
                    case BodyNode.T12: return t12;
                    case BodyNode.T7: return t7;
                    case BodyNode.C7: return c7;
                    case BodyNode.Head: return head;
                    case BodyNode.Eyes: return eyes;
                    case BodyNode.HeadTop: return headTop;
                    case BodyNode.LeftClavicle: return leftClavicle;
                    //case BodyNode.LeftShoulder: return leftShoulder;
                    case BodyNode.LeftUpperArm: return leftUpperArm;
                    case BodyNode.LeftForearm: return leftForearm;
                    case BodyNode.LeftWrist: return leftWrist;
                    case BodyNode.LeftHand: return leftHand;
                    case BodyNode.RightClavicle: return rightClavicle;
                    //case BodyNode.RightShoulder: return rightShoulder;
                    case BodyNode.RightUpperArm: return rightUpperArm;
                    case BodyNode.RightForearm: return rightForearm;
                    case BodyNode.RightWrist: return rightWrist;
                    case BodyNode.RightHand: return rightHand;
                    case BodyNode.LeftUpperLeg: return leftUpperLeg;
                    case BodyNode.LeftLowerLeg: return leftLowerLeg;
                    case BodyNode.LeftFoot: return leftFoot;
                    case BodyNode.LeftToes: return leftToes;
                    case BodyNode.RightUpperLeg: return rightUpperLeg;
                    case BodyNode.RightLowerLeg: return rightLowerLeg;
                    case BodyNode.RightFoot: return rightFoot;
                    case BodyNode.RightToes: return rightToes;
                }

                throw new System.ArgumentOutOfRangeException();
            }
            set
            {
                switch (node)
                {
                    case BodyNode.Pelvis: pelvis = value; return;
                    case BodyNode.L3: l3 = value; return;
                    case BodyNode.T12: t12 = value; return;
                    case BodyNode.T7: t7 = value; return;
                    case BodyNode.C7: c7 = value; return;
                    case BodyNode.Head: head = value; return;
                    case BodyNode.Eyes: eyes = value; return;
                    case BodyNode.HeadTop: headTop = value; return;
                    case BodyNode.LeftClavicle: leftClavicle = value; return;
                    //case BodyNode.LeftShoulder: leftShoulder = value; return;
                    case BodyNode.LeftUpperArm: leftUpperArm = value; return;
                    case BodyNode.LeftForearm: leftForearm = value; return;
                    case BodyNode.LeftWrist: leftWrist = value; return;
                    case BodyNode.LeftHand: leftHand = value; return;
                    case BodyNode.RightClavicle: rightClavicle = value; return;
                    //case BodyNode.RightShoulder: rightShoulder = value; return;
                    case BodyNode.RightUpperArm: rightUpperArm = value; return;
                    case BodyNode.RightForearm: rightForearm = value; return;
                    case BodyNode.RightWrist: rightWrist = value; return;
                    case BodyNode.RightHand: rightHand = value; return;
                    case BodyNode.LeftUpperLeg: leftUpperLeg = value; return;
                    case BodyNode.LeftLowerLeg: leftLowerLeg = value; return;
                    case BodyNode.LeftFoot: leftFoot = value; return;
                    case BodyNode.LeftToes: leftToes = value; return;
                    case BodyNode.RightUpperLeg: rightUpperLeg = value; return;
                    case BodyNode.RightLowerLeg: rightLowerLeg = value; return;
                    case BodyNode.RightFoot: rightFoot = value; return;
                    case BodyNode.RightToes: rightToes = value; return;
                }

                throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
