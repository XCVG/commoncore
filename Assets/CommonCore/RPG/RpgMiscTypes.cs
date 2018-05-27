using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Rpg
{
    public enum DamageType
    {
        Normal, Impact, Explosive, Energy, Poison, Radiation
    }

    //kinda game dependent
    public enum StatType
    {
        Resilience, Dexterity, Erudition, Intuition, Dialectic, Subterfuge, Serendipity
    }

    //mostly game dependent
    //will handle variant trees in leveling logic rather than here
    public enum SkillType
    {
        Melee, MeleeAlacrity, MeleePrecision, MeleeBrawn,
        Archery, ArcheryDraw, ArcherySteady,
        Guns, GunsAccuracy, GunsRapidity,
        Athletics, AthleticsFurtive, AthleticsFleet,
        Magic, MagicForce, MagicElemental, MagicDark,
        Social, SocialExchange, SocialLeverage,
        Security, SecurityMechanisms, SecurityComputers
    }

    //character model will have two of these: base and derived
    public class StatsSet
    {
        public float MaxHealth { get; set; }
        public float[] DamageResistance { get; set; }
        public float[] DamageThreshold { get; set; }
        
        public int[] Stats { get; set; }
        public int[] Skills { get; set; }

        public StatsSet()
        {
            DamageResistance = new float[Enum.GetNames(typeof(DamageType)).Length];
            DamageThreshold = new float[Enum.GetNames(typeof(DamageType)).Length];

            Stats = new int[Enum.GetNames(typeof(StatType)).Length];
            Skills = new int[Enum.GetNames(typeof(SkillType)).Length];
        }
        
    }

    //base class for permanent and temporary status conditions
    public abstract class Condition
    {

    }
}
