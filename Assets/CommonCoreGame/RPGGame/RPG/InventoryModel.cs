using CommonCore.DebugLog;
using CommonCore.Messaging;
using CommonCore.Scripting;
using CommonCore.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;


namespace CommonCore.RpgGame.Rpg
{

    /// <summary>
    /// A model representing a character's inventory. Inventory item models and definitions are also handled here.
    /// </summary>
    public class InventoryModel
    {
        //combining both responsibilities here was a mistake but I'd have to rip out a lot to fix it at this point... maybe for 3.x or 4.x

        private static Dictionary<string, InventoryItemModel> Models;
        private static Dictionary<string, InventoryItemDef> Defs;

        //this is maximum jank
        private static int LoadErrorCount;
        private static int LoadItemCount;
        private static int LoadDefCount;

        /// <summary>
        /// Loads all inventory models and defs
        /// </summary>
        internal static void Load()
        {
            //a bit of a hack, this was originally a static constructor

            Models = new Dictionary<string, InventoryItemModel>();
            Defs = new Dictionary<string, InventoryItemDef>();

            LoadErrorCount = 0;
            LoadItemCount = 0;
            LoadDefCount = 0;

            LoadInventoryModels();

            CDebug.LogEx(string.Format("Loaded inventory ({0} items, {1} defs, {2} errors)", LoadItemCount, LoadDefCount, LoadErrorCount), LoadErrorCount > 0 ? LogLevel.Error : LogLevel.Message, null);
        }

        /// <summary>
        /// Loads inventory models and defs from an addon
        /// </summary>
        /// <param name="data"></param>
        internal static void LoadFromAddon(AddonLoadData data)
        {
            LoadErrorCount = 0;
            LoadItemCount = 0;
            LoadDefCount = 0;

            var assets = data.LoadedResources
                .Where(kvp => kvp.Key.StartsWith("Data/Items/"))
                .Where(kvp => kvp.Value.Resource is TextAsset)
                .Select(kvp => (TextAsset)kvp.Value.Resource);

            LoadInventoryModelsFromAssets(assets);

            CDebug.LogEx(string.Format("Loaded inventory from addon ({0} items, {1} defs, {2} errors)", LoadItemCount, LoadDefCount, LoadErrorCount), LoadErrorCount > 0 ? LogLevel.Error : LogLevel.Message, null);
        }

        /// <summary>
        /// Loads all inventory item models from individual files (new style)
        /// </summary>
        private static void LoadInventoryModels()
        {
            //load new model/def/etc file-per-item entries
            //we've turned our data structures sideways pretty much
            //we could add more try/catch and make this absolutely bulletproof but I feel it isn't necessary
            TextAsset[] tas = CoreUtils.LoadResources<TextAsset>("Data/Items/");
            LoadInventoryModelsFromAssets(tas);
        }

        private static void LoadInventoryModelsFromAssets(IEnumerable<TextAsset> inventoryAssets)
        {
            foreach (TextAsset ta in inventoryAssets)
            {
                try
                {
                    JObject outerJObject = JObject.Parse(ta.text); //this contains one or more items
                    foreach (JProperty itemJProperty in outerJObject.Properties())
                    {
                        string itemName = itemJProperty.Name;
                        JToken itemJToken = itemJProperty.Value;

                        //parse model and def
                        JToken modelJToken = itemJToken["model"];
                        if (modelJToken != null)
                        {
                            var model = JsonConvert.DeserializeObject<InventoryItemModel>(modelJToken.ToString(), CoreParams.DefaultJsonSerializerSettings);
                            model.GetType().GetProperty("Name").SetValue(model, itemName); //slight hack to set name field
                            Models[itemName] = model;
                            LoadItemCount++;
                        }

                        JToken defJToken = itemJToken["def"];
                        if (defJToken != null)
                        {
                            Defs[itemName] = JsonConvert.DeserializeObject<InventoryItemDef>(defJToken.ToString(), CoreParams.DefaultJsonSerializerSettings);
                            LoadDefCount++;
                        }
                    }
                }
                catch (Exception e)
                {
                    CDebug.LogEx("Failed to load inventory model " + ta.name, LogLevel.Verbose, null);
                    CDebug.LogEx(e.ToString(), LogLevel.Verbose, null);
                    LoadErrorCount++;
                }
            }
        }

        /// <summary>
        /// Checks if an inventory item model exists
        /// </summary>
        public static bool HasModel(string name)
        {
            return Models.ContainsKey(name);
        }

        /// <summary>
        /// Gets an inventory item model by name
        /// </summary>
        public static InventoryItemModel GetModel(string name)
        {
            if (!Models.ContainsKey(name))
                return null;

            return Models[name];
        }

        /// <summary>
        /// Adds an inventory item model
        /// </summary>
        public static void AddModel(InventoryItemModel model, bool overwrite = false)
        {
            string name = model.Name;

            if(overwrite || !Models.ContainsKey(name))
            {
                Models[name] = model;
            }
            else
            {
                throw new InvalidOperationException("A model by that name already exists");
            }
        }
        
        /// <summary>
        /// Returns an enumerable collection of inventory item models
        /// </summary>
        public static IEnumerable<InventoryItemModel> EnumerateModels()
        {
            //note we don't have to return KeyValuePair because InventoryItemModel contains the name
            return Models.Values.ToArray();
        }

        /// <summary>
        /// Checks if an inventory item definition exists
        /// </summary>
        public static bool HasDef(string name)
        {
            return Defs.ContainsKey(name);
        }

        /// <summary>
        /// Gets an inventory item definition by name
        /// </summary>
        public static InventoryItemDef GetDef(string name)
        {
            if (!Defs.ContainsKey(name))
                return null;

            return Defs[name];
        }

        /// <summary>
        /// Adds an inventory item definition
        /// </summary>
        public static void AddDef(string name, InventoryItemDef def, bool overwrite = false)
        {
            if (overwrite || !Defs.ContainsKey(name))
            {
                Defs[name] = def;
            }
            else
            {
                throw new InvalidOperationException("A definition by that name already exists");
            }
        }

        /// <summary>
        /// Returns an enumerable collection of inventory item definitions
        /// </summary>
        public static IEnumerable<KeyValuePair<string, InventoryItemDef>> EnumerateDefs()
        {
            return Defs.ToArray();
        }

        /// <summary>
        /// Gets the nice name of an item, or its plain name if the nice name isn't available
        /// </summary>
        public static string GetNiceName(InventoryItemModel item)
        {
            var def = GetDef(item.Name);
            if (def != null)
                return def.NiceName;

            return item.Name;
        }

        /// <summary>
        /// Gets the nice name of an item, or its plain name if the nice name isn't available
        /// </summary>
        public static string GetNiceName(string name)
        {
            var def = GetDef(name);
            if (def != null)
                return def.NiceName;

            return name;
        }

        public static string GetModelsList()
        {
            StringBuilder sb = new StringBuilder(Models.Count * 64);

            foreach(var kvp in Models)
            {
                sb.AppendFormat("{0}: {1} \n", kvp.Key, kvp.Value.ToString());
            }

            return sb.ToString();
        }

        public static string GetDefsList()
        {
            StringBuilder sb = new StringBuilder(Defs.Count * 64);

            foreach (var kvp in Defs)
            {
                sb.AppendFormat("{0}: {1} \n", kvp.Key, kvp.Value.ToString());
            }

            return sb.ToString();
        }

        public static int GetItemSlot(InventoryItemModel item)
        {
            //LeftWeapon isn't actually supported
            if (item is WeaponItemModel)
                return (int)EquipSlot.RightWeapon;
            else if (item is ArmorItemModel aim)
                return aim.Slot;
            else
                return (int)EquipSlot.None;
        }

        /// <summary>
        /// Assigns UIDs to inventory item instances with UID=0
        /// </summary>
        public static void AssignUIDs(IEnumerable<InventoryItemInstance> items) => AssignUIDs(items, false);

        /// <summary>
        /// Assigns UIDs to inventory item instances with UID=0
        /// </summary>
        public static void AssignUIDs(IEnumerable<InventoryItemInstance> items, bool warn)
        {
            foreach(var item in items)
            {
                if (item.InstanceUID == 0)
                {
                    item.ResetUID();
                    if (warn)
                        Debug.LogWarning($"[InventoryModel] Set UID of item {item.ItemModel?.Name} to {item.InstanceUID} (item had no UID)");
                }
            }
        }

        //actually inventory model stuff begins here

        [JsonProperty]
        private List<InventoryItemInstance> Items;

        [JsonIgnore]
        public CharacterModel Character { get; internal set; }

        public InventoryModel()
        {
            Items = new List<InventoryItemInstance>();
        }

        public int CountItem(string item)
        {
            int quantity = 0;
            foreach(InventoryItemInstance i in Items)
            {
                if (i.ItemModel.Name == item && i.Quantity == -1)
                    quantity++;
                else if (i.ItemModel.Name == item && i.Quantity > 0)
                    quantity += i.Quantity;
            }

            return quantity;
        }

        public IEnumerable<InventoryItemInstance> EnumerateItems()
        {
            return Items;
        }

        public InventoryItemInstance GetItem(long uid)
        {
            InventoryItemInstance instance = null;
            foreach (InventoryItemInstance i in Items)
            {
                if (i.InstanceUID == uid)
                {
                    instance = i;
                    break;
                }
            }
            return instance;
        }

        //like the old deprecated GetItem but better defined
        //finds all instances of a specified item model in this inventory
        public InventoryItemInstance[] FindItem(string item)
        {
            List<InventoryItemInstance> items = new List<InventoryItemInstance>();

            foreach (InventoryItemInstance i in Items)
            {
                if (i.ItemModel.Name == item)
                    items.Add(i);
            }

            return items.ToArray();
        }

        private InventoryItemInstance FindFirstItem(string item)
        {
            InventoryItemInstance instance = null;
            foreach (InventoryItemInstance i in Items)
            {
                if (i.ItemModel.Name == item)
                {
                    instance = i;
                    break;
                }
            }
            return instance;
        }

        public bool RemoveItem(long uid)
        {
            InventoryItemInstance instance = GetItem(uid);
            if (instance == null)
                return false;
            return RemoveItem(instance);
        }

        public bool RemoveItem(long uid, int quantity)
        {
            InventoryItemInstance instance = GetItem(uid);
            if (instance == null)
                return false;
            return RemoveItem(instance, quantity);
        }

        public bool RemoveItem(InventoryItemInstance item)
        {
            CallOnRemove(item);
            return Items.Remove(item);
        }

        public bool RemoveItem(InventoryItemInstance item, int quantity)
        {
            if(item.ItemModel.Stackable)
            {
                //reduce quantity
                int oldQuantity = item.Quantity;
                item.Quantity = Math.Max(0, (item.Quantity - quantity));
                CallOnQuantityChanged(item, oldQuantity);
                if (item.Quantity == 0)
                    return RemoveItem(item);
                return true;
            }
            else
            {
                if (quantity == 1)
                    return RemoveItem(item);
                else
                    return false;
            }
        }

        //very limited, only useful for stacked items
        public bool RemoveItem(string item, int quantity)
        {
            var items = FindItem(item);
            if (items.Length != 1)
                return false;
            return RemoveItem(items[0], quantity);
        }

        public InventoryItemModel UseItem(string item, int quantity)
        {
            int foundIndex = -1;
            InventoryItemModel foundModel = null;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ItemModel.Name == item)
                {
                    foundIndex = i;
                    foundModel = Items[i].ItemModel;
                    break;
                }
            }
            if (foundIndex >= 0)
            {
                if (foundModel.Stackable)
                {
                    if (Items[foundIndex].Quantity < quantity)
                        throw new InvalidOperationException();

                    int oldQuantity = Items[foundIndex].Quantity;
                    Items[foundIndex].Quantity -= quantity;
                    CallOnQuantityChanged(Items[foundIndex], oldQuantity);
                    if (Items[foundIndex].Quantity == 0)
                    {
                        CallOnRemove(Items[foundIndex]);
                        Items.RemoveAt(foundIndex);
                    }
                }
                else
                {
                    if (quantity > 1)
                    {
                        //TODO fuck this is horrible
                        for(int j = 0; j < quantity; j++)
                        {
                            UseItem(item);
                        }
                    }
                    else
                    {
                        CallOnRemove(Items[foundIndex]);
                        Items.RemoveAt(foundIndex);
                    }

                    
                }

            }


            return foundModel;
        }

        public InventoryItemModel UseItem(string item)
        {
            //search list for first instance
            int foundIndex = -1;
            InventoryItemModel foundModel = null;
            for(int i = 0; i < Items.Count; i++)
            {
                if(Items[i].ItemModel.Name == item)
                {
                    foundIndex = i;
                    foundModel = Items[i].ItemModel;
                    break;
                }
            }
            if(foundIndex >= 0)
            {
                if(foundModel.Stackable)
                {
                    int oldQuantity = Items[foundIndex].Quantity;
                    Items[foundIndex].Quantity -= 1;
                    CallOnQuantityChanged(Items[foundIndex], oldQuantity);
                    if (Items[foundIndex].Quantity == 0)
                    {
                        CallOnRemove(Items[foundIndex]);
                        Items.RemoveAt(foundIndex);
                    }
                }
                else
                {
                    CallOnRemove(Items[foundIndex]);
                    Items.RemoveAt(foundIndex);
                }
                
            }
                

            return foundModel;
        }

        /// <summary>
        /// Adds a stacked item up to the quantity limit.
        /// </summary>
        /// <remarks>
        /// <para>Will create or return a new item instance and modify the one passed in</para>
        /// <para>This is a really weirdly specific one and probably only used for containers</para>
        /// </remarks>
        /// <returns>The instance of the item actually in the inventory</returns>
        public InventoryItemInstance AddItemsToQuantityLimit(InventoryItemInstance item)
        {
            if(!item.ItemModel.Stackable)
            {
                throw new InvalidOperationException("Non-stackable items cannot be added with this API, use AddItemIfPossible instead!");
            }

            InventoryItemInstance instance = FindFirstItem(item.ItemModel.Name);
            if (instance == null)
            {
                instance = new InventoryItemInstance(item.ItemModel);
                Items.Add(instance);
                CallOnAdd(instance);
                instance.Quantity = Math.Min(item.Quantity, item.ItemModel.MaxQuantity);
                CallOnQuantityChanged(instance, 0);
                int quantityRemaining = item.Quantity - instance.Quantity;
                item.Quantity = quantityRemaining;
            }
            else
            {
                int quantityToAdd = Math.Min(item.Quantity, item.ItemModel.MaxQuantity - instance.Quantity);
                int oldQuantity = instance.Quantity;
                instance.Quantity += quantityToAdd;
                CallOnQuantityChanged(instance, oldQuantity);
                int quantityRemaining = item.Quantity - quantityToAdd;
                item.Quantity = quantityRemaining;
            }

            return instance;

        }

        /// <summary>
        /// Adds a non-stackable item if it is possible to do so while respecting quantity limits
        /// </summary>
        public bool AddItemIfPossible(InventoryItemInstance item)
        {
            if (item.ItemModel.Stackable)
            {
                throw new InvalidOperationException("Stackable items cannot be added with this API, use AddItemsToQuantityLimit instead!");                
            }

            if (item.ItemModel.MaxQuantity <= 0)
            {
                AddItem(item);
                return true;
            }

            int numItem = CountItem(item.ItemModel.Name);
            if(numItem < item.ItemModel.MaxQuantity)
            {
                AddItem(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds an item up to the quantity limit specified by MaxQuantity of the item model.
        /// </summary>
        /// <returns>The quantity remaining after adding the item (MaxQuantity - existing)</returns>
        public int AddItemsToQuantityLimit(string item)
        {
            int quantity = GetModel(item).MaxQuantity;
            return AddItemsToQuantityLimit(item, quantity);
        }

        /// <summary>
        /// Adds an item up to the quantity limit.
        /// </summary>
        /// <returns>The quantity remaining after adding the item (quantity - existing)</returns>
        public int AddItemsToQuantityLimit(string item, int quantity)
        {
            AddItem(item, quantity, true, out var quantityRemaining);
            return quantityRemaining;
        }

        /// <summary>
        /// Adds item directly. Does not enforce stacking or quantity limits.
        /// </summary>
        public void AddItem(InventoryItemInstance item)
        {
            CallOnAdd(item);
            Items.Add(item);
        }

        /// <summary>
        /// Adds a quantity of an item, stacking but not enforcing quantity limits
        /// </summary>
        public void AddItem(string item, int quantity)
        {
            AddItem(item, quantity, false, out _);

        }

        private void AddItem(string item, int quantity, bool enforceQuantityLimit, out int quantityRemaining)
        {
            quantityRemaining = 0;

            if (quantity <= 0)
                return;

            InventoryItemModel mdl = Models[item];

            enforceQuantityLimit &= mdl.MaxQuantity > 0;

            if (enforceQuantityLimit)
            {
                int existingQuantity = CountItem(item);
                int quantityToAdd = Math.Min(quantity, mdl.MaxQuantity - existingQuantity);
                quantityRemaining = quantity - quantityToAdd;
                quantity = quantityToAdd;
            }

            if (mdl.Stackable)
            {
                InventoryItemInstance instance = FindFirstItem(mdl.Name);
                if (instance == null)
                {
                    instance = new InventoryItemInstance(mdl);
                    Items.Add(instance);
                    CallOnAdd(instance);
                    instance.Quantity = 0;
                }
                int oldQuantity = instance.Quantity;

                instance.Quantity += quantity;
                CallOnQuantityChanged(instance, oldQuantity);
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                {
                    var instance = new InventoryItemInstance(mdl);
                    Items.Add(instance);
                    CallOnAdd(instance);
                }
            }
        }

        //scripting handling

        private void CallOnQuantityChanged(InventoryItemInstance instance, int oldQuantity, int? newQuantity = null)
        {
            if(!instance.ItemModel.Stackable)
            {
                Debug.LogError($"Tried to call OnQuantityChange script \"{instance.ItemModel.Scripts.OnQuantityChange}\" on non-stackable model \"{instance.ItemModel}\"");
                return;
            }

            QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgInventoryModified", new Dictionary<string, object>() {
                { "ChangeType", "QuantityChanged" },
                { "InventoryModel", this },
                { "InventoryItemInstance", instance },
                { "InventoryItemModel", instance.ItemModel },
                { "OldQuantity", oldQuantity },
                { "NewQuantity", newQuantity ?? instance.Quantity },
            }));

            if (string.IsNullOrEmpty(instance?.ItemModel?.Scripts?.OnQuantityChange))
                return;

            ScriptingModule.Call(instance.ItemModel.Scripts.OnQuantityChange, new ScriptExecutionContext() { Caller = this }, instance.ItemModel, instance, oldQuantity, newQuantity ?? instance.Quantity);
        }

        private void CallOnAdd(InventoryItemInstance instance)
        {
            QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgInventoryModified", new Dictionary<string, object>() {
                { "ChangeType", "Add" },
                { "InventoryModel", this },
                { "InventoryItemInstance", instance },
                { "InventoryItemModel", instance.ItemModel }
            }));

            if (string.IsNullOrEmpty(instance?.ItemModel?.Scripts?.OnAdd))
                return;
            
            ScriptingModule.Call(instance.ItemModel.Scripts.OnAdd, new ScriptExecutionContext() { Caller = this }, instance.ItemModel, instance);
        }

        private void CallOnRemove(InventoryItemInstance instance)
        {
            QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgInventoryModified", new Dictionary<string, object>() {
                { "ChangeType", "Remove" },
                { "InventoryModel", this },
                { "InventoryItemInstance", instance },
                { "InventoryItemModel", instance.ItemModel }
            }));

            if (string.IsNullOrEmpty(instance?.ItemModel?.Scripts?.OnRemove))
                return;
            
            ScriptingModule.Call(instance.ItemModel.Scripts.OnRemove, new ScriptExecutionContext() { Caller = this }, instance.ItemModel, instance);
        }

    }
}