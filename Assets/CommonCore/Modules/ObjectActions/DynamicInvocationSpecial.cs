using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{
    public class DynamicInvocationSpecial : ActionSpecial
    {
        public bool TryThis;
        public bool TryParent;
        public bool TryChildren;
        public string TryTarget;

        public string ScriptName;
        public string MethodName;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked)
                return;

            MonoBehaviour targetScript = FindScript();
            targetScript.Invoke(MethodName, 0);

            if (!Repeatable)
                Locked = true;
        }

        private MonoBehaviour FindScript()
        {
            Component scriptBehaviour = null;

            if (TryThis)
            {
                scriptBehaviour = GetComponent(ScriptName);
            }

            if (scriptBehaviour != null && scriptBehaviour is MonoBehaviour)
                return (MonoBehaviour)scriptBehaviour;

            if (TryParent && transform.parent != null)
            {
                scriptBehaviour = transform.parent.GetComponent(ScriptName);
            }

            if (scriptBehaviour != null && scriptBehaviour is MonoBehaviour)
                return (MonoBehaviour)scriptBehaviour;

            if(TryChildren)
            {
                foreach(Transform t in transform)
                {
                    scriptBehaviour = t.GetComponent(ScriptName);
                    if(scriptBehaviour != null && scriptBehaviour is MonoBehaviour)
                        break;
                }
            }

            if (scriptBehaviour != null && scriptBehaviour is MonoBehaviour)
                return (MonoBehaviour)scriptBehaviour;

            if(!string.IsNullOrEmpty(TryTarget))
            {
                var go = GameObject.Find(TryTarget);
                if(go != null)
                {
                    scriptBehaviour = go.GetComponent(ScriptName);
                }
            }

            if (scriptBehaviour != null && scriptBehaviour is MonoBehaviour)
                return (MonoBehaviour)scriptBehaviour;

            return null;

        }

    }
}