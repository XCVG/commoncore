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
            if (!CCParams.AutoInit)
                return;

            Debug.Log("Initializing CommonCore...");

            HookApplicationQuit();
            HookSceneLoad();            

            InitializeModules();

            Debug.Log("...done!");
        }

        private static void InitializeModules()
        {
            //TODO initialize modules, try using reflection
        }

        private static void HookSceneLoad()
        {            
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Debug.Log("Hooked scene load!");
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded: " + scene.name);
            Debug.Log(mode);
        }

        static void OnSceneUnloaded(Scene current)
        {
            Debug.Log("OnSceneUnloaded: " + current);
        }

        private static void HookApplicationQuit()
        {
            //TODO hook application unload (will be a bit hacky)
        }

    }
}