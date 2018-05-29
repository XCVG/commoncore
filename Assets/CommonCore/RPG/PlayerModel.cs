using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace CommonCore.Rpg
{ 
    //TODO split into PlayerModel and CharacterModel
    public class PlayerModel
    {
        public float Health
        {
            get
            {
                return DerivedStats.MaxHealth * HealthFraction;
            }
            set
            {
                HealthFraction = Health / DerivedStats.MaxHealth;
            }
        }

        public float HealthFraction { get; set; }

        public StatsSet BaseStats { get; private set; }
        public StatsSet DerivedStats { get; private set; }

        public List<Condition> Conditions { get; private set; }

        public PlayerModel()
        {
            Conditions = new List<Condition>();

            //create blank stats and derive stats
            BaseStats = new StatsSet(); //TODO load defaults
            UpdateStats();
        }

        public void UpdateStats()
        {
            //TODO update stats
            DerivedStats = new StatsSet(); //for now
        }

        
    }
}
