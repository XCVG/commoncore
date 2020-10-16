using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace CommonCore.AddonSupport
{

    /// <summary>
    /// Utilities useful to addons
    /// </summary>
    public static class AddonUtils
    {
        //better utils for dealing with state are coming at some point, for now it's just kind of a "here, you deal with it"

        public static object GetGameStateInstance()
        {
            Type type = CCBase.BaseGameTypes
                .Where(t => t.Name == "GameState")
                .Single();

            return type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

        public static object GetPersistStateInstance()
        {
            Type type = CCBase.BaseGameTypes
                .Where(t => t.Name == "PersistState")
                .Single();

            return type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

        public static object GetMetaStateInstance()
        {
            Type type = CCBase.BaseGameTypes
                .Where(t => t.Name == "MetaState")
                .Single();

            return type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

    }
}