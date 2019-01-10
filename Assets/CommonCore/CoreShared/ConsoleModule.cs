using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;
using CommonCore.Messaging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CommonCore.Console
{

    /// <summary>
    /// CommonCore Console Integration Module
    /// Provides integration of third-party console components
    /// WIP - will eventually have flexible backends
    /// </summary>
    [CCExplicitModule]
    public class ConsoleModule : CCModule
    {
        //TODO change this over to provide actual abstraction

        private static ConsoleModule Instance;

        private GameObject ConsoleObject;
        private ConsoleMessagingIntegrationComponent ConsoleMessagingThing;

        /// <summary>
        /// Initialize the Console module
        /// </summary>
        public ConsoleModule()
        {
            Instance = this;

            GameObject ConsolePrefab = Resources.Load<GameObject>("DevConsole");
            ConsoleObject = GameObject.Instantiate(ConsolePrefab);

            //any hooking into the console could be done here
            AddCommands();
            ConsoleMessagingThing = new ConsoleMessagingIntegrationComponent();

            Debug.Log("Console module loaded!");
        }

        public override void Dispose()
        {
            base.Dispose();

            ConsoleMessagingThing = null;
            GameObject.Destroy(ConsoleObject);
            Debug.Log("Console module unloaded!");
        }

        /// <summary>
        /// Register a command with the command parser
        /// </summary>
        /// <param name="command">The method the command will execute</param>
        /// <param name="useClassName">Whether to prepend the class name or not</param>
        /// <param name="alias">Override for command name (optional)</param>
        /// <param name="className">Override for the class name (optional)</param>
        /// <param name="description">Description of the command (optional)</param>
        public static void RegisterCommand(Delegate command, bool useClassName, string alias, string className, string description)
        {
            Instance.AddCommand(command, useClassName, alias, className, description);
        }

        /// <summary>
        /// Write a line of text to the active command console
        /// </summary>
        /// <param name="text">The text to write</param>
        public static void WriteLine(string text)
        {
            DevConsole.singleton.Log(text);
        }

        private void AddCommands()
        {
            DevConsole.singleton.AddCommand(new ActionCommand(Quit) { useClassName = false });
            DevConsole.singleton.AddCommand(new FuncCommand<string>(GetVersion) { className = "Core" });

            //this is pretty tightly coupled still but we'll fix that soon enough

            var allCommands = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !(a.FullName.StartsWith("Unity") || a.FullName.StartsWith("System") ||
                            a.FullName.StartsWith("mscorlib") || a.FullName.StartsWith("mono") ||
                            a.FullName.StartsWith("Boo") || a.FullName.StartsWith("I18N")))
                .SelectMany((assembly) => assembly.GetTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                .ToArray();

            Debug.Log($"Registering {allCommands.Length} console commands!");

            foreach(var command in allCommands)
            {
                try
                {
                    CommandAttribute commandAttr = (CommandAttribute)command.GetCustomAttributes(typeof(CommandAttribute), false)[0];
                    AddCommand(CreateDelegate(command), commandAttr.useClassName, commandAttr.alias, commandAttr.className, commandAttr.description);
                }
                catch(Exception e)
                {
                    Debug.LogError("Failed to add command " + command.Name);
                    Debug.LogException(e);
                }
            }
        }
               

        private void AddCommand(Delegate command, bool useClassName, string alias, string className, string description)
        {
            SickDev.CommandSystem.Command sdCommand = new Command(command);

            if (!string.IsNullOrEmpty(alias))
                sdCommand.alias = alias;
            if (!string.IsNullOrEmpty(className))
                sdCommand.className = className;
            if (!string.IsNullOrEmpty(description))
                sdCommand.description = description;
            sdCommand.useClassName = useClassName;

            DevConsole.singleton.AddCommand(sdCommand);
        }

        private static Delegate CreateDelegate(MethodInfo methodInfo)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType.Equals((typeof(void)));
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isAction)
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
            }

            throw new ArgumentException("Method must be static!", "methodInfo");
        }

        //System commands TODO MOVE

        private static string GetVersion()
        {
            return string.Format("{0} {1} (Unity {2})", CoreParams.VersionCode, CoreParams.VersionName, CoreParams.UnityVersion);
        }

        private static void Quit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Provides integration between the console system and messaging system
        /// </summary>
        private class ConsoleMessagingIntegrationComponent : IQdmsMessageReceiver
        {
            public ConsoleMessagingIntegrationComponent()
            {
                QdmsMessageBus.Instance.RegisterReceiver(this);
            }

            ~ConsoleMessagingIntegrationComponent()
            {
                QdmsMessageBus.Instance.UnregisterReceiver(this);
            }

            bool IQdmsMessageReceiver.IsValid //AFAIK this is only used for destroying components
            {
                get
                {
                    return true;
                }

                set
                {
                    //do nothing
                }
            }

            void IQdmsMessageReceiver.ReceiveMessage(QdmsMessage msg)
            {
                if(msg is HUDPushMessage)
                {
                    DevConsole.singleton.Log(string.Format("{0} : {1}", "*HUD PUSH MESSAGE*", ((HUDPushMessage)msg).Contents));
                }
            }
        }

    }

    /// <summary>
    /// Test commands for the command system
    /// </summary>
    public class TestCommands
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
    }
}