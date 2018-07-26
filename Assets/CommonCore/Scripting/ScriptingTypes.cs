using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Scripting
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CCScriptAttribute : Attribute
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
    }

    public struct ScriptExecutionContext
    {
        public object Caller;
        public GameObject Activator;

        public override string ToString()
        {
            return string.Format("{0} : [Caller:{1}, Activator:{2}]", GetType().Name, Caller, Activator);
        }
    }
}