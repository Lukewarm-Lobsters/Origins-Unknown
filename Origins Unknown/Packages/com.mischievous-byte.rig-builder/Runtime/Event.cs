using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.RigBuilder
{
    public class Event
    {
        private struct Callback
        {
            public Action action;
            public RigModule module;
        }

        private List<Callback> callbacks = new List<Callback>();

        
        public void Bind(Action callback)
        {
            Bind(callback, callback.Target is RigModule ? callback.Target as RigModule : null);
        }

        public void Bind(Action callback, RigModule module)
        {
            callbacks.Add(new Callback { action = callback, module = module });
        }

        public void Unbind(Action callback)
        {
            if(callbacks.Any(c => c.action == callback))
                callbacks.Remove(callbacks.Where(c => c.action == callback).First());
        }

        internal void Invoke(IEnumerable<RigModule> modules)
        {
            List<RigModule> moduleList = modules.ToList();

            foreach (var callback in callbacks.Where(c => c.module?.isActiveAndEnabled ?? true).OrderBy(
                c => modules.Contains(c.module)
                ? moduleList.IndexOf(c.module)
                : moduleList.Count
            ))
            {
                try
                {
                    callback.action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
                
                
        }
    }
}
