using CommonCore.Console;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CommonCore.BasicConsole
{
    public class BasicCommandConsoleImplementation : IConsole
    {
        private BasicConsoleController Console;

        public BasicCommandConsoleImplementation()
        {
            var consoleObject = Object.Instantiate(CoreUtils.LoadResource<GameObject>("BasicConsole/BasicConsole"));
            Console = consoleObject.GetComponent<BasicConsoleController>();

            //TODO other setup
        }

        public void AddCommand(MethodInfo command, bool useClassName, string alias, string className, string description)
        {
            //throw new System.NotImplementedException();
            //quietly nop for now
        }

        public void WriteLine(string line)
        {
            Console?.HandleExplicitLog(line, LogType.Log);
        }

        public void WriteLineEx(string line, LogLevel type, object context)
        {
            if (type == LogLevel.Verbose && !CoreParams.UseVerboseLogging)
                return;

            LogType logType;
            switch (type)
            {
                case LogLevel.Error:
                    logType = LogType.Error;
                    break;
                case LogLevel.Warning:
                    logType = LogType.Warning;
                    break;
                default:
                    logType = LogType.Log;
                    break;
            }

            Console?.HandleExplicitLog(line, logType);
        }
    }
}