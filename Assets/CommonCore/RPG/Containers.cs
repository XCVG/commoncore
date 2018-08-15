
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using CommonCore.State;

namespace CommonCore.Rpg
{

    public class ContainerModel
    {

        [JsonProperty]
        private List<InventoryItemInstance> Items;

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
                    if(item.Quantity <= 0)
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

        public InventoryItemInstance[] ListItems()
        {
            return Items.ToArray();
        }
    }

    //TODO need a serializable version for editor and defs that gets populated with "real" items at runtime
}