using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using CommonCore.GameData;
using Newtonsoft.Json.Linq;

namespace CommonCore
{

    /// <summary>
    /// Core Utilities class: Contains functions for loading resources, saving/loading JSON, getting root transforms and a few other miscellaneous things.
    /// </summary>
    public static class CoreUtils
    {
        internal static LegacyResourceManager ResourceManager {get; set;}

        /// <summary>
        /// Load a resource, respecting virtual/redirected paths
        /// </summary>
        public static T LoadResource<T>(string path) where T: UnityEngine.Object
        {
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManager.GetResource<T>(path);
            else
                return CCBase.ResourceManager.GetResource<T>(path, false);
        }

        /// <summary>
        /// Load resources from a folder, respecting virtual/redirected paths
        /// </summary>
        public static T[] LoadResources<T>(string path) where T: UnityEngine.Object
        {           
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManager.GetResources<T>(path);
            else
                return CCBase.ResourceManager.GetResources<T>(path, false);
        }

        /// <summary>
        /// Load a resource, returning a prioritized array based on resource priority
        /// </summary>
        /// <remarks>
        /// <para>Drop-in replacement for <see cref="LoadDataResource{T}(string)"/></para>
        /// </remarks>
        public static T[] LoadResourceVariants<T>(string path) where T : UnityEngine.Object
        {            
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManager.GetDataResource<T>(path);
            else
                return CCBase.ResourceManager.GetResourceAllVariants<T>(path, true);
        }

        /// <summary>
        /// Load all resources in a folder, returning an array of arrays of resource variants [resource][priority]
        /// </summary>
        /// <remarks>
        /// <para>Note that this is "sideways" versus LoadDataResource which is [priority][resource]</para>
        /// </remarks>
        public static T[][] LoadResourcesVariants<T>(string path) where T : UnityEngine.Object
        {
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManagement.ResourceManager.DataResourcesToResourceVariants(ResourceManager.GetDataResources<T>(path));
            else
                return CCBase.ResourceManager.GetResourcesAllVariants<T>(path, true);
        }

        /// <summary>
        /// Check if a resource exists, respecting virtual/redirected paths 
        /// </summary>
        /// <remarks>
        /// <para>Use this instead of doing LoadResource and checking null, as it is at worst only somewhat slower and at best far faster</para>
        /// </remarks>
        public static bool CheckResource<T>(string path) where T: UnityEngine.Object
        {
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManager.ContainsResource<T>(path);
            else
                return CCBase.ResourceManager.ResourceExists<T>(path, false);
        }

        /// <summary>
        /// Gets a chunk of game data by type through GameDataModule
        /// </summary>
        public static T GetGameData<T>()
        {
            return CCBase.GetModule<GameDataModule>().Get<T>();
        }

        /// <summary>
        /// Gets a chunk of game data by type through GameDataModule
        /// </summary>
        public static object GetGameData(Type t)
        {
            return CCBase.GetModule<GameDataModule>().Get(t);
        }

        public static System.Random Random
        {
            get
            {
                if (_random == null)
                    _random = new System.Random();
                return _random;

            }
        }

        private static System.Random _random;

        public static JToken ReadExternalJson(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            string text = File.ReadAllText(path);
            return ReadJson(text);
        }

        public static JToken ReadJson(string text)
        {
            return JToken.Parse(text);
        }

        public static void WriteExternalJson(string path, JToken jt)
        {
            string json = WriteJson(jt);
            File.WriteAllText(path, json);
        }

        public static string WriteJson(JToken jt)
        {
            return jt.ToString(Formatting.Indented);
        }

        public static T InterpretJson<T>(JToken jt)
        {
            return jt.ToObject<T>(JsonSerializer.Create(CoreParams.DefaultJsonSerializerSettings));
        }

        public static JToken ConstructJson(object obj)
        {
            return JToken.FromObject(obj, JsonSerializer.Create(CoreParams.DefaultJsonSerializerSettings));
        }

        public static T LoadExternalJson<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            string text = File.ReadAllText(path);
            return LoadJson<T>(text);
        }

        public static T LoadJson<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text, CoreParams.DefaultJsonSerializerSettings);
        }

        public static void SaveExternalJson(string path, object obj)
        {
            string json = SaveJson(obj);
            File.WriteAllText(path, json);
        }

        public static string SaveJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, CoreParams.DefaultJsonSerializerSettings);
        }


        /// <summary>
        /// Gets "relevant" types: relevant types from CCBase.BaseGameTypes plus all addon loaded types (ie excluding framework types)
        /// </summary>
        /// <returns>A list of "relevant" types</returns>
        public static IEnumerable<Type> GetLoadedTypes()
        {
            IEnumerable<Type> baseTypes = CCBase.BaseGameTypes;
            if (!CoreParams.LoadAddons)
                return baseTypes;

            IEnumerable<Type> addonTypes = CCBase.AddonManager.EnumerateAddonAssemblies().SelectMany(a => a.GetTypes());

            return baseTypes.Concat(addonTypes);
        }

        /// <summary>
        /// Gets a list of scenes (by name) in the game
        /// </summary>
        /// <returns>A list of scenes in the game</returns>
        public static string[] GetSceneList() //TODO we'll probably move this into some kind of CommonCore.SceneManagement
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            var scenes = new List<string>(sceneCount);
            for (int i = 0; i < sceneCount; i++)
            {
                try
                {
                    scenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
                }
                catch (Exception)
                {
                    //ignore it, we've gone over or some stupid bullshit
                }

            }

            if(CCBase.AddonManager != null)
                scenes.AddRange(CCBase.AddonManager.EnumerateAddonScenePaths());

            return scenes.ToArray();

        }

        private static Transform WorldRoot;
        public static Transform GetWorldRoot() //TODO really ought to move this
        {
            if (WorldRoot == null)
            {
                GameObject rootGo = GameObject.Find("WorldRoot");
                if (rootGo == null)
                    return null;
                WorldRoot = rootGo.transform;
            }
            return WorldRoot;
        }

        private static Transform UIRoot;
        public static Transform GetUIRoot()
        {
            if(UIRoot == null)
            {
                GameObject rootGo = GameObject.Find("UIRoot");
                if (rootGo == null)
                    rootGo = new GameObject("UIRoot");
                UIRoot = rootGo.transform;
            }
            return UIRoot;
        }

        /// <summary>
        /// Immediately exits the game
        /// </summary>
        public static void Quit() => Quit(0);

        /// <summary>
        /// Immediately exits the game
        /// </summary>
        /// <param name="exitCode">The status code to return</param>
        public static void Quit(int exitCode)
        {
            //handling WebGL (and other platforms?) that can't really quit
            if(CoreParams.Platform == RuntimePlatform.WebGLPlayer)
            {
                SceneManager.LoadScene("FakeExitScene");
            }
            else
            {
                //call through- we use hooks to execute our code on exit
                Application.Quit(exitCode);
            }
        }

        /// <summary>
        /// Runs the garbage collector
        /// </summary>
        public static void CollectGarbage() => CollectGarbage(false);

        /// <summary>
        /// Runs the garbage collector
        /// </summary>
        public static void CollectGarbage(bool waitForPendingFinalizers)
        {
#if !UNITY_WEBGL //not supported on webGL, NOP

            if (GarbageCollector.GCMode == GarbageCollector.Mode.Disabled && !CoreParams.AlwaysEnableGCBeforeCollect)
                return;

            var oldMode = GarbageCollector.GCMode;
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;

            GC.Collect();

            if(waitForPendingFinalizers)
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            GarbageCollector.GCMode = oldMode;
#endif
        }

    }
}