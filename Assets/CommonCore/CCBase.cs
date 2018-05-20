using CommonCore.Messaging;
using System;
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
        public static bool Initialized { get; private set; }
        private static List<CCModule> Modules;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnApplicationStart()
        {
            if (!CCParams.AutoInit)
                return;

            Debug.Log("Initializing CommonCore...");

            HookApplicationQuit();
            HookSceneEvents();

            Modules = new List<CCModule>();

            InitializeEarlyModules();
            InitializeModules();

            Initialized = true;
            Debug.Log("...done!");
        }

        private static void InitializeEarlyModules()
        {
            //TODO initialize Debug, Config, Console, MessageBus

            Modules.Add(new QdmsMessageBus());
        }

        private static void InitializeModules()
        {
            //TODO initialize other modules using reflection
        }

        private static void HookSceneEvents()
        {            
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Debug.Log("Hooked scene events!");
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("CommonCore: Executing OnSceneLoaded...");

            foreach(CCModule m in Modules)
            {
                try
                {
                    m.OnSceneLoaded();
                }
                catch(Exception e)
                {
                    Debug.Log(e);
                }
            }

            Debug.Log("CommonCore: ...done!");
        }

        static void OnSceneUnloaded(Scene current)
        {
            Debug.Log("CommonCore: Executing OnSceneUnloaded...");

            foreach (CCModule m in Modules)
            {
                try
                {
                    m.OnSceneUnloaded();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            Debug.Log("CommonCore: ...done!");
        }

        //Game start and end are not hooked and must be explicitly called
        public static void OnGameStart()
        {
            Debug.Log("CommonCore: Executing OnGameStart...");

            foreach (CCModule m in Modules)
            {
                try
                {
                    m.OnGameStart();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            Debug.Log("CommonCore: ...done!");
        }

        public static void OnGameEnd()
        {
            Debug.Log("CommonCore: Executing OnGameEnd...");

            foreach (CCModule m in Modules)
            {
                try
                {
                    m.OnGameEnd();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            Debug.Log("CommonCore: ...done!");
        }

        private static void HookApplicationQuit()
        {
            //TODO hook application unload (will be a bit hacky)
        }

        static void OnApplicationQuit()
        {
            Debug.Log("Cleaning up CommonCore...");

            //execute quit methods and unload modules
            foreach(CCModule m in Modules)
            {
                try
                {
                    m.OnApplicationQuit();
                }
                catch(Exception e)
                {
                    Debug.Log(e);
                }
            }

            Modules = null;
            GC.Collect();

            Debug.Log("...done!");
        }

    }
}