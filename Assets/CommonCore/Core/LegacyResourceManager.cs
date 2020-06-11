using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// Manager for flexible resource loading, with virtual path handling
    /// </summary>
    internal class LegacyResourceManager
    {
        private Dictionary<string, UnityEngine.Object> RemapTable = new Dictionary<string, UnityEngine.Object>();
        //convention will be path/to/resource.type
        //we may move to a tree structure which would greatly speed up GetResources() at the cost of complexity
        //we may also only store/cache paths instead of the actual objects
        //we may need to rework it to handle mod support/resource injection
        //actually we'd basically need a tree for that or it would be dog slow because we'd have to traverse the list whenever we call GetResources()

        internal LegacyResourceManager()
        {
            //eventually we will support preloading the table (if set in config settings) but probably not until Citadel
        }

        /// <summary>
        /// Get all resources at a virtual resource path
        /// </summary>
        internal T[] GetResources<T>(string path) where T : UnityEngine.Object
        {
            List<T> resources = new List<T>();

            //add resources from main path first
            resources.AddRange(Resources.LoadAll<T>(path));

            //then add resources from game module if and only if they aren't already loaded
            foreach (var resource in Resources.LoadAll<T>("Game/" + path))
            {
                if (!resources.Find(x => x.name == resource.name))
                    resources.Add(resource);
            }

            //then add resources from core if and only if they aren't already loaded
            foreach (var resource in Resources.LoadAll<T>("Core/" + path))
            {
                if (!resources.Find(x => x.name == resource.name))
                    resources.Add(resource);
            }

            //this is a slow implementation and we will optimize it later

            return resources.ToArray();
        }

        /// <summary>
        /// Get the resource at the virtual path
        /// </summary>
        internal T GetResource<T>(string name) where T : UnityEngine.Object
        {
            string fullname = name + "." + typeof(T).Name;

            if (RemapTable.TryGetValue(fullname, out UnityEngine.Object tResource))
            {
                return (T)tResource;
            }

            return TryLoadResource<T>(name);
        }

        /// <summary>
        /// Gets all versions of all resources at a virtual resource path, ordered from lowest to highest precedence
        /// </summary>
        /// <remarks>
        /// Intended for loading Data resources where overriding will be handled in the modules, hence the name
        /// </remarks>
        internal T[][] GetDataResources<T>(string path) where T : UnityEngine.Object
        {
            List<T[]> resources = new List<T[]>();

            //try loading from core namespace
            T[] coreResources = Resources.LoadAll<T>("Core/" + path);
            if (coreResources != null && coreResources.Length > 0)
                resources.Add(coreResources);

            //try loading from game namespace
            T[] gameResources = Resources.LoadAll<T>("Game/" + path);
            if (gameResources != null && gameResources.Length > 0)
                resources.Add(gameResources);

            //try loading from main namespace
            T[] mainResources = Resources.LoadAll<T>(path);
            if (mainResources != null && mainResources.Length > 0)
                resources.Add(mainResources);

            return resources.ToArray();
        }

        /// <summary>
        /// Gets all versions of a resource at virtual path, ordered from lowest to highest precedence
        /// </summary>
        /// <remarks>
        /// Intended for loading Data resources where overriding will be handled in the modules, hence the name
        /// </remarks>
        internal T[] GetDataResource<T>(string name) where T : UnityEngine.Object
        {
            List<T> resources = new List<T>();

            //try loading from core namespace
            T coreResource = Resources.Load<T>("Core/" + name);
            if (coreResource != null)
            {
                resources.Add(coreResource);
            }

            //try loading from game namespace
            T gameResource = Resources.Load<T>("Game/" + name);
            if (gameResource != null)
            {
                resources.Add(gameResource);
            }

            //try loading from main namespace
            T mainResource = Resources.Load<T>(name);
            if (mainResource != null)
            {
                resources.Add(mainResource);
            }

            //highest precedence is the one from the remap table, if different
            if (RemapTable.TryGetValue(name + "." + typeof(T).Name, out UnityEngine.Object rtResource) && rtResource is T tResource && !resources.Contains(tResource))
                resources.Add(tResource);

            return resources.ToArray();
        }

        /// <summary>
        /// Check if a resource exists
        /// </summary>
        internal bool ContainsResource<T>(string name) where T : UnityEngine.Object
        {
            string fullname = name + "." + typeof(T).Name;

            if (RemapTable.ContainsKey(fullname))
                return true;

            return (TryLoadResource<T>(name) != null);
        }

        private T TryLoadResource<T>(string name) where T : UnityEngine.Object
        {
            string fullname = name + "." + typeof(T).Name;

            //try loading from main namespace
            T resource = Resources.Load<T>(name);
            if (resource != null)
            {
                RemapTable.Add(fullname, resource);
                return resource;
            }

            //try loading from game namespace
            resource = Resources.Load<T>("Game/" + name);
            if (resource != null)
            {
                RemapTable.Add(fullname, resource);
                return resource;
            }

            //try loading from core namespace
            resource = Resources.Load<T>("Core/" + name);
            if (resource != null)
            {
                RemapTable.Add(fullname, resource);
                return resource;
            }

            return null;
        }

    }
}