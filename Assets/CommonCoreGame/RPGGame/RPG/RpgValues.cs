using CommonCore.World;
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

        public static IRpgDefaultValues DefaultValues { get; private set; }

        public static void SetDefaults(IRpgDefaultValues defaultValues)
        {
            DefaultValues = defaultValues;
        }

        public static void SetOverride<T>(string name, T function) where T : Delegate
        {
            string lookupName = name + "Impl";
            Type type = typeof(RpgValues);
            PropertyInfo property = type.GetProperty(lookupName, BindingFlags.Static | BindingFlags.NonPublic);
            property.SetValue(null, function); //will this work?
        }

        public static int XPToNext(int level) => (XPToNextImpl ?? DefaultValues.XPToNext)(level);
        private static Func<int, int> XPToNextImpl { get; set; } = null;

        public static int PotentialPointsForLevel(int newLevel, CharacterModel character) => (PotentialPointsForLevelImpl ?? DefaultValues.PotentialPointsForLevel)(newLevel, character);
        private static Func<int, CharacterModel, int> PotentialPointsForLevelImpl { get; set; } = null;

        public static int SkillGainForPoints(int points) => (SkillGainForPointsImpl ?? DefaultValues.SkillGainForPoints)(points);
        private static Func<int, int> SkillGainForPointsImpl { get; set; } = null;

        public static int LevelsForExperience(CharacterModel character) => (LevelsForExperienceImpl ?? DefaultValues.LevelsForExperience)(character);
        private static Func<CharacterModel, int> LevelsForExperienceImpl { get; set; } = null;

        public static int XPAfterMaxLevel(CharacterModel character) => (XPAfterMaxLevelImpl ?? DefaultValues.XPAfterMaxLevel)(character);
        private static Func<CharacterModel, int> XPAfterMaxLevelImpl { get; set; } = null;

        public static float MaxHealth(CharacterModel characterModel) => (MaxHealthImpl ?? DefaultValues.MaxHealth)(characterModel);
        private static Func<CharacterModel, float> MaxHealthImpl { get; set; } = null;

        public static float MaxEnergy(CharacterModel characterModel) => (MaxEnergyImpl ?? DefaultValues.MaxEnergy)(characterModel);
        private static Func<CharacterModel, float> MaxEnergyImpl { get; set; } = null;

        public static float MaxMagic(CharacterModel characterModel) => (MaxMagicImpl ?? DefaultValues.MaxMagic)(characterModel);
        private static Func<CharacterModel, float> MaxMagicImpl { get; set; } = null;

        public static ShieldParams ShieldParams(CharacterModel characterModel) => (ShieldParamsImpl ?? DefaultValues.ShieldParams)(characterModel);
        private static Func<CharacterModel, ShieldParams> ShieldParamsImpl { get; set; } = null;

        public static int AdjustedBuyPrice(CharacterModel character, float value) => (AdjustedBuyPriceImpl ?? DefaultValues.AdjustedBuyPrice)(character, value);
        private static Func<CharacterModel, float, int> AdjustedBuyPriceImpl { get; set; } = null;

        public static int AdjustedSellPrice(CharacterModel character, float value) => (AdjustedSellPriceImpl ?? DefaultValues.AdjustedSellPrice)(character, value);
        private static Func<CharacterModel, float, int> AdjustedSellPriceImpl { get; set; } = null;

        public static void SkillsFromStats(StatsSet baseStats, StatsSet derivedStats) => (SkillsFromStatsImpl ?? DefaultValues.SkillsFromStats)(baseStats, derivedStats);
        private static Action<StatsSet, StatsSet> SkillsFromStatsImpl { get; set; } = null;

        //both the damage methods below need to be reworked/replaced to handle flags among other things

        /// <summary>
        /// Calculates applied damage given input damage and resistance
        /// </summary>
        public static float DamageTaken(ActorHitInfo hitInfo, float threshold, float resistance) //this is a dumb spot and we will move it later
=> (DamageTakenImpl ?? DefaultValues.DamageTaken)(hitInfo, threshold, resistance);
        private static Func<ActorHitInfo, float, float, float> DamageTakenImpl { get; set; } = null;

        /// <summary>
        /// Calculates damage to shields, armor, and character given damage and a character model
        /// </summary>
        public static (float damageToShields, float damageToArmor, float damageToCharacter) DamageRatio(ActorHitInfo hitInfo, CharacterModel character) => (DamageRatioImpl ?? DefaultValues.DamageRatio)(hitInfo, character);
        private static Func<ActorHitInfo, CharacterModel, (float, float, float)> DamageRatioImpl { get; set; } = null;

        /// <summary>
        /// Calculates applied damage given the velocity of a fall
        /// </summary>
        public static float FallDamage(CharacterModel character, Vector3 velocity) => (FallDamageImpl ?? DefaultValues.FallDamage)(character, velocity);
        private static Func<CharacterModel, Vector3, float> FallDamageImpl { get; set; } = null;

        public static float DetectionChance(CharacterModel character, bool isSneaking, bool isRunning) => (DetectionChanceImpl ?? DefaultValues.DetectionChance)(character, isSneaking, isRunning);
        private static Func<CharacterModel, bool, bool, float> DetectionChanceImpl { get; set; } = null;

        //*********************************
        //***** movement calculations *****

        public static float GetMoveSpeedMultiplier(CharacterModel character) => (GetMoveSpeedMultiplierImpl ?? DefaultValues.GetMoveSpeedMultiplier)(character);
        private static Func<CharacterModel, float> GetMoveSpeedMultiplierImpl { get; set; } = null;

        public static float GetRunSpeedMultiplier(CharacterModel character) => (GetRunSpeedMultiplierImpl ?? DefaultValues.GetRunSpeedMultiplier)(character);
        private static Func<CharacterModel, float> GetRunSpeedMultiplierImpl { get; set; } = null;

        public static float GetRunEnergyRate(CharacterModel character) => (GetRunEnergyRateImpl ?? DefaultValues.GetRunEnergyRate)(character);
        private static Func<CharacterModel, float> GetRunEnergyRateImpl { get; set; } = null;

        public static float GetJumpVelocityMultiplier(CharacterModel character) => (GetJumpVelocityMultiplierImpl ?? DefaultValues.GetJumpVelocityMultiplier)(character);
        private static Func<CharacterModel, float> GetJumpVelocityMultiplierImpl { get; set; } = null;

        public static float GetJumpEnergyUse(CharacterModel character) => (GetJumpEnergyUseImpl ?? DefaultValues.GetJumpEnergyUse)(character);
        private static Func<CharacterModel, float> GetJumpEnergyUseImpl { get; set; } = null;

        public static float GetAirMoveMultiplier(CharacterModel character) => (GetAirMoveMultiplierImpl ?? DefaultValues.GetAirMoveMultiplier)(character);
        private static Func<CharacterModel, float> GetAirMoveMultiplierImpl { get; set; } = null;

        public static float GetIdleEnergyRecoveryRate(CharacterModel character) => (GetIdleEnergyRecoveryRateImpl ?? DefaultValues.GetIdleEnergyRecoveryRate)(character);
        private static Func<CharacterModel, float> GetIdleEnergyRecoveryRateImpl { get; set; } = null;

        public static float GetMovingEnergyRecoveryRate(CharacterModel character) => (GetMovingEnergyRecoveryRateImpl ?? DefaultValues.GetMovingEnergyRecoveryRate)(character);
        private static Func<CharacterModel, float> GetMovingEnergyRecoveryRateImpl { get; set; } = null;



        //*******************************
        //***** weapon calculations *****

        public static float GetKickDamageFactor(CharacterModel character) => (GetKickDamageFactorImpl ?? DefaultValues.GetKickDamageFactor)(character);
        private static Func<CharacterModel, float> GetKickDamageFactorImpl { get; set; } = null;

        public static float GetKickForceFactor(CharacterModel character) => (GetKickForceFactorImpl ?? DefaultValues.GetKickForceFactor)(character);
        private static Func<CharacterModel, float> GetKickForceFactorImpl { get; set; } = null;

        public static float GetKickRateFactor(CharacterModel character) => (GetKickRateFactorImpl ?? DefaultValues.GetKickRateFactor)(character);
        private static Func<CharacterModel, float> GetKickRateFactorImpl { get; set; } = null;

        public static float GetWeaponRateFactor(CharacterModel character, WeaponItemModel itemModel) => (GetWeaponRateFactorImpl ?? DefaultValues.GetWeaponRateFactor)(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponRateFactorImpl { get; set; } = null;

        public static float GetWeaponReloadFactor(CharacterModel character, WeaponItemModel itemModel) => (GetWeaponReloadFactorImpl ?? DefaultValues.GetWeaponReloadFactor)(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponReloadFactorImpl { get; set; } = null;

        public static float GetWeaponSpreadFactor(CharacterModel character, WeaponItemModel itemModel) => (GetWeaponSpreadFactorImpl ?? DefaultValues.GetWeaponSpreadFactor)(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponSpreadFactorImpl { get; set; } = null;

        public static float GetWeaponInstabilityFactor(CharacterModel character, WeaponItemModel itemModel) => (GetWeaponInstabilityFactorImpl ?? DefaultValues.GetWeaponInstabilityFactor)(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponInstabilityFactorImpl { get; set; } = null;

        public static float GetWeaponRecoveryFactor(CharacterModel character, WeaponItemModel itemModel) => (GetWeaponRecoveryFactorImpl ?? DefaultValues.GetWeaponRecoveryFactor)(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponRecoveryFactorImpl { get; set; } = null;

        public static float GetWeaponDamageFactor(CharacterModel character, WeaponItemModel itemModel) => (GetWeaponDamageFactorImpl ?? DefaultValues.GetWeaponDamageFactor)(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponDamageFactorImpl { get; set; } = null;

        public static float GetWeaponEnergyCostFactor(CharacterModel character, WeaponItemModel itemModel) => (GetWeaponEnergyCostFactorImpl ?? DefaultValues.GetWeaponEnergyCostFactor)(character, itemModel);
        private static Func<CharacterModel, WeaponItemModel, float> GetWeaponEnergyCostFactorImpl { get; set; } = null;
    }
}