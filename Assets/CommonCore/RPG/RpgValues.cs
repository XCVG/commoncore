using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Rpg
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
                + character.BaseStats.Stats[(int)StatType.Erudition]
                + (int)(0.5f * (float)character.BaseStats.Stats[(int)StatType.Intuition])
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

        public static float GetMeleeDamage(CharacterModel character, float baseDamage)
        {
            return Mathf.RoundToInt(baseDamage
                + (2 * character.DerivedStats.Skills[(int)SkillType.MeleeBrawn])
                + (0.5f * character.DerivedStats.Skills[(int)SkillType.Melee])
                + (0.5f * character.DerivedStats.Stats[(int)StatType.Resilience])
                + (0.25f * character.DerivedStats.Stats[(int)StatType.Dexterity])
                );
        }       

        public static float MaxEnergy(CharacterModel characterModel)
        {
            return Mathf.FloorToInt(100 
                + characterModel.DerivedStats.Stats[(int)StatType.Dexterity] * 5.0f
                + characterModel.DerivedStats.Stats[(int)StatType.Resilience] * 2.0f
                + characterModel.DerivedStats.Skills[(int)SkillType.Athletics] * 5.0f
                + characterModel.DerivedStats.Skills[(int)SkillType.AthleticsFleet] * 2.0f);
        }

        public static int AdjustedBuyPrice(CharacterModel character, float value)
        {
            float adjValue = (value * 2.0f)
                - (character.DerivedStats.Skills[(int)SkillType.Social] / 200 * value)
                - (character.DerivedStats.Skills[(int)SkillType.SocialExchange] / 100 * value)
                - (character.DerivedStats.Skills[(int)SkillType.SocialLeverage] / 200 * value)
                - (character.DerivedStats.Stats[(int)StatType.Dialectic] / 1000 * value)
                - (character.DerivedStats.Stats[(int)StatType.Subterfuge] / 1000 * value);

            return Mathf.CeilToInt(Mathf.Max(value, adjValue));
        }

        public static int AdjustedSellPrice(CharacterModel character, float value)
        {
            float halfValue = value * 0.5f;

            float adjValue = halfValue
                + (character.DerivedStats.Skills[(int)SkillType.Social] / 200 * halfValue)
                + (character.DerivedStats.Skills[(int)SkillType.SocialExchange] / 200 * halfValue)
                + (character.DerivedStats.Skills[(int)SkillType.SocialLeverage] / 200 * halfValue)
                + (character.DerivedStats.Stats[(int)StatType.Dialectic] / 1000 * halfValue)
                + (character.DerivedStats.Stats[(int)StatType.Subterfuge] / 1000 * halfValue);

            return Mathf.FloorToInt(Mathf.Min(adjValue, value));
        }

        public static void SkillsFromStats(StatsSet baseStats, StatsSet derivedStats)
        {
            //derive skill values from base stats if you want to; you should never modify baseStats, and ADD to the skills on derivedStats

            //basically for testing; I had no plans to implement this in Ascension III
            //derivedStats.Skills[(int)SkillType.Melee] += derivedStats.Stats[(int)StatType.Resilience] * 100;
        }

        public static float DetectionChance(CharacterModel character, bool isSneaking, bool isRunning)
        {

            float r1 = character.DerivedStats.Skills[(int)SkillType.AthleticsFurtive];
            float r2 = character.DerivedStats.Skills[(int)SkillType.Athletics] * 0.25f;
            float r3 = character.DerivedStats.Skills[(int)StatType.Dexterity] * 2f;
            float rawChance = (100f - (r1 + r2 + r3)) / 100f;

            rawChance *= isSneaking ? 0.5f : 1.0f;
            rawChance *= isRunning ? 1.5f : 1.0f;

            return Mathf.Clamp(rawChance, 0.05f, 0.95f);
        }
    }
}