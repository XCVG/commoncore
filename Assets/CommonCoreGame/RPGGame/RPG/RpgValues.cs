﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{
    /*
     * This is pretty hacky, but honestly I'd expect it to stay until at least Downwarren
     */
    public static class RpgValues
    {
        public static int XPToNext(int level)
        {
            return level * 10 + 100; //for now
        }

        public static float MaxHealthForLevel(int level)
        {
            return 100 + level * 10;
        }

        public static int PotentialPointsForLevel(int newLevel, CharacterModel character)
        {
            return (2
                + character.BaseStats.Stats[StatType.Erudition]
                + (int)(0.5f * (float)character.BaseStats.Stats[StatType.Intuition])
                );
        }

        public static int SkillGainForPoints(int points)
        {
            return points * 10;
        }

        public static int LevelsForExperience(CharacterModel character)
        {
            int newXP = character.Experience;
            int newLevel = character.Level;
            while(newXP >= XPToNext(newLevel))
            {
                newLevel++;
                newXP -= XPToNext(newLevel);
            }

            return newLevel - character.Level;
        }

        public static int XPAfterMaxLevel(CharacterModel character)
        {
            int newXP = character.Experience;
            int newLevel = character.Level;
            while (newXP >= XPToNext(newLevel-1))
            {
                newLevel++;
                newXP -= XPToNext(newLevel-1);
            }

            return newXP;
        } 

        public static float MaxEnergy(CharacterModel characterModel)
        {
            return Mathf.FloorToInt(100 
                + characterModel.DerivedStats.Stats[StatType.Dexterity] * 5.0f
                + characterModel.DerivedStats.Stats[StatType.Resilience] * 2.0f
                + characterModel.DerivedStats.Skills[SkillType.Athletics] * 5.0f
                + characterModel.DerivedStats.Skills[SkillType.AthleticsFleet] * 2.0f);
        }

        public static int AdjustedBuyPrice(CharacterModel character, float value)
        {
            float adjValue = (value * 2.0f)
                - (character.DerivedStats.Skills[SkillType.Social] / 200 * value)
                - (character.DerivedStats.Skills[SkillType.SocialExchange] / 100 * value)
                - (character.DerivedStats.Skills[SkillType.SocialLeverage] / 200 * value)
                - (character.DerivedStats.Stats[StatType.Dialectic] / 1000 * value)
                - (character.DerivedStats.Stats[StatType.Subterfuge] / 1000 * value);

            return Mathf.CeilToInt(Mathf.Max(value, adjValue));
        }

        public static int AdjustedSellPrice(CharacterModel character, float value)
        {
            float halfValue = value * 0.5f;

            float adjValue = halfValue
                + (character.DerivedStats.Skills[SkillType.Social] / 200 * halfValue)
                + (character.DerivedStats.Skills[SkillType.SocialExchange] / 200 * halfValue)
                + (character.DerivedStats.Skills[SkillType.SocialLeverage] / 200 * halfValue)
                + (character.DerivedStats.Stats[StatType.Dialectic] / 1000 * halfValue)
                + (character.DerivedStats.Stats[StatType.Subterfuge] / 1000 * halfValue);

            return Mathf.FloorToInt(Mathf.Min(adjValue, value));
        }

        public static void SkillsFromStats(StatsSet baseStats, StatsSet derivedStats)
        {
            //derive skill values from base stats if you want to; you should never modify baseStats, and ADD to the skills on derivedStats

            //basically for testing; I had no plans to implement this in Ascension III
            //derivedStats.Skills[(int)SkillType.Melee] += derivedStats.Stats[(int)StatType.Resilience] * 100;
        }

        /// <summary>
        /// Calculates applied damage given input damage and resistance
        /// </summary>
        public static float DamageTaken(float damage, float pierce, float threshold, float resistance) //this is a dumb spot and we will move it later
        {
            float d1 = damage * ((100f - Mathf.Min(resistance, 99f)) / 100f);
            float dt = Mathf.Max(0, threshold - pierce);
            float d2 = Mathf.Max(d1 - dt, damage * 0.1f);
            if (GameParams.UseRandomDamage)
                d2 *= UnityEngine.Random.Range(0.75f, 1.25f);
            return d2;
        }

        public static float DetectionChance(CharacterModel character, bool isSneaking, bool isRunning)
        {

            float r1 = character.DerivedStats.Skills[SkillType.AthleticsFurtive];
            float r2 = character.DerivedStats.Skills[SkillType.Athletics] * 0.25f;
            float r3 = character.DerivedStats.Stats[StatType.Dexterity] * 2f;
            float rawChance = (100f - (r1 + r2 + r3)) / 100f;

            rawChance *= isSneaking ? 0.5f : 1.0f;
            rawChance *= isRunning ? 1.5f : 1.0f;

            return Mathf.Clamp(rawChance, 0.05f, 0.95f);
        }

        //weapon calculations

        public static float GetKickDamageFactor(CharacterModel character)
        {
            //higher is better
            return Mathf.Clamp( 
                    1.0f
                    + (character.DerivedStats.Stats[StatType.Resilience] / 20f)
                    + (character.DerivedStats.Stats[StatType.Dexterity] / 30f)
                    + (character.DerivedStats.Skills[SkillType.MeleeBrawn] / 100f)
                    + (character.DerivedStats.Skills[SkillType.Melee] / 200f),
                0.5f, 3.0f);
        }

        public static float GetKickForceFactor(CharacterModel character)
        {
            //higher is better
            return Mathf.Clamp(
                    1.0f
                    + (character.DerivedStats.Stats[StatType.Resilience] / 20f)
                    + (character.DerivedStats.Stats[StatType.Dexterity] / 30f)
                    + (character.DerivedStats.Skills[SkillType.MeleeBrawn] / 100f)
                    + (character.DerivedStats.Skills[SkillType.Melee] / 200f),
                0.5f, 2.0f);
        }

        public static float GetKickRateFactor(CharacterModel character)
        {
            //lower is better
            return Mathf.Clamp(
                    1.0f
                    - (character.DerivedStats.Stats[StatType.Resilience] / 20f)
                    - (character.DerivedStats.Stats[StatType.Dexterity] / 30f)
                    - (character.DerivedStats.Skills[SkillType.MeleeBrawn] / 100f)
                    - (character.DerivedStats.Skills[SkillType.Melee] / 200f),
                0.5f, 2.0f);
        }

        public static float GetWeaponRateFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            float factor = 1.0f;

            switch (itemModel.SkillType)
            {
                case WeaponSkillType.Melee:
                    factor -= character.DerivedStats.Skills[SkillType.MeleeAlacrity] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    break;
                case WeaponSkillType.Archery:
                    factor -= character.DerivedStats.Skills[SkillType.Archery] / 400f; //you can't really increase archery fire rate much
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    break;
                case WeaponSkillType.Guns:
                    factor -= character.DerivedStats.Skills[SkillType.GunsRapidity] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 200f;
                    break;
            }

            return Mathf.Clamp(factor, 0.5f, 2.0f);
        }

        public static float GetWeaponReloadFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            float factor = 1.0f;

            switch (itemModel.SkillType)
            {
                case WeaponSkillType.Guns:
                    factor -= character.DerivedStats.Skills[SkillType.GunsRapidity] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    break;
            }

            return Mathf.Clamp(factor, 0.5f, 2.0f);
        }

        public static float GetWeaponSpreadFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            float factor = 1.0f;

            switch (itemModel.SkillType)
            {
                case WeaponSkillType.Archery:
                    factor -= character.DerivedStats.Skills[SkillType.ArcherySteady] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    break;
                case WeaponSkillType.Guns:
                    factor -= character.DerivedStats.Skills[SkillType.GunsAccuracy] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    break;
            }

            return Mathf.Clamp(factor, 0.25f, 2.0f);
        }

        public static float GetWeaponInstabilityFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            float factor = 1.0f;

            switch (itemModel.SkillType)
            {
                case WeaponSkillType.Archery:
                    factor -= character.DerivedStats.Skills[SkillType.ArcherySteady] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    break;
                case WeaponSkillType.Guns:
                    factor -= character.DerivedStats.Skills[SkillType.GunsAccuracy] / 200f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    break;
            }

            return Mathf.Clamp(factor, 0.25f, 2.0f);
        }

        public static float GetWeaponRecoveryFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //higher is better
            float factor = 1.0f;

            switch (itemModel.SkillType)
            {
                case WeaponSkillType.Archery:
                    factor += character.DerivedStats.Skills[SkillType.ArcherySteady] / 100f;
                    factor += character.DerivedStats.Stats[StatType.Dexterity] / 50f;
                    break;
                case WeaponSkillType.Guns:
                    factor += character.DerivedStats.Skills[SkillType.GunsAccuracy] / 200f;
                    factor += character.DerivedStats.Stats[StatType.Dexterity] / 50f;
                    break;
            }

            return Mathf.Clamp(factor, 0.25f, 2.0f);
        }

        public static float GetWeaponDamageFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //higher is better
            float factor = 1.0f;

            switch (itemModel.SkillType)
            {
                case WeaponSkillType.Melee:
                    factor += character.DerivedStats.Stats[StatType.Resilience] / 20f;
                    factor += character.DerivedStats.Stats[StatType.Dexterity] / 30f;
                    factor += character.DerivedStats.Skills[SkillType.MeleeBrawn] / 100f;
                    factor += character.DerivedStats.Skills[SkillType.Melee] / 200f;
                    if(!itemModel.CheckFlag(ItemFlag.WeaponNeverRandomize))
                    {
                        float invPrecision = Mathf.Clamp(100f - character.DerivedStats.Skills[SkillType.MeleePrecision], 0, 100f);
                        float precisionMax = invPrecision / 200f;
                        float randomFactor = UnityEngine.Random.Range(0, precisionMax);
                        factor -= randomFactor;                     
                    }
                    break;
                case WeaponSkillType.Archery:
                    factor += character.DerivedStats.Skills[SkillType.ArcheryDraw] / 100f;
                    factor += character.DerivedStats.Stats[StatType.Resilience] / 50f;
                    factor += character.DerivedStats.Stats[StatType.Dexterity] / 50f;
                    break;
            }

            return Mathf.Clamp(factor, 0.25f, 4.0f);
        }

        public static float GetWeaponEnergyCostFactor(CharacterModel character, WeaponItemModel itemModel)
        {
            //lower is better
            float factor = 1.0f;

            switch (itemModel.SkillType)
            {
                case WeaponSkillType.Melee:
                    factor -= character.DerivedStats.Skills[SkillType.MeleePrecision] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Resilience] / 100f;
                    break;
                case WeaponSkillType.Archery:
                    factor -= character.DerivedStats.Skills[SkillType.Archery] / 200f;
                    factor -= character.DerivedStats.Skills[SkillType.ArcheryDraw] / 100f;
                    factor -= character.DerivedStats.Stats[StatType.Dexterity] / 100f;
                    break;
            }

            return Mathf.Clamp(factor, 0.25f, 2.0f);
        }
    }
}