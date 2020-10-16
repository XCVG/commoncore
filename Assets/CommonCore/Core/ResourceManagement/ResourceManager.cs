using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonCore.ResourceManagement
{

    /// <summary>
    /// The shiny new resource manager (WIP)
    /// </summary>
    public class ResourceManager
    {
        public ResourceLoader ResourceLoader { get; private set; }

        public int NextResourceHandleID => ++CurrentResourceHandleID;
        private int CurrentResourceHandleID = 0;

        private Dictionary<string, ResourceObject> ResourceObjectCache = new Dictionary<string, ResourceObject>();

        private Dictionary<string, ResourceFolder> ResourceFolders = new Dictionary<string, ResourceFolder>();

        public ResourceManager()
        {
            ResourceLoader = new ResourceLoader();
        }

        //TODO get handle (instead of resource) variants

        /// <summary>
        /// Gets a resource
        /// </summary>
        public T GetResource<T>(string path, bool typeExact) where T : UnityEngine.Object
        {
            //this is actually kinda expensive and we'll probably remove or hide it behind a flag
            if (path.StartsWith("Game/") || path.StartsWith("Core/"))// || path.StartsWith("Addons/")) //actually the Addons path is probably safe; it's just loading virtual elocal
            {
                Debug.LogWarning($"Resource path starts with special folder, this case isn't currently handled! ({path})");
            }

            ResourceObject rObject = RetrieveResourceObject(path);

            return rObject.GetResource<T>(typeExact);
        }
        
        /// <summary>
        /// Gets all variants of a resource
        /// </summary>
        public T[] GetResourceAllVariants<T>(string path, bool typeExact) where T : UnityEngine.Object
        {
            if (path.StartsWith("Game/") || path.StartsWith("Core/"))// || path.StartsWith("Addons/"))
            {
                Debug.LogWarning($"Resource path starts with special folder, this case isn't currently handled! ({path})");
            }

            ResourceObject rObject = RetrieveResourceObject(path);

            return rObject.GetResourceAll<T>(typeExact);
        }

        /// <summary>
        /// Gets all resources in a folder
        /// </summary>
        public T[] GetResources<T>(string path, bool typeExact) where T : UnityEngine.Object
        {
            if (path.StartsWith("Game/") || path.StartsWith("Core/"))// || path.StartsWith("Addons/")) //actually the Addons path is probably safe; it's just loading virtual elocal
            {
                Debug.LogWarning($"Resource path starts with special folder, this case isn't currently handled! ({path})");
            }

            string folder = GetAsFolderPath(path);
            ResourceFolder rFolder = RetrieveResourceFolder(folder);

            return rFolder.GetResources<T>(typeExact);
        }

        /// <summary>
        /// Gets all variants of all resources in a folder
        /// </summary>
        /// <remarks>
        /// <para>This returns [resources][variants] which is sideways from the old convention</para>
        /// </remarks>
        public T[][] GetResourcesAllVariants<T>(string path, bool typeExact) where T : UnityEngine.Object
        {
            if (path.StartsWith("Game/") || path.StartsWith("Core/"))// || path.StartsWith("Addons/")) //actually the Addons path is probably safe; it's just loading virtual elocal
            {
                Debug.LogWarning($"Resource path starts with special folder, this case isn't currently handled! ({path})");
            }

            string folder = GetAsFolderPath(path);
            ResourceFolder rFolder = RetrieveResourceFolder(folder);

            return rFolder.GetResourcesAll<T>(typeExact);
        }

        /// <summary>
        /// Checks if a resource exists
        /// </summary>
        /// <remarks>
        /// <para>Note that this *can* be faster than GetResource depending on caching and backing type</para>
        /// </remarks>
        public bool ResourceExists<T>(string path, bool typeExact) where T : UnityEngine.Object
        {
            if (path.StartsWith("Game/") || path.StartsWith("Core/"))// || path.StartsWith("Addons/"))
            {
                Debug.LogWarning($"Resource path starts with special folder, this case isn't currently handled! ({path})");
            }

            string folder = GetFolderName(path);
            ResourceFolder rFolder = RetrieveResourceFolder(folder);
            string file = GetFileName(path);
            ResourceObject rObject = rFolder.RetrieveResourceObject(file);

            return rObject.ExistsForType<T>(typeExact);
        }
        
        //WIP AddResourceX methods
        public ResourceHandle AddResource(string path, UnityEngine.Object resource, ResourcePriority priority)
        {
            var ro = RetrieveResourceObject(path);
            var handleType = typeof(RuntimeResourceHandle<>).MakeGenericType(resource.GetType());
            var rh = (ResourceHandle)Activator.CreateInstance(handleType, resource, priority);
            ro.AddResourceHandle(rh, resource.GetType());
            return rh;
        }

        public ResourceHandle<T> AddResource<T>(string path, T resource, ResourcePriority priority) where T : UnityEngine.Object //I think this is the only one that'll work on IL2CPP
        {
            var ro = RetrieveResourceObject(path);
            var rh = new RuntimeResourceHandle<T>(resource, priority);
            ro.AddResourceHandle(rh);
            return rh;
        }

        public ResourceHandle AddResourceFromAssetBundle(string targetPath, string bundledName, AssetBundle bundle, ResourcePriority priority)
        {
            //var resource = bundle.LoadAsset(bundledName);
            var resource = LoadAssetFromBundle(bundledName, bundle);
            Type resourceType = resource.GetType();

            var handleType = typeof(AssetBundleResourceHandle<>).MakeGenericType(resourceType);
            var rh = (ResourceHandle)Activator.CreateInstance(handleType, resource, bundledName, bundle, priority);

            var ro = RetrieveResourceObject(targetPath);
            ro.AddResourceHandle(rh, resource.GetType());
            return rh;
        }

        public async Task<ResourceHandle> AddResourceFromAssetBundleAsync(string targetPath, string bundledName, AssetBundle bundle, ResourcePriority priority)
        {
            /*
            var bundleLoadRequest = bundle.LoadAssetAsync(bundledName);

            while (!bundleLoadRequest.isDone)
                await Task.Yield();

            var resource = bundleLoadRequest.asset;
            */
            var resource = await LoadAssetFromBundleAsync(bundledName, bundle);

            Type resourceType = resource.GetType();

            var handleType = typeof(AssetBundleResourceHandle<>).MakeGenericType(resourceType);
            var rh = (ResourceHandle)Activator.CreateInstance(handleType, resource, bundledName, bundle, priority);

            var ro = RetrieveResourceObject(targetPath);
            ro.AddResourceHandle(rh, resource.GetType());
            return rh;
        }

        

        public ResourceHandle AddResourceFromFile(string targetPath, string filePath, ResourcePriority priority)
        {
            ResourceLoadContext context = new ResourceLoadContext() { AttemptingSyncLoad = true, ResourceManager = this, ResourceLoader = ResourceLoader, TargetPath = targetPath };
            var resource = ResourceLoader.Load(filePath, context);

            var ro = RetrieveResourceObject(targetPath);
            var handleType = typeof(FileResourceHandle<>).MakeGenericType(context.ResourceType);
            var rh = (ResourceHandle)Activator.CreateInstance(handleType, resource, filePath, priority, context);
            ro.AddResourceHandle(rh, resource.GetType());
            return rh;
        }

        public async Task<ResourceHandle> AddResourceFromFileAsync(string targetPath, string filePath, ResourcePriority priority)
        {
            ResourceLoadContext context = new ResourceLoadContext() { AttemptingSyncLoad = false, ResourceManager = this, ResourceLoader = ResourceLoader, TargetPath = targetPath };
            var resource = await ResourceLoader.LoadAsync(filePath, context);

            var ro = RetrieveResourceObject(targetPath);
            var handleType = typeof(FileResourceHandle<>).MakeGenericType(context.ResourceType);
            var rh = (ResourceHandle)Activator.CreateInstance(handleType, resource, filePath, priority, context);
            ro.AddResourceHandle(rh, resource.GetType());
            return rh;
        }


        //these two are probably junk
        //needs to be renamed, needs to not use StreamingAssets, needs to at least give the option to preload
        [Obsolete]
        public void AddStreamingResource(string path, string assetPath, ResourcePriority priority) //logical path, physical path...
        {
            //will probably fail on IL2CPP but I don't think failure is guaranteed
            var type = ResourceLoader.DetermineResourceType(Path.Combine(CoreParams.StreamingAssetsPath, assetPath), new ResourceLoadContext() { TargetPath = path, ResourceManager = this, ResourceLoader = ResourceLoader});
            var handleType = typeof(FileResourceHandle<>).MakeGenericType(type);
            ResourceHandle resourceHandle = (ResourceHandle)Activator.CreateInstance(handleType, Path.Combine(CoreParams.StreamingAssetsPath, assetPath), priority);

            var ro = RetrieveResourceObject(path);
            ro.AddResourceHandle(resourceHandle, type);
        }

        [Obsolete]
        public void AddStreamingResource<T>(string path, string assetPath, ResourcePriority priority) where T : UnityEngine.Object
        {
            var ro = RetrieveResourceObject(path);
            ro.AddResourceHandle(new FileResourceHandle<T>(Path.Combine(CoreParams.StreamingAssetsPath, assetPath), priority));
        }

        //gets a resource folder from the dictionary if it exists, creates it if it does not
        internal ResourceFolder RetrieveResourceFolder(string folderPath)
        {
            if (ResourceFolders.TryGetValue(folderPath, out var folder))
                return folder;

            ResourceFolder rf = new ResourceFolder(folderPath);
            ResourceFolders.Add(folderPath, rf);
            return rf;
        }

        private void InvalidatePathCache()
        {
            ResourceObjectCache.Clear();
        }

        internal ResourceObject RetrieveResourceObject(string path)
        {
            ResourceObject rObject;
            if (!ResourceObjectCache.TryGetValue(path, out rObject))
            {
                string folder = GetFolderName(path);
                ResourceFolder rFolder = RetrieveResourceFolder(folder);
                string file = GetFileName(path);
                rObject = rFolder.RetrieveResourceObject(file);
                ResourceObjectCache.Add(path, rObject);
            }

            return rObject;
        }

        /// <summary>
        /// Gets a consistently formatted folder path
        /// </summary>
        private static string GetAsFolderPath(string path)
        {
            string formattedPath;
            if (!path.Contains('\\'))
                formattedPath = path;
            else
                formattedPath = path.Replace('\\', '/');

            if (!(path.EndsWith(@"\") || path.EndsWith("/")))
                return formattedPath + "/";
            else
                return formattedPath;
        }

        /// <summary>
        /// Gets the path to the last director for a full path, assuming the end is a file
        /// </summary>
        private static string GetFolderName(string path)
        {
            return GetAsFolderPath(Path.GetDirectoryName(path));
        }

        /// <summary>
        /// Gets the name of the file from a full path
        /// </summary>
        private static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Transforms ResourcesVariants result into something suitable for DataResources
        /// </summary>
        /// <remarks>
        /// <para>Note that the result won't be identical, but it should be compatible with a sane loading routine</para>
        /// <para>Slow</para>
        /// </remarks>
        public static T[][] ResourcesVariantsToDataResources<T>(T[][] variantsResourcesArrays)
        {
            //basically we need to turn it "sideways", but it's complicated by being jagged
            List<List<T>> resourceLists = new List<List<T>>();

            for(int resIdx = 0; resIdx < variantsResourcesArrays.Length; resIdx++)
            {
                T[] variantResourceArray = variantsResourcesArrays[resIdx];
                for(int varIdx = 0; varIdx < variantResourceArray.Length; varIdx++)
                {
                    if(resourceLists.Count <= varIdx)
                    {
                        resourceLists.Add(new List<T>());
                    }

                    resourceLists[varIdx].Add(variantResourceArray[varIdx]);
                }
            }

            return resourceLists.Select(l => l.ToArray()).ToArray();
        }

        /// <summary>
        /// Transforms DataResources result into something suitable for ResourcesVariants
        /// </summary>
        /// <remarks>
        /// <para>Note that the result won't be identical, but it should be compatible with a sane loading routine</para>
        /// <para>Extremely slow</para>
        /// </remarks>
        public static T[][] DataResourcesToResourceVariants<T>(T[][] dataResourcesArrays) where T: UnityEngine.Object
        {
            Dictionary<string, List<T>> resourceDictionary = new Dictionary<string, List<T>>();

            for(int varIdx = 0; varIdx < dataResourcesArrays.Length; varIdx++)
            {
                T[] dataResourcesArray = dataResourcesArrays[varIdx];
                for(int resIdx = 0; resIdx < dataResourcesArray.Length; resIdx++)
                {
                    T resource = dataResourcesArray[resIdx];
                    string resName = resource.name;
                    if (!resourceDictionary.TryGetValue(resName, out var variantList))
                    {
                        variantList = new List<T>();
                        resourceDictionary.Add(resName, variantList);
                    }
                    variantList.Add(resource);
                }
            }

            return resourceDictionary.Values.Select(l => l.ToArray()).ToArray();
        }

        //don't really have a better place to put these yet

        /// <summary>
        /// Loads an object from an asset bundle, special-casing sprites and other subasset edge cases
        /// </summary>
        public static UnityEngine.Object LoadAssetFromBundle(string bundledName, AssetBundle bundle)
        {
            var asset = bundle.LoadAsset(bundledName);
            Type assetType = asset.GetType();

            if (assetType == typeof(Texture2D)) //could be a sprite!
            {
                var spriteAssets = bundle.LoadAssetWithSubAssets<Sprite>(bundledName);
                if (spriteAssets != null && spriteAssets.Length > 0)
                    return spriteAssets[0];

                //var subassets = bundle.LoadAssetWithSubAssets(bundledName);

                //foreach(var subasset in subassets)
                //    if(subasset is Sprite)
                //        return subasset;
            }

            return asset;
        }

        /// <summary>
        /// Loads an object from an asset bundle, special-casing sprites and other subasset edge cases (asynchronous version)
        /// </summary>
        public static async Task<UnityEngine.Object> LoadAssetFromBundleAsync(string bundledName, AssetBundle bundle)
        {
            var bundleLoadRequest = bundle.LoadAssetAsync(bundledName);

            while (!bundleLoadRequest.isDone)
                await Task.Yield();

            var asset = bundleLoadRequest.asset;
            Type assetType = asset.GetType();

            if (assetType == typeof(Texture2D)) //could be a sprite!
            {
                var bundleLoadRequest2 = bundle.LoadAssetWithSubAssetsAsync<Sprite>(bundledName);

                while (!bundleLoadRequest2.isDone)
                    await Task.Yield();

                var spriteAssets = bundleLoadRequest2.allAssets;
                if (spriteAssets != null && spriteAssets.Length > 0)
                    return spriteAssets[0];

                //will that work?
            }

            return asset;
        }

    }
}