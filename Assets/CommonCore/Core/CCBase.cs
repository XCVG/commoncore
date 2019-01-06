using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            if (!CoreParams.AutoInit)
                return;

            Debug.Log("Initializing CommonCore...");

            HookMonobehaviourEvents();
            HookQuitEvent();
            HookSceneEvents();
            CreateFolders();

            Modules = new List<CCModule>();

            if (CoreParams.AutoloadModules)
            {
                PrintSystemData(); //we wait until the console is loaded so we can see it in the console
                InitializeModules();
                ExecuteAllModulesLoaded();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Initialized = true;
            Debug.Log("...done!");
        }

        public static T GetModule<T>() where T : CCModule
        {
            if (Modules == null || Modules.Count < 1)
                return null;

            foreach(CCModule module in Modules)
            {
                if (module is T)
                    return (T)module;
            }

            return null;
        }

        public static CCModule GetModule(string moduleName)
        {
            if (Modules == null || Modules.Count < 1)
                return null;

            foreach (CCModule module in Modules)
            {
                if (module.GetType().Name == moduleName)
                    return module;
            }

            return null;
        }

        private static void PrintSystemData()
        {
            //this is not efficient, but it's a hell of a lot more readable than a gigantic string.format
            StringBuilder sb = new StringBuilder(1024);

            sb.AppendLine("----------------------------------------");
            sb.AppendFormat("{1} v{3} {4} by {0} (appversion: {2})\n", Application.companyName, Application.productName, Application.version, Application.version, CoreParams.GameVersionName);
            sb.AppendFormat("CommonCore {0} {1}\n", CoreParams.VersionCode.ToString(), CoreParams.VersionName);
            sb.AppendFormat("Unity {0} [{3} | {1} on {2}]\n", Application.unityVersion, Application.platform, SystemInfo.operatingSystem, SystemInfo.graphicsDeviceType);
            sb.AppendLine("persistentDataPath: " + Application.persistentDataPath);
            sb.AppendLine("dataPath: " + Application.dataPath);
            sb.AppendLine(Environment.CommandLine);
            sb.AppendLine("----------------------------------------");

            Debug.Log(sb.ToString());
        }

        private static void InitializeModules()
        {
            //initialize modules using reflection

            var allModules = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany((assembly) => assembly.GetTypes())
                .Where((type) => typeof(CCModule).IsAssignableFrom(type))
                .Where((type) => (!type.IsAbstract && !type.IsGenericTypeDefinition))
                .Where((type) => null != type.GetConstructor(new Type[0]))                
                .ToList();

            //initialize explicit modules
            foreach(string moduleName in CoreParams.ExplicitModules)
            {
                Type t = allModules.Find(x => x.Name == moduleName);
                if(t != null)
                {
                    InitializeModule(t);
                }
                else
                    Debug.LogError("Can't find explicit module " + moduleName);
            }

            //sort out our modules
            var earlyModules = new List<Type>();
            var normalModules = new List<Type>();
            var lateModules = new List<Type>();

            foreach(var t in allModules)
            {
                if (t.GetCustomAttributes(typeof(CCExplicitModuleAttribute), true).Length > 0)
                    continue;

                bool isEarly = t.GetCustomAttributes(typeof(CCEarlyModuleAttribute), true).Length > 0;
                bool isLate = t.GetCustomAttributes(typeof(CCLateModuleAttribute), true).Length > 0;

                if(isEarly ^ isLate)
                {
                    if (isEarly)
                        earlyModules.Add(t);
                    else
                        lateModules.Add(t);
                }
                else
                {
                    if (isEarly && isLate)
                        Debug.LogWarning($"Module {t.Name} is declared as both early and late (attributes will be ignored)");

                    normalModules.Add(t);
                }
            }

            //initialize early modules
            foreach (var t in earlyModules)
            {
                InitializeModule(t);
            }

            //initialize non-explicit modules
            foreach (var t in normalModules)
            {
                InitializeModule(t);
            }

            //initialize late modules
            foreach(var t in lateModules)
            {
                InitializeModule(t);
            }
        }

        private static void InitializeModule(Type moduleType)
        {
            try
            {
                Modules.Add((CCModule)Activator.CreateInstance(moduleType));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static void ExecuteAllModulesLoaded()
        {
            foreach(var m in Modules)
            {
                try
                {
                    m.OnAllModulesLoaded();
                }
                catch(Exception e)
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

        private static void HookMonobehaviourEvents()
        {
            GameObject hookObject = new GameObject();
            CCMonoBehaviourHook hookScript = hookObject.AddComponent<CCMonoBehaviourHook>();
            hookScript.OnUpdateDelegate = new LifecycleEventDelegate(OnFrameUpdate);

            Debug.Log("Hooked MonoBehaviour events!");
        }

        internal static void OnFrameUpdate()
        {
            foreach (CCModule m in Modules)
            {
                try
                {
                    m.OnFrameUpdate();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        private static void HookQuitEvent()
        {
            Application.quitting += OnApplicationQuit;
        }

        internal static void OnApplicationQuit()
        {
            Debug.Log("Cleaning up CommonCore...");

            //execute quit methods and unload modules
            foreach(CCModule m in Modules)
            {
                try
                {
                    m.Dispose();
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

        private static void CreateFolders()
        {
            try
            {
                Directory.CreateDirectory(CoreParams.SavePath);
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to setup directories (may cause problems during game execution)");
                Debug.LogException(e);
            }
        }

    }
}