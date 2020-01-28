using CommonCore.Config;
using CommonCore.DebugLog;
using System.Collections;
using System.Collections.Generic;
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
            LoadFactionModels();
            LoadCharacterModels();
            LoadInventoryModels();
            LoadQuestModels();

            //install gameplay config panel
            ConfigModule.Instance.RegisterConfigPanel("GameplayOptionsPanel", 500, CoreUtils.LoadResource<GameObject>("UI/GameplayOptionsPanel"));
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