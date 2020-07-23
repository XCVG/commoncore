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
    /// Console commands for manipulating campaign state
    /// </summary>
    public static class CampaignConsoleCommands
    {
        [Command(className = "Campaign")]
        static void GetVar(string varName)
        {
            var value = GameState.Instance.CampaignState.GetVar<object>(varName);
            ConsoleModule.WriteLine($"{value} ({value.GetType().Name})");
        }

        [Command(className = "Campaign")]
        static void SetVar(string varName, string newValue)
        {
            GameState.Instance.CampaignState.SetVarEx(varName, newValue);
        }

        [Command(className = "Campaign")]
        static void SetVarAuto(string varName, string newValue)
        {
            object value = TypeUtils.StringToNumericAuto(newValue);

            GameState.Instance.CampaignState.SetVarEx(varName, value);
        }

        [Command(className = "Campaign")]
        static void SetVarTyped(string varName, string newValue, string typeName)
        {
            object value = Convert.ChangeType(newValue, Type.GetType(typeName));

            GameState.Instance.CampaignState.SetVar<object>(varName, value);
        }

        [Command(className = "Campaign")]
        static void ListAllVars()
        {
            ConsoleModule.WriteLine(GameState.Instance.CampaignState.ListAllVars());
        }

        [Command(className = "Campaign")]
        static void GetFlag(string flagName)
        {
            ConsoleModule.WriteLine(GameState.Instance.CampaignState.HasFlag(flagName).ToString());
        }

        [Command(className = "Campaign")]
        static void SetFlag(string flagName, string flagState)
        {
            GameState.Instance.CampaignState.SetFlag(flagName, Convert.ToBoolean(flagState));
        }

        [Command(className = "Campaign")]
        static void ListAllFlags()
        {
            ConsoleModule.WriteLine(GameState.Instance.CampaignState.ListAllFlags());
        }

        [Command(className = "Campaign")]
        static void GetQuestStage(string questName)
        {
            ConsoleModule.WriteLine(GameState.Instance.CampaignState.GetQuestStage(questName).ToString());
        }

        [Command(className = "Campaign")]
        static void SetQuestStage(string questName, string questStage)
        {
            GameState.Instance.CampaignState.SetQuestStage(questName, Convert.ToInt32(questStage));
        }

        [Command(className = "Campaign")]
        static void ListAllQuests()
        {
            ConsoleModule.WriteLine(GameState.Instance.CampaignState.ListAllQuests());
        }
    }
}