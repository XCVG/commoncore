using CommonCore.Messaging;
using CommonCore.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace CommonCore.Console
{

    /// <summary>
    /// CommonCore Console Integration Module
    /// Provides integration of third-party console components
    /// </summary>
    [CCExplicitModule]
    public class ConsoleModule : CCModule
    {

        private static ConsoleModule Instance;

        private IConsole Console;
        private ConsoleMessagingIntegrationComponent ConsoleMessagingThing;

        /// <summary>
        /// Initialize the Console module
        /// </summary>
        public ConsoleModule()
        {
            Instance = this;

            CreateConsole();

            //any hooking into the console could be done here
            AddCommands();
            ConsoleMessagingThing = new ConsoleMessagingIntegrationComponent();

        }

        /// <summary>
        /// Find a console implemntation and instantiate it
        /// </summary>
        private void CreateConsole()
        {
            Type[] possibleConsoles = CCBase.BaseGameTypes
                .Where((type) => type.GetInterfaces().Contains(typeof(IConsole)))
                .Where((type) => type != typeof(DummyCommandConsoleImplementation))
                .ToArray();

            Log(possibleConsoles.ToNiceString(t => t.Name));

            //get our preferred console implmentation...
            Type preferredConsole = Array.Find(possibleConsoles, (t) => t.Name == CoreParams.PreferredCommandConsole);

            //...fall back to whatever we can find
            if (preferredConsole == null)
            {
                if (possibleConsoles.Length > 0)
                    preferredConsole = possibleConsoles[0];
                else
                    preferredConsole = typeof(DummyCommandConsoleImplementation);
            }

            if (preferredConsole != null)
            {
                Console = (IConsole)Activator.CreateInstance(preferredConsole);

                Log($"Using {preferredConsole.Name} console implementation");
            }

        }

        /// <summary>
        /// Clean up the console module
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            ConsoleMessagingThing = null;
            (Console as IDisposable)?.Dispose();
            Log("Console module unloaded!");
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            AddCommandsFromAddon(data);
        }

        /// <summary>
        /// Register a command with the command parser
        /// </summary>
        /// <param name="command">The method the command will execute</param>
        /// <param name="useClassName">Whether to prepend the class name or not</param>
        /// <param name="alias">Override for command name (optional)</param>
        /// <param name="className">Override for the class name (optional)</param>
        /// <param name="description">Description of the command (optional)</param>
        public static void RegisterCommand(MethodInfo command, bool useClassName, string alias, string className, string description)
        {
            Instance.Console.AddCommand(command, useClassName, alias, className, description);
        }

        /// <summary>
        /// Write a line of text to the active command console
        /// </summary>
        /// <param name="text">The text to write</param>
        public static void WriteLine(string text)
        {
            Instance.Console.WriteLine(text);
        }

        /// <summary>
        /// Write a line of text to the active command console
        /// </summary>
        /// <param name="text">The text to write</param>
        /// <param name="type">The type of message to write</param>
        public static void WriteLine(string text, LogLevel type)
        {
            Instance.Console.WriteLineEx(text, type, null);
        }

        /// <summary>
        /// Write a line of text to the active command console
        /// </summary>
        /// <param name="text">The text to write</param>
        /// <param name="type">The type of message to write</param>
        /// <param name="context">The object that is writing the message</param>
        public static void WriteLine(string text, LogLevel type, object context)
        {
            Instance.Console.WriteLineEx(text, type, context);
        }

        private void AddCommandsFromAddon(AddonLoadData data)
        {
            if (data == null || data.LoadedAssemblies == null || data.LoadedAssemblies.Count == 0)
                return;

            IEnumerable<MethodInfo> commands = data.LoadedAssemblies.SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                .ToArray();

            AddCommands(commands);
        }

        private void AddCommands()
        {
            //DevConsole.singleton.AddCommand(new ActionCommand(Quit) { useClassName = false });
            //DevConsole.singleton.AddCommand(new FuncCommand<string>(GetVersion) { className = "Core" });

            //this is pretty tightly coupled still but we'll fix that soon enough

            MethodInfo[] allCommands = CCBase.BaseGameTypes
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                .ToArray();

            AddCommands(allCommands);
        }
        
        private void AddCommands(IEnumerable<MethodInfo> commands)
        {
            int numCommands = 0;

            foreach (var command in commands)
            {
                //Debug.Log(command.Name);

                try
                {
                    CommandAttribute commandAttr = (CommandAttribute)command.GetCustomAttributes(typeof(CommandAttribute), false)[0];
                    Console.AddCommand(command, commandAttr.useClassName, commandAttr.alias, commandAttr.className, commandAttr.description);
                    numCommands++;
                }
                catch (Exception e)
                {
                    LogError("Failed to add command " + command.Name);
                    LogException(e);
                }

            }

            Log($"Registered {numCommands} console commands!");
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
            }

            void IQdmsMessageReceiver.ReceiveMessage(QdmsMessage msg)
            {
                if(msg is HUDPushMessage)
                {
                    ConsoleModule.WriteLine(string.Format("{0} [{2}] : {1}", "*HUD PUSH MESSAGE*", ((HUDPushMessage)msg).Contents, ((HUDPushMessage)msg).Tags.ToNiceString()));
                }
            }
        }

    }

}