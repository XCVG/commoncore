using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;

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

        public ConsoleModule()
        {
            GameObject ConsolePrefab = Resources.Load<GameObject>("DevConsole");
            ConsoleObject = GameObject.Instantiate(ConsolePrefab);

            //any hooking into the console could be done here
            AddCommands();

            Debug.Log("Console module loaded!");
        }

        public override void OnApplicationQuit()
        {
            GameObject.Destroy(ConsoleObject);
            Debug.Log("Console module unloaded!");
        }

        public void AddCommands()
        {
            DevConsole.singleton.AddCommand(new ActionCommand(Quit) { useClassName = false });
            DevConsole.singleton.AddCommand(new FuncCommand<string>(GetVersion) {className = "Core"});
        }

        string GetVersion()
        {
            return string.Format("{0} {1} (Unity {2})", CCParams.VersionCode, CCParams.VersionName, CCParams.UnityVersion);
        }

        void Quit()
        {
            Application.Quit();
        }
    }
}