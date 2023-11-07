using CommonCore.Config;
using CommonCore.DelayedEvents;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.State;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
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

        public RpgModule()
        {
            LoadGameParams();
            DifficultyValues.LoadDefaults();

            FactionModel.Load();
            InventoryModel.Load();
            QuestModel.Load();

            SetRpgDefaultValues();

            ConditionalModule.Instance.LoadBaseHandlers(); //TODO

            //install gameplay config panel
            ConfigModule.Instance.RegisterConfigPanel("GameplayOptionsPanel", 500, CoreUtils.LoadResource<GameObject>("UI/GameplayOptionsPanel"));
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            FactionModel.LoadFromAddon(data);
            InventoryModel.LoadFromAddon(data);
            QuestModel.LoadFromAddon(data);

            ConditionalModule.Instance.LoadHandlersFromAddon(data); //TODO
        }

        private void LoadGameParams()
        {
            //load values from in-project def if exists
            if(CoreUtils.CheckResource<TextAsset>("Data/RPGDefs/rpg_params"))
            {
                TextAsset ta = CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/rpg_params");
                populateGameParams(ta.text);
            }
            

            //load override if it exists
            string path = Path.Combine(CoreParams.PersistentDataPath, "gameparams.json");
            if (File.Exists(path))
            {
                populateGameParams(File.ReadAllText(path));
            }

            void populateGameParams(string paramsString)
            {
                try
                {
                    TypeUtils.PopulateStaticObject(typeof(GameParams), paramsString, new JsonSerializerSettings() { Converters = CCJsonConverters.Defaults.Converters, NullValueHandling = NullValueHandling.Ignore });
                    Debug.LogWarning($"Loaded gameparams overrides from file (this may cause moderately weird things to happen)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load gameparams overrides from file!");
                    Debug.LogException(e);
                }
            }
        }

        private void SetRpgDefaultValues()
        {
            //look for an override class if it exists, this exists to support the legacy scenario of CaliforniumRpgDefaultValues
            Type overrideType = CCBase.BaseGameTypes
                .Where(t => t.GetInterface(nameof(IRpgDefaultValues)) != null && t.GetCustomAttributes(false).Any(a => a is RpgDefaultValuesOverrideAttribute))
                .FirstOrDefault();

            IRpgDefaultValues defaultValues;
            if(overrideType != null)
            {
                Log($"Using RPG default overrides class {overrideType.Name}");
                defaultValues = (IRpgDefaultValues)Activator.CreateInstance(overrideType);
            }
            else
            {
                defaultValues = new RpgDefaultValues();
            }

            RpgValues.SetDefaults(defaultValues);
        }

    }
}