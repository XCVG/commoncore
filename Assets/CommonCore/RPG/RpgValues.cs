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

        public static int SkillPointsForLevel(int newLevel, CharacterModel character)
        {
            return (2
                + character.BaseStats.Stats[(int)StatType.Erudition]
                + (int)(0.5f * (float)character.BaseStats.Stats[(int)StatType.Intuition])
                );
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
            while (newXP >= XPToNext(newLevel))
            {
                newLevel++;
                newXP -= XPToNext(newLevel);
            }

            return newXP;
        }
    }
}