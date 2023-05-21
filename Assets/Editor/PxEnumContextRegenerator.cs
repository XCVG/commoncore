using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;
using PseudoExtensibleEnum;
using System;
using System.Linq;

public static class PxEnumContextRegenerator
{
    [DidReloadScripts]
    private static void OnReloadRegenerateContext()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !(a.FullName.StartsWith("Unity") || a.FullName.StartsWith("System") || a.FullName.StartsWith("netstandard") ||
                            a.FullName.StartsWith("mscorlib") || a.FullName.StartsWith("mono", StringComparison.OrdinalIgnoreCase) ||
                            a.FullName.StartsWith("Boo") || a.FullName.StartsWith("I18N")))
                .SelectMany((assembly) => assembly.GetTypes());

        PxEnum.RecreateCurrentContext();
        PxEnum.CurrentContext.LoadTypes(types);
    }
}
