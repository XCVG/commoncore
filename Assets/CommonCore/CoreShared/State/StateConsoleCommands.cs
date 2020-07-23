using CommonCore.Console;
using CommonCore.DebugLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CommonCore.State
{
    /// <summary>
    /// Extra console commands for manipulating non-specific state
    /// </summary>
    /// <remarks>
    /// Originally bodged together for Sandstorm, then backported into mainline
    /// </remarks>
    public static class StateConsoleCommands
    {
        [Command(alias = "PrintCampaignHash", className = "GameState", useClassName = true)]
        public static void DumpCampaignHash()
        {
            Debug.Log(GameState.Instance.CampaignIdentifier);
        }

        [Command(alias = "PrintTime", className = "GameState", useClassName = true)]
        static void DisplayTime()
        {
            string timeStr = string.Format("Real Time:  {0:F1}s\nGame Time:  {1:F1}s\nWorld Time: {2:F1}d, {3}s\n\nUnity Timescale: {4}\nWorld Timescale: {5}",
                GameState.Instance.WorldTimeState.RealTimeElapsed,
                GameState.Instance.WorldTimeState.GameTimeElapsed,
                GameState.Instance.WorldTimeState.WorldDaysElapsed,
                GameState.Instance.WorldTimeState.WorldSecondsElapsed,
                Time.timeScale,
                GameState.Instance.WorldTimeState.WorldTimeScale);
            ConsoleModule.WriteLine(timeStr);
        }

        [Command(alias = "Exists", className = "GameState", useClassName = true)]
        public static void GameStateExists()
        {
            ConsoleModule.WriteLine(GameState.Exists ? "true" : "false");
        }

        [Command(alias = "Print", className = "GameState", useClassName = true)]
        public static void DumpGameState()
        {
            var gs = DebugUtils.JsonStringify(GameState.Instance, true);
            Debug.Log(gs);
        }

        [Command(alias = "Print", className = "MetaState", useClassName = true)]
        public static void DumpMetaState()
        {
            var ms = DebugUtils.JsonStringify(MetaState.Instance, true);
            Debug.Log(ms);
        }

        [Command(alias = "Print", className = "PersistState", useClassName = true)]
        public static void DumpPersistState()
        {
            var ps = DebugUtils.JsonStringify(PersistState.Instance, true);
            Debug.Log(ps);
        }

        [Command(alias = "SetVar", className = "GameState", useClassName = true)]
        public static void SetGameStateVar(string var, string value)
        {
            SetVariable(GameState.Instance, var, value);
        }

        [Command(alias = "SetVar", className = "PersistState", useClassName = true)]
        public static void SetPersistStateVar(string var, string value)
        {
            SetVariable(PersistState.Instance, var, value);
        }

        [Command(alias = "SetFlag", className = "MetaState", useClassName = true)]
        public static void SetMetaStateFlag(string flag, bool state)
        {
            if (state && !MetaState.Instance.SessionFlags.Contains(flag))
                MetaState.Instance.SessionFlags.Add(flag);
            else if (!state && MetaState.Instance.SessionFlags.Contains(flag))
                MetaState.Instance.SessionFlags.Remove(flag);

            Debug.Log($"{flag}: {MetaState.Instance.SessionFlags.Contains(flag)}");
        }

        private static void SetVariable(object obj, string var, string value) //TODO move this into a util class
        {
            var property = obj.GetType().GetProperty(var, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                property.SetValue(obj, TypeUtils.CoerceValue(value, property.PropertyType));

                Debug.Log($"{property.Name} = {property.GetValue(obj)}");

                return;
            }
            else
            {
                var field = obj.GetType().GetField(var, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(obj, TypeUtils.CoerceValue(value, field.FieldType));

                    Debug.Log($"{field.Name} = {field.GetValue(obj)}");
                    return;
                }
            }

            Debug.LogError($"Failed to set {var}");
        }
    }
}