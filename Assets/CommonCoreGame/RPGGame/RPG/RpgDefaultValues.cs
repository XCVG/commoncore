using CommonCore.Config;
using CommonCore.World;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{
    /// <summary>
    /// Default implementation of RPG values/calculations
    /// </summary>
    public class RpgDefaultValues : IRpgDefaultValues
    {

        public int XPToNext(int level)
        {
            return level * 10 + 100; //for now
        }

        public int PotentialPointsForLevel(int newLevel, CharacterModel character)
        {
            return 2;
        }

        public int SkillGainForPoints(int points)
        {
            return points * 3;
        }

        public int LevelsForExperience(CharacterModel character)
        {
            int newXP = character.Experience;
            int newLevel = character.Level;
            while (newXP >= XPToNext(newLevel))
            {
                newLevel++;
                newXP -= XPToNext(newLevel);
            }

            return newLevel - character.Level;
        }

        public int XPAfterMaxLevel(CharacterModel character)
        {
            int newXP = character.Experience;
            int newLevel = character.Level;
            while (newXP >= XPToNext(newLevel - 1))
            {
                newLevel++;
                newXP -= XPToNext(newLevel - 1);
            }

            return newXP;
        }

        public float MaxHealth(CharacterModel characterModel)
        {
            return 100 * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerEndurance;
        }

        public float MaxEnergy(CharacterModel characterModel)
        {
            return 100 * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerEndurance;
        }

        public float MaxMagic(CharacterModel characterModel)
        {
            return 100 * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerEndurance;
        }


        public ShieldParams ShieldParams(CharacterModel characterModel)
        {
            //try the ShieldGenerator slot first
            {
                if (characterModel.IsEquipped((int)EquipSlot.ShieldGenerator) && characterModel.Equipped[(int)EquipSlot.ShieldGenerator].ItemModel is ArmorItemModel aim)
                {
                    if (aim.Shields != null)
                        return aim.Shields;
                }
            }

            //also try the Body slot
            {
                if (characterModel.IsEquipped((int)EquipSlot.Body) && characterModel.Equipped[(int)EquipSlot.Body].ItemModel is ArmorItemModel aim)
                {
                    if (aim.Shields != null)
                        return aim.Shields;
                }
            }

            //then try any other slot
            foreach (var item in characterModel.Equipped.Values)
            {
                if (item.ItemModel is ArmorItemModel aim)
                {
                    if (aim.Shields != null)
                        return aim.Shields;
                }
            }

            //return new ShieldParams(100f, 100f, 5f, 2f, 0.1f); //for testing

            return new ShieldParams(); //default/none
        }

        public int AdjustedBuyPrice(CharacterModel character, float value)
        {
            return Mathf.FloorToInt(value);
        }

        public int AdjustedSellPrice(CharacterModel character, float value)
        {
            return Mathf.FloorToInt(value);
        }

        public void SkillsFromStats(StatsSet baseStats, StatsSet derivedStats)
        {
            //derive skill values from base stats if you want to; you should never modify baseStats, only ADD to the skills on derivedStats

        }

        /// <summary>
        /// Calculates applied damage given hitinfo and armor values
        /// </summary>
        public float DamageTaken(ActorHitInfo hitInfo, float threshold, float resistance)
        {
            float damage = hitInfo.Damage;
            float damagePierce = hitInfo.DamagePierce;

            if (hitInfo.HitFlags.HasFlag(BuiltinHitFlags.IgnoreArmor))
                return damage + damagePierce;

            if (hitInfo.HitFlags.HasFlag(BuiltinHitFlags.PierceConsiderArmor))
            {
                damage += damagePierce;
                damagePierce = 0;
            }

            float d1 = damage * ((100f - Mathf.Min(resistance, 99f)) / 100f);
            float dt = Mathf.Max(0, threshold); //threshold-pierce
            float d2 = Mathf.Max(d1 - dt, damage * 0.1f);

            float dp = damagePierce;

            return d2 + dp;
        }

        public (float damageToShields, float damageToArmor, float damageToCharacter) DamageRatio(ActorHitInfo hitInfo, CharacterModel character)
        {
            //for now we'll keep returning all 3 values but we'll probably combine the functionality of DamageTaken into this as well and just spit out "damage to shields" and "raw damage"

            float damage = hitInfo.Damage;
            float damagePierce = hitInfo.DamagePierce;

            if(hitInfo.HitFlags.HasFlag(BuiltinHitFlags.DamageOnlyShields))
            {
                return (Mathf.Min(character.Shields, damage + damagePierce), 0, 0);
            }

            if (character.DerivedStats.ShieldParams.MaxShields == 0 || character.Shields == 0 || hitInfo.HitFlags.HasFlag(BuiltinHitFlags.IgnoreShields))
            {
                if (hitInfo.HitFlags.HasFlag(BuiltinHitFlags.PierceConsiderArmor))
                {
                    damage += damagePierce;
                    damagePierce = 0;
                }

                return (0, damage, damagePierce);
            }

            //keep it simple for now
            float shields = character.Shields;
            float leakRate = character.DerivedStats.ShieldParams.LeakRate;
            float damageToShieldsFromPierce = 0;

            if (hitInfo.HitFlags.HasFlag(BuiltinHitFlags.PierceConsiderShields))
            {
                damageToShieldsFromPierce = Mathf.Min(shields, damagePierce - (damagePierce * leakRate));
                damagePierce -= damageToShieldsFromPierce;
            }

            float damageToShields = Mathf.Min(Mathf.Max(0, shields - damageToShieldsFromPierce), (damage - (damage * leakRate)));
            float damageToArmor = damage - damageToShields;

            if (hitInfo.HitFlags.HasFlag(BuiltinHitFlags.PierceConsiderArmor))
            {
                damageToArmor += damagePierce;
                damagePierce = 0;
            }

            return (damageToShields, damageToArmor, damagePierce);
        }

        /// <summary>
        /// Calculates applied damage given the velocity of a fall
        /// </summary>
        public float FallDamage(CharacterModel character, Vector3 velocity)
        {
            //parameters, local because stupid
            const float minDamageVelocity = 15f;
            const float instantKillVelocity = 100f;
            const float minDamage = 5f; //minimum damage for any fall that counts
            const float maxDamage = 50f; //maximum damage for any non-fatal fall
            const float damageVelocityFactor = 1f; //1 damage per m/s of velocity

            float yVelocity = Mathf.Abs(velocity.y); //for now we only care about y-velocity. Smash into a wall as fast as you want!

            if (yVelocity < minDamageVelocity)
                return 0;

            if (yVelocity >= instantKillVelocity)
                return character.Health + 1;

            float damageFromVelocity = yVelocity * damageVelocityFactor;

            return Mathf.Clamp(damageFromVelocity, minDamage, maxDamage);
        }

        public float DetectionChance(CharacterModel character, bool isSneaking, bool isRunning)
        {
            float rawChance = 1f;

            rawChance *= isSneaking ? 0.5f : 1.0f;
            rawChance *= isRunning ? 1.5f : 1.0f;

            return Mathf.Clamp(rawChance, 0.05f, 0.95f);
        }

        //movement calculations

        public float GetMoveSpeedMultiplier(CharacterModel character)
        {
            return 1f * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerAgility;
        }

        public float GetRunSpeedMultiplier(CharacterModel character)
        {
            return GetMoveSpeedMultiplier(character); //we use the same multiplier here
        }

        public float GetRunEnergyRate(CharacterModel character)
        {
            return 0.1f;
        }

        public float GetJumpVelocityMultiplier(CharacterModel character)
        {
            return 1f * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerAgility;
        }

        public float GetJumpEnergyUse(CharacterModel character)
        {
            return 10f;
        }

        public float GetAirMoveMultiplier(CharacterModel character)
        {
            return 1f * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerAgility;
        }

        public float GetIdleEnergyRecoveryRate(CharacterModel character)
        {
            return 5f;
        }

        public float GetMovingEnergyRecoveryRate(CharacterModel character)
        {
            return GetIdleEnergyRecoveryRate(character) / 3f;
        }

        //weapon calculations

        public float GetKickDamageFactor(CharacterModel character)
        {
            //higher is better
            return 1f * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerStrength;
        }

        public float GetKickForceFactor(CharacterModel character)
        {
            //higher is better
            return 1f;
        }

        public float GetKickRateFactor(CharacterModel character)
        {
            //lower is better
            return 1f;
        }

        public float GetWeaponRateFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            return 1f;
        }

        public float GetWeaponReloadFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            return 1f;
        }

        public float GetWeaponSpreadFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            return 1f;
        }

        public float GetWeaponInstabilityFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            return 1f;
        }

        public float GetWeaponRecoveryFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //higher is better
            return 1f;
        }

        public float GetWeaponDamageFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //higher is better
            return 1f * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerStrength;
        }

        public float GetWeaponEnergyCostFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            return 1f;
        }
    }
}