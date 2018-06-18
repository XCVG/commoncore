using CommonCore.Config;
using CommonCore.Console;
using CommonCore.DebugLog;
using CommonCore.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            if (CCParams.AutoloadModules)
            {
                InitializeEarlyModules();
                InitializeModules();
            }

            Initialized = true;
            Debug.Log("...done!");
        }

        private static void InitializeEarlyModules()
        {
            //initialize Debug, Config, Console, MessageBus
            Modules.Add(new DebugModule());
            Modules.Add(new QdmsMessageBus());
            Modules.Add(new ConfigModule());
            Modules.Add(new ConsoleModule());            
        }

        private static void InitializeModules()
        {
            //initialize other modules using reflection

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany((assembly) => assembly.GetTypes())
                .Where((type) => typeof(CCModule).IsAssignableFrom(type))
                .Where((type) => (!type.IsAbstract && !type.IsGenericTypeDefinition))
                .Where((type) => null != type.GetConstructor(new Type[0]))
                .Where((type) => type.GetCustomAttributes(typeof(CCExplicitModule),true).Length == 0)
                .ToArray();

            foreach (var t in types)
            {
                try
                {
                    Modules.Add((CCModule)Activator.CreateInstance(t));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
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
            //hook application unload (a bit hacky)
            GameObject hookObject = new GameObject();
            CCExitHook hookScript = hookObject.AddComponent<CCExitHook>();
            hookScript.OnApplicationQuitDelegate = new LifecycleEventDelegate(OnApplicationQuit);

            Debug.Log("Hooked application quit!");
        }

        internal static void OnApplicationQuit()
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