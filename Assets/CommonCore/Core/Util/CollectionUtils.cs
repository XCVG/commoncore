using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonCore
{

    /// <summary>
    /// Utility functions for manipulating collections
    /// </summary>
    public static class CollectionUtils
    {
        //slightly horrifying dictionary helpers

        //deliberately not an extension method so it doesn't clobber other things
        public static bool TryGetValue<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> dCollection, TKey key, out TValue value)
        {
            if (dCollection is IDictionary<TKey, TValue> dictionary)
                return dictionary.TryGetValue(key, out value);
            if (dCollection is IReadOnlyDictionary<TKey, TValue> rDictionary)
                return rDictionary.TryGetValue(key, out value);

            var fCollection = dCollection.Where(kvp => kvp.Key.Equals(key));
            value = fCollection.FirstOrDefault().Value;
            return fCollection.Any();
        }

        public static TOut GetAs<TOut, TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dCollection, TKey key)
        {
            if (TryGetValue(dCollection, key, out TValue val))
            {
                if (typeof(TOut).IsAssignableFrom(val.GetType()))
                    return (TOut)(object)val;
                else
                    return TypeUtils.CoerceValue<TOut>(val);
            }
            return default;
        }

        public static T GetAs<T>(this IEnumerable<KeyValuePair<string, object>> dCollection, string key)
        {
            return GetAs<T, string, object>(dCollection, key);
        }

        /// <summary>
        /// AddRange for dictionary that uses add method, throwing on duplicate items
        /// </summary>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach(var kvp in items)
            {
                dictionary.Add(kvp);
            }
        }

        /// <summary>
        /// AddRange for dictionary, ignoring duplicates of existing keys
        /// </summary>
        public static void AddRangeKeepExisting<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var kvp in items)
            {
                if (!dictionary.ContainsKey(kvp.Key))
                    dictionary.Add(kvp);
            }
        }

        /// <summary>
        /// AddRange for dictionary, overriding duplicates of existing keys
        /// </summary>
        public static void AddRangeReplaceExisting<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var kvp in items)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }

        public static TValue GetOrDefault<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dCollection, TKey key)
        {
            return GetOrDefault(dCollection, key, default);
        }

        public static TValue GetOrDefault<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dCollection, TKey key, TValue def)
        {
            if (TryGetValue(dCollection, key, out var result))
                return result;

            return def;
        }

        public static TKey GetKeyForValue<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dCollection, TValue value)
        {
            foreach(var kvp in dCollection)
            {
                if (kvp.Value.Equals(value)) //safe?
                    return kvp.Key;
            }

            return default;
        }

        public static TKey GetKeyForValue<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dCollection, TValue value, out bool duplicateValuesExist)
        {
            var keys = dCollection.GetKeysForValue(value);
            if (keys.Count > 1)
                duplicateValuesExist = true;
            else
                duplicateValuesExist = false;

            if (keys.Count == 0)
                return default;
            else
                return keys[0];
        }

        public static List<TKey> GetKeysForValue<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dCollection, TValue value)
        {
            List<TKey> keys = new List<TKey>();

            foreach(var kvp in dCollection)
            {
                if (kvp.Value.Equals(value))
                    keys.Add(kvp.Key);
            }

            return keys;
        }

        /// <summary>
        /// Gets the first value for a key from a dictionary, ignoring the case of the key
        /// </summary>
        public static T GetIgnoreCase<T>(this IEnumerable<KeyValuePair<string, T>> dCollection, string key)
        {
            if (TryGetValue(dCollection, key, out var val)) //try to just get it as a shortcut
                return val;

            foreach(var kvp in dCollection)
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
        public static void SetupFromEnum<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue defaultValue) where TKey : Enum
        {
            foreach(TKey key in Enum.GetValues(typeof(TKey)))
            {
                dictionary[key] = defaultValue;
            }
        }

        /// <summary>
        /// Fills up a dictionary with all enum values as keys (int-indexed version)
        /// </summary>
        public static void SetupFromEnum<TValue>(this IDictionary<int, TValue> dictionary, Type enumType, TValue defaultValue)
        {
            foreach(int key in Enum.GetValues(enumType))
            {
                dictionary[key] = defaultValue;
            }
        }

        /// <summary>
        /// Fills up a set with all members of an enum as values
        /// </summary>
        public static void SetupFromEnum<T>(this ISet<T> set) where T : Enum
        {
            foreach(T key in Enum.GetValues(typeof(T)))
            {
                set.Add(key);
            }
        }

        /// <summary>
        /// Fills up a set with all names of an enum as values
        /// </summary>
        public static void SetupFromEnum<T>(this ISet<string> set) where T : Enum
        {
            foreach(string key in Enum.GetNames(typeof(T)))
            {
                set.Add(key);
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

        /// <summary>
        /// IndexOf extension method for IReadOnlyList
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/37431844/why-ireadonlycollection-has-elementat-but-not-indexof for where this is from and why it exists</remarks>
        public static int IndexOf<T>(this IReadOnlyList<T> self, T elementToFind)
        {
            int i = 0;
            foreach (T element in self)
            {
                if (Equals(element, elementToFind))
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// IndexOf extension method for IReadOnlyList (specialized version for strings)
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/37431844/why-ireadonlycollection-has-elementat-but-not-indexof for where this is from and why it exists</remarks>
        public static int IndexOf(this IReadOnlyList<string> self, string elementToFind, StringComparison comparisonType)
        {
            int i = 0;
            foreach (string element in self)
            {
                if (string.Equals(element, elementToFind, comparisonType))
                    return i;
                i++;
            }
            return -1;
        }

        public static string ToNiceString<T>(this IEnumerable<T> collection, Func<T, string> toStringFunction)
        {
            StringBuilder sb = new StringBuilder(256);
            sb.Append("[");

            IEnumerator<T> enumerator = collection.GetEnumerator();
            bool eHasNext = enumerator.MoveNext();
            while (eHasNext)
            {
                sb.Append(toStringFunction(enumerator.Current));

                eHasNext = enumerator.MoveNext();
                if (eHasNext)
                    sb.Append(", ");
            }
            sb.Append("]");

            return sb.ToString();
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