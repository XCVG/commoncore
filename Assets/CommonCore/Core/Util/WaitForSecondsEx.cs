using CommonCore.Input;
using CommonCore.LockPause;
using System;
using UnityEngine;

namespace CommonCore
{
    
    /// <summary>
    /// Extended WaitForSeconds allowing skippable wait and awareness of PauseLock
    /// </summary>
    public class WaitForSecondsEx : CustomYieldInstruction
    {
        private float TimeToWait;
        private bool Skippable;
        private bool AllowWhenPaused;
        private PauseLockType HighestPauseState;
        private bool UseRealtime;

        private float Elapsed;

        public WaitForSecondsEx(float time, bool skippable = false, PauseLockType? lowestPauseState = null, bool useRealtime = false)
        {
            TimeToWait = time;
            Skippable = skippable;
            AllowWhenPaused = lowestPauseState.HasValue;
            HighestPauseState = lowestPauseState ?? default;
            UseRealtime = useRealtime;
        }

        public override bool keepWaiting
        {
            get
            {
                bool skipped = Skippable && SkippableWait.RequestedSkip;

                var pls = LockPauseModule.GetPauseLockState();
                if(pls == null || (AllowWhenPaused && pls >= HighestPauseState))
                {
                    float timeScale = UseRealtime ? 1 : (Mathf.Approximately(Time.timeScale, 0) ? 1 : Time.timeScale);
                    Elapsed += Time.unscaledDeltaTime * timeScale;
                }

                return !(skipped || Elapsed >= TimeToWait);
            }
        }
    }
}