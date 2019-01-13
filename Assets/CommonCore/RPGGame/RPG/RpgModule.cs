using CommonCore.DebugLog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{
    /*
     * CommonCore RPG Module
     * Initializes character and inventory models
     */
    public class RpgModule : CCModule
    {
        public RpgModule()
        {
            LoadFactionModels();
            LoadCharacterModels();
            LoadInventoryModels();
            LoadQuestModels();
            
            CDebug.Log("RPG module loaded!");
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