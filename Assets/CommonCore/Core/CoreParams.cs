using System;
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
        public static Version VersionCode { get; private set; } = new Version(2, 0, 0); //2.0.0
        public static string VersionName { get; private set; } = "Balmora"; //start with A, locations from RPGs
        public static Version UnityVersion { get; private set; } //auto-set
        public static string UnityVersionName { get; private set; } //auto-set
        public static RuntimePlatform Platform { get; private set; } //auto-set

        //*****game version info
        public static string GameName { get; private set; } //auto-set from Unity settings
        public static Version GameVersion { get; private set; } //auto-set from Unity settings
        public static string GameVersionName { get; private set; } = "Holiday 2019 Demo";

        //*****basic config settings
        public static bool AutoInit { get; private set; } = true;
        public static bool AutoloadModules { get; private set; } = true;
        public static ImmutableArray<string> ExplicitModules { get; private set; } = new string[] { "DebugModule", "QdmsMessageBus", "ConfigModule", "AsyncModule", "ScriptingModule", "ConsoleModule" }.ToImmutableArray();
        private static DataLoadPolicy LoadData = DataLoadPolicy.OnStart;
        public static string PreferredCommandConsole { get; private set; } = "SickDevConsoleImplementation";
        private static WindowsPersistentDataPath PersistentDataPathWindows = WindowsPersistentDataPath.Roaming;
        private static bool UseGlobalScreenshotFolder = true;

        //*****additional config settings
        public static bool UseVerboseLogging { get; private set; } = true;
        public static float DelayedEventPollInterval { get; private set; } = 1.0f;
        public static bool UseAggressiveLookups { get; private set; } = true;
        public static bool UseDirectSceneTransitions { get; private set; } = false;

        //*****game config settings
        public static string InitialScene { get; private set; } = "TestScene";

        //*****path variables (some hackery to provide thread-safeish versions)
        public static string DataPath { get; private set; }
        public static string GameFolderPath { get; private set; }
        public static string PersistentDataPath { get; private set; }
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

        public static string SavePath
        {
            get
            {
                return PersistentDataPath + "/saves";
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

        /// <summary>
        /// A hack necessary to preset variables so they can be safely accessed across threads
        /// </summary>
        internal static void SetInitial()
        {
            //VERSION/NAME HANDLING
            UnityVersion = TypeUtils.ParseVersion(Application.unityVersion);
            UnityVersionName = Application.unityVersion;
            GameName = Application.productName;

            try
            {
                GameVersion = TypeUtils.ParseVersion(Application.version);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to decode version string \"{Application.version}\" (please use something resembling semantic versioning)");
                Debug.LogException(e);
            }

            Platform = Application.platform;

            //PATH HANDLING

            //normal handling for DataPath and StreamingAssetsPath
            DataPath = Application.dataPath;
            StreamingAssetsPath = Application.streamingAssetsPath;

            //GameFolderPath (ported from Sandstorm)
            GameFolderPath = Directory.GetParent(Application.dataPath).ToString();

            //special handling for PersistentDataPath
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
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
                default:
                    PersistentDataPath = Application.persistentDataPath;
                    break;
            }
#else
            PersistentDataPath = Application.persistentDataPath;
#endif

            //create data folder if it doesn't exist
            if (!Directory.Exists(PersistentDataPath))
                Directory.CreateDirectory(PersistentDataPath);
            
            //special handling for ScreenshotPath
            if(UseGlobalScreenshotFolder)
            {
                ScreenshotsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
            }
            else
            {
                ScreenshotsPath = Path.Combine(PersistentDataPath, "screenshot");
            }

            //create screenshot folder if it doesn't exist
            if (!Directory.Exists(ScreenshotsPath))
                Directory.CreateDirectory(ScreenshotsPath);
        }
    }


}