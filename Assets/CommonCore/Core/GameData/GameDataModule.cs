using CommonCore.DebugLog;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.GameData
{

    /// <summary>
    /// Module for loading arbitrary game data objects for definining whatever you want
    /// </summary>
    public class GameDataModule : CCModule
    {
        public static readonly string Path = "Data/GameData/";

        private IDictionary<Type, object> CachedData = new Dictionary<Type, object>();

        public GameDataModule()
        {
            if (CoreParams.LoadPolicy == DataLoadPolicy.OnStart)
                PreloadCache();
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            //on addon load, we invalidate the cache and fully reload it if applicable, which is stupid but oh well
            ClearCache();
            if (CoreParams.LoadPolicy == DataLoadPolicy.OnStart)
                PreloadCache();
        }

        private void PreloadCache()
        {

            var types = CoreUtils.GetLoadedTypes();

            var textAssets = CoreUtils.LoadResources<TextAsset>(Path);
            foreach(var textAsset in textAssets)
            {
                try
                {
                    Type assetType = types.Where(t => t.Name == textAsset.name).Single();
                    object data = JsonConvert.DeserializeObject(textAsset.text, assetType, CoreParams.DefaultJsonSerializerSettings);
                    CachedData.Add(assetType, data);
                }
                catch(Exception e)
                {
                    LogError($"Failed to load {textAsset.name} ({e.GetType().Name})");
                    LogException(e);
                }
            }
        }

        /// <summary>
        /// Clears the cache of game data objects
        /// </summary>
        public void ClearCache()
        {
            CachedData.Clear();
        }
        
        /// <summary>
        /// Gets a game data object
        /// </summary>
        public T Get<T>()
        {
            if (CachedData.TryGetValue(typeof(T), out var data))
                return (T)data;
            return (T)LoadAndCache(typeof(T));
        }

        /// <summary>
        /// Gets a game data object
        /// </summary>
        public object Get(Type t)
        {
            if (CachedData.TryGetValue(t, out var data))
                return data;
            return LoadAndCache(t);
        }

        private object LoadAndCache(Type t)
        {
            object data;
            if(CoreUtils.CheckResource<TextAsset>(Path + t.Name))
            {
                TextAsset textAsset = CoreUtils.LoadResource<TextAsset>(Path + t.Name);
                data = JsonConvert.DeserializeObject(textAsset.text, t, CoreParams.DefaultJsonSerializerSettings);
            }
            else
            {
                data = Activator.CreateInstance(t);
            }
            
            if(CoreParams.LoadPolicy == DataLoadPolicy.Cached || CoreParams.LoadPolicy == DataLoadPolicy.OnStart)
                CachedData.Add(t, data);

            return data;
        }

        [Command(alias = "ClearCache", className = "GameData", useClassName = true)]
        private static void ClearCacheCommand()
        {
            CCBase.GetModule<GameDataModule>().ClearCache();
        }

        [Command(alias = "DumpCached", className = "GameData", useClassName = true)]
        private static void DumpCacheCommand()
        {
            var data = CCBase.GetModule<GameDataModule>().CachedData;
            DebugUtils.JsonWrite(data, "GameData");
        }

    }
}