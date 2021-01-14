using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using CommonCore.State;
using CommonCore.DebugLog;

namespace CommonCore.RpgGame.Rpg
{

    public class ContainerModel
    {

        [JsonProperty]
        private List<InventoryItemInstance> Items;
        [JsonProperty]
        public bool EnforceQuantityLimits { get; set; } //note this enforces quantity limits _on the player_, not on the container
        [JsonProperty]
        public bool TakeOnly { get; set; }

        public ContainerModel()
        {
            Items = new List<InventoryItemInstance>();
        }

        //returns found instance
        public InventoryItemInstance TakeItem(InventoryItemModel item)
        {
            InventoryItemInstance foundInstance = FindItem(item);
            if (foundInstance == null)
                return null;

            Items.Remove(foundInstance);
            return foundInstance;
        }
        //this is arguably useless actually but I'm drunk and tired

        private InventoryItemInstance FindItem(InventoryItemModel item)
        {
            InventoryItemInstance foundInstance = null;

            foreach (InventoryItemInstance i in Items)
            {
                if (i.ItemModel == item)
                {
                    foundInstance = i;
                    break;
                }
            }

            return foundInstance;
        }

        /*
        //returns quantity taken, or -1 for failure
        public int TakeItem(InventoryItemModel item, int quantity)
        {
            if (quantity == 1)
                return TakeItem(item) == null ? -1 : 1;
            else if (quantity <= 0)
                return -1;
            else
            {
                //handle stacks
                if (!item.Stackable)
                    return -1;

                InventoryItemInstance foundInstance = FindItem(item);


            }
        }
        */
        //started writing that, realized it had no use case and was stupid

        public bool TakeItem(InventoryItemInstance item)
        {
            if (Items.Contains(item))
            {
                Items.Remove(item);
                return true;
            }
            else
                return false;

        }

        public int TakeItem(InventoryItemInstance item, int quantity)
        {
            if (quantity == 1)
                return TakeItem(item) ? -1 : 1;
            else if (quantity <= 0)
                return -1;
            else
            {
                if (Items.Contains(item) && item.ItemModel.Stackable) //check that it actually exists...
                {
                    int origQuantity = item.Quantity; //there's a smarter way but I'm too tired
                    item.Quantity -= quantity;
                    if (item.Quantity <= 0)
                    {
                        Items.Remove(item);
                    }
                    if (item.Quantity < 0)
                    {
                        return origQuantity;
                    }
                    else return quantity;

                }
                else
                    return -1;
            }
        }

        public bool PutItem(InventoryItemInstance item)
        {
            if (Items.Contains(item))
                return false;

            Items.Add(item);
            return true;
        }

        public int PutItem(InventoryItemInstance item, int quantity)
        {
            if (quantity == 1)
                return PutItem(item) ? -1 : 1;
            else if (quantity <= 0)
                return -1;
            else if (item.ItemModel.Stackable)
            {
                var fItem = Items.Find(x => x.ItemModel == item.ItemModel);

                if (fItem != null) //check that it actually exists...
                {
                    fItem.Quantity += quantity;

                    return quantity;
                }
                else //otherwise create a new one
                {
                    var newItem = new InventoryItemInstance(item.ItemModel);
                    newItem.Quantity = quantity;
                    Items.Add(newItem);

                    return quantity;
                }
                    
            }
            else
                return -1;
        }

        public InventoryItemInstance[] ListItems()
        {
            return Items.ToArray();
        }

        public List<InventoryItemInstance> GetItemsListActual()
        {
            return Items;
        }

        public int CountItem(string item)
        {
            int quantity = 0;
            foreach (InventoryItemInstance i in Items)
            {
                if (i.ItemModel.Name == item && i.Quantity == -1)
                    quantity++;
                else if (i.ItemModel.Name == item && i.Quantity > 0)
                    quantity += i.Quantity;
            }

            return quantity;
        }

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

        public void FixStacks()
        {
            //TODO implement stack consolidation
        }

    }

    //need a serializable version for editor and defs that gets populated with "real" items at runtime

    [System.Serializable]
    public class SerializableItemInstance
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long InstanceUID = 0;
        [JsonProperty(NullValueHandling= NullValueHandling.Ignore)]
        public int Quantity = 1;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float Condition = 1.0f;
        public string ItemModel;

        public static InventoryItemInstance MakeItemInstance(SerializableItemInstance sItemInstance)
        {
            InventoryItemModel model = InventoryModel.GetModel(sItemInstance.ItemModel);

            if (model == null)
            {
                CDebug.LogEx(string.Format("Couldn't find model {0} for SerializableItemInstance", sItemInstance.ItemModel), LogLevel.Error, sItemInstance);
                return null;
            }
                

            InventoryItemInstance rItemInstance = new InventoryItemInstance(model, sItemInstance.InstanceUID, sItemInstance.Condition, sItemInstance.Quantity, false);

            if (rItemInstance.InstanceUID == 0)
                rItemInstance.ResetUID();

            return rItemInstance;
        }

        public static SerializableItemInstance MakeSerializableItemInstance(InventoryItemInstance rItemInstance)
        {
            var sItemInstance = new SerializableItemInstance();
            sItemInstance.InstanceUID = rItemInstance.InstanceUID;
            sItemInstance.Quantity = rItemInstance.Quantity;
            sItemInstance.Condition = rItemInstance.Condition;
            sItemInstance.ItemModel = rItemInstance.ItemModel.Name;
            return sItemInstance;
        }
    }

    [System.Serializable]
    public class SerializableContainerModel
    {
        public SerializableItemInstance[] Items;
        public bool EnforceQuantityLimits;
        public bool TakeOnly;

        public static ContainerModel MakeContainerModel(SerializableContainerModel sContainerModel)
        {
            ContainerModel rContainerModel = new ContainerModel();
            rContainerModel.EnforceQuantityLimits = sContainerModel.EnforceQuantityLimits;
            rContainerModel.TakeOnly = sContainerModel.TakeOnly;

            foreach(SerializableItemInstance sItemInstance in sContainerModel.Items)
            {
                InventoryItemInstance rItemInstance = SerializableItemInstance.MakeItemInstance(sItemInstance);
                if(rItemInstance != null)
                {
                    rContainerModel.PutItem(rItemInstance);
                }
                else
                {
                    CDebug.LogEx(string.Format("Couldn't create real item instance for item in container"), LogLevel.Error, sContainerModel);
                }
            }

            return rContainerModel;
        }

        public static SerializableContainerModel MakeSerializableContainerModel(ContainerModel rContainerModel)
        {
            var rItems = rContainerModel.ListItems();
            SerializableItemInstance[] sItems = new SerializableItemInstance[rItems.Length];
            for(int i = 0; i < rItems.Length; i++)
            {
                sItems[i] = SerializableItemInstance.MakeSerializableItemInstance(rItems[i]);                
            }
            var sContainerModel = new SerializableContainerModel();
            sContainerModel.Items = sItems;
            sContainerModel.EnforceQuantityLimits = rContainerModel.EnforceQuantityLimits;
            sContainerModel.TakeOnly = rContainerModel.TakeOnly;
            return sContainerModel;
        }
    }
}