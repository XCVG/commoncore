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
        public const bool UseCustomLevelling = true; //TODO hook this up for generic levelling dialog

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

        public static float MaxEnergy(CharacterModel characterModel)
        {
            return Mathf.FloorToInt(100 
                + characterModel.DerivedStats.Stats[(int)StatType.Dexterity] * 5.0f
                + characterModel.DerivedStats.Stats[(int)StatType.Resilience] * 2.0f
                + characterModel.DerivedStats.Skills[(int)SkillType.Athletics] * 5.0f
                + characterModel.DerivedStats.Skills[(int)SkillType.AthleticsFleet] * 2.0f);
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
    }
}