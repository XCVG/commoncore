using System;
using UnityEngine;

namespace PseudoExtensibleEnum
{
    /// <summary>
    /// Attribute to mark a field as using psudo-enum-extension. Note that the type of the field itself should be numeric.
    /// </summary>
    public class PxEnumPropertyAttribute : PropertyAttribute
    {
        public readonly Type BaseType;

        public PxEnumPropertyAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }
}
