using CommonCore;
using CommonCore.Console;
using System;
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

    public void WriteLineEx(string line, LogLevel type, object context)
    {
        switch (type)
        {
            case LogLevel.Error:
                {
                    if (context is UnityEngine.Object uContext)
                        Debug.LogError(line, uContext);
                    else
                        Debug.LogError(line);
                }
                break;
            case LogLevel.Warning:
                {
                    if (context is UnityEngine.Object uContext)
                        Debug.LogWarning(line, uContext);
                    else
                        Debug.LogWarning(line);
                }
                break;
            default:
                {
                    if (context is UnityEngine.Object uContext)
                        Debug.Log(line, uContext);
                    else
                        Debug.Log(line);
                }
                break;
        }
    }
}
