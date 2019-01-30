using CommonCore.DebugLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace CommonCore.RpgGame.Rpg
{

    public class InventoryModel
    {
        const bool AutocreateModels = true;

        private static Dictionary<string, InventoryItemModel> Models;
        private static Dictionary<string, InventoryItemDef> Defs;

        private static int LoadErrorCount;
        private static int LoadItemCount;
        private static int LoadDefCount;

        internal static void Load()
        {
            //a bit of a hack, this was originally a static constructor

            LoadErrorCount = 0;
            LoadItemCount = 0;
            LoadDefCount = 0;

            LoadAllModels();
            LoadAllDefs();
            LoadAllNew();

            CDebug.LogEx(string.Format("Loaded inventory ({0} items, {1} defs, {2} errors)", LoadItemCount, LoadDefCount, LoadErrorCount), LogLevel.Message, null);
        }

        private static void LoadAllModels()
        {
            string data = CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/rpg_items").text;
            Models = new Dictionary<string, InventoryItemModel>();

            //first autocreate models (if enabled)
            if(AutocreateModels)
            {
                CDebug.LogEx("Autocreating item models!", LogLevel.Verbose, null);
                foreach (AmmoType at in Enum.GetValues(typeof(AmmoType)))
                {
                    AmmoItemModel aim = new AmmoItemModel(at.ToString(), 0, 1, 1, false, false, null, null, at);
                    Models.Add(at.ToString(), aim);
                    LoadItemCount++;
                }
                
                foreach(MoneyType mt in Enum.GetValues(typeof(MoneyType)))
                {
                    MoneyItemModel mim = new MoneyItemModel(mt.ToString(), 0, 1, 1, false, false, null, null, mt);
                    Models.Add(mt.ToString(), mim);
                    LoadItemCount++;
                }
            }

            //then load legacy models
            CDebug.LogEx("Loading legacy item models!", LogLevel.Verbose, null);
            try
            {
                var newModels = JsonConvert.DeserializeObject<Dictionary<string, InventoryItemModel>>(data, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                newModels.ToList().ForEach(x => Models[x.Key] = x.Value);
                LoadItemCount += newModels.Count;
            }
            catch(Exception e)
            {
                CDebug.LogEx(e.ToString(), LogLevel.Verbose, null);
                LoadErrorCount++;
            }
        }

        private static void LoadAllDefs()
        {
            TextAsset ta = CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/rpg_items_defs");
            try
            {

                Defs = JsonConvert.DeserializeObject<Dictionary<string, InventoryItemDef>>(ta.text);
                LoadDefCount += Defs.Count;
            }
            catch(Exception e)
            {
                CDebug.LogError(e);
                LoadErrorCount++;
            }
        }

        private static void LoadAllNew()
        {
            //load new model/def/etc file-per-item entries
            //we've turned our data structures sideways pretty much
            //we could add more try/catch and make this absolutely bulletproof but I feel it isn't necessary
            TextAsset[] tas = CoreUtils.LoadResources<TextAsset>("Data/Items/");
            foreach(TextAsset ta in tas)
            {
                try
                {
                    JObject outerJObject = JObject.Parse(ta.text); //this contains one or more items
                    foreach(JProperty itemJProperty in outerJObject.Properties())
                    {
                        string itemName = itemJProperty.Name;
                        JToken itemJToken = itemJProperty.Value;

                        //parse model and def
                        JToken modelJToken = itemJToken["model"];
                        if(modelJToken != null)
                        {
                            Models[itemName] = JsonConvert.DeserializeObject<InventoryItemModel>(modelJToken.ToString(), new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            });
                            LoadItemCount++;
                        }

                        JToken defJToken = itemJToken["def"];
                        if(defJToken != null)
                        {
                            Defs[itemName] = JsonConvert.DeserializeObject<InventoryItemDef>(defJToken.ToString(), new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            });
                            LoadDefCount++;
                        }
                    }
                }
                catch(Exception e)
                {
                    CDebug.LogEx(e.ToString(), LogLevel.Verbose, null);
                    LoadErrorCount++;
                }
            }
        }

        public static InventoryItemModel GetModel(string name)
        {
            if (!Models.ContainsKey(name))
                return null;

            return Models[name];
        }

        public static InventoryItemDef GetDef(string name)
        {
            if (!Defs.ContainsKey(name))
                return null;

            return Defs[name];
        }

        public static string GetName(InventoryItemModel item)
        {
            var def = GetDef(item.Name);
            if (def != null)
                return def.NiceName;

            return item.Name;
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

        public static EquipSlot GetItemSlot(InventoryItemModel item)
        {
            if (item is RangedWeaponItemModel)
                return EquipSlot.RangedWeapon;
            else if (item is MeleeWeaponItemModel)
                return EquipSlot.MeleeWeapon;
            else if (item is ArmorItemModel)
                return EquipSlot.Body;
            else
                return EquipSlot.None;
        }

        [JsonProperty]
        private List<InventoryItemInstance> Items;
        
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

        public List<InventoryItemInstance> GetItemsListActual()
        {
            return Items;
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

        public bool RemoveItem(InventoryItemInstance item)
        {
            return Items.Remove(item);
        }

        public bool RemoveItem(InventoryItemInstance item, int quantity)
        {
            if(item.ItemModel.Stackable)
            {
                //reduce quantity
                item.Quantity = Math.Max(0, (item.Quantity - quantity));
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

                    Items[foundIndex].Quantity -= quantity;
                    if (Items[foundIndex].Quantity == 0)
                        Items.RemoveAt(foundIndex);
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
                    Items[foundIndex].Quantity -= 1;
                    if (Items[foundIndex].Quantity == 0)
                        Items.RemoveAt(foundIndex);
                }
                else
                {
                    Items.RemoveAt(foundIndex);
                }
                
            }
                

            return foundModel;
        }

        public void AddItem(InventoryItemInstance item)
        {
            Items.Add(item);
        }

        public void AddItem(string item, int quantity)
        {
            if (quantity <= 0)
                return;

            InventoryItemModel mdl = Models[item];

            if(mdl.Stackable)
            {
                InventoryItemInstance instance = null;
                foreach(InventoryItemInstance i in Items)
                {
                    if(i.ItemModel.Name == mdl.Name)
                    {
                        instance = i;
                        break;
                    }
                }
                if(instance == null)
                {
                    instance = new InventoryItemInstance(mdl);
                    Items.Add(instance);
                    instance.Quantity = 0;
                }
                
                instance.Quantity += quantity;
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                {
                    Items.Add(new InventoryItemInstance(mdl));
                }
            }

        }

    }
}