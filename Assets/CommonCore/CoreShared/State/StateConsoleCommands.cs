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
                property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));

                Debug.Log($"{property.Name} = {property.GetValue(obj)}");

                return;
            }
            else
            {
                var field = obj.GetType().GetField(var, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(obj, Convert.ChangeType(value, field.FieldType));

                    Debug.Log($"{field.Name} = {field.GetValue(obj)}");
                    return;
                }
            }

            Debug.LogError($"Failed to set {var}");
        }
    }
}