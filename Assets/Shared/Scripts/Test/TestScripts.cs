using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;
using CommonCore.State;
using System.Globalization;
using CommonCore;
using CommonCore.StringSub;

public static class TestScripts
{

    [CCScript]
    public static bool LogAndReturnTrue(ScriptExecutionContext ctx)
    {
        Debug.Log("LogAndReturnTrue");
        return true;
    }

    [CCScript]
    public static bool LogAndReturnTrueEx(ScriptExecutionContext ctx, string arg)
    {
        Debug.Log("LogAndReturnTrueEx: " + arg);
        return true;
    }

    [CCScript]
    public static void ToggleManualSave()
    {
        GameState.Instance.ManualSaveLocked = !GameState.Instance.ManualSaveLocked;
        Debug.Log($"Manual Save Locked: {GameState.Instance.ManualSaveLocked}");
    }

    [CCScript(ClassName = "Test", Name = "PrintLocale"), CCScriptHook(AllowExplicitCalls = true, Hook = ScriptHook.AfterModulesLoaded)]
    public static void PrintLocale()
    {
        Debug.Log("Current culture: " + CultureInfo.CurrentCulture);
    }

    [CCScript(ClassName = "Test", Name = "PrintColoredText")]
    public static void PrintColoredText()
    {
        Subtitle.Show("<color=blue>Blue text!</color>\n<color=red>Red text!</color>", 5f, true);
    }

    [CCScript(ClassName = "Test", Name = "LiteralStringSub"), CCScriptHook(AllowExplicitCalls = true, Hook = ScriptHook.AfterModulesLoaded)]
    public static void TestLiteralStringSub()
    {
        string testCase = "random text<p:\"I am a \\\"literal string\\\"!\">";
        Debug.Log($"{nameof(TestLiteralStringSub)}\n{testCase}\n{Sub.Macro(testCase)}");
    }
}
