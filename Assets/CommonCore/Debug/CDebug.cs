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
        
        //new unified log method
        public static void LogEx(string message, LogType type, System.Object context)
        {
            switch (type)
            {
                case LogType.Error:
                    if (context is UnityEngine.Object)
                        Debug.LogError(message, (UnityEngine.Object)context);
                    else
                        Debug.LogError(string.Format("{0} [{1}]", message, context.ToString()));
                    break;
                case LogType.Assert:
                    if (context is UnityEngine.Object)
                        Debug.LogAssertion(message, (UnityEngine.Object)context);
                    else
                        Debug.LogAssertion(string.Format("{0} [{1}]", message, context.ToString()));
                    break;
                case LogType.Warning:
                    if (context is UnityEngine.Object)
                        Debug.LogWarning(message, (UnityEngine.Object)context);
                    else
                        Debug.LogWarning(string.Format("{0} [{1}]", message, context.ToString()));
                    break;
                case LogType.Log:
                    if (context is UnityEngine.Object)
                        Debug.Log(message, (UnityEngine.Object)context);
                    else
                        Debug.Log(string.Format("{0} [{1}]", message, context.ToString()));
                    break;
                case LogType.Exception:
                    if (context is UnityEngine.Object)
                        Debug.LogError(message, (UnityEngine.Object)context);
                    else
                        Debug.LogError(string.Format("{0} [{1}]", message, context.ToString()));
                    break;
            }
        }


    }
}