using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CommonCore.Scripting
{
    /*
     * CommonCore Scripting Module
     * Allows execution of arbitray methods as scripts
     * Will eventually allow adding new ones at runtime
     */
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

            //this is INCREDIBLY dumb- it scans ALL loaded assemblies
            //btw, both CCBase and WanzyeeStudio Json do this
            //the former will be (eventually) fixed, and the latter will be (eventually) replaced
            //I fixed it kinda
            var methods = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !(a.FullName.StartsWith("Unity") || a.FullName.StartsWith("System") ||  
                            a.FullName.StartsWith("mscorlib") || a.FullName.StartsWith("mono") ||
                            a.FullName.StartsWith("Boo") || a.FullName.StartsWith("I18N") ))
                    .SelectMany((assembly) => assembly.GetTypes())
                    .SelectMany(t => t.GetMethods())
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

                    Debug.Log(string.Format("Loaded script: {0}", callableName));

                    if (CallableMethods.ContainsKey(callableName))
                    {
                        Debug.LogWarning(string.Format("Multiple scripts with name: {0}", callableName));
                    }

                    CallableMethods.Add(callableName, m);
                }
                catch(Exception e)
                {
                    Debug.LogError("Failed to register method " + m.Name);
                    Debug.LogException(e);
                }

            }

            Debug.Log(string.Format("Scripting module loaded! ({0:f2} ms)", sw.ElapsedMilliseconds));

        }

        private void CallScript(string name, ScriptExecutionContext context, params object[] args)
        {
            if (!CallableMethods.ContainsKey(name))
                return;

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

        public static void Call(string name, ScriptExecutionContext context, params object[] args)
        {
            Instance.CallScript(name, context, args);
        }

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

    public class ScriptingTest
    {
        
        [CCScript(ClassName = "Test", Name = "HelloWorld")]
        public void TestMethod(ScriptExecutionContext context)
        {
            Debug.Log("Hello world!");
        }

        [CCScript(ClassName = "Test", Name = "NoArgs")]
        public void NoArgsTest()
        {
            Debug.Log("Hello world! (no args)");
        }

        [CCScript(ClassName = "Test", Name = "ContextArg")]
        public void ContextArgTest(ScriptExecutionContext context)
        {
            Debug.Log(context);
        }

        [CCScript(ClassName = "Test", Name = "SingleArg")]
        public void SingleArgTest(ScriptExecutionContext context, string arg0)
        {
            Debug.Log(arg0);
        }
    }

    
}