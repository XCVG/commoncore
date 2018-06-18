using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;
using CommonCore.Messaging;

namespace CommonCore.Console
{
    /*
     * CommonCore Console Integration Module
     * Provides lifecycle management and integration of third-party command console
     */
    [CCExplicitModule]
    public class ConsoleModule : CCModule
    {
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

        public override void OnApplicationQuit()
        {
            ConsoleMessagingThing = null;
            GameObject.Destroy(ConsoleObject);
            Debug.Log("Console module unloaded!");
        }

        public void AddCommands()
        {
            DevConsole.singleton.AddCommand(new ActionCommand(Quit) { useClassName = false });
            DevConsole.singleton.AddCommand(new FuncCommand<string>(GetVersion) { className = "Core" });
        }

        string GetVersion()
        {
            return string.Format("{0} {1} (Unity {2})", CCParams.VersionCode, CCParams.VersionName, CCParams.UnityVersion);
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
}