using System;
using UnityEngine;

namespace CommonCore.DebugLog
{
    /*
     * CommonCore Debug/Log Module
     * CDebug class provides (mostly) drop-in replacement for UnityEngine.Debug
     * Right now it basically passes it through, this will change eventually
     */
    public static class CDebug
    {
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        public static void Log(object message, UnityEngine.Object context)
        {
            Debug.Log(message, context);
        }

        public static void Log2(string message, System.Object context)
        {
            Debug.Log(message);
        }

        public static void LogAssertion(object message)
        {
            Debug.LogAssertion(message);
        }

        public static void LogAssertion(object message, UnityEngine.Object context)
        {
            Debug.LogAssertion(message, context);
        }

        public static void LogAssertion2(string message, System.Object context)
        {
            Debug.LogAssertion(message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }

        public static void LogError(object message, UnityEngine.Object context)
        {
            Debug.LogError(message, context);
        }

        public static void LogError2(string message, System.Object context)
        {
            Debug.LogError(message);
        }

        public static void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }

        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            Debug.LogException(exception, context);
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public static void LogWarning(object message, UnityEngine.Object context)
        {
            Debug.LogWarning(message, context);
        }

        public static void LogWarning2(string message, System.Object context)
        {
            Debug.LogError(message);
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