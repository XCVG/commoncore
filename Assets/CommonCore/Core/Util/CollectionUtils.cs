using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CommonCore
{

    /// <summary>
    /// Utility functions for manipulating collections
    /// </summary>
    public static class CollectionUtils
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return GetOrDefault(dictionary, key, default(TValue));
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue def)
        {
            TValue result;
            if (dictionary.TryGetValue(key, out result))
                return result;

            return def;
        }

        /// <summary>
        /// Gets the first value for a key from a dictionary, ignoring the case of the key
        /// </summary>
        public static T GetIgnoreCase<T>(this IDictionary<string, T> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var val)) //try to just get it as a shortcut
                return val;

            foreach(var kvp in dictionary)
            {
                if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }

            return default;
        }

        public static void Swap<T>(this IList<T> list, int index0, int index1)
        {
            T temp = list[index0];
            list[index0] = list[index1];
            list[index1] = temp;
        }

        /// <summary>
        /// Fills up a dictionary with all enum values as keys
        /// </summary>
        public static void SetupFromEnum<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue defaultValue) where TKey : Enum
        {
            foreach(TKey key in Enum.GetValues(typeof(TKey)))
            {
                dictionary[key] = defaultValue;
            }
        }

        /// <summary>
        /// Fills up a dictionary with all enum values as keys (int-indexed version)
        /// </summary>
        public static void SetupFromEnum<TValue>(this Dictionary<int, TValue> dictionary, Type enumType, TValue defaultValue)
        {
            foreach(int key in Enum.GetValues(enumType))
            {
                dictionary[key] = defaultValue;
            }
        }

        /// <summary>
        /// Shuffles a list in-place
        /// </summary>
        /// <remarks>
        /// <para>Based on https://stackoverflow.com/questions/273313/randomize-a-listt </para>
        /// <para>Is not the best. But should be good enough.</para>
        /// </remarks>
        public static void Shuffle<T>(this IList<T> list)
        {
            var rng = new System.Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string ToNiceString(this IEnumerable collection)
        {
            StringBuilder sb = new StringBuilder(256);
            sb.Append("[");

            IEnumerator enumerator = collection.GetEnumerator();
            bool eHasNext = enumerator.MoveNext();
            while (eHasNext)
            {
                sb.Append(enumerator.Current.ToString());

                eHasNext = enumerator.MoveNext();
                if (eHasNext)
                    sb.Append(", ");
            }
            sb.Append("]");

            return sb.ToString();
        }
    }


}