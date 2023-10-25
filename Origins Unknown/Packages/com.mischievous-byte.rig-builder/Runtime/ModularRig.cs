using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace MischievousByte.RigBuilder
{
    [AddComponentMenu(PackageInfo.ComponentMenuPrefix + "Modular Rig")]
    public class ModularRig : MonoBehaviour
    {
        public class Context
        {
            public readonly Event onUpdate = new Event();
        }

        private Context context;
        private IEnumerable<RigModule> modules;

        public IEnumerable<RigModule> Modules => Application.isPlaying ? modules == null ? Invalidate() : modules : GetChildModules();


        private void Awake()
        {
            if (modules == null)
                Invalidate();
        }
        private void OnTransformChildrenChanged() => Invalidate();


        public IEnumerable<RigModule> Invalidate()
        {
            if (!Application.isPlaying)
                return new RigModule[] { };

            modules = GetChildModules();

            context = new Context();

            foreach (var module in modules)
                module.SendMessage("OnContextChanged", context, SendMessageOptions.DontRequireReceiver);

            return modules;
        }


        private IEnumerable<RigModule> GetChildModules() => transform.Cast<Transform>().ToList().Select(t => t.GetComponent<RigModule>()).Where(m => m != null).ToList().AsReadOnly();
        public T GetModule<T>() => Modules.Where(m => m is T).Cast<T>().FirstOrDefault();
        public IEnumerable<T> GetModules<T>() => Modules.Where(m => m is T).Cast<T>();


        private void Update() => context.onUpdate.Invoke(modules);
    }
}
