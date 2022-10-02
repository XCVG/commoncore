using CommonCore.World;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{
    public interface IRpgDefaultValues
    {
        int AdjustedBuyPrice(CharacterModel character, float value);
        int AdjustedSellPrice(CharacterModel character, float value);
        (float damageToShields, float damageToArmor, float damageToCharacter) DamageRatio(ActorHitInfo hitInfo, CharacterModel character);
        float DamageTaken(ActorHitInfo hitInfo, float threshold, float resistance);
        float DetectionChance(CharacterModel character, bool isSneaking, bool isRunning);
        float FallDamage(CharacterModel character, Vector3 velocity);
        float GetAirMoveMultiplier(CharacterModel character);
        float GetIdleEnergyRecoveryRate(CharacterModel character);
        float GetJumpEnergyUse(CharacterModel character);
        float GetJumpVelocityMultiplier(CharacterModel character);
        float GetKickDamageFactor(CharacterModel character);
        float GetKickForceFactor(CharacterModel character);
        float GetKickRateFactor(CharacterModel character);
        float GetMoveSpeedMultiplier(CharacterModel character);
        float GetMovingEnergyRecoveryRate(CharacterModel character);
        float GetRunEnergyRate(CharacterModel character);
        float GetRunSpeedMultiplier(CharacterModel character);
        float GetWeaponDamageFactor(CharacterModel character, WeaponItemModel itemModel);
        float GetWeaponEnergyCostFactor(CharacterModel character, WeaponItemModel itemModel);
        float GetWeaponInstabilityFactor(CharacterModel character, WeaponItemModel itemModel);
        float GetWeaponRateFactor(CharacterModel character, WeaponItemModel itemModel);
        float GetWeaponRecoveryFactor(CharacterModel character, WeaponItemModel itemModel);
        float GetWeaponReloadFactor(CharacterModel character, WeaponItemModel itemModel);
        float GetWeaponSpreadFactor(CharacterModel character, WeaponItemModel itemModel);
        int LevelsForExperience(CharacterModel character);
        float MaxEnergy(CharacterModel characterModel);
        float MaxHealth(CharacterModel characterModel);
        float MaxMagic(CharacterModel characterModel);
        int PotentialPointsForLevel(int newLevel, CharacterModel character);
        ShieldParams ShieldParams(CharacterModel characterModel);
        int SkillGainForPoints(int points);
        void SkillsFromStats(StatsSet baseStats, StatsSet derivedStats);
        int XPAfterMaxLevel(CharacterModel character);
        int XPToNext(int level);
    }
}