using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.Scripting
{

    /// <summary>
    /// Provides services for executing arbitrary methods as standalone scripts
    /// </summary>
    [CCExplicitModule]
    public class ScriptingModule : CCModule
    {
        private static ScriptingModule Instance;

        private Dictionary<string, MethodInfo> CallableMethods;
        private Dictionary<ScriptHook, List<MethodInfo>> HookedMethods;
        private Dictionary<string, List<MethodInfo>> NamedHookedMethods;

        public ScriptingModule()
        {
            Instance = this;

            FindAllScripts();
        }

        public override void OnAllModulesLoaded()
        {
            CallHooked(ScriptHook.AfterModulesLoaded, null);
        }

        public override void OnFrameUpdate()
        {
            if (HookedMethods[ScriptHook.OnFrameUpdate].Count > 0)
                CallHooked(ScriptHook.OnFrameUpdate, null);
        }

        public override void OnGameStart()
        {
            CallHooked(ScriptHook.OnGameStart, null);
        }

        public override void OnGameEnd()
        {
            CallHooked(ScriptHook.OnGameEnd, null);
        }

        public override void OnSceneLoaded()
        {
            CallHooked(ScriptHook.OnSceneLoad, null);

            Log(SceneManager.GetActiveScene().name); //TODO handle calling main menu here?
        }

        public override void OnSceneUnloaded()
        {
            CallHooked(ScriptHook.OnSceneUnload, null);
        }

        public override void Dispose()
        {
            CallHooked(ScriptHook.BeforeApplicationExit, null);
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

            CallableMethods = new Dictionary<string, MethodInfo>();
            HookedMethods = SetupHookedMethodsDictionary();
            NamedHookedMethods = new Dictionary<string, List<MethodInfo>>();

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

            sw.Stop();

            Log(string.Format("Loaded {1} scripts in {0:f2} ms", sw.ElapsedMilliseconds, CallableMethods.Count));

        }

        private static Dictionary<ScriptHook, List<MethodInfo>> SetupHookedMethodsDictionary()
        {
            var hookedMethods = new Dictionary<ScriptHook, List<MethodInfo>>();

            foreach(ScriptHook hook in Enum.GetValues(typeof(ScriptHook)))
            {
                hookedMethods[hook] = new List<MethodInfo>();
            }

            return hookedMethods;
        }

        private void RegisterScript(MethodInfo scriptMethod)
        {
            string callableName = GetCallableName(scriptMethod);

            //Debug.Log(string.Format("Loaded script: {0}", callableName));

            //handle hooks
            var hookAttributes = scriptMethod.GetCustomAttributes(typeof(CCScriptHookAttribute), false);
            if(hookAttributes.Length > 0)
            {
                var firstHookAttribute = (CCScriptHookAttribute)hookAttributes[0];

                foreach(var hookAttribute in hookAttributes)
                {
                    var hookAttributeTyped = hookAttribute as CCScriptHookAttribute;

                    if(hookAttributeTyped.Hook != ScriptHook.None)
                        HookedMethods[hookAttributeTyped.Hook].Add(scriptMethod);
                    if(!string.IsNullOrEmpty(hookAttributeTyped.NamedHook))
                    {
                        if(!NamedHookedMethods.TryGetValue(hookAttributeTyped.NamedHook, out var list))
                        {
                            list = new List<MethodInfo>();
                            NamedHookedMethods[hookAttributeTyped.NamedHook] = list;
                        }

                        list.Add(scriptMethod);
                    }
                }

                if (!firstHookAttribute.AllowExplicitCalls)
                    return; //do not register the script
            }

            if (CallableMethods.ContainsKey(callableName))
            {
                LogError(string.Format("Tried to add a script with name \"{0}\" but that name is already registered!", callableName));
                throw new ScriptAlreadyRegisteredException();
            }

            CallableMethods.Add(callableName, scriptMethod);
        }

        private void RegisterScript(Delegate scriptDelegate, string className, string methodName)
        {
            string callableName = className + "." + methodName;

            if (CallableMethods.ContainsKey(callableName))
            {
                LogError(string.Format("Tried to add a script with name \"{0}\" but that name is already registered!", callableName));
                throw new ScriptAlreadyRegisteredException();
            }

            CallableMethods.Add(callableName, scriptDelegate.Method);
        }

        private void UnregisterScript(string className, string methodName)
        {
            string callableName = className + "." + methodName;

            if(CallableMethods.ContainsKey(callableName))
                CallableMethods.Remove(callableName);
        }

        private void CallHookedScripts(ScriptHook hook, ScriptExecutionContext context, params object[] args)
        {
            foreach(var scriptMethod in HookedMethods[hook])
            {
                try
                {
                    CallScriptMethod(scriptMethod, null, context, args);
                }
                catch(Exception e)
                {
                    LogError($"Failed to execute {hook.ToString()} script \"{scriptMethod.DeclaringType.Name}.{scriptMethod.Name}\" ({e.Message})");
                    LogException(e);
                }
            }
        }

        private void CallNamedHookedScripts(string hook, ScriptExecutionContext context, params object[] args)
        {
            foreach(var scriptMethod in NamedHookedMethods[hook])
            {
                try
                {
                    CallScriptMethod(scriptMethod, null, context, args);
                }
                catch (Exception e)
                {
                    LogError($"Failed to execute {hook} script \"{scriptMethod.DeclaringType.Name}.{scriptMethod.Name}\" ({e.Message})");
                    LogException(e);
                }
            }
        }

        private object CallScript(string script, object instance, ScriptExecutionContext context, params object[] args)
        {
            try
            {
                if (!CallableMethods.ContainsKey(script))
                    throw new ScriptNotFoundException();

                MethodInfo m = CallableMethods[script];

                return CallScriptMethod(m, instance, context, args);
            }
            catch (Exception e)
            {
                LogError($"Failed to execute script \"{script}\" ({e.Message})");
                LogException(e);

                throw new ScriptExecutionFailedException(script, e);
            }
        }

        private static object CallScriptMethod(MethodInfo scriptMethod, object instance, ScriptExecutionContext context, object[] args)
        {
            //get script attribute
            CCScriptAttribute scriptAttribute = null;
            var scriptAttributes = scriptMethod.GetCustomAttributes(typeof(CCScriptAttribute), false);
            if (scriptAttributes != null && scriptAttributes.Length > 0)
                scriptAttribute = (CCScriptAttribute)scriptAttributes[0];

            //attempt to resolve instance
            if (!scriptMethod.IsStatic && instance == null)
            {
                if (scriptAttribute != null && scriptAttribute.AutoResolveInstance)
                {
                    var property = scriptMethod.DeclaringType.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (property != null)
                        instance = property.GetValue(null);
                    else
                    {
                        var field = scriptMethod.DeclaringType.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        if (field != null)
                            instance = field.GetValue(null);
                    }
                }

                if (instance == null)
                    throw new NoInstanceForNonStaticMethodException();
            }

            var scriptParameters = scriptMethod.GetParameters();

            //trim and coerce arguments
            object[] trimmedArgs;
            if (scriptParameters == null || scriptParameters.Length == 0)
            {
                //cheap path: pass empty array
                trimmedArgs = new object[] { };
            }
            else if ((scriptAttribute != null && scriptAttribute.NeverPassExecutionContext) || !(scriptParameters != null && scriptParameters.Length > 0 && scriptParameters[0].ParameterType.IsAssignableFrom(typeof(ScriptExecutionContext))))
            {
                //do not pass ScriptExecutionContext
                trimmedArgs = CoerceAndTrimArguments(args, scriptParameters, null);

                //display warning if we had to trim
                if (scriptParameters.Length != args.Length)
                {
                    Debug.LogWarning($"[{nameof(ScriptingModule)}] Argument coercion warning: {GetCallableName(scriptMethod)} expected {scriptParameters.Length} arguments but was passed {args.Length}");
                }
            }
            else
            {
                //pass all args including ScriptExecutionContext
                if (scriptParameters.Length == 1)
                {
                    trimmedArgs = new object[] { context };
                }
                else
                {
                    trimmedArgs = CoerceAndTrimArguments(args, scriptParameters, context);
                }

                //display warning if we had to trim
                if (scriptParameters.Length != args.Length + 1)
                {
                    Debug.LogWarning($"[{nameof(ScriptingModule)}] Argument coercion warning: {GetCallableName(scriptMethod)} expected 1+{scriptParameters.Length-1} arguments but was passed 1+{args.Length}");
                }

            }

            return scriptMethod.Invoke(instance, trimmedArgs);
        }

        private static object[] CoerceAndTrimArguments(object[] args, ParameterInfo[] parameters, ScriptExecutionContext? context)
        {
            int numArgs = parameters.Length;

            object[] trimmedArgs = new object[numArgs];

            for(int i = 0; i < numArgs; i++)
            {
                if (i == 0 && context.HasValue)
                {
                    trimmedArgs[0] = context; //we know it's assignable at this point
                    continue;
                }

                int srcI = context.HasValue ? i - 1 : i;
                if (args.Length > srcI)
                {
                    try
                    {
                        trimmedArgs[i] = TypeUtils.CoerceValue(args[srcI], parameters[i].ParameterType);
                    }
                    catch(Exception e)
                    {
                        throw new ArgumentCoercionException(args[srcI].GetType(), parameters[i].ParameterType, e);
                    }

                }
                else
                    trimmedArgs[i] = TypeUtils.GetDefault(parameters[i].ParameterType);
            }

            return trimmedArgs;
        }

        private static string GetCallableName(MethodInfo method)
        {
            string className = method.DeclaringType.Name;
            string methodName = method.Name;

            var scriptAttribute = (CCScriptAttribute)method.GetCustomAttributes(typeof(CCScriptAttribute), false)[0];

            if (scriptAttribute != null)
            {
                if (!string.IsNullOrEmpty(scriptAttribute.Name))
                    methodName = scriptAttribute.Name;

                if (!string.IsNullOrEmpty(scriptAttribute.ClassName))
                    className = scriptAttribute.ClassName;
            }

            string callableName = className + "." + methodName;
            return callableName;
        }

        /// <summary>
        /// Calls a script through the scripting system
        /// </summary>
        public static void Call(string script, ScriptExecutionContext context, params object[] args)
        {
            Instance.CallScript(script, null, context, args);
        }

        /// <summary>
        /// Calls a script through the scripting system on an object
        /// </summary>
        public static void CallOn(string script, object instance, ScriptExecutionContext context, params object[] args)
        {
            Instance.CallScript(script, instance, context, args);
        }

        /// <summary>
        /// Calls a script through the scripting system, returning a result
        /// </summary>
        public static object CallForResult(string script, ScriptExecutionContext context, params object[] args)
        {
            return Instance.CallScript(script, null, context, args);
        }

        /// <summary>
        /// Calls a script through the scripting system on an object, returning a result
        /// </summary>
        public static object CallOnForResult(string script, object instance, ScriptExecutionContext context, params object[] args)
        {
            return Instance.CallScript(script, instance, context, args);
        }

        /// <summary>
        /// Calls a script through the scripting system, returning a typed result
        /// </summary>
        public static T CallForResult<T>(string script, ScriptExecutionContext context, params object[] args)
        {
            return TypeUtils.CoerceValue<T>(Instance.CallScript(script, null, context, args));
        }

        /// <summary>
        /// Calls a script through the scripting system on an object, returning a typed result
        /// </summary>
        public static T CallOnForResult<T>(string script, object instance, ScriptExecutionContext context, params object[] args)
        {
            return TypeUtils.CoerceValue<T>(Instance.CallScript(script, instance, context, args));
        }

        /// <summary>
        /// Calls all scripts registered to run with a certain hook
        /// </summary>
        public static void CallHooked(ScriptHook hook, object caller, params object[] args)
        {
            ScriptExecutionContext context = new ScriptExecutionContext() { Caller = caller, Hook = hook };
            Instance.CallHookedScripts(hook, context, args);
        }

        /// <summary>
        /// Calls all scripts registered to run with a named hook
        /// </summary>
        public static void CallNamedHooked(string hook, object caller, params object[] args)
        {
            ScriptExecutionContext context = new ScriptExecutionContext() { Caller = caller, NamedHook = hook };
            Instance.CallNamedHookedScripts(hook, context, args);
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
        public static void Register(Delegate scriptDelegate, string className, string methodName)
        {
            throw new NotSupportedException(); //doesn't work. Need to rethink the scripting module
            //Instance.RegisterScript(scriptDelegate, className, methodName);
        }

        /// <summary>
        /// Unregisters a script from the scripting system
        /// </summary>
        /// <remarks>
        /// Can be dangerous, for obvious reasons. Use sparingly.
        /// </remarks>
        public static void Unregister(string className, string methodName)
        {
            Instance.UnregisterScript(className, methodName);
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