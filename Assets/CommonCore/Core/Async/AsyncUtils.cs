using CommonCore.LockPause;
using System;
using System.Collections;
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
        /// Runs a coroutine in the ambient context
        /// </summary>
        public static Coroutine RunCoroutine(Func<IEnumerator> coroutine)
        {
            return AsyncCoroutineRunner.Instance.StartCoroutine(coroutine());
        }

        /// <summary>
        /// Stops a coroutine in the ambient context
        /// </summary>
        public static void StopCoroutine(Coroutine coroutine)
        {
            AsyncCoroutineRunner.Instance.StopCoroutine(coroutine);
        }

        /// <summary>
        /// Runs an action on the main thread and waits for it to complete
        /// </summary>
        public static Task RunOnMainThread(Action action)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            IEnumeratorAwaitExtensions.RunOnUnityScheduler(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch(Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Runs an async action on the main thread and waits for it to complete
        /// </summary>
        public static Task RunOnMainThread(Func<Task> asyncAction)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            IEnumeratorAwaitExtensions.RunOnUnityScheduler(() =>
            {
                runTask();
            });

            return tcs.Task;

            async void runTask()
            {
                try
                {
                    await asyncAction();
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }
        }

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
        [Obsolete]
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

        [Obsolete]
        public static void ThrowIfEditorStopped()
        {
            Debug.LogWarning("AsyncUtils.ThrowIfEditorStopped is deprecated, use ThrowIfStopped instead");
            ThrowIfStopped();
        }

        /// <summary>
        /// Checks if the editor is in play mode and CommonCore is running, and throws if it is not
        /// </summary>
        /// <remarks>Use this as a guard so your async methods terminate when they really should not be running anymore</remarks>
        public static void ThrowIfStopped()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                throw new InvalidOperationException("Async method aborted because play mode was exited!");
#endif
            if (CCBase.Terminated)
                throw new InvalidOperationException("Async method aborted because CommonCore was terminated!");
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
        public static async Task DelayScaled(float timeToWait)
        {
            if (IsOnMainThread())
                await DelayScaledInternal(timeToWait);
            else
                await RunOnMainThread(() => DelayScaledInternal(timeToWait));
        }

        private static async Task DelayScaledInternal(float timeToWait)
        {
            float startTime = Time.time;
            while (Time.time - startTime < timeToWait)
                await Task.Yield();
        }

        /// <summary>
        /// Waits a specified number of seconds in scaled time (respecting pause state), can be skipped with skip button
        /// </summary>
        public static async Task DelayScaled(float time, PauseLockType lowestPauseState, bool useRealtime = false)
        {
            if (IsOnMainThread())
                await DelayScaledInternal(time, lowestPauseState, useRealtime);
            else
                await RunOnMainThread(() => DelayScaledInternal(time, lowestPauseState, useRealtime));
        }

        private static async Task DelayScaledInternal(float time, PauseLockType lowestPauseState, bool useRealtime)
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