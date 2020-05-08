using CommonCore;
using CommonCore.DebugLog;
using CommonCore.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Temporary "test" console commands for trying out various things
/// </summary>
public static class TestConsoleCommands
{
    [Command]
    public static void CCTestCommand()
    {
        Debug.Log("hello world");
    }

    [Command]
    public static void CCTestArgCommand(string mystring)
    {
        Debug.Log(mystring);
    }

    [Command]
    public static void DumpQualitySettings()
    {
        Dictionary<string, Dictionary<string, object>> allQSettings = new Dictionary<string, Dictionary<string, object>>();

        for(int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, true);
            Dictionary<string, object> qualitySettings = new Dictionary<string, object>();
            var props = typeof(QualitySettings).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach(var prop in props)
            {
                qualitySettings.Add(prop.Name, prop.GetValue(null));
            }
            allQSettings.Add(i.ToString(), qualitySettings);
        }

        DebugUtils.JsonWrite(allQSettings, "qualitysettings");
    }

}
