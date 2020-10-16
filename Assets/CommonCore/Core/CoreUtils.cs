using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseNew)
            {
                var legacyStopwatch = System.Diagnostics.Stopwatch.StartNew();
                T legacyResult = ResourceManager.GetResource<T>(path);
                legacyStopwatch.Stop();
                try
                {
                    var newStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    T newResult = CCBase.ResourceManager.GetResource<T>(path, false);
                    newStopwatch.Stop();
                    if (legacyResult == newResult)
                        Debug.Log($"[ResourceManagerTest] LoadResource result ok (expected {legacyResult}, got {newResult})");
                    else
                        Debug.LogError($"[ResourceManagerTest] LoadResource mismatch (expected {legacyResult}, got {newResult})");

                    Debug.Log($"[ResourceManagerTest] LoadResource done! (old: {legacyStopwatch.Elapsed.TotalMilliseconds:F4}ms | new: {newStopwatch.Elapsed.TotalMilliseconds:F4}ms)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ResourceManagerTest] error in LoadResource: new ResourceManager threw {e.GetType().Name} ({e.Message})");
                    Debug.LogException(e);
                }
            }

            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManager.GetResource<T>(path);
            else
                return CCBase.ResourceManager.GetResource<T>(path, false);
        }

        /// <summary>
        /// Load resources from a folder, respecting virtual/redirected paths
        /// </summary>
        public static T[] LoadResources<T>(string path) where T: UnityEngine.Object
        {
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseNew)
            {
                var legacyStopwatch = System.Diagnostics.Stopwatch.StartNew();
                T[] legacyResults = ResourceManager.GetResources<T>(path);
                legacyStopwatch.Stop();
                try
                {
                    var newStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    T[] newResults = CCBase.ResourceManager.GetResources<T>(path, false);
                    newStopwatch.Stop();

                    //sort before compare
                    legacyResults = legacyResults.OrderBy(x => x.name).ToArray();
                    newResults = newResults.OrderBy(x => x.name).ToArray();

                    int mismatches = 0;
                    for (int i = 0; i < Math.Min(legacyResults.Length, newResults.Length); i++)
                    {
                        if (legacyResults[i] != newResults[i])
                        {
                            mismatches++;
                            Debug.LogWarning($"[ResourceManagerTest] LoadResources mismatch (old: {legacyResults[i]}, new: {newResults[i]})");
                        }
                    }

                    if (mismatches > 0)
                        Debug.LogWarning($"[ResourceManagerTest] LoadResources had {mismatches} mismatches");
                    else if(newResults.Length != legacyResults.Length)
                        Debug.LogWarning($"[ResourceManagerTest] LoadResources bad, no mismatches but expected {legacyResults.Length} results, got {newResults.Length} results");
                    else
                        Debug.Log($"[ResourceManagerTest] LoadResources result ok");

                    Debug.Log($"[ResourceManagerTest] LoadResources done! (old: {legacyStopwatch.Elapsed.TotalMilliseconds:F4}ms | new: {newStopwatch.Elapsed.TotalMilliseconds:F4}ms)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ResourceManagerTest] error in LoadResources: new ResourceManager threw {e.GetType().Name} ({e.Message})");
                    Debug.LogException(e);
                }
            }

            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
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
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseNew)
            {
                var legacyStopwatch = System.Diagnostics.Stopwatch.StartNew();
                T[] legacyResults = ResourceManager.GetDataResource<T>(path);
                legacyStopwatch.Stop();
                try
                {
                    var newStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    T[] newResults = CCBase.ResourceManager.GetResourceAllVariants<T>(path, true);
                    newStopwatch.Stop();

                    int mismatches = 0;
                    for (int i = 0; i < Math.Min(legacyResults.Length, newResults.Length); i++)
                    {
                        if (legacyResults[i] != newResults[i])
                        {
                            mismatches++;
                            Debug.LogWarning($"[ResourceManagerTest] LoadDataResource mismatch (old: {legacyResults[i]}, new: {newResults[i]})");
                        }
                    }

                    if (mismatches > 0)
                        Debug.LogWarning($"[ResourceManagerTest] LoadDataResource had {mismatches} mismatches");
                    else if (newResults.Length != legacyResults.Length)
                        Debug.LogWarning($"[ResourceManagerTest] LoadDataResource bad, no mismatches but expected {legacyResults.Length} results, got {newResults.Length} results");
                    else
                        Debug.Log($"[ResourceManagerTest] LoadDataResource result ok");

                    Debug.Log($"[ResourceManagerTest] LoadDataResource done! (old: {legacyStopwatch.Elapsed.TotalMilliseconds:F4}ms | new: {newStopwatch.Elapsed.TotalMilliseconds:F4}ms)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ResourceManagerTest] error in LoadDataResource: new ResourceManager threw {e.GetType().Name} ({e.Message})");
                    Debug.LogException(e);
                }
            }

            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
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
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseNew)
            {
                var legacyStopwatch = System.Diagnostics.Stopwatch.StartNew();
                T[][] legacyResults = ResourceManager.GetDataResources<T>(path);
                legacyStopwatch.Stop();
                try
                {
                    var newStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    T[][] newResults = CCBase.ResourceManager.GetResourcesAllVariants<T>(path, true);
                    newStopwatch.Stop();

                    legacyResults = ResourceManagement.ResourceManager.DataResourcesToResourceVariants(legacyResults);

                    DebugLog.DebugUtils.TextWrite(JsonConvert.SerializeObject(legacyResults, new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto,
                        Converters = CCJsonConverters.Defaults.Converters
                    }), "legacy");
                    DebugLog.DebugUtils.TextWrite(JsonConvert.SerializeObject(newResults, new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto,
                        Converters = CCJsonConverters.Defaults.Converters
                    }), "new");

                    int mismatches = 0;

                    for (int i = 0; i < Math.Min(legacyResults.Length, newResults.Length); i++)
                    {
                        legacyResults[i] = legacyResults[i].OrderBy(x => x.name).ToArray();
                        newResults[i] = newResults[i].OrderBy(x => x.name).ToArray();

                        for (int j = 0; j < Math.Min(legacyResults[i].Length, newResults[i].Length); j++)
                        {
                            if (legacyResults[i][j] != newResults[i][j])
                            {
                                mismatches++;
                                Debug.LogWarning($"[ResourceManagerTest] LoadDataResources mismatch (old: {legacyResults[i]}, new: {newResults[i]})");
                            }
                        }

                        if (newResults[i].Length != legacyResults[i].Length)
                            Debug.LogWarning($"[ResourceManagerTest] LoadDataResources bad, no mismatches but expected (inner array) {legacyResults[i].Length} results, got {newResults[i].Length} results");
                    }

                    if (mismatches > 0)
                        Debug.LogWarning($"[ResourceManagerTest] LoadDataResources had {mismatches} mismatches");
                    else if (newResults.Length != legacyResults.Length)
                        Debug.LogWarning($"[ResourceManagerTest] LoadDataResources bad, no mismatches but expected (outer array) {legacyResults.Length} results, got {newResults.Length} results");
                    else
                        Debug.Log($"[ResourceManagerTest] LoadDataResources result ok");

                    Debug.Log($"[ResourceManagerTest] LoadDataResources done! (old: {legacyStopwatch.Elapsed.TotalMilliseconds:F4}ms | new: {newStopwatch.Elapsed.TotalMilliseconds:F4}ms)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ResourceManagerTest] error in LoadDataResources: new ResourceManager threw {e.GetType().Name} ({e.Message})");
                    Debug.LogException(e);
                }
            }

            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManagement.ResourceManager.DataResourcesToResourceVariants(ResourceManager.GetDataResources<T>(path));
            else
                return CCBase.ResourceManager.GetResourcesAllVariants<T>(path, true);
        }

        /// <summary>
        /// Load a resource, returning a prioritized collection based on virtual/redirected path precedence
        /// </summary>
        /// <remarks>Obsolete: use <see cref="LoadResourceVariants{T}(string)"/> instead</remarks>
        [Obsolete("Use LoadResourceVariants instead of LoadDataResource", false)]
        public static T[] LoadDataResource<T>(string path) where T: UnityEngine.Object
        {
            return LoadResourceVariants<T>(path);
        }

        /// <summary>
        /// Load resources from a folder, returning a prioritized collection based on virtual/redirected path precedence
        /// </summary>
        /// <remarks>Obsolete: use <see cref="LoadDataResources{T}(string)"/> instead. Note that this is not a direct replacement.</remarks>
        [Obsolete("Use LoadResourcesVariants instead of LoadDataResources", false)]
        public static T[][] LoadDataResources<T>(string path) where T: UnityEngine.Object
        {
            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseNew)
            {
                var legacyStopwatch = System.Diagnostics.Stopwatch.StartNew();
                T[][] legacyResults = ResourceManager.GetDataResources<T>(path);
                legacyStopwatch.Stop();
                try
                {
                    //TODO do we need to sort first?
                    var newStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    T[][] newResults = CCBase.ResourceManager.GetResourcesAllVariants<T>(path, true);
                    newStopwatch.Stop();

                    newResults = ResourceManagement.ResourceManager.ResourcesVariantsToDataResources(newResults);

                    /*
                    DebugLog.DebugUtils.TextWrite(JsonConvert.SerializeObject(legacyResults, new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto,
                        Converters = CCJsonConverters.Defaults.Converters
                    }), "legacy");
                    DebugLog.DebugUtils.TextWrite(JsonConvert.SerializeObject(newResults, new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto,
                        Converters = CCJsonConverters.Defaults.Converters
                    }), "new");
                    */

                    int mismatches = 0;

                    for (int i = 0; i < Math.Min(legacyResults.Length, newResults.Length); i++)
                    {
                        legacyResults[i] = legacyResults[i].OrderBy(x => x.name).ToArray();
                        newResults[i] = newResults[i].OrderBy(x => x.name).ToArray();

                        for (int j = 0; j < Math.Min(legacyResults[i].Length, newResults[i].Length); j++)
                        {
                            if (legacyResults[i][j] != newResults[i][j])
                            {
                                mismatches++;
                                Debug.LogWarning($"[ResourceManagerTest] LoadDataResources mismatch (old: {legacyResults[i]}, new: {newResults[i]})");
                            }
                        }

                        if(newResults[i].Length != legacyResults[i].Length)
                            Debug.LogWarning($"[ResourceManagerTest] LoadDataResources bad, no mismatches but expected (inner array) {legacyResults[i].Length} results, got {newResults[i].Length} results");
                    }

                    if (mismatches > 0)
                        Debug.LogWarning($"[ResourceManagerTest] LoadDataResources had {mismatches} mismatches");
                    else if (newResults.Length != legacyResults.Length)
                        Debug.LogWarning($"[ResourceManagerTest] LoadDataResources bad, no mismatches but expected (outer array) {legacyResults.Length} results, got {newResults.Length} results");
                    else
                        Debug.Log($"[ResourceManagerTest] LoadDataResources result ok");

                    Debug.Log($"[ResourceManagerTest] LoadDataResources done! (old: {legacyStopwatch.Elapsed.TotalMilliseconds:F4}ms | new: {newStopwatch.Elapsed.TotalMilliseconds:F4}ms)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ResourceManagerTest] error in LoadDataResources: new ResourceManager threw {e.GetType().Name} ({e.Message})");
                    Debug.LogException(e);
                }
            }

            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManager.GetDataResources<T>(path);
            else
                return ResourceManagement.ResourceManager.ResourcesVariantsToDataResources(CCBase.ResourceManager.GetResourcesAllVariants<T>(path, true));
        }

        /// <summary>
        /// Check if a resource exists, respecting virtual/redirected paths 
        /// </summary>
        /// <remarks>
        /// <para>Use this instead of doing LoadResource and checking null, as it is at worst only somewhat slower and at best far faster</para>
        /// </remarks>
        public static bool CheckResource<T>(string path) where T: UnityEngine.Object
        {
            if(CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseNew)
            {
                var legacyStopwatch = System.Diagnostics.Stopwatch.StartNew();
                bool legacyResult = ResourceManager.ContainsResource<T>(path);
                legacyStopwatch.Stop();
                try
                {
                    var newStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    bool newResult = CCBase.ResourceManager.ResourceExists<T>(path, false);
                    newStopwatch.Stop();
                    if (legacyResult == newResult)
                        Debug.Log($"[ResourceManagerTest] CheckResource result ok (old: {legacyResult}, new: {newResult})");
                    else
                        Debug.LogWarning($"[ResourceManagerTest] CheckResource mismatch (old: {legacyResult}, new: {newResult})");

                    Debug.Log($"[ResourceManagerTest] CheckResource done! (old: {legacyStopwatch.Elapsed.TotalMilliseconds:F4}ms | new: {newStopwatch.Elapsed.TotalMilliseconds:F4}ms)");
                }
                catch(Exception e)
                {
                    Debug.LogError($"[ResourceManagerTest] error in CheckResource: new ResourceManager threw {e.GetType().Name} ({e.Message})");
                    Debug.LogException(e);
                }
            }

            if (CoreParams.DefaultResourceManager == ResourceManagerPolicy.TestBothUseLegacy || CoreParams.DefaultResourceManager == ResourceManagerPolicy.UseLegacy)
                return ResourceManager.ContainsResource<T>(path);
            else
                return CCBase.ResourceManager.ResourceExists<T>(path, false);
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
            return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings
            {
                Converters = CCJsonConverters.Defaults.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void SaveExternalJson(string path, object obj)
        {
            string json = SaveJson(obj);
            File.WriteAllText(path, json);
        }

        public static string SaveJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = CCJsonConverters.Defaults.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
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
            //for now, call through- we use hooks to execute our code on exit
            Application.Quit(exitCode);
        }

    }
}