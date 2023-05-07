using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DO NOT USE THIS CLASS
/// </summary>
/// <remarks>
/// <para>This exists only as a dummy/placeholder class to force IL2CPP to include various classes and generics</para>
/// </remarks>
public class CoreAotTypeEnforcer : MonoBehaviour
{
    public void Awake()
    {
        AotHelper.EnsureType<VersionConverter>();
        AotHelper.EnsureDictionary<string, object>();
        AotHelper.EnsureList<string>();

        //attempt to preserve Action<T> types
        AotHelper.Ensure(() =>
        {
            Action<string> action1 = (s) => { };

            Action<string, Vector3> action2 = (s, v) => { };

            Action<string, Vector3, Vector3> action3 = (s, v1, v2) => { };
        });
    }
}