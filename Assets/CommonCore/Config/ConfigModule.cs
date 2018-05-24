using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Config
{
    /*
     * CommonCore Config Module
     * Provides settings save/load/apply, as well as maintaining PersistState
     */
    [CCExplicitModule]
    public class ConfigModule : CCModule
    {
        public ConfigModule()
        {
            ConfigState.Load();
            PersistState.Load();
            ConfigState.Save();
            PersistState.Save();
            Debug.Log("Config module loaded!");
        }

        public void ApplyConfiguration()
        {
            //TODO apply configuration changes
        }

    }
}