using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{
    public enum Sex //we could get into arguments about sex vs gender etc but these are enough for game logic purposes
    {
        Undefined, Female, Male, Other
    }

    //TODO move into world (?)
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

    public enum EquipSlot
    {
        None, RangedWeapon, MeleeWeapon, Body
    }

    //character model will have two of these: base and derived
    public class StatsSet
    {
        public float MaxHealth { get; set; }
        public float MaxEnergy { get; set; }
        public float[] DamageResistance { get; set; } //this is stupid, TODO change to dictionary with enum keys
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

        public StatsSet(StatsSet original) : this()
        {
            MaxHealth = original.MaxHealth;
            original.DamageResistance.CopyTo(DamageResistance, 0);
            original.DamageThreshold.CopyTo(DamageThreshold, 0);
            original.Stats.CopyTo(Stats, 0);
            original.Skills.CopyTo(Skills, 0);            
        }
        
        public void SetStat(string stat, object value)
        {
            if(stat.Contains("."))
            {
                var statSplit = stat.Split('.');
                string propertyName = statSplit[0];
                string propertyAlias = statSplit[1];

                //explicit handling
                if (propertyName == "DamageResistance" || propertyName == "DamageThreshold")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(DamageType), propertyAlias);
                    if (propertyName == "DamageResistance")
                        DamageResistance[propertyIndex] = Convert.ToSingle(value);
                    else if (propertyName == "DamageThreshold")
                        DamageThreshold[propertyIndex] = Convert.ToSingle(value);
                }
                else if (propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    Stats[propertyIndex] = Convert.ToInt32(value);
                }
                else if (propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    Skills[propertyIndex] = Convert.ToInt32(value);
                }
            }
            else
            {
                var prop = GetType().GetProperty(stat);
                prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType), null);
            }
        }

        public void ModStat(string stat, object value)
        {
            if (stat.Contains("."))
            {
                var statSplit = stat.Split('.');
                string propertyName = statSplit[0];
                string propertyAlias = statSplit[1];

                //explicit handling
                if (propertyName == "DamageResistance" || propertyName == "DamageThreshold")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(DamageType), propertyAlias);
                    if (propertyName == "DamageResistance")
                        DamageResistance[propertyIndex] += Convert.ToSingle(value);
                    else if (propertyName == "DamageThreshold")
                        DamageThreshold[propertyIndex] += Convert.ToSingle(value);
                }
                else if (propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    Stats[propertyIndex] += Convert.ToInt32(value);
                }
                else if (propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    Skills[propertyIndex] += Convert.ToInt32(value);
                }
            }
            else
            {
                var prop = GetType().GetProperty(stat);
                if (CoreUtils.IsNumericType(prop.PropertyType))
                {
                    decimal newVal = Convert.ToDecimal(prop.GetValue(this, null)) + Convert.ToDecimal(value);
                    prop.SetValue(this, Convert.ChangeType(newVal, prop.PropertyType), null);
                }
                else if (prop.PropertyType == typeof(string))
                {
                    string newVal = ((string)prop.GetValue(this, null)) + (string)(object)value;
                    prop.SetValue(this, newVal, null);
                }
                else
                {
                    prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType), null);
                }
            }
        }

        public T GetStat<T>(string stat)
        {
            return (T)Convert.ChangeType(GetStat(stat), typeof(T));
        }

        internal object GetStat(string stat)
        {
            object result = null;

            if (stat.Contains("."))
            {
                var statSplit = stat.Split('.');
                string propertyName = statSplit[0];
                string propertyAlias = statSplit[1];

                //explicit handling
                if (propertyName == "DamageResistance" || propertyName == "DamageThreshold")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(DamageType), propertyAlias);
                    if (propertyName == "DamageResistance")
                        result = DamageResistance[propertyIndex];
                    else if (propertyName == "DamageThreshold")
                        result = DamageThreshold[propertyIndex];
                }
                else if (propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    result = Stats[propertyIndex];
                }
                else if (propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    result = Skills[propertyIndex];
                }
            }
            else
            {
                result = GetType().GetProperty(stat).GetValue(this, null);
            }

            return result;
        }
    }

    //base class for permanent and temporary status conditions
    public abstract class Condition
    { 
        public virtual string NiceName { get; protected set; }
        public virtual string Description { get; protected set; }
        public abstract void Apply(StatsSet original, StatsSet target);
    }
}