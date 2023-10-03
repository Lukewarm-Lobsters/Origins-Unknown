using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MischievousByte.Silhouette
{

    public static class SilhouetteManager 
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod()]
#endif
        private static void OnLoad() { }


        static SilhouetteManager()
        {
            Initialize();
        }

        private static void Initialize()
        {
            InputSystem.onAfterUpdate += Update;

            Application.quitting += OnDestroy;
        }

        private static void OnDestroy()
        {
            Application.quitting -= OnDestroy;
            InputSystem.onAfterUpdate -= Update;
        }

        private static void Update()
        {
            foreach (HumanBody body in HumanBody.instances)
                body.Update_Internal();
        }
    }
}
