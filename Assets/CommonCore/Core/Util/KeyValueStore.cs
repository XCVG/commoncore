using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.Util
{

    /// <summary>
    /// Generic key-value store component. Unoptimized and slow. I don't know why I made this.
    /// </summary>
    [AddComponentMenu("CommonCore/KeyValueStore")]
    public sealed class KeyValueStore : MonoBehaviour, IDictionary<string, string>
    {
        [SerializeField]
        private bool CaseSensitive = false;

        [SerializeField, NonReorderable]
        private List<KeyValueStoreNode> KeyValues = new List<KeyValueStoreNode>();

        private StringComparison ComparisonInternal => CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        private void AddInternal(string key, string value)
        {
            if (KeyValues.Any(kv => kv.Key.Equals(key, ComparisonInternal)))
                throw new ArgumentException($"Key {key} already exists in KeyValueStore");

            KeyValues.Add(new KeyValueStoreNode() { Key = key, Value = value });
        }

        private bool TryGetTypedInternal(string key, Type type, out object val)
        {
            try
            {
                if(TryGetValue(key, out string rawValue))
                {
                    val = TypeUtils.CoerceValue(rawValue, type, false);
                    return true;
                }
            }
            catch(Exception)
            {

            }

            val = null;
            return false;
        }

        public string this[string key]
        {
            get => KeyValues.Where(kv => kv.Key.Equals(key, ComparisonInternal)).FirstOrDefault()?.Value;
            set
            {
                int idx = KeyValues.FindIndex(kv => kv.Key.Equals(key, ComparisonInternal));
                if (idx >= 0)
                    KeyValues.RemoveAt(idx);
                KeyValues.Add(new KeyValueStoreNode() { Key = key, Value = value });
            }
        }

        public ICollection<string> Keys => KeyValues.Select(kv => kv.Key).ToArray();

        public ICollection<string> Values => KeyValues.Select(kv => kv.Value).ToArray();

        public int Count => KeyValues.Count;

        public bool IsReadOnly => false;

        public void Add(string key, string value)
        {
            AddInternal(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            AddInternal(item.Key, item.Value);
        }

        public void Clear()
        {
            KeyValues.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item) => KeyValues.Any(kv => kv.Key.Equals(item.Key, ComparisonInternal) && kv.Value == item.Value);

        public bool ContainsKey(string key) => KeyValues.Any(kv => kv.Key.Equals(key, ComparisonInternal));

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            for(int i = 0; i < KeyValues.Count; i++)
            {
                var kv = KeyValues[i];
                array[i] = new KeyValuePair<string, string>(kv.Key, kv.Value);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => KeyValues.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value)).GetEnumerator();

        public bool Remove(string key)
        {
            int idx = KeyValues.FindIndex(kv => kv.Key.Equals(key, ComparisonInternal));
            if (idx < 0)
                return false;
            KeyValues.RemoveAt(idx);
            return true;
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            int idx = KeyValues.FindIndex(kv => kv.Key.Equals(item.Key, ComparisonInternal) && kv.Value == item.Value);
            if (idx < 0)
                return false;
            KeyValues.RemoveAt(idx);
            return true;
        }

        public bool TryGetValue(string key, out string value)
        {
            var node = KeyValues.Where(kv => kv.Key.Equals(key, ComparisonInternal)).FirstOrDefault();
            if (node == null)
            {
                value = default;
                return false;
            }
            else
            {
                value = node.Value;
                return true;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //type-coercing APIS
        //note that these only work reliably 
        public bool ContainsKeyForType<T>(string key)
        {
            return TryGetTypedInternal(key, typeof(T), out _);
        }

        public T GetItemOfType<T>(string key)
        {
            if (TryGetTypedInternal(key, typeof(T), out object val))
                return (T)val;
            else
                throw new KeyNotFoundException($"An item with key {key} and type {typeof(T).Name} could not be found in the KeyValueStore");
        }

    }

    [Serializable]
    public class KeyValueStoreNode
    {
        public string Key;
        public string Value;
    }
}


