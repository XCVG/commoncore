using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// CommonCore Parameters- core config, versioning, paths, etc
    /// </summary>
    public static class CoreParams
    {

        //*****system version info
        public static Version VersionCode { get; private set; } = new Version(4, 0, 0); //4.0.0
        public static string VersionName { get; private set; } = "Downwarren";
        public static Version UnityVersion { get; private set; } //auto-set
        public static string UnityVersionName { get; private set; } //auto-set
        public static RuntimePlatform Platform { get; private set; } //auto-set
        public static ScriptingImplementation ScriptingBackend { get; private set; } //auto-set

        //*****game version info 
        public static string CompanyName { get; private set; } //auto-set from Unity settings
        public static string GameName { get; private set; } //auto-set from Unity settings
        public static Version GameVersion { get; private set; } //auto-set from Unity settings
        public static string GameVersionName { get; private set; } = "Indev";

        //*****basic config settings
        public static bool AutoInit { get; private set; } = true;
        public static ImmutableArray<string> ExplicitModules { get; private set; } = new string[] { "QdmsMessageBus", "ConfigModule", "DebugModule", "AsyncModule", "ScriptingModule", "ConsoleModule" }.ToImmutableArray();
        private static DataLoadPolicy LoadData = DataLoadPolicy.OnStart;
        public static ResourceManagerPolicy DefaultResourceManager { get; private set; } = ResourceManagerPolicy.UseNew;
        public static string PreferredCommandConsole { get; private set; } = "SickDevConsoleImplementation";

        public static StartupPolicy EditorStartupPolicy { get; private set; } = StartupPolicy.Asynchronous;
        public static StartupPolicy PlayerStartupPolicy { get; private set; } = StartupPolicy.Asynchronous;

        public static bool LoadAddons { get; private set; } = true; //if set, will allow loading addons if configured in ConfigState

        public static bool UseSeparateEditorConfigFile { get; private set; } = false; //if set, will use config.editor.json while in editor
        private static WindowsPersistentDataPath PersistentDataPathWindows = WindowsPersistentDataPath.Roaming;
        private static bool CorrectWindowsLocalDataPath = false; //if set, use AppData/Local/* instead of AppData/LocalLow/* for LocalDataPath
        private static bool UseGlobalScreenshotFolder = true; //ignored on UWP and probably other platforms
        public static bool UseMigrationBackups { get; private set; } = true; //if set, will save backups before migrating

        public static bool SetSafeResolutionOnExit { get; private set; } = true;
        public static Vector2Int SafeResolution { get; private set; } = new Vector2Int(1280, 720);

        public static UIThemePolicy UIThemeMode { get; private set; } = UIThemePolicy.ExplicitOnly;
        public static string DefaultUITheme { get; private set; } = "ThresholdTheme";

        public static float DelayedEventPollInterval { get; private set; } = 1.0f;
        //public static bool UseAggressiveLookups { get; private set; } = true; //may bring this back someday if performance is an issue
        public static int ResourceMaxRecurseDepth { get; private set; } = 32;
        public static bool TryLoadResourceManifest { get; private set; } = true;
        public static bool RequireResourceManifest { get; private set; } = false;
        public static bool EnableSpawnScriptingHooks { get; private set; } = true;

        public static bool AlwaysEnableGCBeforeCollect { get; private set; } = true;

        //*****scene settings        
        public static string MainMenuScene { get; private set; } = "MainMenuScene";
        public static string LoadingScene { get; private set; } = "LoadingScene";
        public static string InitialScene { get; private set; } = "TestScene";
        public static string GameOverScene { get; private set; } = "GameOverScene";


        //*****game config settings       
		public static long ReserveUIDs { get; private set; } = 10000L;
        public static bool UseCampaignIdentifier { get; private set; } = true;
        public static bool UseCampaignStartDate { get; private set; } = true;
        public static bool AllowSaveLoad { get; private set; } = true;
        public static bool AllowManualSave { get; private set; } = true;
        public static IReadOnlyList<string> AdditionalAxes { get; private set; } = ImmutableArray.Create<string>(); //specify additional axes your game will use; it's up to individual input mappers to handle these
        public static IReadOnlyList<string> AdditionalButtons { get; private set; } = ImmutableArray.Create<string>(); //same, but for buttons
        public static IReadOnlyList<string> HideControlMappings { get; private set; } = ImmutableArray.Create<string>("OpenMenu"); //add things to this to hide DefaultControls you're not using, note that it's not guaranteed to stop the control from responding to input
        public static bool ForcePlayerLightReporting { get; set; } = false; //forces spriteweapon/player light probe on, if available
        public static bool AlwaysPreactivateEntityPlaceholders { get; set; } = false; //forces ActivateEntityPlaceholders to run in world scenes even if Restore() is not called

        //*****module params (interim)
        public static IReadOnlyDictionary<string, object> ModuleParams { get; private set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
            { "TestModuleTest1", 1 },
            { "TestModule.Test2", "Two" },
            { "TestModule-Test3", 3.0f },
            { "TestModule", "this should be ignored" }
        }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase); //TODO should this be ignoring case?

        //*****path variables (some hackery to provide thread-safeish versions)
        public static string DataPath { get; private set; }
        public static string GameFolderPath { get; private set; }
        public static string PersistentDataPath { get; private set; }
        public static string SaveBasePath { get; private set; }
        public static string LocalDataPath { get; private set; }
        public static string StreamingAssetsPath { get; private set; }
        public static string ScreenshotsPath { get; private set; }

        //*****automatic environment params
        public static bool IsDebug
        {
            get
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static bool PlatformSupportsAsync
        {
            get
            {
#if UNITY_WEBGL
                return false;
#else
                return true;
#endif
            }
        }

        public static bool PlatformMaySuddenlyExit
        {
            get
            {
#if UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        public static string SavePath
        {
            get
            {
                return SaveBasePath + Path.DirectorySeparatorChar + "saves";
            }
        }

        public static string FinalSavePath
        {
            get
            {
                return SaveBasePath + Path.DirectorySeparatorChar + "finalsave";
            }
        }

        public static string DebugPath
        {
            get
            {
                return PersistentDataPath + Path.DirectorySeparatorChar + "debug";
            }
        }

        public static DataLoadPolicy LoadPolicy
        {
            get
            {
                if (LoadData == DataLoadPolicy.Auto)
                {
#if UNITY_EDITOR
                    return DataLoadPolicy.OnDemand;
#else
                    return DataLoadPolicy.OnStart;
#endif
                }
                else
                    return LoadData;
            }
        }

        public static StartupPolicy StartupPolicy
        {
            get
            {
#if UNITY_EDITOR
                return EditorStartupPolicy;
#else
                return PlayerStartupPolicy;
#endif
            }
        }

        public static IReadOnlyList<string> CommandLineArgs { get; set; }

        public static JsonSerializerSettings DefaultJsonSerializerSettings => new JsonSerializerSettings() {
            Formatting = Formatting.Indented,
            Converters = CCJsonConverters.Defaults.Converters,
            TypeNameHandling = TypeNameHandling.Auto
        };

        /// <summary>
        /// A hack necessary to preset variables so they can be safely accessed across threads
        /// </summary>
        internal static void SetInitial()
        {
            //VERSION/NAME HANDLING
            UnityVersion = TypeUtils.ParseVersion(Application.unityVersion);
            UnityVersionName = Application.unityVersion;
            GameName = Application.productName;
            CompanyName = Application.companyName;

            try
            {
                GameVersion = TypeUtils.ParseVersion(Application.version);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to decode version string \"{Application.version}\" (please use something resembling semantic versioning)");
                Debug.LogException(e);
            }

            //PLATFORM HANDLING
            Platform = Application.platform;

            //afaict no way to check these at runtime
#if !UNITY_EDITOR && UNITY_WSA && !ENABLE_IL2CPP
            ScriptingBackend = ScriptingImplementation.WinRTDotNET;
#elif ENABLE_IL2CPP
            ScriptingBackend = ScriptingImplementation.IL2CPP;
#else
            ScriptingBackend = ScriptingImplementation.Mono2x; //default
#endif

            //PATH HANDLING

            //normal handling for DataPath and StreamingAssetsPath
            DataPath = Application.dataPath;
            StreamingAssetsPath = Application.streamingAssetsPath;

            //GameFolderPath (ported from Sandstorm)
            GameFolderPath = Directory.GetParent(Application.dataPath).ToString();

            //special handling for PersistentDataPath and LocalDataPath
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && !UNITY_WSA
            switch (PersistentDataPathWindows)
            {
                case WindowsPersistentDataPath.Corrected:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.Roaming:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.Documents:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.MyGames:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.SavedGames:
                    PersistentDataPath = NativeHelpers.GetSavedGamesFolderPath();
                    break;
                default:
                    PersistentDataPath = Application.persistentDataPath;
                    break;
            }
            if (CorrectWindowsLocalDataPath)
                LocalDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.companyName, Application.productName, "local");
            else
                LocalDataPath = Path.Combine(Application.persistentDataPath, "local");
#else
            PersistentDataPath = Application.persistentDataPath;
            LocalDataPath = Path.Combine(Application.persistentDataPath, "local");
#endif

            //create data folder if it doesn't exist
            if (!Directory.Exists(PersistentDataPath))
                Directory.CreateDirectory(PersistentDataPath); //failing this is considered fatal

            SaveBasePath = PersistentDataPath; //someday we may have more flexibility

            //special handling for ScreenshotPath
#if UNITY_WSA
            ScreenshotsPath = Path.Combine(PersistentDataPath, "screenshot");
#else
            if (UseGlobalScreenshotFolder)
            {
                ScreenshotsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
            }
            else
            {
                ScreenshotsPath = Path.Combine(PersistentDataPath, "screenshot");
            }
#endif

            //create screenshot folder if it doesn't exist (this is a survivable error)
            try
            {
                if (!Directory.Exists(ScreenshotsPath))
                    Directory.CreateDirectory(ScreenshotsPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create screenshots directory ({ScreenshotsPath})");
                Debug.LogException(e);
            }

            //command line args
            CommandLineArgs = Environment.GetCommandLineArgs().ToImmutableArray();
        }

        /// <summary>
        /// Attempts to load overrides from file
        /// </summary>
        internal static void LoadOverrides()
        {
            string path = Path.Combine(GameFolderPath, "coreparams.json");
            if (File.Exists(path))
            {
                try
                {
                    TypeUtils.PopulateStaticObject(typeof(CoreParams), File.ReadAllText(path), new JsonSerializerSettings() { Converters = CCJsonConverters.Defaults.Converters, NullValueHandling = NullValueHandling.Ignore });
                    Debug.LogWarning($"Loaded coreparams overrides from file (this may cause really weird things to happen)");
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to load coreparams overrides from file!");
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Returns a "short" description of the application name, version, Unity environment (shown on main menu)
        /// </summary>
        public static string GetShortSystemText()
        {
            return string.Format("{0}\n{1} {2}\nCommonCore {3} {4}\nUnity {5}",
                GameName,
                GameVersion, GameVersionName,
                VersionCode.ToString(), VersionName,
                UnityVersionName);
        }

        /// <summary>
        /// Gets versioninfo of the current application
        /// </summary>
        /// <returns></returns>
        public static VersionInfo GetCurrentVersion()
        {
            return new VersionInfo(GameVersion, VersionCode, UnityVersion);
        }

        //***** module params getters (experimental/WIP)

        public static IReadOnlyDictionary<string, object> GetParamsForModule<T>()
        {
            return GetParamsForModule(typeof(T));
        }

        public static IReadOnlyDictionary<string, object> GetParamsForModule(Type moduleType)
        {
            return GetParamsForModule(moduleType.Name);
        }

        public static IReadOnlyDictionary<string, object> GetParamsForModule(string moduleName)
        {
            var dictionary = new Dictionary<string, object>();
            foreach(var kvp in ModuleParams)
            {
                if(kvp.Key.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    if(kvp.Value is IEnumerable<KeyValuePair<string, object>> col)
                    {
                        dictionary.AddRangeKeepExisting(col);
                    }
                }
                else if(kvp.Key.StartsWith(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    if(kvp.Key.Substring(moduleName.Length, 1) == "." || kvp.Key.Substring(moduleName.Length, 1) == "_" || kvp.Key.Substring(moduleName.Length, 1) == "-")
                    {
                        string key = kvp.Key.Substring(moduleName.Length).TrimStart('.', '_', '-');
                        dictionary[key] = kvp.Value;
                    }
                }
            }
            return dictionary;
        }
    }


}