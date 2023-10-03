using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Silhouette
{
    [System.Serializable]
    public struct BodyConstraints
    {
        [SerializeField] private float spineBend;
        [SerializeField] private float maxShoulderYRotation;
        [SerializeField] private float maxShoulderZRotation;

        public float SpineBend => spineBend;
        public float MaxShoulderYRotation => maxShoulderYRotation;
        public float MaxShoulderZRotation => maxShoulderZRotation;
    }


    public interface IBodyConstraintsContainer
    {
        public BodyConstraints BodyConstraints { get; }
    }

}
