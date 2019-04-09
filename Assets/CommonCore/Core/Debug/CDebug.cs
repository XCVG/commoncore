using System;
using UnityEngine;

namespace CommonCore.DebugLog
{
    /*
     * CommonCore Debug/Log Module
     * CDebug class provides (mostly) drop-in replacement for UnityEngine.Debug
     * Right now it basically passes it through, this will change eventually
     */

    /// <summary>
    /// Originally a mostly drop-in replacement for UnityEngine.Debug, repurposed when we realized we didn't need that.
    /// </summary>
    public static class CDebug
    {
        /// <summary>
        /// Log a message, if and only if verbose logging is enabled
        /// </summary>
        public static void LogVerbose(object message)
        {
            if(CoreParams.UseVerboseLogging)
                Debug.Log(message);
        }

        /// <summary>
        /// Log a message, if and only if verbose logging is enabled
        /// </summary>
        public static void LogVerbose(object message, UnityEngine.Object context)
        {
            if (CoreParams.UseVerboseLogging)
                Debug.Log(message, context);
        }
                
        /// <summary>
        /// Unified log method
        /// </summary>
        /// <param name="message">The text to log</param>
        /// <param name="type">The type/level of the log</param>
        /// <param name="context">The context from which the log was sent (optional)</param>
        public static void LogEx(string message, LogLevel type, object context)
        {
            switch (type)
            {
                case LogLevel.Error:
                    if (context is UnityEngine.Object)
                        Debug.LogError(message, (UnityEngine.Object)context);
                    else
                        Debug.LogError(string.Format("[{1}] {0}", message, context));
                    break;
                case LogLevel.Warning:
                    if (context is UnityEngine.Object)
                        Debug.LogWarning(message, (UnityEngine.Object)context);
                    else
                        Debug.LogWarning(string.Format("[{1}] {0}", message, context));
                    break;
                case LogLevel.Message:
                    if (context is UnityEngine.Object)
                        Debug.Log(message, (UnityEngine.Object)context);
                    else
                        Debug.Log(string.Format("[{1}] {0}", message, context));
                    break;
                case LogLevel.Verbose:
                    if (CoreParams.UseVerboseLogging)
                    {
                        if (context is UnityEngine.Object)
                            Debug.Log(message, (UnityEngine.Object)context);
                        else
                            Debug.Log(string.Format("[{1}] {0}", message, context));
                    }
                    break;
            }
        }

    }

}