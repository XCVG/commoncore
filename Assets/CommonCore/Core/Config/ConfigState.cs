﻿using CommonCore.DebugLog;
using CommonCore.Migrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

namespace CommonCore.Config
{
    public class ConfigState : IMigratable
    {
        private static string Path => System.IO.Path.Combine(CoreParams.PersistentDataPath, (CoreParams.UseSeparateEditorConfigFile && CoreParams.IsEditor) ? "config.editor.json" : "config.json");

        public static ConfigState Instance { get; private set; }

        public static void Load()
        {
            if (File.Exists(Path))
            {
                try
                {
                    //backup the config file first
                    try
                    {
                        File.Copy(Path, System.IO.Path.Combine(CoreParams.PersistentDataPath, "config.backup.json"), true);
                    }
                    catch (Exception)
                    {

                    }

                    //handle migrations
                    var rawConfig = CoreUtils.ReadExternalJson(Path) as JObject;
                    var newRawConfig = MigrationsManager.Instance.MigrateToLatest<ConfigState>(rawConfig, true, out bool didMigrate);
                    if (didMigrate)
                    {
                        Debug.Log("[Config] Config file was migrated successfully");
                        if (CoreParams.UseSystemMigrationBackups || CoreParams.IsDebug)
                        {
                            Directory.CreateDirectory(System.IO.Path.Combine(CoreParams.PersistentDataPath, "migrationbackups"));
                            File.Copy(Path, System.IO.Path.Combine(CoreParams.PersistentDataPath, "migrationbackups", $"config.migrated.{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}.json"), true);
                        }
                        CoreUtils.WriteExternalJson(Path, rawConfig);
                    }
                    Instance = CoreUtils.InterpretJson<ConfigState>(newRawConfig ?? rawConfig);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Config] Failed to load config from file, using defaults");
                    Debug.LogException(e);
                    try
                    {
                        if (File.Exists(Path))
                            DebugUtils.TextWrite(File.ReadAllText(Path), "brokenconfig");
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            if (Instance == null)
            {
                Instance = new ConfigState();
                Instance.Init();
            }

        }

        public static void Save()
        {
            Instance.CurrentVersion = CoreParams.GetCurrentVersion();
            CoreUtils.SaveExternalJson(Path, Instance);
        }

        [JsonConstructor]
        private ConfigState()
        {

        }

        /// <summary>
        /// Will run on create but not before deserializing an existing config file. Set defaults in collections here
        /// </summary>
        private void Init()
        {
            if(CoreParams.SetNativeResolutionOnFirstRun && !CoreParams.IsEditor && 
                (CoreParams.Platform == RuntimePlatform.LinuxPlayer || CoreParams.Platform == RuntimePlatform.OSXPlayer || CoreParams.Platform == RuntimePlatform.WindowsPlayer))
            {
                Resolution = new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
                Debug.Log("Detected native screen resolution: " + Resolution);
            }
        }

        /// <summary>
        /// Checks if a custom config flag is set
        /// </summary>
        public bool HasCustomFlag(string flag)
        {
            return CustomConfigFlags.Contains(flag);
        }

        /// <summary>
        /// Sets/unsets a custom config flag
        /// </summary>
        public void SetCustomFlag(string flag, bool state)
        {
            if (!state && CustomConfigFlags.Contains(flag))
                CustomConfigFlags.Remove(flag);
            else if (state && !CustomConfigFlags.Contains(flag))
                CustomConfigFlags.Add(flag);
        }

        /// <summary>
        /// Adds an object to the custom config vars if and only if it does not already exist
        /// </summary>
        public void AddCustomVarIfNotExists(string name, object customVar)
        {
            if (CustomConfigVars.ContainsKey(name))
            {
                if (CustomConfigVars[name] == null || CustomConfigVars[name].GetType() != customVar.GetType())
                {
                    Debug.LogWarning($"[Config] Custom config var {name} exists but contains a {CustomConfigVars[name]?.GetType()?.Name} instead of a {customVar.GetType().Name}");
                }
            }
            else
            {
                CustomConfigVars.Add(name, customVar);
            }
        }

        /// <summary>
        /// Adds an object to the custom config vars if and only if it does not already exist (function version)
        /// </summary>
        public void AddCustomVarIfNotExists<T>(string name, Func<T> customVarBuilder)
        {
            if (CustomConfigVars.ContainsKey(name))
            {
                if (CustomConfigVars[name] == null || CustomConfigVars[name].GetType() != typeof(T))
                {
                    Debug.LogWarning($"[Config] Custom config var {name} exists but contains a {CustomConfigVars[name]?.GetType()?.Name} instead of a {typeof(T).Name}");
                }
            }
            else
            {
                CustomConfigVars.Add(name, customVarBuilder());
            }
        }

        /// <summary>
        /// Handle (some) config file errors
        /// </summary>
        /// <remarks>
        /// This is mostly to deal with custom variables where the types may no longer exist if a module or addon was added and then removed.
        /// </remarks>
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {

            if (errorContext.Path.StartsWith(nameof(CustomConfigVars)))
            {
                Debug.LogWarning($"Failed to load a custom config var (path: {errorContext.Path}). Please check your config file.");
                errorContext.Handled = true;
            }
            else
            {
                Debug.LogError($"A fatal error occurred during config file loading. Please check your config file. \n{errorContext.Error.Message}");
                //errorContext.Handled = true;
            }

        }

        //version metadata

        /// <summary>
        /// Version information of the initial state or last migration
        /// </summary>
        [JsonProperty]
        public VersionInfo LastMigratedVersion { get; private set; } = CoreParams.GetCurrentVersion();

        /// <summary>
        /// Version information of the current state
        /// </summary>
        [JsonProperty]
        public VersionInfo CurrentVersion { get; private set; } = CoreParams.GetCurrentVersion();

        Version IMigratable.LastMigratedVersion => LastMigratedVersion.GameVersion;

        //actual config data here

        //SYSTEM CONFIG
        public bool UseVerboseLogging { get; set; } = true;
        public bool UseCampaignIdentifier { get; set; } = true;
        public float DefaultTimescale { get; set; } = 1;
        public float WorldTimescale { get; set; } = 1;
        public float EntityBudget { get; set; } = 1;
        public float EffectBudget { get; set; } = 1;

        //THEME CONFIG
        public bool SuppressThemeWarnings { get; set; } = true;

        //ADDON CONFIG
        public bool LoadAddons { get; set; } = true;
        public List<string> AddonsToLoad { get; private set; } = new List<string>();

        //AUDIO CONFIG
        public float MusicVolume { get; set; } = 0.8f;
        public float SoundVolume { get; set; } = 0.8f; //due to a historical quirk, this is actually main volume
        public AudioSpeakerMode SpeakerMode { get; set; } = AudioSpeakerMode.Stereo; //safe default on all platforms

        //VIDEO CONFIG
        [JsonIgnore]
        public bool UseCustomVideoSettings => (GraphicsQuality >= QualitySettings.names.Length - 1);
        public int GraphicsQuality { get; set; } = 3;
        public Vector2Int Resolution { get; set; } = new Vector2Int(1920, 1080);
        public int RefreshRate { get; set; } = 60;
        public bool FullScreen { get; set; } = true;
        public int MaxFrames { get; set; } = 120;
        public int VsyncCount { get; set; } = 0;
        public int AntialiasingQuality { get; set; } = 1;
        public float ViewDistance { get; set; } = 1000.0f;
        public bool ShowFps { get; set; } = false;
        public float EffectDwellTime { get; set; } = 30;
        public float FieldOfView { get; set; } = 60;
        public float HudScale { get; set; } = 1;
        public float Brightness { get; set; } = 1f;

        //VIDEO CONFIG (EXTENDED)
        public QualityLevel ShadowQuality { get; set; } = QualityLevel.Medium;
        public float ShadowDistance { get; set; } = 40.0f;
        public QualityLevel LightingQuality { get; set; } = QualityLevel.Medium;
        public QualityLevel MeshQuality { get; set; } = QualityLevel.Medium;
        public TextureScale TextureScale { get; set; } = TextureScale.Full;
        public AnisotropicFiltering AnisotropicFiltering { get; set; } = AnisotropicFiltering.Enable;
        public QualityLevel RenderingQuality { get; set; } = QualityLevel.Medium;

        //VIDEO CONFIG (SPECIAL HANDLING)
        [JsonIgnore]
        public PlayerLightReportingType PlayerLightReporting {
            get
            {
                if (CoreParams.ForcePlayerLightReporting)
                    return PlayerLightReportingType.Probed;

                if (UseCustomVideoSettings)
                    return _PlayerLightReporting;

                var q = GraphicsQuality;
                if (q >= 4) //ultra or better
                    return PlayerLightReportingType.Probed;
                else if (q >= 2) //medium or better (medium/high)
                    return PlayerLightReportingType.Calculated;
                else
                    return PlayerLightReportingType.None;

            }
            set
            {
                _PlayerLightReporting = value;
            }
        }
        [JsonProperty(PropertyName = "PlayerLightReporting")]
        private PlayerLightReportingType _PlayerLightReporting;

        //GAME/GAMEPLAY CONFIG
        public SubtitlesLevel Subtitles { get; set; } = SubtitlesLevel.Always;
        public bool ShakeEffects { get; set; } = true;
        public bool FlashEffects { get; set; } = true;
        public float GameSpeed { get; set; } = 1; //experimental, must be explicitly handled        
        public int AutosaveCount { get; set; } = 5;
        public int Difficulty { get; set; } = 1;

        //INPUT CONFIG
        public string InputMapper { get; set; } = "ExplicitKBMInputMapper";
        public bool SuppressInputMapperWarnings { get; set; } = true;
        public float LookSpeed { get; set; } = 1.0f;
        public bool LookInvert { get; set; } = false;
        public float AxisDeadzone { get; set; } = 0.1f;
        public float UIScrollSpeed { get; set; } = 25f;
        public KeyCode ScreenshotKey { get; set; } = KeyCode.F12;
        public KeyCode QuicksaveKey { get; set; } =
#if UNITY_WEBGL 
            KeyCode.F6;
#else
            KeyCode.F5;
#endif
        public KeyCode QuickloadKey { get; set; } = KeyCode.F9;

        //EXTRA/GAME-SPECIFIC CONFIG
        public HashSet<string> CustomConfigFlags { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        [JsonIgnore]
        public Dictionary<string, object> CustomConfigVars { get; private set; } = new Dictionary<string, object>(); //note that serialization/deserialization can explode in edge cases
        [JsonIgnore]
        private Dictionary<string, JToken> UnparseableCustomConfigVars { get; set; } = new Dictionary<string, JToken>(); //used to preserve unparseable config vars when loading config

        //this bit of hackery lets us preserve unparseable/broken data in the config.json file
        [JsonProperty(PropertyName = nameof(CustomConfigVars))]
        private JToken CustomConfigVarsSerializable
        {
            get
            {
                JObject jo = new JObject();
                foreach(KeyValuePair<string, object> kvp in CustomConfigVars)
                {
                    if (kvp.Value == null)
                        continue;
                    var token = JToken.FromObject(kvp.Value);
                    if(token.Type == JTokenType.Object)
                    {
                        JObject subObject = JObject.FromObject(kvp.Value);
                        var type = kvp.Value.GetType();
                        subObject.AddFirst(new JProperty("$type", string.Format("{0}, {1}", type.ToString(), type.Assembly.GetName().Name)));
                        jo.Add(kvp.Key, subObject);
                    }
                    else
                    {
                        jo.Add(kvp.Key, token);
                    }                   
                }
                foreach(KeyValuePair<string, JToken> kvp in UnparseableCustomConfigVars)
                {
                    //we may end up with duplicates in rare edge cases
                    if(!jo.ContainsKey(kvp.Key))
                        jo.Add(kvp.Key, kvp.Value);
                    else
                    {
                        int id = 1;
                        string name = $"{kvp.Key}_{id}";
                        while(jo.ContainsKey(name))
                        {
                            id++;
                            name = $"{kvp.Key}_{id}";
                        }
                        jo.Add(name, kvp.Value);
                    }
                }
                return jo;
            }
            set
            {
                var jo = value as JObject;
                foreach(KeyValuePair<string, JToken> kvp in jo)
                {
                    JToken item = kvp.Value;
                    if (item.IsNullOrEmpty())
                        continue;

                    try
                    {

                        //new for 5.x: allow primitive values
                        var primitiveValue = item.ToValueAuto();
                        if(primitiveValue != null)
                        {
                            CustomConfigVars[kvp.Key] = primitiveValue;
                            continue;
                        }

                        if (item["$type"] == null || string.IsNullOrEmpty(item["$type"].Value<string>()))
                        {
                            //can't get type, add as unparseable
                            Debug.LogWarning($"[Config] Can't parse custom config node {kvp.Key} because no type information is provided");
                            UnparseableCustomConfigVars.Add(kvp.Key, item);
                            continue;
                        }

                        var type = Type.GetType(item["$type"].Value<string>());
                        if (type == null)
                        {
                            //can't find type, add as unparseable
                            Debug.LogWarning($"[Config] Can't parse custom config node {kvp.Key} because type \"{type}\" could not be found");
                            UnparseableCustomConfigVars.Add(kvp.Key, item);
                            continue;
                        }

                        CustomConfigVars[kvp.Key] = item.ToObject(type);
                    }
                    catch(Exception e)
                    {
                        //failed somewhere, add as unparseable
                        Debug.LogWarning($"[Config] Can't parse custom config node {kvp.Key} because of an error ({e.GetType().Name})");
                        UnparseableCustomConfigVars[kvp.Key] = item;
                        if (CoreParams.IsDebug)
                            Debug.LogException(e);
                    }
                }
            }
        }

        //ADDON CONFIG
        public HashSet<string> AddonConfigFlags { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        [JsonIgnore]
        public LazyLooseDictionary AddonConfigVars { get; private set; } = new LazyLooseDictionary();

        //small hackery
        [JsonProperty(PropertyName = nameof(AddonConfigVars))]
        private JObject AddonConfigVarsSerializable
        {
            get => AddonConfigVars.GetFullJObject();
            set => AddonConfigVars = new LazyLooseDictionary(value);
        }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }

    }


}