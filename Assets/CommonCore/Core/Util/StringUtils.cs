using System;

namespace CommonCore
{

    /// <summary>
    /// Utilities for strings
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Checks if a string contains another string, using specified comparison type
        /// </summary>
        public static bool Contains(this string strA, string strB, StringComparison comparisonType)
        {
            return strA?.IndexOf(strB, comparisonType) >= 0;
        }
    }
}