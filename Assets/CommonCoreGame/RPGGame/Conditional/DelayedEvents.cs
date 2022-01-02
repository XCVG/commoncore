using CommonCore.RpgGame.State;
using CommonCore.Scripting;
using CommonCore.State;
using System;
using UnityEngine;

namespace CommonCore.DelayedEvents
{

    public enum DelayTimeType
    {
        None, Real, Game, World
    }

    public class DelayedEvent
    {
        public readonly MicroscriptNode Event;
        public readonly DelayTimeType DelayType;
        public double DelayTime;
        public bool DelayAbsolute;

        public DelayedEvent(MicroscriptNode ev, DelayTimeType delayType, double delayTime, bool delayAbsolute)
        {
            Event = ev;
            DelayType = delayType;
            DelayTime = delayTime;
            DelayAbsolute = delayAbsolute;
        }

    }

    public static class DelayedEventScheduler
    {

        const float SecondsInDay = 24 * 60 * 60;

        public static void ScheduleEvent(DelayedEvent ev)
        {
            if(!ev.DelayAbsolute) //if not absolute, calculate absolute time
            {
                switch (ev.DelayType)
                {
                    case DelayTimeType.Real:
                        ev.DelayTime = GameState.Instance.WorldTimeState.RealTimeElapsed + ev.DelayTime;
                        break;
                    case DelayTimeType.Game:
                        ev.DelayTime = GameState.Instance.WorldTimeState.GameTimeElapsed + ev.DelayTime;
                        break;
                    case DelayTimeType.World:
                        ev.DelayTime = GameState.Instance.WorldTimeState.WorldDaysElapsed * SecondsInDay + GameState.Instance.WorldTimeState.WorldSecondsElapsed + ev.DelayTime;
                        break;
                }

                ev.DelayAbsolute = true;
            }

            GameState.Instance.DelayedEvents.Add(ev);
        }

        public static void ScheduleEvent(MicroscriptNode action, DelayTimeType timeType, double delayTime)
        {
            ScheduleEvent(new DelayedEvent(action, timeType, delayTime, false));
        }

        public static void ScheduleEvent(MicroscriptNode action, DelayTimeType timeType, double delayTime, bool delayAbsolute)
        {
            ScheduleEvent(new DelayedEvent(action, timeType, delayTime, delayAbsolute));
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.OnWorldTimeUpdate)]
        public static void ExecuteScheduledEvents()
        {
            var delayedEvents = GameState.Instance.DelayedEvents;

            //reverse iterate, executing scheduled events
            try
            {
                for (int i = delayedEvents.Count - 1; i >= 0; i--)
                {
                    DelayedEvent delayedEvent = delayedEvents[i];

                    if (!delayedEvent.DelayAbsolute)
                        throw new InvalidOperationException();

                    double fireTime = delayedEvent.DelayTime;
                    double elapsedTime = 0;
                    switch (delayedEvent.DelayType)
                    {
                        case DelayTimeType.Real:
                            elapsedTime = GameState.Instance.WorldTimeState.RealTimeElapsed;
                            break;
                        case DelayTimeType.Game:
                            elapsedTime = GameState.Instance.WorldTimeState.GameTimeElapsed;
                            break;
                        case DelayTimeType.World:
                            elapsedTime = GameState.Instance.WorldTimeState.WorldDaysElapsed * SecondsInDay + GameState.Instance.WorldTimeState.WorldSecondsElapsed;
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
                            Debug.LogError("Failed to execute microscript of delayed event");
                            Debug.LogException(e);
                        }

                        delayedEvents.RemoveAt(i);
                    }

                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                GameState.Instance.DelayedEvents.Clear(); //purge the list, something went terribly wrong
            }
        }
    }
}