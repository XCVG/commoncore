using CommonCore.Config;
using CommonCore.RpgGame;
using CommonCore.RpgGame.State;
using CommonCore.State;
using System;
using UnityEngine;

namespace CommonCore.DelayedEvents
{

    public class TimeHook : MonoBehaviour
    {
        const float SecondsInDay = 24 * 60 * 60;

        private double Elapsed;

        private double LastScaledTime;
        private double LastRealTime;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        //a Coroutine might be more efficient, I'm not sure
        private void Update()
        {
            Elapsed += Time.deltaTime;

            if(Elapsed >= CoreParams.DelayedEventPollInterval)
            {
                WorldModel wm = GameState.Instance.WorldState;
                
                //advance gametime and realtime
                wm.GameTimeElapsed += (Time.time - LastScaledTime);
                wm.RealTimeElapsed = DateTimeOffset.Now.ToUnixTimeSeconds();

                //advance world (scaled) time
                wm.WorldSecondsElapsed += (Time.time - LastScaledTime) * wm.WorldTimeScale * ConfigState.Instance.GetGameplayConfig().WorldTimeScaleBias;
                while (wm.WorldTimeUseRollover && wm.WorldSecondsElapsed > SecondsInDay)
                {
                    wm.WorldSecondsElapsed -= SecondsInDay;
                    wm.WorldDaysElapsed += 1;
                }

                //set/reset all time values
                LastScaledTime = Time.time;
                LastRealTime = Time.realtimeSinceStartup;
                Elapsed = 0;

                DelayedEventScheduler.ExecuteScheduledEvents();
            }
        }

    }
}