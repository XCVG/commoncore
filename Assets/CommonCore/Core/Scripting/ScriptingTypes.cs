using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace CommonCore.Scripting
{
    /// <summary>
    /// Attach to a method to register it with the scripting system
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CCScriptAttribute : PreserveAttribute
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        /// <summary>
        /// If true, will automatically try to find a static singleton instance
        /// </summary>
        public bool AutoResolveInstance { get; set; } = false;
        /// <summary>
        /// If set, will never pass ScriptExecutionContext even if the types match
        /// </summary>
        public bool NeverPassExecutionContext { get; set; } = false;
    }

    /// <summary>
    /// Attach to a method along with <see cref="CCScriptAttribute"/> to register it to run on certain events
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CCScriptHookAttribute : Attribute
    {
        public ScriptHook Hook { get; set; } = ScriptHook.None;
        public string NamedHook { get; set; } = null;

        /// <summary>
        /// If true, will allow the script to be called through the scripting system as well as automatically
        /// </summary>
        public bool AllowExplicitCalls { get; set; } = false;
    }

    public enum ScriptHook
    {
        None, AfterModulesLoaded, AfterAddonsLoaded, BeforeApplicationExit, OnGameStart, OnGameEnd, OnGameLoad, OnSceneTransition, OnSceneLoad, AfterSceneLoad, OnSceneUnload, OnPlayerSpawn, OnGameOver, AfterMainMenuCreate, AfterIGUIMenuCreate, OnIGUIMenuOpen, OnFrameUpdate, OnWorldTimeUpdate, AfterAddonLoaded, OnLoadingSceneOpen, OnConfigPanelOpen, OnConfigPanelRendered
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

        /// <summary>
        /// The calling hook if it was called by a hook
        /// </summary>
        public ScriptHook Hook;

        /// <summary>
        /// The calling hook if it was called by a named hook
        /// </summary>
        public string NamedHook;

        /// <summary>
        /// A collection of all arguments passed to the script
        /// </summary>
        /// <remarks>
        /// <para>Note that these are NEVER COERCED</para>
        /// </remarks>
        public IReadOnlyList<object> Args;

        public override string ToString()
        {
            return string.Format("{0} : [Caller:{1}, Activator:{2}, Hook:{3}]", GetType().Name, Caller, Activator, Hook == ScriptHook.None ? (string.IsNullOrEmpty(NamedHook) ? "null" : NamedHook) : Hook.ToString());
        }
    }

    public class ScriptExecutionFailedException : Exception
    {
        public string ScriptName { get; private set; }

        public ScriptExecutionFailedException(string scriptName, Exception cause) : base($"Execution of the script \"{scriptName}\" failed", cause)
        {
            ScriptName = scriptName;
        }
    }

    public class NoInstanceForNonStaticMethodException : Exception
    {
        public override string Message => "The script was not a static method and no object instance was available";
    }

    public class ScriptNotFoundException : Exception
    {
        public override string Message => "A matching script could not be found.";
    }

    public class ArgumentCoercionException : Exception
    {
        public ArgumentCoercionException(Type sourceType, Type targetType, Exception innerException) : base($"Failed to coerce {sourceType.Name} to {targetType.Name}", innerException)
        {

        }
    }

}