using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore
{
    /*
     * CommonCore Base class
     * Initializes CommonCore components
     */
    public class CCBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnGameStart()
        {
            Debug.Log("Initializing CommonCore...");

            HookSceneLoad();

            //TODO initialize modules

            Debug.Log("...done!");
        }

        private static void HookSceneLoad()
        {            
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("Hooked scene load!");
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded: " + scene.name);
            Debug.Log(mode);
        }
    }
}