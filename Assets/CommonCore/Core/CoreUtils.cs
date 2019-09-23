using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
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
        internal static CCResourceManager ResourceManager {get; set;}

        /// <summary>
        /// Load a resource, respecting virtual/redirected paths
        /// </summary>
        public static T LoadResource<T>(string path) where T: UnityEngine.Object
        {
            return ResourceManager.GetResource<T>(path);
        }

        /// <summary>
        /// Load resources from a folder, respecting virtual/redirected paths
        /// </summary>
        public static T[] LoadResources<T>(string path) where T: UnityEngine.Object
        {
            return ResourceManager.GetResources<T>(path);
        }

        /// <summary>
        /// Load a resource, returning a prioritized collection based on virtual/redirected path precedence
        /// </summary>
        public static T[] LoadDataResource<T>(string path) where T: UnityEngine.Object
        {
            return ResourceManager.GetDataResource<T>(path);
        }

        /// <summary>
        /// Load resources from a folder, returning a prioritized collection based on virtual/redirected path precedence
        /// </summary>
        public static T[][] LoadDataResources<T>(string path) where T: UnityEngine.Object
        {
            return ResourceManager.GetDataResources<T>(path);
        }

        /// <summary>
        /// Check if a resource exists, respecting virtual/redirected paths 
        /// </summary>
        public static bool CheckResource<T>(string path) where T: UnityEngine.Object
        {
            return ResourceManager.ContainsResource<T>(path);
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
            return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings
            {
                Converters = CCJsonConverters.Defaults.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void SaveExternalJson(string path, System.Object obj)
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
                catch (Exception e)
                {
                    //ignore it, we've gone over or some stupid bullshit
                }

            }

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

    }
}