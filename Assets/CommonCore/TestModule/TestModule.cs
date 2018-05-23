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

        ~TestModule()
        {
            Debug.Log("Test module unloaded!");
        }

    }
}