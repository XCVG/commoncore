using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;
using CommonCore.Messaging;
using System;
using System.Linq;

namespace CommonCore.Console
{
    /*
     * CommonCore Console Integration Module
     * Provides lifecycle management and integration of third-party command console
     */
    [CCExplicitModule]
    public class ConsoleModule : CCModule
    {
        //TODO change this over to provide actual abstraction

        private GameObject ConsoleObject;
        private ConsoleMessagingIntegrationComponent ConsoleMessagingThing;

        public ConsoleModule()
        {
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

        public void AddCommands()
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
                    SickDev.CommandSystem.Command sdCommand;
                    if (command.ReturnType == typeof(string))
                    {
                        sdCommand = new FuncCommand<string>((Func<string>)Delegate.CreateDelegate(typeof(Func<string>), command));
                    }
                    else
                    {
                        sdCommand = new ActionCommand((Action)Delegate.CreateDelegate(typeof(Action), command));
                    }

                    CommandAttribute commandAttr = (CommandAttribute)command.GetCustomAttributes(typeof(CommandAttribute), false)[0];
                    if (!string.IsNullOrEmpty(commandAttr.alias))
                        sdCommand.alias = commandAttr.alias;
                    if (!string.IsNullOrEmpty(commandAttr.className))
                        sdCommand.className = commandAttr.className;
                    if (!string.IsNullOrEmpty(commandAttr.description))
                        sdCommand.description = commandAttr.description;
                    sdCommand.useClassName = commandAttr.useClassName;

                    DevConsole.singleton.AddCommand(sdCommand);
                }
                catch(Exception e)
                {
                    Debug.LogError("Failed to add command " + command.Name);
                    Debug.LogException(e);
                }
            }
        }

        string GetVersion()
        {
            return string.Format("{0} {1} (Unity {2})", CoreParams.VersionCode, CoreParams.VersionName, CoreParams.UnityVersion);
        }

        void Quit()
        {
            Application.Quit();
        }

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

    public class TestCommands
    {
        [Command]
        public static void CCTestCommand()
        {
            Debug.Log("hello world");
        }
    }
}