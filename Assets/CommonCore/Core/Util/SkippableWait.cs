using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using CommonCore.Input;
using System.Collections;

namespace CommonCore
{
    /// <summary>
    /// Skippable wait functions
    /// </summary>
    public static class SkippableWait
    {
        private static readonly string SkipButton = DefaultControls.Use;
        private static readonly string AltSkipButton = DefaultControls.Fire;
        private static readonly string TerSkipButton = DefaultControls.Confirm;

        /// <summary>
        /// Waits for specified time, can be skipped with skip button
        /// </summary>
        public static IEnumerator WaitForSeconds(float time)
        {
            for (float elapsed = 0; elapsed < time; elapsed += Time.deltaTime)
            {
                if (MappedInput.GetButtonDown(SkipButton) || MappedInput.GetButtonDown(AltSkipButton) || MappedInput.GetButtonDown(TerSkipButton))
                    break;

                yield return null;
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
                if (MappedInput.GetButtonDown(SkipButton) || MappedInput.GetButtonDown(AltSkipButton) || MappedInput.GetButtonDown(TerSkipButton))
                    break;

                yield return null;
            }

            yield return null; //necessary for debouncing
        }

        /// <summary>
        /// Waits a specified number of seconds in scaled (game) time, can be skipped with skip button
        /// </summary>
        /// <remarks>Can only be used from the main thread</remarks>
        public static async Task DelayScaled(float time)
        {
            for(float elapsed = 0; elapsed < time; elapsed += Time.deltaTime)
            {
                if (MappedInput.GetButtonDown(SkipButton) || MappedInput.GetButtonDown(AltSkipButton) || MappedInput.GetButtonDown(TerSkipButton))
                    break;

                await Task.Yield();
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
                if (MappedInput.GetButtonDown(SkipButton) || MappedInput.GetButtonDown(AltSkipButton) || MappedInput.GetButtonDown(TerSkipButton))
                    break;

                await Task.Yield();
            }

            await Task.Yield();
        }
    }
}