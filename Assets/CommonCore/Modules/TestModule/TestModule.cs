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
            Log("Test module loaded!");
        }

        public override void Dispose()
        {
            Log("Test module unloaded!");
        }

        public override void OnSceneLoaded()
        {
            Log("Test module: scene loaded!");
        }

        public override void OnSceneUnloaded()
        {
            Log("Test module: scene unloaded!");
        }

        public override void OnGameStart()
        {
            Log("Test module: game start!");
        }

        public override void OnGameEnd()
        {
            Log("Test module: game end!");
        }

        public override void OnAllModulesLoaded()
        {
            Log("Test module: all modules loaded!");
        }

    }
}