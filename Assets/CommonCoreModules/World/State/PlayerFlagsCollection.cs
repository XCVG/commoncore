using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// A collection of player flags, stored in this object or pulled from other sources.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PlayerFlagsCollection
    {
        [JsonProperty]
        private HashSet<string> Flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        private List<IPlayerFlagsSource> FlagSources = new List<IPlayerFlagsSource>();


        public PlayerFlagsCollection()
        {

        }

        public bool HasSource(IPlayerFlagsSource source)
        {
            return FlagSources.Contains(source);
        }

        public void RegisterSource(IPlayerFlagsSource source)
        {
            if(FlagSources.Contains(source))
            {
                Debug.LogWarning($"Tried to add player flag source ({source.GetType().Name}) that already exists!");
                return;
            }

            FlagSources.Add(source);
        }

        public void UnregisterSource(IPlayerFlagsSource source)
        {
            if (FlagSources.Contains(source))
                FlagSources.Remove(source);
            else
                Debug.LogWarning($"Tried to remove player flag source ({source.GetType().Name}) that didn't exist!");
        }

        private void ScrubSources()
        {
            for(int i = FlagSources.Count - 1; i >= 0; i--)
            {
                if (FlagSources[i] == null)
                    FlagSources.RemoveAt(i);
            }
        }


        //I almost made this implement ICollection<string> but it doesn't really fit the contract (mostly due to weirdness around pulling flags from other sources)

        public int Count
        {
            get
            {
                int count = Flags.Count;
                foreach (var source in FlagSources)
                    count += source.Count;

                return count;
            }
        }

        public void Add<T>(T item) where T : Enum
        {
            Flags.Add(item.ToString());
        }

        public void Add(string item)
        {
            Flags.Add(item);
        }

        public void Clear()
        {
            Flags.Clear();
        }

        public bool Contains<T>(T item) where T : Enum
        {
            return Contains(item.ToString());
        }

        public bool Contains(string item)
        {
            //this is probably too slow/garbage-generating to be workable

            if (Flags.Contains(item))
                return true;

            if (FlagSources.Count == 0)
                return false;

            foreach(var source in FlagSources)
            {
                if (source.Contains(item))
                    return true;
            }

            return false;
        }

        public bool Remove<T>(T item) where T : Enum
        {
            return Flags.Remove(item.ToString());
        }

        public bool Remove(string item)
        {
            return Flags.Remove(item);
        }

        public IList<string> GetAllFlags()
        {
            IEnumerable<string> flags = new HashSet<string>(Flags);
            foreach(var source in FlagSources)
            {
                flags = flags.Concat(source.Flags);
            }

            return flags.ToList();
        }

    }
}