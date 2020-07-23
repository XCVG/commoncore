using System;
using System.Reflection;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{
    /// <summary>
    /// RPG value calculations
    /// </summary>
    public static class RpgValues
    {
        //this is basically a facade class, we've separated the implementation so mods etc can alter these

        public static void SetOverride<T>(string name, T function) where T : Delegate
        {
            string lookupName = name + "Impl";
            Type type = typeof(RpgValues);
            PropertyInfo property = type.GetProperty(lookupName, BindingFlags.Static | BindingFlags.NonPublic);
            property.SetValue(null, function); //will this work?
        }

        public static int XPToNext(int level) => XPToNextImpl(level);
        private static Func<int, int> XPToNextImpl { get; set; } = RpgDefaultValues.XPToNext;

        public static int PotentialPointsForLevel(int newLevel, CharacterModel character) => PotentialPointsForLevelImpl(newLevel, character);
        private static Func<int, CharacterModel, int> PotentialPointsForLevelImpl { get; set; } = RpgDefaultValues.PotentialPointsForLevel;

        public static int SkillGainForPoints(int points) => SkillGainForPointsImpl(points);
        private static Func<int, int> SkillGainForPointsImpl { get; set; } = RpgDefaultValues.SkillGainForPoints;

        public static int LevelsForExperience(CharacterModel character) => LevelsForExperienceImpl(character);
        private static Func<CharacterModel, int> LevelsForExperienceImpl { get; set; } = RpgDefaultValues.LevelsForExperience;

        public static int XPAfterMaxLevel(CharacterModel character) => XPAfterMaxLevelImpl(character);
        private static Func<CharacterModel, int> XPAfterMaxLevelImpl { get; set; } = RpgDefaultValues.XPAfterMaxLevel;

        public static float MaxHealth(CharacterModel characterModel) => MaxHealthImpl(characterModel);
        private static Func<CharacterModel, float> MaxHealthImpl { get; set; } = RpgDefaultValues.MaxHealth;

        public static float MaxEnergy(CharacterModel characterModel) => MaxEnergyImpl(characterModel);
        private static Func<CharacterModel, float> MaxEnergyImpl { get; set; } = RpgDefaultValues.MaxEnergy;

        public static int AdjustedBuyPrice(CharacterModel character, float value) => AdjustedBuyPriceImpl(character, value);
        private static Func<CharacterModel, float, int> AdjustedBuyPriceImpl { get; set; } = RpgDefaultValues.AdjustedBuyPrice;

        public static int AdjustedSellPrice(CharacterModel character, float value) => AdjustedSellPriceImpl(character, value);
        private static Func<CharacterModel, float, int> AdjustedSellPriceImpl { get; set; } = RpgDefaultValues.AdjustedSellPrice;

        public static void SkillsFromStats(StatsSet baseStats, StatsSet derivedStats) => SkillsFromStatsImpl(baseStats, derivedStats);
        private static Action<StatsSet, StatsSet> SkillsFromStatsImpl { get; set; } = RpgDefaultValues.SkillsFromStats;

        /// <summary>
        /// Calculates applied damage given input damage and resistance
        /// </summary>
        public static float DamageTaken(float damage, float pierce, float threshold, float resistance) //this is a dumb spot and we will move it later
=> DamageTakenImpl(damage, pierce, threshold, resistance);
        private static Func<float, float, float, float, float> DamageTakenImpl { get; set; } = RpgDefaultValues.DamageTaken;

        /// <summary>
        /// Calculates applied damage given the velocity of a fall
        /// </summary>
        public static float FallDamage(CharacterModel character, Vector3 velocity) => FallDamageImpl(character, velocity);
        private static Func<CharacterModel, Vector3, float> FallDamageImpl { get; set; } = RpgDefaultValues.FallDamage;

        public static float DetectionChance(CharacterModel character, bool isSneaking, bool isRunning) => DetectionChanceImpl(character, isSneaking, isRunning);
        private static Func<CharacterModel, bool, bool, float> DetectionChanceImpl { get; set; } = RpgDefaultValues.DetectionChance;




        //*********************************
        //***** movement calculations *****

        public static float GetMoveSpeedMultiplier(CharacterModel character) => GetMoveSpeedMultiplierImpl(character);
        private static Func<CharacterModel, float> GetMoveSpeedMultiplierImpl { get; set; } = RpgDefaultValues.GetMoveSpeedMultiplier;

        public static float GetRunSpeedMultiplier(CharacterModel character) => GetRunSpeedMultiplierImpl(character);
        private static Func<CharacterModel, float> GetRunSpeedMultiplierImpl { get; set; } = RpgDefaultValues.GetRunSpeedMultiplier;

        public static float GetRunEnergyRate(CharacterModel character) => GetRunEnergyRateImpl(character);
        private static Func<CharacterModel, float> GetRunEnergyRateImpl { get; set; } = RpgDefaultValues.GetRunEnergyRate;

        public static float GetJumpVelocityMultiplier(CharacterModel character) => GetJumpVelocityMultiplierImpl(character);
        private static Func<CharacterModel, float> GetJumpVelocityMultiplierImpl { get; set; } = RpgDefaultValues.GetJumpVelocityMultiplier;

        public static float GetJumpEnergyUse(CharacterModel character) => GetJumpEnergyUseImpl(character);
        private static Func<CharacterModel, float> GetJumpEnergyUseImpl { get; set; } = RpgDefaultValues.GetJumpEnergyUse;

        public static float GetAirMoveMultiplier(CharacterModel character) => GetAirMoveMultiplierImpl(character);
        private static Func<CharacterModel, float> GetAirMoveMultiplierImpl { get; set; } = RpgDefaultValues.GetAirMoveMultiplier;

        public static float GetIdleEnergyRecoveryRate(CharacterModel character) => GetIdleEnergyRecoveryRateImpl(character);
        private static Func<CharacterModel, float> GetIdleEnergyRecoveryRateImpl { get; set; } = RpgDefaultValues.GetIdleEnergyRecoveryRate;

        public static float GetMovingEnergyRecoveryRate(CharacterModel character) => GetMovingEnergyRecoveryRateImpl(character);
        private static Func<CharacterModel, float> GetMovingEnergyRecoveryRateImpl { get; set; } = RpgDefaultValues.GetMovingEnergyRecoveryRate;



        //*******************************
        //***** weapon calculations *****

        public static float GetKickDamageFactor(CharacterModel character) => GetKickDamageFactorImpl(character);
        private static Func<CharacterModel, float> GetKickDamageFactorImpl { get; set; } = RpgDefaultValues.GetKickDamageFactor;

        public static float GetKickForceFactor(CharacterModel character) => GetKickForceFactorImpl(character);
        private static Func<CharacterModel, float> GetKickForceFactorImpl { get; set; } = RpgDefaultValues.GetKickForceFactor;

        public static float GetKickRateFactor(CharacterModel character) => GetKickRateFactorImpl(character);
        private static Func<CharacterModel, float> GetKickRateFactorImpl { get; set; } = RpgDefaultValues.GetKickRateFactor;

        public static float GetWeaponRateFactor(CharacterModel character, WeaponItemModel itemModel) => GetWeaponRateFactorImpl(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponRateFactorImpl { get; set; } = RpgDefaultValues.GetWeaponRateFactor;

        public static float GetWeaponReloadFactor(CharacterModel character, WeaponItemModel itemModel) => GetWeaponReloadFactorImpl(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponReloadFactorImpl { get; set; } = RpgDefaultValues.GetWeaponReloadFactor;

        public static float GetWeaponSpreadFactor(CharacterModel character, WeaponItemModel itemModel) => GetWeaponSpreadFactorImpl(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponSpreadFactorImpl { get; set; } = RpgDefaultValues.GetWeaponSpreadFactor;

        public static float GetWeaponInstabilityFactor(CharacterModel character, WeaponItemModel itemModel) => GetWeaponInstabilityFactorImpl(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponInstabilityFactorImpl { get; set; } = RpgDefaultValues.GetWeaponInstabilityFactor;

        public static float GetWeaponRecoveryFactor(CharacterModel character, WeaponItemModel itemModel) => GetWeaponRecoveryFactorImpl(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponRecoveryFactorImpl { get; set; } = RpgDefaultValues.GetWeaponRecoveryFactor;

        public static float GetWeaponDamageFactor(CharacterModel character, WeaponItemModel itemModel) => GetWeaponDamageFactorImpl(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponDamageFactorImpl { get; set; } = RpgDefaultValues.GetWeaponDamageFactor;

        public static float GetWeaponEnergyCostFactor(CharacterModel character, WeaponItemModel itemModel) => GetWeaponEnergyCostFactorImpl(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponEnergyCostFactorImpl { get; set; } = RpgValues.GetWeaponEnergyCostFactor;
    }
}