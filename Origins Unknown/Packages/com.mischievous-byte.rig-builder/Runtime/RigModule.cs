using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.RigBuilder
{
    
    public abstract class RigModule : MonoBehaviour
    {
        private ModularRig rig;
        public ModularRig Rig => Application.isPlaying ? rig == null ? rig = GetRig() : rig : GetRig();
        private void OnTransformParentChanged() => GetRig();

        private ModularRig GetRig() => transform.parent?.GetComponent<ModularRig>() ?? null;

        //Start forces unity to draw the enable toggle for this component
        private void Start() { }
    }
}
