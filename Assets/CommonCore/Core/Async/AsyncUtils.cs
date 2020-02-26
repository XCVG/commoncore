using System;
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

        /// <summary>
        /// Checks if this is called from/running on the main thread
        /// </summary>
        public static bool IsOnMainThread()
        {
            return SyncContextUtil.UnityThreadId == Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Waits a specified number of seconds in scaled (game) time
        /// </summary>
        /// <remarks>Can only be used from the main thread</remarks>
        public static async Task DelayScaled(float timeToWait)
        {
            float startTime = Time.time;
            while (Time.time - startTime < timeToWait)
                await Task.Yield();
        }
    }
}