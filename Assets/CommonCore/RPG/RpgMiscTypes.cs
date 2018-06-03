using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

        public StatsSet(StatsSet original) : this()
        {
            MaxHealth = original.MaxHealth;
            original.DamageResistance.CopyTo(DamageResistance, 0);
            original.DamageThreshold.CopyTo(DamageThreshold, 0);
            original.Stats.CopyTo(Stats, 0);
            original.Skills.CopyTo(Skills, 0);            
        }
        
        public void SetStat<T>(string stat, T value)
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
                        DamageResistance[propertyIndex] = (float)(object)value; //YES I AM VERY SURE
                    else if (propertyName == "DamageThreshold")
                        DamageThreshold[propertyIndex] = (float)(object)value;
                }
                else if (propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    Stats[propertyIndex] = (int)(object)value;
                }
                else if (propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    Skills[propertyIndex] = (int)(object)value;
                }
            }
            else
            {
                GetType().GetProperty(stat).SetValue(this, value, null);
            }
        }

        public void ModStat<T>(string stat, T value)
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
                        DamageResistance[propertyIndex] += (float)(object)value; //YES I AM VERY SURE
                    else if (propertyName == "DamageThreshold")
                        DamageThreshold[propertyIndex] += (float)(object)value;
                }
                else if (propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    Stats[propertyIndex] += (int)(object)value;
                }
                else if (propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    Skills[propertyIndex] += (int)(object)value;
                }
            }
            else
            {
                PropertyInfo property = GetType().GetProperty(stat);
                T oldValue = (T)property.GetValue(this, null);
                //TODO consider switching to .net 4 (experimental) and using generics
                double newValue = ((double)(object)oldValue + (double)(object)value); //as long as it's numeric...
                if (property.GetType() == typeof(double))
                {
                    property.SetValue(this, newValue, null);
                }
                else if (property.GetType() == typeof(float))
                {
                    property.SetValue(this, (float)newValue, null);
                }
                else if(property.GetType() == typeof(int))
                {
                    property.SetValue(this, (int)newValue, null);
                }
                else
                {
                    property.SetValue(this, (T)(object)newValue, null); //attempt to blindly cast
                }
            }
        }

        public T GetStat<T>(string stat)
        {
            object result = null;

            if (stat.Contains("."))
            {
                var statSplit = stat.Split('.');
                string propertyName = statSplit[0];
                string propertyAlias = statSplit[1];

                //explicit handling
                if(propertyName == "DamageResistance" || propertyName == "DamageThreshold")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(DamageType), propertyAlias);
                    if (propertyName == "DamageResistance")
                        result = DamageResistance[propertyIndex];
                    else if (propertyName == "DamageThreshold")
                        result = DamageThreshold[propertyIndex];
                }
                else if(propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    result = Stats[propertyIndex];
                }
                else if(propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    result = Skills[propertyIndex];
                }
            }
            else
            {
                result = GetType().GetProperty(stat).GetValue(this, null);                
            }

            return (T)result;
        }
    }

    //base class for permanent and temporary status conditions
    public abstract class Condition
    {
        public abstract void Apply(StatsSet original, StatsSet target);
    }
}
