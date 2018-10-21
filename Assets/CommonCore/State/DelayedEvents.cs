using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;

namespace CommonCore.State
{

    public enum DelayTimeType
    {
        Real, Game, World
    }

    public class DelayedEvent
    {
        public readonly MicroscriptNode Event;
        public readonly DelayTimeType DelayType;
        public readonly float DelayTime;
        public float StartTime;

        public DelayedEvent(MicroscriptNode ev, DelayTimeType delayType, float delayTime)
        {
            Event = ev;
            DelayType = delayType;
            DelayTime = delayTime;
        }

    }

    public class DelayedEventScheduler
    {

        const float SecondsInDay = 24 * 60 * 60;

        public static void ScheduleEvent(DelayedEvent ev)
        {
            //TODO handle start time
            switch (ev.DelayType)
            {
                case DelayTimeType.Real:
                    ev.StartTime = GameState.Instance.WorldState.RealTimeElapsed;
                    break;
                case DelayTimeType.Game:
                    ev.StartTime = GameState.Instance.WorldState.GameTimeElapsed;
                    break;
                case DelayTimeType.World:
                    ev.StartTime = GameState.Instance.WorldState.WorldDaysElapsed * SecondsInDay + GameState.Instance.WorldState.WorldSecondsElapsed;
                    break;
            }

            GameState.Instance.DelayedEvents.Add(ev);
        }

        public void ScheduleEvent(MicroscriptNode action, DelayTimeType timeType, float delayTime)
        {
            ScheduleEvent(new DelayedEvent(action, timeType, delayTime));
        }

        public static void ExecuteScheduledEvents()
        {
            var delayedEvents = GameState.Instance.DelayedEvents;

            //reverse iterate, executing scheduled events
            try
            {
                for (int i = delayedEvents.Count - 1; i >= 0; i--)
                {
                    DelayedEvent delayedEvent = delayedEvents[i];

                    float fireTime = delayedEvent.StartTime + delayedEvent.DelayTime;
                    float elapsedTime = 0;
                    switch (delayedEvent.DelayType)
                    {
                        case DelayTimeType.Real:
                            elapsedTime = GameState.Instance.WorldState.RealTimeElapsed;
                            break;
                        case DelayTimeType.Game:
                            elapsedTime = GameState.Instance.WorldState.GameTimeElapsed;
                            break;
                        case DelayTimeType.World:
                            elapsedTime = GameState.Instance.WorldState.WorldDaysElapsed * SecondsInDay + GameState.Instance.WorldState.WorldSecondsElapsed;
                            break;
                    }

                    if (elapsedTime >= fireTime)
                    {
                        try
                        {
                            delayedEvent.Event.Execute();
                        }
                        catch (Exception e)
                        {
                            CDebug.LogError("Failed to execute microscript of delayed event");
                            CDebug.LogException(e);
                        }

                        delayedEvents.RemoveAt(i);
                    }

                }
            }
            catch(Exception e)
            {
                CDebug.LogException(e);
                GameState.Instance.DelayedEvents.Clear(); //purge the list, something went terribly wrong
            }
        }
    }
}