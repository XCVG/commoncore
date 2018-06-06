using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//TODO split this up because, I mean, damn
namespace CommonCore.Rpg
{

    public class InventoryModel
    {
        const bool AutocreateModels = true;

        private static Dictionary<string, InventoryItemModel> Models;
        private static Dictionary<string, InventoryItemDef> Defs;

        internal static void Load()
        {
            //a bit of a hack, this was originally a static constructor
            LoadAllModels();
            LoadAllDefs();
        }

        private static void LoadAllModels()
        {
            string data = Resources.Load<TextAsset>("RPGDefs/rpg_items").text;
            Models = JsonConvert.DeserializeObject<Dictionary<string, InventoryItemModel>>(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if(AutocreateModels) //this no longer makes sense for a variety of reasons
            {

                foreach (AmmoType at in Enum.GetValues(typeof(AmmoType)))
                {
                    AmmoItemModel aim = new AmmoItemModel(at.ToString(), 0, 1, 1, false, false, at);
                    Models.Add(at.ToString(), aim);
                }

                foreach(MoneyType mt in Enum.GetValues(typeof(MoneyType)))
                {
                    MoneyItemModel mim = new MoneyItemModel(mt.ToString(), 0, 1, 1, false, false, mt);
                    Models.Add(mt.ToString(), mim);
                }
            }

            //TODO: loaded models should override autocreated ones
            //TODO: switch from one big file to more smaller files
            //TODO: should tolerate malformed entries rather than crashing on one

        }

        private static void LoadAllDefs()
        {
            TextAsset ta = Resources.Load<TextAsset>("RPGDefs/rpg_items_defs");
            try
            {

                Defs = JsonConvert.DeserializeObject<Dictionary<string, InventoryItemDef>>(ta.text);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static InventoryItemModel GetModel(string name)
        {
            return Models[name];
        }

        public static InventoryItemDef GetDef(string name)
        {
            if (!Defs.ContainsKey(name))
                return null;

            return Defs[name];
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

        [Obsolete("I don't even know what GetItem was supposed to do")]
        public InventoryItemInstance[] GetItem(string item) //lack of unique keys makes this essentially useless
        {
            Debug.LogWarning("GetItem is deprecated!");

            List<InventoryItemInstance> items = new List<InventoryItemInstance>();

            foreach(InventoryItemInstance i in Items)
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

        public void AddItem(string item, int quantity)
        {
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