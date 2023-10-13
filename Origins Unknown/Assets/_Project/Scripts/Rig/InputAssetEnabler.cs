using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LukewarmLobsters.OriginsUnknown
{
    public class InputAssetEnabler : MonoBehaviour
    {
        public InputActionAsset asset;


        private void OnEnable()
        {
            asset.Enable();
        }

        private void OnDisable()
        {
            asset.Disable();
        }
    }
}

