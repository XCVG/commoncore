using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Rpg
{
    /*
     * CommonCore RPG Module
     * Initializes character and inventory models
     */
    public class RpgModule : CCModule
    {
        public RpgModule()
        {
            LoadCharacterModels();
            LoadInventoryModels();

            Debug.Log("RPG module loaded!");
        }

        private void LoadCharacterModels()
        {
            //TODO set this up
        }

        private void LoadInventoryModels()
        {
            InventoryModel.Load();
        }
       
    }
}