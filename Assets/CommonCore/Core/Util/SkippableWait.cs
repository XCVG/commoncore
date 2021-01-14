using CommonCore.Input;
using CommonCore.LockPause;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonCore
{
    /// <summary>
    /// Skippable wait functions
    /// </summary>
    public static class SkippableWait
    {
        private static readonly string SkipButton = DefaultControls.Use;
        private static readonly string AltSkipButton = DefaultControls.Fire;
        private static readonly string TerSkipButton = DefaultControls.Submit;
        
        /// <summary>
        /// Waits for specified time, can be skipped with skip button
        /// </summary>
        public static IEnumerator WaitForSeconds(float time)
        {
            for (float elapsed = 0; elapsed < time; elapsed += Time.deltaTime)
            {
                if (RequestedSkip)
                    break;

                yield return null;
            }

            yield return null; //necessary for debouncing
        }

        /// <summary>
        /// Waits for specified time (respecting pause state), can be skipped with skip button
        /// </summary>
        public static IEnumerator WaitForSeconds(float time, PauseLockType lowestPauseState, bool useRealtime = false)
        {           
            for (float elapsed = 0; elapsed < time;)
            {
                if (RequestedSkip)
                    break;

                yield return null;
                var pls = LockPauseModule.GetPauseLockState();
                if (pls == null || pls >= lowestPauseState)
                {
                    float timeScale = useRealtime ? 1 : (Mathf.Approximately(Time.timeScale, 0) ? 1 : Time.timeScale);
                    elapsed += Time.unscaledDeltaTime * timeScale;
                }
            }

            yield return null; //necessary for debouncing
        }

        /// <summary>
        /// Waits for specified time (in real time), can be skipped with skip button
        /// </summary>
        public static IEnumerator WaitForSecondsRealtime(float time)
        {
            for (float elapsed = 0; elapsed < time; elapsed += Time.unscaledDeltaTime)
            {
                if (RequestedSkip)
                    break;

                yield return null;
            }

            yield return null; //necessary for debouncing
        }

        /// <summary>
        /// Waits a specified number of seconds in scaled (game) time , can be skipped with skip button
        /// </summary>
        /// <remarks>Can only be used from the main thread</remarks>
        public static async Task DelayScaled(float time)
        {
            for(float elapsed = 0; elapsed < time; elapsed += Time.deltaTime)
            {
                if (RequestedSkip)
                    break;

                await Task.Yield();
            }

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
                if (RequestedSkip)
                    break;

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

        /// <summary>
        /// Waits a specified number of seconds in real time, can be skipped with skip button
        /// </summary>
        /// <remarks>Can only be used from the main thread</remarks>
        public static async Task DelayRealtime(float time)
        {
            for (float elapsed = 0; elapsed < time; elapsed += Time.unscaledDeltaTime)
            {
                if (RequestedSkip)
                    break;

                await Task.Yield();
            }

            await Task.Yield();
        }

        internal static bool RequestedSkip => !LockPauseModule.IsInputLocked() && (MappedInput.GetButtonDown(SkipButton) || MappedInput.GetButtonDown(AltSkipButton) || MappedInput.GetButtonDown(TerSkipButton));

    }
}