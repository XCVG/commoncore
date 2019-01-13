using CommonCore.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dummy command console implementation for testing instantiation
/// </summary>
public class DummyCommandConsoleImplementation : IConsole
{
    public void AddCommand(Delegate command, bool useClassName, string alias, string className, string description)
    {
        //we can't do anything here
    }

    public void WriteLine(string line)
    {
        //we can at least log this
        Debug.Log(line);
    }
}
