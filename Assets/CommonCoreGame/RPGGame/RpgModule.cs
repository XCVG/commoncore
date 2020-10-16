using CommonCore.Config;
using CommonCore.DelayedEvents;
using CommonCore.RpgGame.Rpg;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace CommonCore.RpgGame
{

    /// <summary>
    /// CommonCore RPG Module
    /// </summary>
    /// <remarks>
    /// <para>Initializes character and inventory models, installs gameplay config panel</para>
    /// </remarks>
    public class RpgModule : CCModule
    {
        private float Elapsed;

        public RpgModule()
        {
            LoadGameParams();

            FactionModel.Load();
            InventoryModel.Load();
            QuestModel.Load();

            //install gameplay config panel
            ConfigModule.Instance.RegisterConfigPanel("GameplayOptionsPanel", 500, CoreUtils.LoadResource<GameObject>("UI/GameplayOptionsPanel"));
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            FactionModel.LoadFromAddon(data);
            InventoryModel.LoadFromAddon(data);
            QuestModel.LoadFromAddon(data);
        }

        private void LoadGameParams()
        {
            //load override if it exists
            string path = Path.Combine(CoreParams.PersistentDataPath, "gameparams.json");
            if (File.Exists(path))
            {
                try
                {
                    TypeUtils.PopulateStaticObject(typeof(GameParams), File.ReadAllText(path), new JsonSerializerSettings() { Converters = CCJsonConverters.Defaults.Converters, NullValueHandling = NullValueHandling.Ignore });
                    Debug.LogWarning($"Loaded gameparams overrides from file (this may cause moderately weird things to happen)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load gameparams overrides from file!");
                    Debug.LogException(e);
                }
            }
        }


    }
}