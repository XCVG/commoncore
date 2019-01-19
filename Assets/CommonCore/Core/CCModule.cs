using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{

    public abstract class CCModule : IDisposable
    {
        //constructor=onapplicationstart

        //other lifecycle events
        public virtual void OnAllModulesLoaded() { }
        public virtual void OnGameStart() { }
        public virtual void OnSceneLoaded() { }
        public virtual void OnFrameUpdate() { }
        public virtual void OnSceneUnloaded() { }
        public virtual void OnGameEnd() { }
        public virtual void Dispose() { }

        /// <summary>
        /// Logs a message to debug output, prepending module name
        /// </summary>
        protected void Log(string text)
        {
            Debug.Log($"[{GetType().Name}] {text}");
        }

        /// <summary>
        /// Logs a message to verbose debug output, prepending module name
        /// </summary>
        protected void LogVerbose(string text)
        {
            if(CoreParams.UseVerboseLogging)
                Debug.Log($"[{GetType().Name}] {text}");
        }

        /// <summary>
        /// Logs a warning message to debug output, prepending module name
        /// </summary>
        protected void LogWarning(string text)
        {
            Debug.LogWarning($"[{GetType().Name}] {text}");
        }

        /// <summary>
        /// Logs an error message to debug output, prepending module name
        /// </summary>
        protected void LogError(string text)
        {
            Debug.LogError($"[{GetType().Name}] {text}");
        }

        /// <summary>
        /// Logs an exception to debug output
        /// </summary>
        protected void LogException(Exception e)
        {
            Debug.LogException(e);
        }

    }

    public class CCExplicitModuleAttribute : System.Attribute
    {

    }

    public class CCEarlyModuleAttribute : System.Attribute
    {

    }

    public class CCLateModuleAttribute : System.Attribute
    {

    }
}
