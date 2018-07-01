using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.TestModule
{

    /*
     * CommonCore Test Module
     * Just a random dummy module to test functionality
     */
    public class TestModule : CCModule
    {

        public TestModule()
        {
            Debug.Log("Test module loaded!");
        }

        public override void OnApplicationQuit()
        {
            Debug.Log("Test module unloaded!");
        }

        public override void OnSceneLoaded()
        {
            Debug.Log("Test module: scene loaded!");
        }

        public override void OnSceneUnloaded()
        {
            Debug.Log("Test module: scene unloaded!");
        }

        public override void OnGameStart()
        {
            Debug.Log("Test module: game start!");
        }

        public override void OnGameEnd()
        {
            Debug.Log("Test module: game end!");
        }

    }
}