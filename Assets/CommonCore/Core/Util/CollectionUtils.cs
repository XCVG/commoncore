using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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