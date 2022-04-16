using CommonCore.Config;
using CommonCore.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CommonCore.BasicConsole
{
    /// <summary>
    /// IConsole implementation using homebuilt basic command console
    /// </summary>
    public class BasicCommandConsoleImplementation : IConsole, IDisposable
    {
        private BasicConsoleController Console;

        private Dictionary<string, List<MethodInfo>> Commands = new Dictionary<string, List<MethodInfo>>();

        public BasicCommandConsoleImplementation()
        {
            var consoleObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Modules/BasicConsole/BasicConsole")); //we use Resources.Load because ResourceManager isn't initialized yet
            Console = consoleObject.GetComponent<BasicConsoleController>();
            Console.SetBaseImplementation(this);
        }

        public void Dispose()
        {
            if (Console != null)
                UnityEngine.Object.Destroy(Console.gameObject);
        }

        /// <summary>
        /// Adds a command to the parser
        /// </summary>
        /// <see cref="IConsole.AddCommand(MethodInfo, bool, string, string, string)"/>
        public void AddCommand(MethodInfo command, bool useClassName, string alias, string className, string description)
        {
            //figure out the name, do nothing with the description
            string commandName = string.IsNullOrEmpty(alias) ? command.Name : alias;
            if(!string.IsNullOrEmpty(className))
                commandName = $"{className}.{commandName}";
            else if (useClassName)
                commandName = $"{command.ReflectedType.Name}.{commandName}";

            //dump this command into its bucket
            if (!Commands.ContainsKey(commandName))
                Commands.Add(commandName, new List<MethodInfo>());

            Commands[commandName].Add(command);
        }

        /// <summary>
        /// Writes a line to the console
        /// </summary>
        /// <see cref="IConsole.WriteLine(string)"/>
        public void WriteLine(string line)
        {
            Console?.HandleExplicitLog(line, LogType.Log);
        }

        /// <summary>
        /// Writes a line to the console
        /// </summary>
        /// <see cref="IConsole.WriteLineEx(string, LogLevel, object)"/>
        public void WriteLineEx(string line, LogLevel type, object context)
        {
            if (type == LogLevel.Verbose && !ConfigState.Instance.UseVerboseLogging)
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

        /// <summary>
        /// Executes a command from a command string
        /// </summary>
        internal void ExecuteCommand(string commandLine)
        {
            string[] splitCommand = SplitCommandLine(commandLine);

            //Debug.Log(splitCommand.ToNiceString());

            if (Commands.ContainsKey(splitCommand[0]))
            {
                int numArgs = splitCommand.Length - 1;
                List<MethodInfo> commandMethodsList = Commands[splitCommand[0]];

                //dependency resolution is pretty stupid: we look at the number of arguments
                var matchingCommandMethods = commandMethodsList.Where(m => m.GetParameters().Length == numArgs).ToArray();
                if(matchingCommandMethods.Length == 0)
                {
                    Debug.LogError($"Failed to run command '{splitCommand[0]}' because no methods with matching parameter count were found!");
                    ConsoleModule.WriteLine("Did you mean:\n" + string.Join("\n", FindSimilarCommands(splitCommand[0], splitCommand.Length - 1)));
                }
                else
                {
                    if (matchingCommandMethods.Length > 1)
                        Debug.LogWarning($"Command '{splitCommand[0]}' has {matchingCommandMethods.Length} possible methods!\n" + matchingCommandMethods.Select(m => m.ReflectedType.Name + "." + m.Name).ToNiceString());

                    MethodInfo commandMethod = matchingCommandMethods[0];
                    ParameterInfo[] parameterInfos = commandMethod.GetParameters();

                    //coerce the arguments into fitting
                    object[] coercedArgs = new object[numArgs];
                    for (int argNum = 0; argNum < numArgs; argNum++)
                    {
                        var parameterInfo = parameterInfos[argNum];
                        if (parameterInfo.ParameterType == typeof(string))
                            coercedArgs[argNum] = splitCommand[argNum + 1];
                        else
                            coercedArgs[argNum] = Convert.ChangeType(splitCommand[argNum + 1], parameterInfo.ParameterType);
                    }

                    try
                    {
                        commandMethod.Invoke(null, coercedArgs);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"Command {splitCommand[0]} failed: {(e.InnerException ?? e).GetType().Name}");
                        Debug.LogException(e);
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to run command '{splitCommand[0]}' because no methods with matching name were found!");
                ConsoleModule.WriteLine("Did you mean:\n" + string.Join("\n", FindSimilarCommands(splitCommand[0], splitCommand.Length - 1)));
            }
        }

        /// <summary>
        /// Finds commands that are similarly named to an attempted command
        /// </summary>
        /// <remarks>
        /// <para>We will eventually expand this to using Levenshtein distance or something</para>
        /// </remarks>
        private string[] FindSimilarCommands(string attemptedCommand, int numArgs)
        {
            List<string> results = new List<string>();

            if(Commands.ContainsKey(attemptedCommand)) //we have a command but either a params number or type mismatch
            {
                foreach(var method in Commands[attemptedCommand])
                {
                    results.Add(getCommandString(attemptedCommand, method));
                }
            }
            else //we don't have a matching command at all
            {
                List<KeyValuePair<string, List<MethodInfo>>> matchingCommands = new List<KeyValuePair<string, List<MethodInfo>>>(); //wtf am i doing with my life

                foreach(var key in Commands.Keys)
                {
                    if(key.Equals(attemptedCommand, StringComparison.InvariantCultureIgnoreCase) || 
                        key.StartsWith(attemptedCommand, StringComparison.InvariantCultureIgnoreCase) ||
                        key.EndsWith(attemptedCommand, StringComparison.InvariantCultureIgnoreCase))
                    {
                        matchingCommands.Add(new KeyValuePair<string, List<MethodInfo>>(key, Commands[key]));
                    }
                }

                foreach(var kvp in matchingCommands)
                {
                    foreach(var method in kvp.Value)
                    {
                        results.Add(getCommandString(kvp.Key, method));
                    }
                }
            }

            return results.ToArray();

            string getCommandString(string cname, MethodInfo m) //this generates a lot of garbage
            {
                string cs = cname;
                foreach (var p in m.GetParameters())
                {
                    cs += $" {p.ParameterType} {p.Name},";
                }
                if (cs.EndsWith(","))
                    cs = cs.Substring(0, cs.Length - 1);

                return cs.Trim();
            }
        }

        /// <summary>
        /// Splits a command line by whitespace, except for quoted strings
        /// </summary>
        /// <remarks>
        /// Based on LINQ magic from https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
        /// </remarks>
        private string[] SplitCommandLine(string commandLine)
        {
            return commandLine.Split('"')
                     .Select((element, index) => ( (index % 2 == 0) ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[] { element }))
                     .SelectMany(element => element).ToArray();
        }
    }
}