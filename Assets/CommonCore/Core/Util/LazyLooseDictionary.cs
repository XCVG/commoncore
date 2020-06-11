using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonCore
{
    /// <summary>
    /// Lazy-loading dictionary referencing a JObject
    /// </summary>
    /// <remarks>At this point it's incredibly halfassed with most of the API not implemented</remarks>
    public class LazyLooseDictionary : IDictionary<string, object>, IReadOnlyDictionary<string, object>
    {

        private JObject BackingJObject = new JObject();
        private Dictionary<string, object> BackingDictionary = new Dictionary<string, object>();

        public LazyLooseDictionary()
        {
            
        }

        public LazyLooseDictionary(JObject jobject)
        {
            BackingJObject = jobject;
        }

        /// <summary>
        /// Gets a JObject with all values
        /// </summary>
        public JObject GetFullJObject()
        {
            UpdateBackingJObject();
            return BackingJObject;
        }

        /// <summary>
        /// Updates the BackingJObject with values from the BackingDictionary (expensive)
        /// </summary>
        private void UpdateBackingJObject()
        {
            foreach(var kvp in BackingDictionary)
            {
                if (BackingJObject.ContainsKey(kvp.Key))
                    BackingJObject.Remove(kvp.Key);

                var subObject = JObject.FromObject(kvp.Value);
                var type = kvp.Value.GetType();
                subObject.AddFirst(new JProperty("$type", string.Format("{0}, {1}", type.ToString(), type.Assembly.GetName().Name)));

                BackingJObject.Add(kvp.Key, subObject);
            }
        }

        public T GetValue<T>(string key)
        {
            if (BackingDictionary.ContainsKey(key))
                return (T)BackingDictionary[key];
            else if (BackingJObject.ContainsKey(key))
                return (T)GetObjectFromToken(BackingJObject[key]);
            return default;
        }

        public object this[string key]
        {
            get
            {
                if (BackingDictionary.ContainsKey(key))
                    return BackingDictionary[key];
                else if (BackingJObject.ContainsKey(key))
                    return GetObjectFromToken(BackingJObject[key]);
                return default;
            }
            set
            {
                BackingDictionary[key] = value;
            }
        }

        public ICollection<string> Keys => throw new NotImplementedException();

        public ICollection<object> Values => throw new NotImplementedException();

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly => false;

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => this.Keys;

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => this.Values;

        public void Add(string key, object value)
        {
            BackingDictionary.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)BackingDictionary).Add(item);
        }

        public void Clear()
        {
            BackingJObject.RemoveAll();
            BackingDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return BackingJObject.ContainsKey(key) || BackingDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        

        public bool Remove(string key)
        {
            return BackingDictionary.Remove(key) || BackingJObject.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            if(ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static object GetObjectFromToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.None:
                    return null;
                case JTokenType.Object:
                    {
                        var type = Type.GetType(token["$type"].Value<string>());
                        return token.ToObject(type);
                    }
                case JTokenType.Array:
                    {
                        List<object> objects = new List<object>();
                        foreach (var jt in ((JArray)token))
                            objects.Add(GetObjectFromToken(jt));
                        return objects;
                    }
                case JTokenType.Integer:
                    return token.Value<int>();
                case JTokenType.Float:
                    return token.Value<float>();
                case JTokenType.String:
                    return token.ToString();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Null:
                    return null;
                case JTokenType.Undefined:
                    return null;
                case JTokenType.Bytes:
                    return token.ToObject<byte[]>();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}