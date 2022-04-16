using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
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
    }
}