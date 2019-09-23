using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CommonCore.Scripting
{

    /// <summary>
    /// Provides services for executing arbitrary methods as standalone scripts
    /// </summary>
    public class ScriptingModule : CCModule
    {
        private static ScriptingModule Instance;

        private Dictionary<string, MethodInfo> CallableMethods;

        public ScriptingModule()
        {
            Instance = this;

            FindAllScripts();
        }

        private void FindAllScripts()
        {
            Assembly[] assems = AppDomain.CurrentDomain.GetAssemblies();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var methods = CCBase.BaseGameTypes
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    .Where(m => !m.IsAbstract)
                    .Where(m => m.GetCustomAttributes(typeof(CCScriptAttribute), false).Length > 0)
                    .ToArray();

            sw.Stop();

            CallableMethods = new Dictionary<string, MethodInfo>();

            foreach (var m in methods)
            {
                try
                {
                    RegisterScript(m);
                }
                catch (Exception e)
                {
                    LogError("Failed to register method " + m.Name);
                    LogException(e);
                }

            }

            Log(string.Format("Loaded {1} scripts in {0:f2} ms", sw.ElapsedMilliseconds, CallableMethods.Count));

        }

        private void RegisterScript(MethodInfo scriptMethod)
        {
            string className = scriptMethod.DeclaringType.Name;
            string methodName = scriptMethod.Name;

            var scriptAttribute = (CCScriptAttribute)scriptMethod.GetCustomAttributes(typeof(CCScriptAttribute), false)[0];

            if (scriptAttribute != null)
            {
                if (!string.IsNullOrEmpty(scriptAttribute.Name))
                    methodName = scriptAttribute.Name;

                if (!string.IsNullOrEmpty(scriptAttribute.ClassName))
                    className = scriptAttribute.ClassName;
            }

            string callableName = className + "." + methodName;

            //Debug.Log(string.Format("Loaded script: {0}", callableName));

            if (CallableMethods.ContainsKey(callableName))
            {
                LogWarning(string.Format("Multiple scripts with name: {0}", callableName));
            }

            CallableMethods.Add(callableName, scriptMethod);
        }

        private object CallScript(string script, object instance, ScriptExecutionContext context, params object[] args)
        {
            if (!CallableMethods.ContainsKey(script))
                throw new ArgumentException("Script not found!", nameof(script));

            MethodInfo m = CallableMethods[script];

            if(!m.IsStatic && instance == null)
            {
                instance = Activator.CreateInstance(m.DeclaringType);
            }

            List<object> allArgs = new List<object>(args.Length + 1);
            allArgs.Add(context);
            allArgs.AddRange(args);

            object[] trimmedArgs = allArgs.GetRange(0, m.GetParameters().Length).ToArray();

            return m.Invoke(instance, trimmedArgs);
        }

        /// <summary>
        /// Calls a script through the scripting system
        /// </summary>
        public static void Call(string script, ScriptExecutionContext context, params object[] args)
        {
            Instance.CallScript(script, null, context, args);
        }

        /// <summary>
        /// Calls a script through the scripting system, returning a result
        /// </summary>
        public static object CallForResult(string script, ScriptExecutionContext context, params object[] args)
        {
            return Instance.CallScript(script, null, context, args);
        }

        /// <summary>
        /// Checks if a script exists
        /// </summary>
        public static bool CheckScript(string script)
        {
            return Instance?.CallableMethods?.ContainsKey(script) ?? false;
        }

        /// <summary>
        /// Registers a method with the scripting system to be called as a script
        /// </summary>
        public static void Register(MethodInfo scriptMethod)
        {
            Instance.RegisterScript(scriptMethod);
        }

        /// <summary>
        /// Registers a delegate with the scripting system to be called as a script
        /// </summary>
        public static void Register(Delegate scriptDelegate)
        {
            Instance.RegisterScript(scriptDelegate.Method);
        }

        /// <summary>
        /// Lists executable scripts
        /// </summary>
        public static List<KeyValuePair<string, MethodInfo>> GetCallableMethods()
        {
            List<KeyValuePair<string, MethodInfo>> results = new List<KeyValuePair<string, MethodInfo>>();

            var collection = Instance.CallableMethods;

            foreach(var k in collection.Keys)
            {
                var v = collection[k];

                results.Add(new KeyValuePair<string, MethodInfo>(k, v));
            }

            return results;
        }
    }
  
}