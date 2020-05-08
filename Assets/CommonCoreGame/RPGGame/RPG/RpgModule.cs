using CommonCore.Config;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{

    /// <summary>
    /// CommonCore RPG Module
    /// </summary>
    /// <remarks>
    /// <para>Initializes character and inventory models, installs gameplay config panel</para>
    /// </remarks>
    public class RpgModule : CCModule
    {
        public RpgModule()
        {
            LoadGameParams();

            LoadFactionModels();
            LoadCharacterModels();
            LoadInventoryModels();
            LoadQuestModels();

            //install gameplay config panel
            ConfigModule.Instance.RegisterConfigPanel("GameplayOptionsPanel", 500, CoreUtils.LoadResource<GameObject>("UI/GameplayOptionsPanel"));
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

        private void LoadFactionModels()
        {
            FactionModel.Load();
        }

        private void LoadCharacterModels()
        {
            //TODO set this up
        }

        private void LoadInventoryModels()
        {
            InventoryModel.Load();           
        }

        private void LoadQuestModels()
        {
            QuestModel.Load();
        }
       
    }
}