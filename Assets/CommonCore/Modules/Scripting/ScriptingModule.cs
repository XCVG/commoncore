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
                    var scriptAttribute = (CCScriptAttribute)m.GetCustomAttributes(typeof(CCScriptAttribute), false)[0];

                    string callableName = m.DeclaringType.Name + "." + m.Name;

                    if (!string.IsNullOrEmpty(scriptAttribute.Name))
                    {
                        if (!string.IsNullOrEmpty(scriptAttribute.ClassName))
                        {
                            callableName = scriptAttribute.ClassName + "." + scriptAttribute.Name;
                        }
                        else
                        {
                            callableName = scriptAttribute.ClassName + "." + m.Name;
                        }
                    }

                    //Debug.Log(string.Format("Loaded script: {0}", callableName));

                    if (CallableMethods.ContainsKey(callableName))
                    {
                        LogWarning(string.Format("Multiple scripts with name: {0}", callableName));
                    }

                    CallableMethods.Add(callableName, m);
                }
                catch(Exception e)
                {
                    LogError("Failed to register method " + m.Name);
                    LogException(e);
                }

            }

            Log(string.Format("Loaded {1} scripts in {0:f2} ms", sw.ElapsedMilliseconds, CallableMethods.Count));

        }

        private void CallScript(string name, ScriptExecutionContext context, params object[] args)
        {
            if (!CallableMethods.ContainsKey(name))
                throw new ArgumentException("Script not found!", "name");

            MethodInfo m = CallableMethods[name];

            object obj = null;
            if(!m.IsStatic)
            {
                obj = Activator.CreateInstance(m.DeclaringType);
            }

            List<object> allArgs = new List<object>(args.Length + 1);
            allArgs.Add(context);
            allArgs.AddRange(args);

            object[] trimmedArgs = allArgs.GetRange(0, m.GetParameters().Length).ToArray();

            m.Invoke(obj, trimmedArgs);
        }

        /// <summary>
        /// Calls a script through the scripting system
        /// </summary>
        public static void Call(string name, ScriptExecutionContext context, params object[] args)
        {
            Instance.CallScript(name, context, args);
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