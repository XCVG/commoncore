using CommonCore;
using CommonCore.Console;
using SickDev.CommandSystem;
using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Console interface that uses the third-party SickDev console system
/// </summary>
public class SickDevConsoleImplementation : IConsole, IDisposable
{
    private GameObject ConsoleObject;

    public SickDevConsoleImplementation()
    {
        GameObject ConsolePrefab = Resources.Load<GameObject>("DevConsole");
        ConsoleObject = GameObject.Instantiate(ConsolePrefab);
    }

    public void Dispose()
    {
        if (ConsoleObject != null)
            UnityEngine.Object.Destroy(ConsoleObject);
    }

    public void AddCommand(MethodInfo command, bool useClassName, string alias, string className, string description)
    {
        Command sdCommand = new Command(ConsoleModule.CreateDelegate(command));

        sdCommand.useClassName = useClassName;

        if (!string.IsNullOrEmpty(alias))
            sdCommand.alias = alias;
        if (!string.IsNullOrEmpty(className))
        {
            sdCommand.className = className;
            sdCommand.useClassName = true;
        }
        if (!string.IsNullOrEmpty(description))
            sdCommand.description = description;


        DevConsole.singleton.AddCommand(sdCommand);
    }    

    public void WriteLine(string line)
    {
        DevConsole.singleton.Log(line);
    }

    public void WriteLineEx(string line, LogLevel type, object context)
    {
        switch (type)
        {
            case LogLevel.Error:
                DevConsole.singleton.LogError(line);
                break;
            case LogLevel.Warning:
                DevConsole.singleton.LogWarning(line);
                break;
            default:
                DevConsole.singleton.Log(line);
                break;
        }
    }
}
