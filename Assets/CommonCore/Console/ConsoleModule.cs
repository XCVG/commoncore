using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            Debug.Log("Console module loaded!");
        }

        public override void OnApplicationQuit()
        {
            GameObject.Destroy(ConsoleObject);
            Debug.Log("Console module unloaded!");
        }
    }
}