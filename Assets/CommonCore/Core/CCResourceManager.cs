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
    internal class CCResourceManager
    {
        private Dictionary<string, UnityEngine.Object> RemapTable = new Dictionary<string, UnityEngine.Object>();
        //convention will be path/to/resource.type
        //we may move to a tree structure which would greatly speed up GetResources() at the cost of complexity
        //we may also only store/cache paths instead of the actual objects

        internal CCResourceManager()
        {
            //eventually we will support preloading the table (if set in config settings) but probably not until Citadel
        }

        internal T[] GetResources<T>(string path) where T : UnityEngine.Object
        {
            List<T> resources = new List<T>();

            //add resources from main path first
            resources.AddRange(Resources.LoadAll<T>(path));
            
            //then add resources from core if and only if they aren't already loaded
            foreach(var resource in Resources.LoadAll<T>("Core/" + path))
            {
                if (!resources.Find(x => x.name == resource.name))
                    resources.Add(resource);
            }

            //this is a slow implementation and we will optimize it later

            //let's see if this will work
            Debug.Log(resources.Select(x => x.name).ToNiceString());

            return resources.ToArray();
        }

        internal T GetResource<T>(string name) where T : UnityEngine.Object
        {
            string fullname = name + "." + typeof(T).Name;

            if (RemapTable.TryGetValue(fullname, out UnityEngine.Object tResource))
            {
                return (T)tResource;
            }

            return TryLoadResource<T>(name);
        }

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