using CommonCore.LockPause;
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
        /// Runs a task and logs an exception if one is thrown
        /// </summary>
        /// <remarks>Use this instead of async void methods</remarks>
        public static async void RunWithExceptionHandling(Task asyncTask)
        {
            try
            {
                await asyncTask;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Runs an async method and logs an exception if one is thrown. Returns the Task representing the async method
        /// </summary>
        /// <remarks>Use this instead of async void methods</remarks>
        public static Task RunWithExceptionHandlingEx(Func<Task> asyncMethod)
        {
            try
            {
                var task = asyncMethod();
                RunWithExceptionHandling(() => task);
                return task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);                
            }
            return null;

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

        /// <summary>
        /// Waits a specified number of seconds in scaled time (respecting pause state), can be skipped with skip button
        /// </summary>
        /// <remarks>Can only be used from the main thread</remarks>
        public static async Task DelayScaled(float time, PauseLockType lowestPauseState, bool useRealtime = false)
        {
            for (float elapsed = 0; elapsed < time;)
            {
                await Task.Yield();
                var pls = LockPauseModule.GetPauseLockState();
                if (pls == null || pls >= lowestPauseState)
                {
                    float timeScale = useRealtime ? 1 : (Mathf.Approximately(Time.timeScale, 0) ? 1 : Time.timeScale);
                    elapsed += Time.unscaledDeltaTime * timeScale;
                }
            }

            await Task.Yield();
        }
    }
}