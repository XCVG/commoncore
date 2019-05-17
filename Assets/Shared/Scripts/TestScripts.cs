using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;

public static class TestScripts
{

    [CCScript]
    public static bool LogAndReturnTrue(string arg)
    {
        Debug.Log("LogAndReturnTrue received arg " + arg);
        return true;
    }
}
