using CommonCore.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CommonCore.RpgGame
{

    /// <summary>
    /// Gameplay and difficulty options
    /// </summary>
    public class GameplayConfig
    {

        public CrosshairState Crosshair { get; set; } = CrosshairState.Auto;
        public AimAssistState AimAssist { get; set; } = AimAssistState.Off;
        public bool BobEffects { get; set; } = true;
        public float RecoilEffectScale { get; set; } = 1.0f;
        public bool HoldAds { get; set; } = false;
        public bool HitIndicatorsVisual { get; set; } = false;
        public bool HitIndicatorsAudio { get; set; } = false;
        public bool FullscreenDamageIndicator { get; set; } = true;    

        //difficulty options
        [JsonIgnore, Obsolete]
        public DifficultyLevel DifficultySetting //moved to base ConfigState, thunks
        { 
            get => (DifficultyLevel)ConfigState.Instance.Difficulty; 
            set { ConfigState.Instance.Difficulty = (int)value; } 
        }
        [JsonIgnore]
        public DifficultyValues Difficulty
        {
            get
            {
                var d = (DifficultyLevel)ConfigState.Instance.Difficulty;
                if (d == DifficultyLevel.Custom)
                    return CustomDifficultyValues;
                return DifficultyValues.GetDefaultDifficulty(d);
            }
        }

        [JsonProperty]
        private DifficultyValues CustomDifficultyValues { get; set; } = new DifficultyValues();

    }

    public enum DifficultyLevel
    {
        Easy, Normal, Hard, Custom
    }

    public enum FollowerMortality
    {
        Invulnerable, Immortal, Mortal
    }

    public enum AimAssistState
    {
        Off, Mild, Strong
    }

    public enum CrosshairState
    {
        Always, Auto, Never
    }

    public static class GameplayConfigExtensions
    {
        public static GameplayConfig GetGameplayConfig(this ConfigState state)
        {
            ConfigState.Instance.AddCustomVarIfNotExists("GameplayConfig", () => new GameplayConfig());

            return ConfigState.Instance.CustomConfigVars["GameplayConfig"] as GameplayConfig;
        }
    }


}