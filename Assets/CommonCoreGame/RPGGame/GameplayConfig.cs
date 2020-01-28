﻿using CommonCore.Config;
using Newtonsoft.Json;
using System;

namespace CommonCore.RpgGame
{

    /// <summary>
    /// Gameplay and difficulty options
    /// </summary>
    public class GameplayConfig
    {

        public CrosshairState Crosshair { get; set; } = CrosshairState.Auto;
        public AimAssistState AimAssist { get; set; } = AimAssistState.Off;
        public bool ShakeEffects { get; set; } = true; //may move this to Core, not sure yet
        public bool BobEffects { get; set; } = true;
        public bool HoldAds { get; set; } = false;
        public bool HitIndicatorsVisual { get; set; } = false;
        public bool HitIndicatorsAudio { get; set; } = false;

        //difficulty options
        public DifficultyLevel DifficultySetting { get; set; } = DifficultyLevel.Normal;
        [JsonIgnore]
        public DifficultyValues Difficulty { get {
                switch (DifficultySetting)
                {
                    case DifficultyLevel.Easy:
                        return EasyDifficulty;
                    case DifficultyLevel.Normal:
                        return NormalDifficulty;
                    case DifficultyLevel.Hard:
                        return HardDifficulty;
                    case DifficultyLevel.Custom:
                        return CustomDifficultyValues;
                    default:
                        throw new NotImplementedException();
                }

            } }

        public DifficultyValues CustomDifficultyValues { get; set; } = NormalDifficulty;

        //default difficulty values
        private static DifficultyValues EasyDifficulty => new DifficultyValues()
        {
            ActorAggression = 0.75f,
            ActorPerception = 0.75f,
            ActorStrength = 0.75f,
            EnvironmentEnemyFrequency = 0.75f,
            EnvironmentLevelBias = 0.75f,
            EnvironmentLootFrequency = 1.5f,
            PlayerEndurance = 1.5f,
            PlayerExperience = 1.5f,
            PlayerSkill = 1.5f,
            PlayerStrength = 1.5f,
            FollowerEndurance = 1.5f,
            FollowerStrength = 1.5f,
            FollowerMortality = FollowerMortality.Invulnerable
        };

        private static DifficultyValues NormalDifficulty => new DifficultyValues()
        {
            ActorAggression = 1f,
            ActorPerception = 1f,
            ActorStrength = 1f,
            EnvironmentEnemyFrequency = 1f,
            EnvironmentLevelBias = 1f,
            EnvironmentLootFrequency = 1f,
            PlayerEndurance = 1f,
            PlayerExperience = 1f,
            PlayerSkill = 1f,
            PlayerStrength = 1f,
            FollowerEndurance = 1f,
            FollowerStrength = 1f,
            FollowerMortality = FollowerMortality.Immortal
        };

        private static DifficultyValues HardDifficulty => new DifficultyValues()
        {
            ActorAggression = 1.5f,
            ActorPerception = 1.5f,
            ActorStrength = 1.5f,
            EnvironmentEnemyFrequency = 1.5f,
            EnvironmentLevelBias = 1.5f,
            EnvironmentLootFrequency = 0.75f,
            PlayerEndurance = 0.75f,
            PlayerExperience = 0.75f,
            PlayerSkill = 0.75f,
            PlayerStrength = 0.75f,
            FollowerEndurance = 0.75f,
            FollowerStrength = 0.75f,
            FollowerMortality = FollowerMortality.Immortal
        };

    }

    public struct DifficultyValues
    {
        public float ActorAggression;
        public float ActorStrength;
        public float ActorPerception;

        public float PlayerEndurance;
        public float PlayerStrength;
        public float PlayerSkill;
        public float PlayerExperience;

        public float EnvironmentLevelBias;
        public float EnvironmentEnemyFrequency;
        public float EnvironmentLootFrequency;

        public float FollowerEndurance;
        public float FollowerStrength;
        public FollowerMortality FollowerMortality;
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