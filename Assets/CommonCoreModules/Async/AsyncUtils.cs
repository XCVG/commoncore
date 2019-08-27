using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonCore.Async
{
    /// <summary>
    /// Utility class for helping with async stuff
    /// </summary>
    public static class AsyncUtils
    {

        /// <summary>
        /// Runs an async method and logs an exception if one is thrown
        /// </summary>
        /// <remarks>Use this instead of async void methods</remarks>
        public static async void RunWithExceptionHandling(Func<Task> asyncMethod)
        {
            try
            {
                await asyncMethod();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Checks if the editor is in play mode and throws if it is not
        /// </summary>
        /// <remarks>Use this to abort async methods when play mode is exited, because for Reasons that's not done by default</remarks>
        public static void ThrowIfEditorStopped()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                throw new InvalidOperationException("Async method aborted because play mode was exited!");
#endif
        }
    }
}