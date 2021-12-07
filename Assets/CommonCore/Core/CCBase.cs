using CommonCore.Migrations;
using CommonCore.ResourceManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <summary>
        /// A collection of all relevant (ie not system or unity) types available when the game was started
        /// </summary>
        public static ImmutableArray<Type> BaseGameTypes { get; private set; }

        /// <summary>
        /// Whether the game is being initialized (CommonCore starting up)
        /// </summary>
        public static bool Initializing { get; private set; }

        /// <summary>
        /// Whether the game has been initialized or not (all modules loaded)
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Whether the game has been terminated (CommonCore shut down)
        /// </summary>
        public static bool Terminated { get; private set; }

        /// <summary>
        /// Whether the game has failed (CommonCore failed to start up or encountered a critical error)
        /// </summary>
        public static bool Failed { get; private set; }

        /// <summary>
        /// The scene to load after completing initialization
        /// </summary>
        public static string LoadSceneAfterInit { get; private set; } = "MainMenuScene";


        /// <summary>
        /// Loaded modules
        /// </summary>
        private static List<CCModule> Modules = new List<CCModule>();

        /// <summary>
        /// Lookup table for modules by type
        /// </summary>
        private static Dictionary<Type, CCModule> ModulesByType;

        public static ResourceManager ResourceManager { get; private set; }
        public static AddonManager AddonManager { get; private set; }

        /// <summary>
        /// Retrieves a loaded module specified by the type parameter
        /// </summary>
        public static T GetModule<T>() where T : CCModule
        {
            if (Modules == null || Modules.Count < 1)
                return null;

            if (ModulesByType != null && ModulesByType.Count > 0)
            {
                if (ModulesByType.TryGetValue(typeof(T), out var module))
                    return (T)module;
            }

            foreach (CCModule module in Modules)
            {
                if (module is T)
                    return (T)module;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a loaded module specified by type
        /// </summary>
        public static CCModule GetModule(Type moduleType)
        {
            if (Modules == null || Modules.Count < 1)
                return null;

            if (ModulesByType != null && ModulesByType.Count > 0)
            {
                if (ModulesByType.TryGetValue(moduleType, out var module))
                    return module;
            }

            foreach (CCModule module in Modules)
            {
                if (module.GetType() == moduleType)
                    return module;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a loaded module specified by name
        /// </summary>
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

        //entry point for early startup
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnApplicationStart()
        {
            if(CoreParams.AutoInit)
            {
                LoadSceneAfterInit = CoreParams.MainMenuScene;
                if(CoreParams.StartupPolicy == StartupPolicy.SynchronousEarly)
                    Startup();
                else if(Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    Debug.LogWarning($"Startup type {CoreParams.StartupPolicy} not supported on WebGL, starting up immediately instead!");
                    Startup();
                }
            }
                
        }

        //entry point for late startup
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnApplicationStartLate()
        {
            if (Initializing || Initialized)
                return;

            if (CoreParams.AutoInit)
            {
                LoadSceneAfterInit = CoreParams.MainMenuScene;
                var currentScene = SceneManager.GetActiveScene();
                if (currentScene != null && currentScene.name != "InitScene")
                {
                    Debug.LogWarning("[Core] Warning: trying to do a late startup from outside InitScene!");

                    if (CoreParams.StartupPolicy == StartupPolicy.Synchronous)
                        SceneManager.LoadScene(currentScene.name); //this actually reloads the scene the next frame, and since we complete our startup in 1 frame...
                                                                   //it's hacky as shit but should work for now
                    else if (CoreParams.StartupPolicy == StartupPolicy.Asynchronous)
                    {
                        SceneManager.LoadScene("InitScene");
                        LoadSceneAfterInit = currentScene.name;
                    }
                }

                if (CoreParams.StartupPolicy == StartupPolicy.Synchronous)
                    Startup();
                else if (CoreParams.StartupPolicy == StartupPolicy.Asynchronous)
                    StartupAsync();
                else
                    Debug.LogError($"[Core] Fatal error: unknown startup policy \"{CoreParams.StartupPolicy}\"");
            }
        }
        
        //synchronous startup method
        public static void Startup()
        {
            try
            {
                Initializing = true;
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                Debug.Log("[Core] Starting up CommonCore...");
                DoInitialSetup();

                var allModules = GetAllModuleTypes();
                InitializeExplicitModules(allModules);
                InitializeResourceManager();
                PrintSystemData(); //we wait until the console is loaded so we can see it in the console
                InitializeModules(allModules);
                SetupModuleLookupTable();
                ExecuteAllModulesLoaded();

                //mod loading can't happen synchronously
                AddonManager.WarnIfUnsupported();
                AddonManager.WarnOnSyncLoad();
                ExecuteAllAddonsLoaded();

                CoreUtils.CollectGarbage(true);

                Initialized = true;
                Initializing = false;

                stopwatch.Stop();

                Debug.Log($"[Core] ...done! ({stopwatch.Elapsed.TotalMilliseconds:F4} ms)");
            }
            catch(Exception e)
            {
                Failed = true;
                Debug.LogError($"[Core] Fatal error in startup: {e.GetType().Name}");
                Debug.LogException(e);
                throw new StartupFailedException(e);
            }
        }        

        //async startup method (not implemented)
        public static async void StartupAsync()
        {
            Initializing = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Debug.Log("[Core] Starting up CommonCore asynchronously...");

            try
            {
                await Task.Yield(); //wait for a scene transition if we had to do that

                DoInitialSetup();                

                var allModules = GetAllModuleTypes();
                await InitializeExplicitModulesAsync(allModules);
                InitializeResourceManager(); //this will be made async someday I think
                PrintSystemData(); //we wait until the console is loaded so we can see it in the console
                await InitializeModulesAsync(allModules);
                SetupModuleLookupTable();
                ExecuteAllModulesLoaded();

                //mod loading will happen here
                AddonManager.WarnIfUnsupported();
                await AddonManager.LoadStreamingAssetsAsync(ExecuteAddonLoaded);
                await AddonManager.LoadAddonsAsync(ExecuteAddonLoaded);
                ExecuteAllAddonsLoaded();

                CoreUtils.CollectGarbage(true);

                Initialized = true;
                Initializing = false;

                stopwatch.Stop();

                Debug.Log($"[Core] ...done! ({stopwatch.Elapsed.TotalMilliseconds:F4} ms)");                
            }
            catch(Exception e)
            {
                Failed = true;
                Debug.LogError($"[Core] Fatal error in startup: {e.GetType().Name}");
                Debug.LogException(e);
                throw new StartupFailedException(e);
            }

        }

        //sets up CoreParams, loads types, hooks some events and creates folders
        //TODO we may make some of this async later but for now we won't
        private static void DoInitialSetup()
        {
            CoreParams.SetInitial();
            CoreParams.LoadOverrides();
            LoadGameTypes();
            MigrationsManager.Instance.LoadMigrationsFromTypes(BaseGameTypes);
            HookMonobehaviourEvents();
            HookQuitEvent();
            HookSceneEvents();
            CreateFolders();
        }

        private static void InitializeResourceManager()
        {
            //TODO async?
            CoreUtils.ResourceManager = new LegacyResourceManager();
            ResourceManager = new ResourceManager();
            AddonManager = new AddonManager();
        }

        private static void LoadGameTypes()
        {
            //TODO refine excluded assemblies
            BaseGameTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !(a.FullName.StartsWith("Unity") || a.FullName.StartsWith("System") || a.FullName.StartsWith("netstandard") ||
                            a.FullName.StartsWith("mscorlib") || a.FullName.StartsWith("mono", StringComparison.OrdinalIgnoreCase) ||
                            a.FullName.StartsWith("Boo") || a.FullName.StartsWith("I18N")))
                .SelectMany((assembly) => assembly.GetTypes())
                .ToImmutableArray();

        }

        private static void PrintSystemData()
        {
            //this is not efficient, but it's a hell of a lot more readable than a gigantic string.format
            StringBuilder sb = new StringBuilder(1024);

            sb.AppendLine("---------------SYSTEM INFO---------------");
            sb.AppendFormat("{1} v{3} {4} by {0} (appversion: {2})\n", Application.companyName, Application.productName, Application.version, Application.version, CoreParams.GameVersionName);
            sb.AppendFormat("CommonCore {0} {1}\n", CoreParams.VersionCode.ToString(), CoreParams.VersionName);
            sb.AppendFormat("Unity {0} [{3} | {1} on {2}] [{4}]\n", Application.unityVersion, Application.platform, SystemInfo.operatingSystem, SystemInfo.graphicsDeviceType, CoreParams.ScriptingBackend);
            if (CoreParams.IsDebug)
                sb.AppendLine("[DEBUG MODE]");
            sb.AppendLine(SystemInfo.graphicsDeviceName);
            sb.AppendLine(Environment.CommandLine);
            sb.AppendFormat("DataPath: {0} | StreamingAssetsPath: {1} | GameFolderPath: {2}\n", CoreParams.DataPath, CoreParams.StreamingAssetsPath, CoreParams.GameFolderPath);
            sb.AppendFormat("PersistentDataPath: {0} | LocalDataPath: {1}\n", CoreParams.PersistentDataPath, CoreParams.LocalDataPath);
            sb.AppendFormat("SavePath: {0} | ScreenshotsPath: {1}\n", CoreParams.SavePath, CoreParams.ScreenshotsPath);            
            sb.AppendLine("-----------------------------------------");

            Debug.Log(sb.ToString());
        }

        private static List<Type> GetAllModuleTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany((assembly) => assembly.GetTypes())
                .Where((type) => typeof(CCModule).IsAssignableFrom(type))
                .Where((type) => (!type.IsAbstract && !type.IsGenericTypeDefinition))
                .Where((type) => null != type.GetConstructor(new Type[0]))
                .ToList();
        }

        private static void GetModuleLists(List<Type> allModules, out List<Type> earlyModules, out List<Type> normalModules, out List<Type> lateModules)
        {
            earlyModules = new List<Type>();
            normalModules = new List<Type>();
            lateModules = new List<Type>();
            foreach (var t in allModules)
            {
                if (t.GetCustomAttributes(typeof(CCExplicitModuleAttribute), true).Length > 0)
                    continue;

                bool isEarly = t.GetCustomAttributes(typeof(CCEarlyModuleAttribute), true).Length > 0;
                bool isLate = t.GetCustomAttributes(typeof(CCLateModuleAttribute), true).Length > 0;

                if (isEarly ^ isLate)
                {
                    if (isEarly)
                        earlyModules.Add(t);
                    else
                        lateModules.Add(t);
                }
                else
                {
                    if (isEarly && isLate)
                        Debug.LogWarning($"[Core] Module {t.Name} is declared as both early and late (attributes will be ignored)");

                    normalModules.Add(t);
                }
            }
        }

        private static void InitializeExplicitModules(List<Type> allModules)
        {
            Debug.Log("[Core] Initializing explicit modules!");
            foreach (string moduleName in CoreParams.ExplicitModules)
            {
                Type t = allModules.Find(x => x.Name == moduleName);
                if (t != null)
                {
                    InitializeModule(t);
                }
                else
                    Debug.LogError("[Core] Can't find explicit module " + moduleName);
            }
        }

        private static async Task InitializeExplicitModulesAsync(List<Type> allModules)
        {
            Debug.Log("[Core] Initializing explicit modules (async)!");
            foreach (string moduleName in CoreParams.ExplicitModules)
            {
                Type t = allModules.Find(x => x.Name == moduleName);
                if (t != null)
                {
                    await InitializeModuleAsync(t);
                }
                else
                    Debug.LogError("[Core] Can't find explicit module " + moduleName);
            }
        }

        private static void InitializeModules(List<Type> allModules)
        {
            //sort out our modules
            GetModuleLists(allModules, out List<Type> earlyModules, out List<Type> normalModules, out List<Type> lateModules);

            //initialize early modules
            Debug.Log("[Core] Initializing early modules!");
            foreach (var t in earlyModules)
            {
                InitializeModule(t);
            }

            Debug.Log("[Core] Initializing normal modules!");
            //initialize non-explicit modules
            foreach (var t in normalModules)
            {
                InitializeModule(t);
            }

            Debug.Log("[Core] Initializing late modules!");
            //initialize late modules
            foreach (var t in lateModules)
            {
                InitializeModule(t);
            }
        }

        private static async Task InitializeModulesAsync(List<Type> allModules)
        {
            //sort out our modules
            GetModuleLists(allModules, out List<Type> earlyModules, out List<Type> normalModules, out List<Type> lateModules);

            //initialize early modules
            Debug.Log("[Core] Initializing early modules (async)!");
            foreach (var t in earlyModules)
            {
                await InitializeModuleAsync(t);
            }

            Debug.Log("[Core] Initializing normal modules (async)!");
            //initialize non-explicit modules
            foreach (var t in normalModules)
            {
                await InitializeModuleAsync(t);
            }

            Debug.Log("[Core] Initializing late modules (async)!");
            //initialize late modules
            foreach (var t in lateModules)
            {
                await InitializeModuleAsync(t);
            }
        }

        private static void InitializeModule(Type moduleType)
        {
            try
            {
                if(Modules.Find(m => m.GetType() == moduleType) != null)
                {
                    Debug.LogWarning("[Core] Attempted to initialize existing module " + moduleType.Name);
                    return;
                }
                var module = (CCModule)Activator.CreateInstance(moduleType);
                if (module is CCAsyncModule aModule)
                {
                    if (aModule.CanLoadSynchronously)
                        aModule.Load();
                    else
                        throw new NotSupportedException($"Module \"{module.GetType().Name}\" cannot be loaded synchronously!");
                }
                Modules.Add(module);
                Debug.Log("[Core] Successfully loaded module " + moduleType.Name);
            }
            catch (Exception e)
            {
                Debug.LogError("[Core] Failed to load module " + moduleType.Name);
                Debug.LogException(e);
            }
        }

        private static async Task InitializeModuleAsync(Type moduleType)
        {
            try
            {
                if (Modules.Find(m => m.GetType() == moduleType) != null)
                {
                    Debug.LogWarning("[Core] Attempted to initialize existing module " + moduleType.Name);
                    return;
                }
                var module = (CCModule)Activator.CreateInstance(moduleType);
                await Task.Yield();
                if (module is CCAsyncModule aModule)
                {
                    await aModule.LoadAsync();
                }
                Modules.Add(module);
                Debug.Log("[Core] Successfully loaded module " + moduleType.Name);
            }
            catch (Exception e)
            {
                Debug.LogError("[Core] Failed to load module " + moduleType.Name);
                Debug.LogException(e);
            }
            
        }

        private static void SetupModuleLookupTable()
        {
            ModulesByType = new Dictionary<Type, CCModule>();

            foreach(var module in Modules)
            {
                var mType = module.GetType();
                if (!ModulesByType.ContainsKey(mType))
                    ModulesByType.Add(mType, module);
                else
                    Debug.LogError($"Tried to add module of type {mType.Name} to module lookup table but it already exists! (you have a duplicate module, which is bad)");
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
                    Debug.LogError($"[Core] Fatal error in module {m.GetType().Name} during {nameof(ExecuteAllModulesLoaded)}");
                    Debug.LogException(e);
                }
            }
        }

        private static void ExecuteAddonLoaded(AddonLoadData data)
        {
            //a bit ugly, but this is where it has to go
            MigrationsManager.Instance.LoadMigrationsFromAssemblies(data.LoadedAssemblies);

            foreach (var m in Modules)
            {
                try
                {
                    m.OnAddonLoaded(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Core] Fatal error in module {m.GetType().Name} during {nameof(ExecuteAddonLoaded)}");
                    Debug.LogException(e);
                }
            }
        }

        private static void ExecuteAllAddonsLoaded()
        {
            foreach (var m in Modules)
            {
                try
                {
                    m.OnAllAddonsLoaded();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Core] Fatal error in module {m.GetType().Name} during {nameof(ExecuteAllAddonsLoaded)}");
                    Debug.LogException(e);
                }
            }
        }

        private static void HookSceneEvents()
        {            
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Debug.Log("[Core] Hooked scene events!");
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("[Core] Executing OnSceneLoaded...");

            foreach(CCModule m in Modules)
            {
                try
                {
                    m.OnSceneLoaded();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Debug.Log("[Core] ...done!");
        }

        static void OnSceneUnloaded(Scene current)
        {
            Debug.Log("[Core] Executing OnSceneUnloaded...");

            foreach (CCModule m in Modules)
            {
                try
                {
                    m.OnSceneUnloaded();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Debug.Log("[Core] ...done!");
        }

        //Game start and end are not hooked and must be explicitly called
        public static void OnGameStart()
        {
            Debug.Log("[Core] Executing OnGameStart...");

            foreach (CCModule m in Modules)
            {
                try
                {
                    m.OnGameStart();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Debug.Log("[Core] ...done!");
        }

        public static void OnGameEnd()
        {
            Debug.Log("[Core] Executing OnGameEnd...");

            foreach (CCModule m in Modules)
            {
                try
                {
                    m.OnGameEnd();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Debug.Log("[Core] ...done!");
        }

        private static void HookMonobehaviourEvents()
        {
            GameObject hookObject = new GameObject(nameof(CCMonoBehaviourHook));
            CCMonoBehaviourHook hookScript = hookObject.AddComponent<CCMonoBehaviourHook>();
            hookScript.OnUpdateDelegate = new LifecycleEventDelegate(OnFrameUpdate);

            Debug.Log("[Core] Hooked MonoBehaviour events!");
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
                    Debug.LogException(e);
                }
            }
        }

        private static void HookQuitEvent()
        {
            Application.quitting += OnApplicationQuit;
        }

        internal static void OnApplicationQuit()
        {
            Debug.Log("[Core] Cleaning up CommonCore...");

            //execute quit methods and unload modules
            foreach(CCModule m in Modules)
            {
                try
                {
                    Debug.Log("[Core] Unloading module " + m.GetType().Name);
                    m.Dispose();
                }
                catch(Exception e)
                {
                    Debug.LogError("[Core] Failed to cleanly unload module " + m.GetType().Name);
                    Debug.LogException(e);
                }
            }

            Modules = null;
            CoreUtils.CollectGarbage(true);
            Terminated = true;

            Debug.Log("[Core] ...done!");
        }

        private static void CreateFolders()
        {
            try
            {
                Directory.CreateDirectory(CoreParams.LocalDataPath);
                Directory.CreateDirectory(CoreParams.PersistentDataPath);
                Directory.CreateDirectory(CoreParams.SavePath);
                Directory.CreateDirectory(CoreParams.FinalSavePath);
                Directory.CreateDirectory(CoreParams.DebugPath);
                Directory.CreateDirectory(CoreParams.ScreenshotsPath);
            }
            catch(Exception e)
            {
                Debug.LogError("[Core] Failed to setup directories (may cause problems during game execution)");
                Debug.LogException(e);
            }
        }

    }
}