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
    public static partial class CoreParams
    {
        //*****game version info 
        //company name, game name, game version are auto-set from Unity settings
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

    }


}