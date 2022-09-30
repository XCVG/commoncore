using Newtonsoft.Json;
using PseudoExtensibleEnum;
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

    [PseudoExtensible]
    public enum DamageType
    {
        Normal, Impact, Explosive, Energy, Poison, Thermal, Radiation
    }

    [PseudoExtensible]
    public enum DamageEffector
    {
        Unspecified, Projectile, Explosion, Melee, Ambient, Internal //matches defaults for now
    }

    //kinda game dependent
    [PseudoExtensible]
    public enum StatType
    {
        Resilience, Dexterity, Erudition, Intuition, Dialectic, Subterfuge, Serendipity
    }

    [Obsolete]
    public enum WeaponSkillType
    {
        Unspecified, Melee, Archery, Guns
    }

    public enum PlayerFlags
    {
        Invulnerable, Immortal, NoTarget, NoFallDamage, NoClip, NoInteract, NoAttack, NoWeapons, NoPhysics, Frozen, TotallyFrozen, HideHud, HideSubtitles, Invisible, NoDropItems, NoJump, NoShieldRecharge
    }

    //mostly game dependent
    //will handle variant trees in leveling logic rather than here
    [PseudoExtensible]
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

    [PseudoExtensible]
    public enum EquipSlot
    {
        None, LeftWeapon, RightWeapon, Body, ShieldGenerator
    }

    //character model will have two of these: base and derived
    public class StatsSet
    {
        public float MaxHealth { get; set; }
        public float MaxEnergy { get; set; }
        public float MaxMagic { get; set; }

        public ShieldParams ShieldParams { get; set; }

        public Dictionary<DamageType, float> DamageResistance { get; set; }
        public Dictionary<DamageType, float> DamageThreshold { get; set; }
        
        public Dictionary<StatType, int> Stats { get; set; }
        public Dictionary<SkillType, int> Skills { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public Dictionary<string, object> ExtraData { get; set; }

        public StatsSet()
        {
            ShieldParams = new ShieldParams();

            DamageResistance = new Dictionary<DamageType, float>();
            DamageResistance.SetupFromEnum(default); //we still do this because there's probably old code that relies on it
            DamageThreshold = new Dictionary<DamageType, float>();
            DamageThreshold.SetupFromEnum(default);

            Stats = new Dictionary<StatType, int>();
            Stats.SetupFromEnum(default);
            Skills = new Dictionary<SkillType, int>();
            Skills.SetupFromEnum(default);

            ExtraData = new Dictionary<string, object>();
        }

        public StatsSet(StatsSet original) : this()
        {
            MaxHealth = original.MaxHealth;
            MaxEnergy = original.MaxEnergy;
            MaxMagic = original.MaxMagic;
            ShieldParams = original.ShieldParams; //okay because ShieldParams is immutable
            DamageResistance = new Dictionary<DamageType, float>(original.DamageResistance);
            DamageThreshold = new Dictionary<DamageType, float>(original.DamageThreshold);
            Stats = new Dictionary<StatType, int>(original.Stats);
            Skills = new Dictionary<SkillType, int>(original.Skills);      
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
                        DamageResistance[(DamageType)propertyIndex] = Convert.ToSingle(value);
                    else if (propertyName == "DamageThreshold")
                        DamageThreshold[(DamageType)propertyIndex] = Convert.ToSingle(value);
                }
                else if (propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    Stats[(StatType)propertyIndex] = Convert.ToInt32(value);
                }
                else if (propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    Skills[(SkillType)propertyIndex] = Convert.ToInt32(value);
                }
                else if(propertyName == "ExtraData")
                {
                    ExtraData[propertyAlias] = value;
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
                        DamageResistance[(DamageType)propertyIndex] += Convert.ToSingle(value);
                    else if (propertyName == "DamageThreshold")
                        DamageThreshold[(DamageType)propertyIndex] += Convert.ToSingle(value);
                }
                else if (propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    Stats[(StatType)propertyIndex] += Convert.ToInt32(value);
                }
                else if (propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    Skills[(SkillType)propertyIndex] += Convert.ToInt32(value);
                }
                else if (propertyName == "ExtraData")
                {
                    if(ExtraData.ContainsKey(propertyAlias))
                    {
                        //dynamic newValue = (dynamic)ExtraData[propertyAlias] + (dynamic)value;
                        //ExtraData[propertyAlias] = newValue;
                        ExtraData[propertyAlias] = TypeUtils.AddValuesDynamic(ExtraData[propertyAlias], value, false);
                    }
                    else
                    {
                        ExtraData[propertyAlias] = value;
                    }
                }
            }
            else
            {
                var prop = GetType().GetProperty(stat);
                if (TypeUtils.IsNumericType(prop.PropertyType))
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
                        result = DamageResistance[(DamageType)propertyIndex];
                    else if (propertyName == "DamageThreshold")
                        result = DamageThreshold[(DamageType)propertyIndex];
                }
                else if (propertyName == "Stats")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(StatType), propertyAlias);
                    result = Stats[(StatType)propertyIndex];
                }
                else if (propertyName == "Skills")
                {
                    int propertyIndex = (int)Enum.Parse(typeof(SkillType), propertyAlias);
                    result = Skills[(SkillType)propertyIndex];
                }
                else if(propertyName == "ExtraData")
                {
                    if (ExtraData.ContainsKey(propertyAlias))
                        return ExtraData[propertyAlias];
                }
            }
            else
            {
                result = GetType().GetProperty(stat).GetValue(this, null);
            }

            return result;
        }
    }

    /// <summary>
    /// Parameter set for shields/barriers (attached to inventory item etc)
    /// </summary>
    public class ShieldParams
    {
        [JsonProperty]
        public float MaxShields { get; private set; }
        [JsonProperty]
        public float RechargeRate { get; private set; }
        [JsonProperty]
        public float RechargeDelay { get; private set; }
        [JsonProperty]
        public float RechargeCancelDamage { get; private set; }
        [JsonProperty]
        public float LeakRate { get; private set; }
        [JsonProperty]
        public float MaxChargeFraction { get; private set; } = 1f;

        public ShieldParams()
        {

        }

        public ShieldParams(float maxShields, float rechargeRate, float rechargeDelay, float rechargeCancelDamage, float leakRate, float maxChargeFraction)
        {
            MaxShields = maxShields;
            RechargeRate = rechargeRate;
            RechargeDelay = rechargeDelay;
            RechargeCancelDamage = rechargeCancelDamage;
            LeakRate = leakRate;
            MaxChargeFraction = maxChargeFraction;
        }
    }

    //base class for permanent and temporary status conditions
    public abstract class Condition
    { 
        public virtual string NiceName { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual void ApplyToStats(StatsSet original, StatsSet target) { }
        public virtual void ApplyToSkills(StatsSet original, StatsSet target) { }
        public virtual void ApplyToDerived(StatsSet original, StatsSet target) { }
    }
}