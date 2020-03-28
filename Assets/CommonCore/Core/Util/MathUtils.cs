using System;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// Utility functions for doing math.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Clamps a Comparable between two values
        /// </summary>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        /// <summary>
        /// Clamps an int between two values
        /// </summary>
        public static int Clamp(int val, int min, int max)
        {
            if (val < min)
                return min;
            else if (val > max)
                return max;
            return val;
        }

        /// <summary>
        /// Scales a value from an old range to a new range
        /// </summary>
        public static float ScaleRange(float val, float oldMin, float oldMax, float newMin, float newMax)
        {
            return (val / ((oldMax - oldMin) / (newMax - newMin))) + newMin;
        }


    }
}