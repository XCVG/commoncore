using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Scripting
{
    /// <summary>
    /// Attach to a method to register it with the scripting system
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CCScriptAttribute : Attribute
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
    }

    /// <summary>
    /// The execution context a script runs in
    /// </summary>
    public struct ScriptExecutionContext
    {
        /// <summary>
        /// The object where the script was called from
        /// </summary>
        public object Caller;

        /// <summary>
        /// The GameObject, if any, that is responsible for activating the script
        /// </summary>
        public GameObject Activator;

        public override string ToString()
        {
            return string.Format("{0} : [Caller:{1}, Activator:{2}]", GetType().Name, Caller, Activator);
        }
    }
}