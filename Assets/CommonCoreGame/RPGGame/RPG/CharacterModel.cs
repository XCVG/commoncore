using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using CommonCore.Messaging;
using CommonCore.UI;
using CommonCore.Config;
using System.Runtime.Serialization;
using CommonCore.World;
using CommonCore.State;
using System.Collections;
using System.Linq;
using CommonCore.Scripting;
using PseudoExtensibleEnum;

namespace CommonCore.RpgGame.Rpg
{
    /*
     * This is a complete character model, right now used only for players
     * Members can be smartly accessed by name via reflection
     */
    public class CharacterModel : IPlayerFlagsSource
    {
        public string FormID { get; set; }
        public string DisplayName { get; set; }

        public Sex Gender { get; set; }

        [JsonIgnore]
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

        [JsonProperty(PropertyName = "Energy")] //we want to serialize this (mostly for debugging) but don't want to load it
        private float EnergyJsonSavable => Energy;

        public float EnergyFraction { get; set; }

        [JsonIgnore]
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

        [JsonProperty(PropertyName = "Health")]
        private float HealthJsonSavable => Health;

        public float HealthFraction { get; set; }

        [JsonIgnore]
        public float Shields
        {
            get
            {
                return DerivedStats.ShieldParams.MaxShields * ShieldsFraction;
            }
            set
            {
                ShieldsFraction = value / DerivedStats.ShieldParams.MaxShields;
            }
        }

        [JsonProperty(PropertyName = "Shields")]
        private float ShieldsJsonSavable => Shields;

        public float ShieldsFraction { get; set; }

        [JsonIgnore]
        public float Magic
        {
            get
            {
                return DerivedStats.MaxMagic * MagicFraction;
            }
            set
            {
                MagicFraction = value / DerivedStats.MaxMagic;
            }
        }

        [JsonProperty(PropertyName = "Magic")] //we want to serialize this (mostly for debugging) but don't want to load it
        private float MagicJsonSavable => Energy;

        public float MagicFraction { get; set; }

        public int Experience { get; set; }
        public int Level { get; set; }


        public StatsSet BaseStats { get; private set; }
        [JsonIgnore]
        public StatsSet DerivedStats { get; private set; }
        [JsonProperty(PropertyName = "DerivedStats")]
        private StatsSet DerivedStatsSavable => DerivedStats;

        public List<Condition> Conditions { get; private set; }

        public IEnumerable<Condition> AllConditions => Conditions; //TODO handle conditions from equipped items and conditions from playerflags

        public InventoryModel Inventory { get; private set; }
        [JsonProperty, JsonConverter(typeof(PxEnumObjectConverter), typeof(EquipSlot))]
        public Dictionary<int, int> AmmoInMagazine { get; private set; }

        [JsonIgnore]
        public IDictionary<int, InventoryItemInstance> Equipped { get; private set; }
        [JsonProperty(PropertyName = "Equipped")]
        private Dictionary<EquipSlot, InventoryItemInstance> EquippedJsonParseable //hack for parsing old saves and old RPG defs
        {
            set
            {
                if (value != null && value.Count > 0)
                {
                    Debug.LogWarning("Loading equipped items through legacy object-ref parsing");
                    foreach (var kvp in value)
                    {
                        if (kvp.Value.InstanceUID == 0)
                        {
                            Debug.LogError($"Cannot equip item instance ({kvp.Value}) because it has no UID");
                            continue;
                        }
                        EquippedIDs[(int)kvp.Key] = kvp.Value.InstanceUID;
                    }
                }

            }
        }
        [JsonProperty, JsonConverter(typeof(PxEnumObjectConverter), typeof(EquipSlot))]
        private Dictionary<int, long> EquippedIDs { get; set; } = new Dictionary<int, long>();

        //mostly for addons/game-specific stuff
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public Dictionary<string, object> ExtraData { get; private set; }

        [JsonIgnore]
        public bool IsPlayer => GameState.Instance.PlayerRpgState == this;

        public CharacterModel() //TODO with a model base parameter
        {
            HealthFraction = 1.0f;
            Level = 1;

            Inventory = new InventoryModel();
            Inventory.Character = this;
            Conditions = new List<Condition>();
            Equipped = new EquippedDictionaryProxy(this);
            AmmoInMagazine = new Dictionary<int, int>();
            ExtraData = new Dictionary<string, object>();

            //create blank stats and derive stats
            BaseStats = new StatsSet();
            RecalculateStats();
        }

        [OnDeserialized]
        private void HandleOnDeserialized(StreamingContext context)
        {
            Inventory.Character = this;
            RecalculateStats(); //recalculate derived stats on load
        }

        private void RecalculateStats()
        {
            //copy base stats
            DerivedStats = new StatsSet(BaseStats);

            //apply conditions
            foreach (Condition c in AllConditions)
            {
                c.ApplyToStats(BaseStats, DerivedStats);
            }

            //apply equipment bonuses (armor basically)
            foreach(var equippedItem in Equipped.Values)
            {
                if (equippedItem != null && equippedItem.ItemModel is ArmorItemModel aim)
                {
                    foreach (var key in BaseStats.DamageResistance.Keys)
                    {
                        if (aim.DamageResistance.ContainsKey((int)key))
                            DerivedStats.DamageResistance[key] = DerivedStats.DamageResistance[key] + aim.DamageResistance[(int)key];
                        if (aim.DamageThreshold.ContainsKey((int)key))
                            DerivedStats.DamageThreshold[key] = DerivedStats.DamageThreshold[key] + aim.DamageThreshold[(int)key];
                    }
                }
            }

            //apply derived skills
            if (GameParams.UseDerivedSkills)
                RpgValues.SkillsFromStats(BaseStats, DerivedStats);

            //apply conditions after skills
            foreach (Condition c in AllConditions)
            {
                c.ApplyToSkills(BaseStats, DerivedStats);
            }

            //recalculate max health, energy, and magic
            DerivedStats.MaxHealth = RpgValues.MaxHealth(this);
            DerivedStats.MaxEnergy = RpgValues.MaxEnergy(this);
            DerivedStats.MaxMagic = RpgValues.MaxMagic(this);

            //recalculate shield parameters
            DerivedStats.ShieldParams = RpgValues.ShieldParams(this);
            if (DerivedStats.ShieldParams.MaxShields <= 0)
                ShieldsFraction = 0;

            //apply conditions late (after deriving values)
            foreach (Condition c in AllConditions)
            {
                c.ApplyToDerived(BaseStats, DerivedStats);
            }

        }

        //recalculates stats and informs other systems that stats have been updated
        public void UpdateStats()
        {
            RecalculateStats();

            QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgStatsUpdated", "CharacterModel", this));

        }

        public void EquipItem(InventoryItemInstance item)
        {
            EquipItem(item, null);
        }

        public void EquipItem(InventoryItemInstance item, int? slotOverride)
        {
            if (item.Equipped)
                throw new InvalidOperationException();

            int slot = slotOverride ?? InventoryModel.GetItemSlot(item.ItemModel);

            if (slot == (int)EquipSlot.None)
                throw new InvalidOperationException();

            //unequip what was in the slot
            if (IsEquipped(slot))
                UnequipItem(Equipped[slot], false);

            //if it's a two-handed weapon, also unequip the other slot
            if (item.ItemModel is WeaponItemModel && item.ItemModel.CheckFlag("TwoHanded") && Equipped.ContainsKey(slot == (int)EquipSlot.LeftWeapon ? (int)EquipSlot.RightWeapon : (int)EquipSlot.LeftWeapon))
                UnequipItem(Equipped[slot == (int)EquipSlot.LeftWeapon ? (int)EquipSlot.RightWeapon : (int)EquipSlot.LeftWeapon], false);

            Equipped[slot] = item;

            //magazine logic
            if (item.ItemModel is RangedWeaponItemModel rwim && rwim.UseMagazine)
            {
                //var rwim = (RangedWeaponItemModel)item.ItemModel;
                AmmoInMagazine[slot] = Math.Min(rwim.MagazineSize, Inventory.CountItem(rwim.AType.ToString()));
                Inventory.RemoveItem(rwim.AType.ToString(), AmmoInMagazine[slot]);
            }

            item.Equipped = true;

            if (!string.IsNullOrEmpty(item?.ItemModel?.Scripts?.OnEquip))
                ScriptingModule.Call(item?.ItemModel?.Scripts?.OnEquip, new ScriptExecutionContext() { Caller = this }, item.ItemModel, item);

            UpdateStats();

            QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgEquipmentChanged", new Dictionary<string, object>() {
                { "Slot", slot },
                { "InventoryItemInstance", item },
                { "ChangeType", "Equip" },
                { "CharacterModel", this },
            }));
        }

        public InventoryItemInstance UnequipItem(int slot)
        {
            if (slot != (int)EquipSlot.None && Equipped.TryGetValue(slot, out var item) && item != null)
            {
                UnequipItem(item);
                return item;
            }

            return null;
        }

        public void UnequipItem(InventoryItemInstance item)
        {
            UnequipItem(item, true);
        }

        private void UnequipItem(InventoryItemInstance item, bool postMessage)
        {
            if (!item.Equipped)
                throw new InvalidOperationException();

            int slot = InventoryModel.GetItemSlot(item.ItemModel);

            if (slot != (int)EquipSlot.None)
            {
                Equipped.Remove(slot);
            }
            //allow continuing even if it's not actually equippable, for fixing bugs

            //magazine logic
            if (item.ItemModel is RangedWeaponItemModel rwim && rwim.UseMagazine)
            {
                Inventory.AddItem(rwim.AType.ToString(), AmmoInMagazine[slot]);
                AmmoInMagazine[slot] = 0;
            }

            item.Equipped = false;

            if (!string.IsNullOrEmpty(item?.ItemModel?.Scripts?.OnUnequip))
                ScriptingModule.Call(item?.ItemModel?.Scripts?.OnUnequip, new ScriptExecutionContext() { Caller = this }, item.ItemModel, item);

            UpdateStats();

            if (postMessage)
                QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgEquipmentChanged", new Dictionary<string, object>() {
                    { "Slot", slot },
                    { "InventoryItemInstance", item },
                    { "ChangeType", "Unequip" },
                    { "CharacterModel", this },
                }));
        }

        public void SetAV(string av, object value)
        {
            SetAV(av, value, null); //null=auto
        }

        public void SetAV(string av, object value, bool? propagate)
        {
            if (av.Contains("."))
            {
                string firstPart = av.Substring(0, av.IndexOf('.'));
                string secondPart = av.Substring(av.IndexOf('.') + 1);
                if (firstPart == "BaseStats")
                {
                    BaseStats.SetStat(secondPart, value);
                    if (!propagate.HasValue)
                        UpdateStats();
                }
                else if (firstPart == "DerivedStats")
                {
                    DerivedStats.SetStat(secondPart, value);
                }
                else if (firstPart == "Conditions")
                {
                    string fqConditionName = GetType().Namespace + "." + value.ToString();
                    Condition c = (Condition)Activator.CreateInstance(Type.GetType(fqConditionName));
                    Conditions.Add(c);

                    if (!propagate.HasValue)
                        UpdateStats();
                }
                else if(firstPart == "ExtraData")
                {
                    ExtraData[secondPart] = value;
                }
            }
            else
            {
                //search and set property
                var prop = GetType().GetProperty(av);
                prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType), null);
            }

            if (propagate.HasValue && propagate.Value)
                UpdateStats();
        }

        public void ModAV(string av, object value)
        {
            ModAV(av, value, null);
        }

        public void ModAV(string av, object value, bool? propagate) //nullable bool is tri-state: force propagate, force nopropagate, or default
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
                        if (c.GetType() == newCondition.GetType())
                        {
                            oldCondition = c;
                            break;
                        }
                    }

                    if (oldCondition != null)
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
                else if (firstPart == "ExtraData")
                {
                    if (ExtraData.ContainsKey(secondPart))
                        ExtraData[secondPart] = TypeUtils.AddValuesDynamic(ExtraData[secondPart], value, true);
                    else
                        ExtraData[secondPart] = value;
                }
            }
            else
            {
                //search and modify property
                var prop = GetType().GetProperty(av);
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

            if (propagate.HasValue && propagate.Value)
                UpdateStats();
        }

        public object GetAV(string av)
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
                else if (firstPart == "ExtraData")
                {
                    return ExtraData[secondPart];
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
            object untypedValue = GetAV(av);
            if (untypedValue is Condition c)
                return (T)untypedValue;

            return (T)Convert.ChangeType(untypedValue, typeof(T));            
        }

        public (float damageThreshold, float damageResistance) GetDamageThresholdAndResistance(int damageType)
        {
            if (DerivedStats == null)
                return (0, 0);

            float dt = DerivedStats.DamageThreshold.GetOrDefault(damageType, 0f);
            float dr = DerivedStats.DamageResistance.GetOrDefault(damageType, 0f);

            return (dt, dr);
        }

        public bool IsEquipped(int slot)
        {
            return (Equipped.ContainsKey(slot) && Equipped[slot] != null);
        }

        public void CheckLevelUp()
        {
            if (Experience >= RpgValues.XPToNext(Level))
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgLevelUp", "CharacterModel", this));
                if(IsPlayer)
                    QdmsMessageBus.Instance.PushBroadcast(new HUDPushMessage("<l:RPG_MESSAGE:LevelUp>", HUDPushMessageDefaultTags.StatusUpdate, "RPG"));
            }
        }

        /// <summary>
        /// Grants experience, scaled by difficulty parameter
        /// </summary>
        /// <param name="xp"></param>
        public void GrantXPScaled(int xp)
        {
            Experience += Mathf.RoundToInt(xp * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerExperience);
        }


        //player flags handling

        IEnumerable<string> IPlayerFlagsSource.Flags =>  new string[] {}; //nop for now

        int IPlayerFlagsSource.Count => 0; //nop for now

        bool IPlayerFlagsSource.Contains(string flag)
        {
            return false; //nop for now
        }


        //how equipped items are now handled
        private class EquippedDictionaryProxy : IDictionary<int, InventoryItemInstance>
        {
            private CharacterModel CharacterModel;

            public EquippedDictionaryProxy(CharacterModel characterModel)
            {
                CharacterModel = characterModel;
            }

            public InventoryItemInstance this[int key] { 
                get => CharacterModel.Inventory.GetItem(CharacterModel.EquippedIDs[key]);
                set => CharacterModel.EquippedIDs[key] = value.InstanceUID; 
            }

            public ICollection<int> Keys => CharacterModel.EquippedIDs.Keys;

            public ICollection<InventoryItemInstance> Values => CharacterModel.EquippedIDs.Values.Select(v => CharacterModel.Inventory.GetItem(v)).ToArray();

            public int Count => CharacterModel.EquippedIDs.Count;

            public bool IsReadOnly => false;

            public void Add(int key, InventoryItemInstance value)
            {
                CharacterModel.EquippedIDs.Add(key, value.InstanceUID);
            }

            public void Add(KeyValuePair<int, InventoryItemInstance> item)
            {
                CharacterModel.EquippedIDs.Add(item.Key, item.Value.InstanceUID);
            }

            public void Clear()
            {
                CharacterModel.EquippedIDs.Clear();
            }

            public bool Contains(KeyValuePair<int, InventoryItemInstance> item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(int key)
            {
                return CharacterModel.EquippedIDs.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<int, InventoryItemInstance>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<KeyValuePair<int, InventoryItemInstance>> GetEnumerator()
            {
                return CharacterModel.EquippedIDs.Select(kvp => new KeyValuePair<int, InventoryItemInstance>(kvp.Key, CharacterModel.Inventory.GetItem(kvp.Value))).GetEnumerator();
            }

            public bool Remove(int key)
            {
                return CharacterModel.EquippedIDs.Remove(key);
            }

            public bool Remove(KeyValuePair<int, InventoryItemInstance> item)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(int key, out InventoryItemInstance value)
            {
                if(CharacterModel.EquippedIDs.TryGetValue(key, out long id))
                {
                    value = CharacterModel.Inventory.GetItem(id);
                    return true;
                }
                value = default;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }


    }
}
