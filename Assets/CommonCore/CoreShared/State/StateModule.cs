using CommonCore.Config;
using CommonCore.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{

    /// <summary>
    /// CommonCore State helper module.
    /// Currently handles PersistState and that's about it.
    /// </summary>
    [CCEarlyModule]
    public class StateModule : CCModule
    {
        public StateModule()
        {
            PersistState.Load();
            PersistState.Instance.LastStartupTime = DateTime.Now;
            PersistState.Instance.LastApplicationPath = CoreParams.GameFolderPath;
            PersistState.Save();
        }

        public override void Dispose()
        {
            PersistState.Instance.LastExitTime = DateTime.Now;
            PersistState.Save();
        }

        //former timehook stuff
        const float SecondsInDay = 24 * 60 * 60;
        private double Elapsed;
        private double LastScaledTime;
        private double LastRealTime;

        public override void OnGameEnd()
        {
            Elapsed = 0;
        }

        public override void OnSceneLoaded()
        {
            MetaState.Instance.TempData.Clear();
        }

        public override void OnSceneUnloaded()
        {
            if(CoreParams.PlatformMaySuddenlyExit)
            {
                PersistState.Save();
            }
        }

        public override void OnFrameUpdate()
        {
            if (!GameState.Exists)
                return;

            Elapsed += Time.deltaTime;

            if (Elapsed >= CoreParams.DelayedEventPollInterval)
            {
                WorldTimeModel wm = GameState.Instance.WorldTimeState;

                //advance gametime and realtime
                wm.GameTimeElapsed += (Time.time - LastScaledTime);
                wm.RealTimeElapsed = DateTimeOffset.Now.ToUnixTimeSeconds();

                //advance world (scaled) time
                wm.WorldSecondsElapsed += (Time.time - LastScaledTime) * wm.WorldTimeScale * ConfigState.Instance.WorldTimescale;
                while (wm.WorldTimeUseRollover && wm.WorldSecondsElapsed > SecondsInDay)
                {
                    wm.WorldSecondsElapsed -= SecondsInDay;
                    wm.WorldDaysElapsed += 1;
                }

                //set/reset all time values
                LastScaledTime = Time.time;
                LastRealTime = Time.realtimeSinceStartup;
                Elapsed = 0;

                ScriptingModule.CallHooked(ScriptHook.OnWorldTimeUpdate, this, wm);
            }
        }
    }
}