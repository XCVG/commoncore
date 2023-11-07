using CommonCore.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame
{
    public partial class DifficultyValues
    {
        [JsonProperty]
        public float ActorAggression { get; private set; } = 1f;
        [JsonProperty]
        public float ActorStrength { get; private set; } = 1f;
        [JsonProperty]
        public float ActorPerception { get; private set; } = 1f;

        [JsonProperty]
        public float PlayerAgility { get; private set; } = 1f;
        [JsonProperty]
        public float PlayerEndurance { get; private set; } = 1f;
        [JsonProperty]
        public float PlayerStrength { get; private set; } = 1f;
        [JsonProperty]
        public float PlayerSkill { get; private set; } = 1f;
        [JsonProperty]
        public float PlayerExperience { get; private set; } = 1f;

        [JsonProperty]
        public float EnvironmentLevelBias { get; private set; } = 1f;
        [JsonProperty]
        public float EnvironmentEnemyFrequency { get; private set; } = 1f;
        [JsonProperty]
        public float EnvironmentLootFrequency { get; private set; } = 1f;

        [JsonProperty]
        public float FollowerEndurance { get; private set; } = 1f;
        [JsonProperty]
        public float FollowerStrength { get; private set; } = 1f;
        [JsonProperty]
        public FollowerMortality FollowerMortality { get; private set; } = FollowerMortality.Invulnerable;

        private static Dictionary<DifficultyLevel, DifficultyValues> DefaultDifficultyValues = new Dictionary<DifficultyLevel, DifficultyValues>();

        internal static void LoadDefaults()
        {
            try
            {
                var ta = CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/difficulty");
                string jsonText = ta.text;
                DefaultDifficultyValues = CoreUtils.LoadJson<Dictionary<DifficultyLevel, DifficultyValues>>(jsonText);
            }
            catch(Exception e)
            {
                Debug.LogErrorFormat("Error loading default difficulty values ({0}: {1})", e.GetType().Name, e.Message);
                if(ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }
        }

        public static DifficultyValues GetDefaultDifficulty(DifficultyLevel level)
        {
            if(DefaultDifficultyValues.TryGetValue(level, out var value)) 
                return value;

            Debug.LogWarning("Can't find default difficulty values for " + level);
            return new DifficultyValues();
        }
    }
}