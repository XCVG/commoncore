using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using CommonCore.Messaging;

namespace CommonCore.Rpg
{ 
    /*
     * This is a complete character model, right now used only for players
     * Members can be smartly accessed by name via reflection
     */
    public class CharacterModel
    {
        public string FormID { get; set; }
        public string DisplayName { get; set; }

        public Sex Gender { get; set; }

        public float Energy
        {
            get
            {
                return DerivedStats.MaxEnergy * EnergyFraction;
            }
            set
            {
                EnergyFraction = value / DerivedStats.MaxEnergy;
            }
        }

        public float EnergyFraction { get; set; }

        public float Health
        {
            get
            {
                return DerivedStats.MaxHealth * HealthFraction;
            }
            set
            {
                HealthFraction = value / DerivedStats.MaxHealth;
            }
        }

        public float HealthFraction { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }


        public StatsSet BaseStats { get; private set; }

        public StatsSet DerivedStats { get; private set; }

        public List<Condition> Conditions { get; private set; }

        
        public InventoryModel Inventory { get; private set; }
        public int AmmoInMagazine { get; set; }
        public Dictionary<EquipSlot, InventoryItemInstance> Equipped { get; private set; }

        public CharacterModel() //TODO with a model base parameter
        {
            HealthFraction = 1.0f;
            Level = 1;

            Inventory = new InventoryModel();
            Conditions = new List<Condition>();
            Equipped = new Dictionary<EquipSlot, InventoryItemInstance>();

            //create blank stats and derive stats
            BaseStats = new StatsSet(); //TODO load defaults
            UpdateStats();
        }

        public void UpdateStats() //TODO link this into messaging at some point
        {
            //copy base stats
            DerivedStats = new StatsSet(BaseStats);

            //apply conditions
            foreach(Condition c in Conditions)
            {
                c.Apply(BaseStats, DerivedStats);
            }

            //apply equipment bonuses (armor basically)
            if (Equipped.ContainsKey(EquipSlot.Body))
            {
                ArmorItemModel aim = Equipped[EquipSlot.Body].ItemModel as ArmorItemModel;
                if(aim != null)
                {
                    for(int i = 0; i < BaseStats.DamageResistance.Length; i++)
                    {
                        if (aim.DamageResistance.ContainsKey((DamageType)i))
                            DerivedStats.DamageResistance[i] = BaseStats.DamageResistance[i] + aim.DamageResistance[(DamageType)i];
                        if (aim.DamageThreshold.ContainsKey((DamageType)i))
                            DerivedStats.DamageThreshold[i] = BaseStats.DamageThreshold[i] + aim.DamageThreshold[(DamageType)i];
                    }
                }
                else
                {
                    Debug.LogWarning("Player has non-armor item in armor slot!");
                }
            }

            //recalculate max health and energy
            DerivedStats.MaxHealth = RpgValues.MaxHealthForLevel(Level);
            DerivedStats.MaxEnergy = RpgValues.MaxEnergy(this);

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("RpgStatsUpdated"));

        }

        public void EquipItem(InventoryItemInstance item)
        {
            if (item.Equipped)
                throw new InvalidOperationException();

            EquipSlot slot = InventoryModel.GetItemSlot(item.ItemModel);

            if (slot == EquipSlot.None)
                throw new InvalidOperationException();

            if (Equipped.ContainsKey(slot))
                UnequipItem(Equipped[slot]);

            Equipped[slot] = item;

            //magazine logic
            if (item.ItemModel is RangedWeaponItemModel)
            {
                var rwim = (RangedWeaponItemModel)item.ItemModel;
                AmmoInMagazine = Math.Min(rwim.MagazineSize, Inventory.CountItem(rwim.AType.ToString()));
                Inventory.RemoveItem(rwim.AType.ToString(), AmmoInMagazine);
            }

            item.Equipped = true;

            UpdateStats();

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("RpgChangeWeapon"));
        }

        public void UnequipItem(InventoryItemInstance item)
        {
            if (!item.Equipped)
                throw new InvalidOperationException();

            EquipSlot slot = InventoryModel.GetItemSlot(item.ItemModel);

            if (slot != EquipSlot.None)
            {
                Equipped.Remove(slot);            
            }           
            //allow continuing even if it's not actually equippable, for fixing bugs

            //magazine logic
            if(item.ItemModel is RangedWeaponItemModel)
            {
                var rwim = (RangedWeaponItemModel)item.ItemModel;
                Inventory.AddItem(rwim.AType.ToString(), AmmoInMagazine);
                AmmoInMagazine = 0;
            }

            item.Equipped = false;

            UpdateStats();

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("RpgChangeWeapon"));
        }

        public void SetAV(string av, object value)
        {
            SetAV(av, value, null); //null=auto
        }

        public void SetAV(string av, object value, bool? propagate)
        {
            if(av.Contains("."))
            {
                string firstPart = av.Substring(0, av.IndexOf('.'));
                string secondPart = av.Substring(av.IndexOf('.') + 1);
                if(firstPart == "BaseStats")
                {
                    BaseStats.SetStat(secondPart, value);
                    if (!propagate.HasValue)
                        UpdateStats();
                }
                else if(firstPart == "DerivedStats")
                {
                    DerivedStats.SetStat(secondPart, value);
                }
                else if(firstPart == "Conditions")
                {
                    string fqConditionName = GetType().Namespace + "." + value.ToString();
                    Condition c = (Condition)Activator.CreateInstance(Type.GetType(fqConditionName));
                    Conditions.Add(c);

                    if (!propagate.HasValue)
                        UpdateStats();
                }
            }
            else
            {
                //search and set property
                var prop = GetType().GetProperty(av);
                prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType), null);
            }

            if(propagate.HasValue && propagate.Value)
                UpdateStats();
        }

        public void ModAV(string av, object value)
        {
            ModAV(av, value, null);
        }

        public void ModAV(string av, object value, bool? propagate)
        {
            if (av.Contains("."))
            {
                string firstPart = av.Substring(0, av.IndexOf('.'));
                string secondPart = av.Substring(av.IndexOf('.') + 1);
                if (firstPart == "BaseStats")
                {
                    BaseStats.ModStat(secondPart, value);
                    if (!propagate.HasValue)
                        UpdateStats();
                }
                else if (firstPart == "DerivedStats")
                {
                    DerivedStats.ModStat(secondPart, value);
                }
                else if (firstPart == "Conditions")
                {
                    //delete if present, add if not

                    string fqConditionName = GetType().Namespace + "." + value.ToString();
                    Condition newCondition = (Condition)Activator.CreateInstance(Type.GetType(fqConditionName));

                    Condition oldCondition = null;
                    foreach (Condition c in Conditions)
                    {
                        if(c.GetType() == newCondition.GetType())
                        {
                            oldCondition = c;
                            break;
                        }
                    }

                    if(oldCondition != null)
                    {
                        Conditions.Remove(oldCondition);
                    }
                    else
                    {
                        Conditions.Add(newCondition);
                    }
                    

                    if (!propagate.HasValue)
                        UpdateStats();
                }
            }
            else
            {
                //search and modify property
                var prop = GetType().GetProperty(av);
                if(CCBaseUtil.IsNumericType(prop.PropertyType))
                {
                    decimal newVal = Convert.ToDecimal(prop.GetValue(this, null)) + Convert.ToDecimal(value);
                    prop.SetValue(this, Convert.ChangeType(newVal, prop.PropertyType), null);
                }
                else if(prop.PropertyType == typeof(string))
                {
                    string newVal = ((string)prop.GetValue(this, null)) + (string)(object)value;
                    prop.SetValue(this, newVal, null);
                }
                else
                {
                    prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType), null);
                }
                
            }

            if (propagate.HasValue && propagate.Value)
                UpdateStats();
        }

        internal object GetAV(string av)
        {
            if (av.Contains("."))
            {
                string firstPart = av.Substring(0, av.IndexOf('.'));
                string secondPart = av.Substring(av.IndexOf('.') + 1);
                if (firstPart == "BaseStats")
                {
                    return BaseStats.GetStat(secondPart);
                }
                else if (firstPart == "DerivedStats")
                {
                    return DerivedStats.GetStat(secondPart);
                }
                else if (firstPart == "Conditions")
                {
                    string fqConditionName = GetType().Namespace + "." + secondPart.ToString();
                    Condition newC = (Condition)Activator.CreateInstance(Type.GetType(fqConditionName));
                    bool found = false;

                    foreach (Condition c in Conditions)
                    {
                        if (c.GetType() == newC.GetType())
                        {
                            found = true;
                            break;
                        }
                    }

                    return found;
                }
            }
            else
            {
                //search and get property
                return GetType().GetProperty(av).GetValue(this, null);
            }

            //fail
            throw new KeyNotFoundException();
        }

        public T GetAV<T>(string av)
        {
            if (av.Contains("."))
            {
                string firstPart = av.Substring(0, av.IndexOf('.'));
                string secondPart = av.Substring(av.IndexOf('.') + 1);
                if (firstPart == "BaseStats")
                {
                    return BaseStats.GetStat<T>(secondPart);                    
                }
                else if (firstPart == "DerivedStats")
                {
                    return DerivedStats.GetStat<T>(secondPart);
                }
                else if (firstPart == "Conditions")
                {
                    string fqConditionName = GetType().Namespace + "." + secondPart.ToString();
                    Condition newC = (Condition)Activator.CreateInstance(Type.GetType(fqConditionName));
                    bool found = false;

                    foreach(Condition c in Conditions)
                    {
                        if(c.GetType() == newC.GetType())
                        {
                            found = true;
                            break;
                        }
                    }

                    return (T)(object)found;                    
                }
            }
            else
            {
                //search and get property
                return (T)Convert.ChangeType(GetType().GetProperty(av).GetValue(this, null), typeof(T));
            }

            //fail
            throw new KeyNotFoundException();
        }

        public bool IsEquipped(EquipSlot slot)
        {
            return (Equipped.ContainsKey(slot) && Equipped[slot] != null);
        }

        public void CheckLevelUp()
        {
            if(Experience >= RpgValues.XPToNext(Level))
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("RPG_LevelUp"));
                QdmsMessageBus.Instance.PushBroadcast(new HUDPushMessage("<l:IGUI_MESSAGE:LevelUp>"));
            }
        }

        
    }
}
