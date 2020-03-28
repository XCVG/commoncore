using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;
using CommonCore.State;

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
}
