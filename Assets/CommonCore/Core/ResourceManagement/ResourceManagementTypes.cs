using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

//TODO fill in and split as needed

namespace CommonCore.ResourceManagement
{
    public enum ResourcePriority
    {
        Core, Game, Module, Normal, Streaming, Addon, Explicit
    }

    public class ResourceFolder
    {
        public string Path { get; private set; } //I don't like this duplication but don't see a better way yet

        private HashSet<Type> ExploredForTypes { get; set; } = new HashSet<Type>();
        private Dictionary<string, ResourceObject> ResourceObjects { get; set; } = new Dictionary<string, ResourceObject>(StringComparer.OrdinalIgnoreCase);

        public ResourceFolder(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Returns a ResourceObject for a name, creating a blank one if it doesn't exist
        /// </summary>
        public ResourceObject RetrieveResourceObject(string name)
        {
            if (ResourceObjects.TryGetValue(name, out var obj))
                return obj;

            ResourceObject ro = new ResourceObject(System.IO.Path.Combine(Path, name));
            ResourceObjects.Add(name, ro);
            return ro;
        }

        //resource object manipulation APIs

        /// <summary>
        /// Returns a ResourceObject for a name, returning null if it doesn't exist
        /// </summary>
        public ResourceObject GetResourceObject(string name)
        {
            if (ResourceObjects.TryGetValue(name, out var obj))
                return obj;

            return null;
        }

        /// <summary>
        /// Removes a ResourceObject by name
        /// </summary>
        /// <remarks>Throws if the ResourceObject doesn't exist</remarks>
        public void RemoveResourceObject(string name)
        {
            if (!ResourceObjects.Remove(name))
                throw new KeyNotFoundException($"Failed to delete \"{name}\" because it doesn't exist!");
        }

        /// <summary>
        /// Removes a ResourceObject by reference
        /// </summary>
        /// <remarks>Throws if the ResourceObject doesn't exist</remarks>
        public void RemoveResourceObject(ResourceObject obj)
        {
            if(!ResourceObjects.ContainsValue(obj))
                throw new KeyNotFoundException($"Failed to delete \"{obj?.Path}\" because it doesn't exist!");

            var key = ResourceObjects.GetKeyForValue(obj);
            ResourceObjects.Remove(key);
        }

        /// <summary>
        /// Checks if a ResourceObject by a name exists
        /// </summary>
        public bool HasResourceObject(string name)
        {
            return ResourceObjects.ContainsKey(name);
        }

        /// <summary>
        /// Checks if a ResourceObject exists
        /// </summary>
        public bool HasResourceObject(ResourceObject obj)
        {
            return ResourceObjects.ContainsValue(obj);
        }

        public void ExploreForType<T>() where T : UnityEngine.Object
        {
            if (ExploredForTypes.Contains(typeof(T)))
                return;

            //explore and add handles
            T[] coreResources = Resources.LoadAll<T>(System.IO.Path.Combine("Core", Path));
            foreach (var resource in coreResources)
                addHandle(resource, ResourcePriority.Core);

            T[] gameResources = Resources.LoadAll<T>(System.IO.Path.Combine("Game", Path));
            foreach (var resource in gameResources)
                addHandle(resource, ResourcePriority.Game);

            T[] resources = Resources.LoadAll<T>(Path);
            foreach (var resource in resources)
                addHandle(resource, ResourcePriority.Normal);

            //don't forget to add to ExploredForTypes when we're done

            ExploredForTypes.Add(typeof(T));

            void addHandle(T resource, ResourcePriority priority)
            {
                string name = resource.name;
                ResourceObject ro;
                if (ResourceObjects.ContainsKey(name))
                {
                    ro = ResourceObjects[name];

                    if (ro.HasResourceOfPriority<T>(priority)) //avoid duplicates!
                        return;
                }
                else
                {
                    ro = new ResourceObject(System.IO.Path.Combine(Path, name));
                    ResourceObjects.Add(name, ro);
                }

                string rpath;
                switch (priority) //could optimize this
                {
                    case ResourcePriority.Core:
                        rpath = System.IO.Path.Combine("Core", Path, name);
                        break;
                    case ResourcePriority.Game:
                        rpath = System.IO.Path.Combine("Game", Path, name);
                        break;
                    case ResourcePriority.Normal:
                        rpath = System.IO.Path.Combine(Path, name);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                ro.AppendResourceHandle<T>(new UnityResourceHandle<T>(rpath, priority)); 
            }
        }

        /// <summary>
        /// Gets all resources (the highest priority of each one) in this folder
        /// </summary>
        public T[] GetResources<T>(bool exactType) where T : UnityEngine.Object
        {
            ExploreForType<T>();

            List<T> resources = new List<T>();
            foreach(var kvp in ResourceObjects)
            {
                var resource = kvp.Value.GetResource<T>(exactType);
                if (resource != null)
                    resources.Add(resource);
            }

            return resources.ToArray();
        }

        /// <summary>
        /// Gets all resources (the highest priority of each one) in this folder as a dictionary
        /// </summary>
        public Dictionary<string, T> GetResourcesDictionary<T>(bool exactType) where T: UnityEngine.Object
        {
            ExploreForType<T>();

            Dictionary<string, T> resources = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach(var kvp in ResourceObjects)
            {
                var resource = kvp.Value.GetResource<T>(exactType);
                if (resource != null)
                    resources.Add(kvp.Key, resource);
            }

            return resources;
        }

        /// <summary>
        /// Gets all resources (all priorities of each one) in this folder
        /// </summary>
        public T[][] GetResourcesAll<T>(bool exactType) where T : UnityEngine.Object
        {
            ExploreForType<T>();

            List<T[]> resources = new List<T[]>();
            foreach(var kvp in ResourceObjects)
            {
                var resource = kvp.Value.GetResourceAll<T>(exactType);
                if (resource != null && resource.Length > 0)
                    resources.Add(resource);
            }

            return resources.ToArray();
        }

        /// <summary>
        /// Gets all resources (all priorities of each one) in this folder as a dictionary
        /// </summary>
        public Dictionary<string, IEnumerable<T>> GetResourcesAllDictionary<T>(bool exactType) where T : UnityEngine.Object
        {
            ExploreForType<T>();
            Dictionary<string, IEnumerable<T>> resources = new Dictionary<string, IEnumerable<T>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in ResourceObjects)
            {
                var resource = kvp.Value.GetResourceAll<T>(exactType);
                if (resource != null && resource.Length > 0)
                    resources.Add(kvp.Key, resource);
            }

            return resources;
        }

    }

    public class ResourceObject
    {
        public string Path { get; private set; }

        private Dictionary<Type, List<ResourceHandle>> ResourceHandleLists { get; set; } = new Dictionary<Type, List<ResourceHandle>>();

        private Dictionary<Type, ResourceHandle> TypeBestResourceCache = new Dictionary<Type, ResourceHandle>(); //stores best-resource-for-type data (limitation: can't cache null)

        public ResourceObject(string path)
        {
            Path = path;
        }

        //explores default resource paths for a given type if it does not exist
        public void ExploreForType<T>() where T : UnityEngine.Object
        {
            if (ResourceHandleLists.ContainsKey(typeof(T)))
                return; //we have already explored, do nothing

            List<ResourceHandle> rHandles = new List<ResourceHandle>();

            // get resources and redirects from unity resources

            string corePath = System.IO.Path.Combine("Core", Path);
            ResourceHandle handle = GetUnityResourceHandleIfExists<T>(corePath, ResourcePriority.Core);
            if (handle != null)
                rHandles.Add(handle);
            handle = GetRedirectResourceHandleIfExists<T>(corePath, ResourcePriority.Core);
            if (handle != null)
                rHandles.Add(handle);

            string gamePath = System.IO.Path.Combine("Game", Path);
            handle = GetUnityResourceHandleIfExists<T>(gamePath, ResourcePriority.Game);
            if (handle != null)
                rHandles.Add(handle);
            handle = GetRedirectResourceHandleIfExists<T>(gamePath, ResourcePriority.Game);
            if (handle != null)
                rHandles.Add(handle);

            handle = GetUnityResourceHandleIfExists<T>(Path, ResourcePriority.Normal);
            if (handle != null)
                rHandles.Add(handle);
            handle = GetRedirectResourceHandleIfExists<T>(Path, ResourcePriority.Normal);
            if (handle != null)
                rHandles.Add(handle);

            ResourceHandleLists.Add(typeof(T), rHandles);
            InvalidateCache();
        }

        public T GetResource<T>(bool exactType) where T : UnityEngine.Object
        {
            if (!exactType)
                return GetResource<T>();
            else
                return GetResourceExactType<T>();
        }

        public T GetResourceSafeRecurse<T>(bool exactType, int depth, ResourceObject root) where T : UnityEngine.Object
        {
            //check depth and root
            if (depth > CoreParams.ResourceMaxRecurseDepth)
                throw new RecursionLimitHitException();
            if (root == this)
                throw new RecursionLoopException();

            if (!exactType)
                return GetResource<T>(depth+1, root);
            else
                return GetResourceExactType<T>(depth+1, root);
        }

        //gets the best-fit, highest-priority resource by name
        private T GetResource<T>(int depth = 0, ResourceObject root = null) where T : UnityEngine.Object
        {
            //no!
            //if (ResourceHandles.ContainsKey(typeof(T)))
            //    return ((ResourceHandle<T>)GetBestResource(ResourceHandles[typeof(T)])).Resource;

            if (TypeBestResourceCache.TryGetValue(typeof(T), out var cachedValue))
                return (T)cachedValue.Resource; //TODO check for null? necessary for redirects I think

            //explore for type to make sure we have that
            ExploreForType<T>();

            //then scan and get the highest priority of assignable types
            List<ResourceHandle> possibleResourceHandles = new List<ResourceHandle>();
            foreach(var kvp in ResourceHandleLists)
            {
                if (typeof(T).IsAssignableFrom(kvp.Key) && kvp.Value.Count > 0)
                    possibleResourceHandles.Add(GetBestResource(kvp.Value));
            }

            if (possibleResourceHandles.Count == 0)
                return null;

            if (possibleResourceHandles.Count == 1)
                return (T)possibleResourceHandles[0].Resource;

            ResourceHandle bestResourceHandle = null;
            T bestRedirectedResource = null;
            foreach(var rHandle in possibleResourceHandles)
            {
                if (bestResourceHandle == null || rHandle.Priority > bestResourceHandle.Priority || (rHandle.Priority == bestResourceHandle.Priority && rHandle.HandleID > bestResourceHandle.HandleID))
                {
                    if (rHandle is IRedirectHandle irh)
                    {
                        object redirectedResource = irh.GetRedirectedResource(false, depth, root ?? this);
                        if (redirectedResource.Ref() == null)
                            continue; //abort
                        else
                        {
                            bestRedirectedResource = (T)redirectedResource;
                            bestResourceHandle = rHandle;
                            continue;
                        }
                            
                    }
                    bestRedirectedResource = null;
                    bestResourceHandle = rHandle;
                }

            }

            if (bestResourceHandle != null)
            {
                if(!(bestResourceHandle is IRedirectHandle))
                    TypeBestResourceCache.Add(typeof(T), bestResourceHandle);

                if (bestRedirectedResource != null)
                    return bestRedirectedResource;

                return (T)bestResourceHandle.Resource;
            }

            return null;
        }

        private T GetResourceExactType<T>(int depth = 0, ResourceObject root = null) where T : UnityEngine.Object
        {
            //get resource, exact type
            ExploreForType<T>();

            if (ResourceHandleLists.ContainsKey(typeof(T)))
            {
                var list = ResourceHandleLists[typeof(T)];
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var rhandle = list[i];

                    if (rhandle is IRedirectHandle irh)
                    {
                        object res = irh.GetRedirectedResource(true, depth, root ?? this);
                        T tRes = res as T;
                        if (tRes != null && tRes.GetType() == typeof(T))
                            return tRes;
                    }
                    else
                    {
                        var res = rhandle.Resource;
                        if (res.GetType() == typeof(T))
                            return (T)res;
                    }
                }
            }
            //return ((ResourceHandle<T>)GetBestResource(ResourceHandleLists[typeof(T)])).Resource;

            return null;
        }

        //gets all variants of a resource object, sorted from lowest to highest priority
        public T[] GetResourceAll<T>(bool exactType) where T : UnityEngine.Object
        {
            ExploreForType<T>();

            if(exactType)
            {
                if(ResourceHandleLists.TryGetValue(typeof(T), out var list))
                {
                    return list.Select(h => h is IRedirectHandle ? (T)((IRedirectHandle)h).GetRedirectedResource(true, 0, this) : (T)h.Resource).Where(r => r != null).ToArray();
                }
                else
                {
                    return null; //should we be returning empty instead?
                }
            }
            else
            {
                //we need to do a lot of fancy footwork here
                
                //get all type-compatible lists
                List<List<ResourceHandle>> relevantLists = new List<List<ResourceHandle>>();
                foreach (var kvp in ResourceHandleLists)
                {
                    if (typeof(T).IsAssignableFrom(kvp.Key) && kvp.Value.Count > 0)
                    {
                        relevantLists.Add(kvp.Value);
                    }
                }

                //concatenate and sort by priority
                List<ResourceHandle> handles = new List<ResourceHandle>();
                foreach (var list in relevantLists)
                    handles.AddRange(list);

                return handles
                    .OrderBy(h => h.Priority)
                    .ThenBy(h => h.HandleID)
                    .Select(h => h is IRedirectHandle ? (T)((IRedirectHandle)h).GetRedirectedResource(true, 0, this) : (T)h.Resource)
                    .Where(r => r != null)
                    .Distinct()
                    .ToArray();
            }
        }

        /// <summary>
        /// Returns true if resource exists for a given type
        /// </summary>
        /// <remarks>
        /// <para>If this returns true, GetResource is guaranteed to return a non-null value</para>
        /// </remarks>
        public bool ExistsForType<T>(bool exactType) where T : UnityEngine.Object
        {
            ExploreForType<T>();

            //all this complexity is due to redirects, fuck you redirects

            if (ResourceHandleLists.ContainsKey(typeof(T)) && ResourceHandleLists[typeof(T)].Count > 0)
            {
                var list = ResourceHandleLists[typeof(T)];
                for(int i = list.Count - 1; i >= 0; i--)
                {
                    ResourceHandle rHandle = list[i];
                    if(rHandle is IRedirectHandle irh)
                    {
                        var resource = irh.GetRedirectedResource(true, 0, this);
                        if (resource.Ref() != null)
                            return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }                
            
            if (exactType)
                return false;

            foreach (var kvp in ResourceHandleLists)
            {
                if (typeof(T).IsAssignableFrom(kvp.Key) && kvp.Value.Count > 0)
                {
                    var list = kvp.Value;

                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        ResourceHandle rHandle = list[i];
                        if (rHandle is IRedirectHandle irh)
                        {
                            var resource = irh.GetRedirectedResource(false, 0, this);
                            if (resource.Ref() != null)
                                return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if any resource variant exists for a given type
        /// </summary>
        /// <remarks>
        /// <para>This does not imply GetResource will return a non-null value, but there will be at least one non-null item in GetResourceAll</para>
        /// </remarks>
        public bool AnyVariantExistsForType<T>(bool exactType) where T : UnityEngine.Object
        {
            return ExistsForType<T>(exactType); //semantics are the same as ExistsForType until knockouts and a few other exotic things are added
        }

        /// <summary>
        /// Gets a list of types we have resource handle lists for
        /// </summary>
        /// <remarks>
        /// <para>Note that no fuzzy type matching is done; we return all the list types that exist even if some would never be used</para>
        /// </remarks>
        public Type[] GetResourceHandleTypes()
        {
            return ResourceHandleLists.Keys.ToArray();
        }

        /// <summary>
        /// Gets a list of resource handles for a given type
        /// </summary>
        /// <remarks>
        /// <para>Note that no fuzzy type matching is done; it will return the list for the exact type</para>
        /// <para>Returns null if there is no list of resource handles for the type</para>
        /// <para>Note that due to Unity shenanigans, the actual resource may not be of type T</para>
        /// </remarks>
        public ResourceHandle<T>[] GetResourceHandles<T>() where T : UnityEngine.Object
        {
            if(ResourceHandleLists.TryGetValue(typeof(T), out var list))
            {
                return list.Select(h => (ResourceHandle<T>)h).ToArray(); //LINQ is probably slow but who will actually use this API?
            }

            return null;
        }

        /// <summary>
        /// Gets a list of resource handles for a given type
        /// </summary>
        /// <remarks>
        /// <para>Note that no fuzzy type matching is done; it will return the list for the exact type</para>
        /// <para>Returns null if there is no list of resource handles for the type</para>
        /// <para>Note that due to Unity shenanigans, the actual resource may not be of type T</para>
        /// </remarks>
        public ResourceHandle[] GetResourceHandles(Type type)
        {
            if (ResourceHandleLists.TryGetValue(type, out var list))
                return list.ToArray();

            return null;
        }

        /// <summary>
        /// Adds a resource handle to a resource handle list, inserting respecting the priority of the handle
        /// </summary>
        /// <remarks>This is the generic version, which is highly recommended</remarks>
        public void AddResourceHandle<T>(ResourceHandle<T> handle) where T : UnityEngine.Object
        {
            ExploreForType<T>(); //so we want this if we're adding a ResourceHandle ex-post-facto, but not if we're doing inital setup
            var list = ResourceHandleLists[typeof(T)];
            int i = 0;
            for(; i < list.Count; i++)
            {
                if (list[i].Priority > handle.Priority)
                    break;
            }
            if (i == 0)
                list.Add(handle);
            else if (i >= list.Count) //should be list.count-1?
                list.Add(handle);
            else
                list.Insert(i - 1, handle);

            InvalidateCache();
        }

        /// <summary>
        /// Adds a resource handle to a resource handle list, inserting respecting the priority of the handle
        /// </summary>
        /// <remarks>This is the non-generic version, which is slow and won't work under IL2CPP</remarks>
        public void AddResourceHandle(ResourceHandle handle, Type type)
        {
#if ENABLE_IL2CPP

            throw new NotImplementedException();
#else
            var eftMethod = GetType().GetMethod("ExploreForType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type); //should be ExistsForType?
            eftMethod.Invoke(this, null);

            var list = ResourceHandleLists[type];
            
            int i = 0;
            for (; i < list.Count; i++)
            {
                if (list[i].Priority > handle.Priority)
                    break;
            }
            if (i == 0)
                list.Add(handle);
            else if (i >= list.Count) //should be list.count-1?
                list.Add(handle);
            else
                list.Insert(i - 1, handle);

            InvalidateCache();
#endif
        }

        /// <summary>
        /// Appends a resource handle to this resource object, without checking for existing resources and not inserting in place
        /// </summary>
        internal void AppendResourceHandle<T>(ResourceHandle<T> handle) where T : UnityEngine.Object
        {
            if(!ResourceHandleLists.TryGetValue(typeof(T), out var list))
            {
                list = new List<ResourceHandle>();
                ResourceHandleLists.Add(typeof(T), list);
            }

            list.Add(handle);
            InvalidateCache();
        }

        /// <summary>
        /// Checks if this object has already been explored for a type
        /// </summary>
        internal bool IsExploredForType<T>() where T : UnityEngine.Object
        {
            return ResourceHandleLists.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Checks if this object contains a resource of priority and type
        /// </summary>
        internal bool HasResourceOfPriority<T>(ResourcePriority priority)
        {
            if(ResourceHandleLists.TryGetValue(typeof(T), out var list))
            {
                if(list.Count > 0)
                {
                    foreach (var item in list)
                        if (item.Priority == priority)
                            return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes an existing resource handle
        /// </summary>
        /// <remarks>
        /// <para>Note that no fuzzy type matching is done; it will only remove from the list for the exact type</para>
        /// </remarks>
        public void RemoveResourceHandle<T>(ResourceHandle handle)
        {
            RemoveResourceHandle(handle, typeof(T));
        }

        /// <summary>
        /// Removes an existing resource handle
        /// </summary>
        /// <remarks>
        /// <para>Note that no fuzzy type matching is done; it will only remove from the list for the exact type</para>
        /// <para>Note also that it will throw if the handle doesn't exist or no list exists for the type</para>
        /// </remarks>
        public void RemoveResourceHandle(ResourceHandle handle, Type type)
        {
            var list = ResourceHandleLists[type];
            if (!list.Remove(handle))
                throw new KeyNotFoundException("Failed to remove handle because it was not found");

            InvalidateCache();
        }

        private void InvalidateCache()
        {
            //invalidate our type->handle resolution cache
            TypeBestResourceCache.Clear();
        }

        private static ResourceHandle GetBestResource(List<ResourceHandle> list)
        {
            return list[list.Count - 1];
        }

        private static UnityResourceHandle<T> GetUnityResourceHandleIfExists<T>(string path, ResourcePriority priority) where T : UnityEngine.Object
        {
            var resource = Resources.Load<T>(path);
            if (resource != null)
            {
                var handle = new UnityResourceHandle<T>(path, priority);
                return handle;
            }

            return null;
        }

        private static RedirectResourceHandle<T> GetRedirectResourceHandleIfExists<T>(string path, ResourcePriority priority) where T : UnityEngine.Object
        {
            var redirectResource = Resources.Load<RedirectAsset>(path);
            if(redirectResource != null)
            {
                string basePath = path.Substring(0, path.LastIndexOf('/')) + "/";
                var handle = new RedirectResourceHandle<T>(redirectResource, basePath, CCBase.ResourceManager, priority); //TODO remove this weird backwards dependency, inject downwards on creation
                return handle;
            }

            return null;
        }
    }

    public abstract class ResourceHandle
    {
        public ResourcePriority Priority { get; protected set; } = ResourcePriority.Normal;
        public int HandleID { get; }
        
        public object Resource => UntypedResource; //wait...
        protected virtual object UntypedResource { get; }

        public virtual Type ResourceType { get; }

        protected ResourceHandle()
        {
            HandleID = CCBase.ResourceManager.NextResourceHandleID; // I don't like this backwards dependency but I don't hate it enough to remove it yet
        }
    }

    public abstract class ResourceHandle<T> : ResourceHandle where T : UnityEngine.Object
    {
        public new abstract T Resource { get; } //...whaaaaaa?
        protected sealed override object UntypedResource => Resource;

        public sealed override Type ResourceType => typeof(T);
    }

    public class UnityResourceHandle<T> : ResourceHandle<T> where T : UnityEngine.Object
    {
        public string ResourcePath { get; private set; }
        public override T Resource
        {
            get
            {
                LoadResourceIfNull();
                return Resources.Load<T>(ResourcePath);
            }
        }

        private T CachedResource = null;

        public UnityResourceHandle(string path, ResourcePriority priority)
        {
            ResourcePath = path;
            Priority = priority;

            LoadResourceIfNull();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LoadResourceIfNull()
        {
            if (CachedResource == null)
                CachedResource = Resources.Load<T>(ResourcePath);
        }

    }

    //disgusting hack
    public interface IRedirectHandle
    {
        object GetRedirectedResource(bool typeExact, int depth, ResourceObject root);
    }

    public class RedirectResourceHandle<T> : ResourceHandle<T>, IRedirectHandle where T : UnityEngine.Object
    {
        public string Path { get; private set; }
        //I was thinking about "absolute path" versus "path" but there's no clear meaning of that so not for now

        private WeakReference<ResourceManager> ResourceManagerReference;
        //TODO we can probably cache _at least_ ResourceObject here

        public override T Resource
        {
            get
            {
                if (ResourceManagerReference.TryGetTarget(out var resourceManager))
                    return resourceManager.GetResource<T>(Path, false); //the "default" is Unity-like loose matching

                return null;
            }
        }
        
        public RedirectResourceHandle(RedirectAsset redirectAsset, string basePath, ResourceManager resourceManager, ResourcePriority priority)
        {
            Path = redirectAsset.Path.StartsWith("/") ? redirectAsset.Path.TrimStart('/') : basePath + redirectAsset.Path;
            Priority = priority;
            ResourceManagerReference = new WeakReference<ResourceManager>(resourceManager);
        }

        public T GetRedirectedResource(bool typeExact, int depth, ResourceObject root)
        {
            if (ResourceManagerReference.TryGetTarget(out var resourceManager))
            {
                var ro = resourceManager.RetrieveResourceObject(Path);
                return ro.GetResourceSafeRecurse<T>(typeExact, depth, root);
            }

            return null;
        }

        object IRedirectHandle.GetRedirectedResource(bool typeExact, int depth, ResourceObject root) => GetRedirectedResource(typeExact, depth, root);
    }

    /// <summary>
    /// Resource handle that stores a reference to something created at runtime
    /// </summary>
    /// <remarks>Be very, very careful about the lifetime of the resource!</remarks>
    public class RuntimeResourceHandle<T> : ResourceHandle<T> where T : UnityEngine.Object
    {
        public override T Resource => _Resource;

        private T _Resource;

        public RuntimeResourceHandle(T resource) : this(resource, ResourcePriority.Explicit)
        {
        }

        public RuntimeResourceHandle(T resource, ResourcePriority priority)
        {
            _Resource = resource;
            Priority = priority;
        }
    }

    public class AssetBundleResourceHandle<T> : ResourceHandle<T> where T : UnityEngine.Object
    {
        public override T Resource
        {
            get
            {
                if (_Resource == null)
                    Load();

                return _Resource;
            }
        }

        private T _Resource;

        private AssetBundle AssetBundle;
        private string AssetName;

        public AssetBundleResourceHandle(T resource, string bundledName, AssetBundle bundle, ResourcePriority priority)
        {
            AssetBundle = bundle;
            AssetName = bundledName;
            _Resource = Resource;
        }

        public bool Load()
        {
            //_Resource = AssetBundle.LoadAsset<T>(AssetName);
            _Resource = (T)ResourceManager.LoadAssetFromBundle(AssetName, AssetBundle);

            return _Resource != null;
        }

        public async Task<bool> LoadAsync()
        {
            /*
            var bundleLoadRequest = AssetBundle.LoadAssetAsync<T>(AssetName);

            while (!bundleLoadRequest.isDone)
                await Task.Yield();

            _Resource = (T)bundleLoadRequest.asset;
            */
            _Resource = (T)await ResourceManager.LoadAssetFromBundleAsync(AssetName, AssetBundle);

            return _Resource != null;
        }
    }

    public class FileResourceHandle<T> : ResourceHandle<T> where T : UnityEngine.Object
    {
        //need to handle the possible case where assets _must_ be preloaded

        public override T Resource
        {
            get
            {
                if (_Resource == null)
                    Load();

                return _Resource;
            }
        }

        public string Path { get; private set; }
        //do we also need to know our logical path?
        private T _Resource;

        //initial import settings if applicable
        private IResourceImporter Importer = null;

        public FileResourceHandle(string path): this(path, ResourcePriority.Streaming)
        {

        }

        public FileResourceHandle(string path, ResourcePriority priority)
        {
            Path = path;
            Priority = priority;
        }

        public FileResourceHandle(T resource, string path, ResourcePriority priority)
        {
            Path = path;
            Priority = priority;
            _Resource = resource;
        }

        public FileResourceHandle(T resource, string path, ResourcePriority priority, ResourceLoadContext initialLoadContext) : this(resource, path, priority)
        {
            Importer = initialLoadContext.ResourceImporter;
        }

        //should we reload or ignore if already loaded?

        public bool Load()
        {
            _Resource = (T)CCBase.ResourceManager.ResourceLoader.LoadTyped(Path, new ResourceLoadContext() { TargetPath = null, ResourceManager = CCBase.ResourceManager, ResourceLoader = CCBase.ResourceManager.ResourceLoader, AttemptingSyncLoad = true, ResourceImporter = Importer, ResourceType = typeof(T)}); //do we need to know our own target path?
            return _Resource != null;
        }

        public async Task<bool> LoadAsync()
        {
            _Resource = (T)await CCBase.ResourceManager.ResourceLoader.LoadTypedAsync(Path, new ResourceLoadContext() { TargetPath = null, ResourceManager = CCBase.ResourceManager, ResourceLoader = CCBase.ResourceManager.ResourceLoader, AttemptingSyncLoad = false, ResourceImporter = Importer, ResourceType = typeof(T) });
            return _Resource != null;
        }
    }

    public class RecursionLimitHitException : Exception
    {
        public override string Message => "The maximum recursion limit was hit";
    }

    public class RecursionLoopException : Exception
    {
        public override string Message => "An infinite recursion loop was detected";
    }

}