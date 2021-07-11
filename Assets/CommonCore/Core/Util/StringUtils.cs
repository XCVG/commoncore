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

        /// <summary>
        /// Counts occurrences of a character in a string
        /// </summary>
        public static int CountChar(this string str, char ch)
        {
            //according to https://stackoverflow.com/questions/541954/how-would-you-count-occurrences-of-a-string-actually-a-char-within-a-string this is the fastest way

            int count = 0;
            for(int i = 0; i < str.Length; i++)
            {
                if (str[i] == ch)
                    count++;
            }

            return count;                
        }
    }
}