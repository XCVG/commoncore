using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.ObjectActions;
using CommonCore.RpgGame.Rpg;
using CommonCore.World;

namespace CommonCore.RpgGame.World
{
    //pretty hacky but okay
    public class ItemController : ThingController
    {
        public GameObject PickupEffect;
        public string ItemId;
        public int ItemQuantity;

        //we'll set these on the spec_item prefab as a "default"
        //overrides won't work consistently the way it's currently set up but that's okay for now
        public bool UseWalkoverPickup;
        public bool UseInteractPickup;

        public override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(ItemId) || ItemQuantity == 0)
                Debug.LogWarning(string.Format("ItemController on {0} has invalid values (id {1}, qty {2})", name, ItemId, ItemQuantity));

            if(!UseInteractPickup)
            {
                //disable interactable (the whole thing is hacky)
                var iobj = transform.Find("Interactable");
                if (iobj != null)
                    iobj.gameObject.SetActive(false);
            }
            else
            {
                //set tooltip
                var iobj = transform.Find("Interactable");
                string itemName = null;
                var itemDef = InventoryModel.GetDef(ItemId);
                if (itemDef != null)
                    itemName = itemDef.NiceName;
                else
                    itemName = ItemId;
                iobj.GetComponent<InteractableComponent>().Tooltip = string.Format("{0} [{1}]", itemName, ItemQuantity);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            OnTriggerEnter(collision.collider);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!UseWalkoverPickup)
                return;

            var pc = other.GetComponent<PlayerController>();
            if(pc != null)
            {
                GrantInventory();
            }
        }

        public void GrantInventory()
        {
            if (string.IsNullOrEmpty(ItemId) || ItemQuantity == 0)
            {
                Debug.LogError(string.Format("ItemController on {0} has invalid values (id {1}, qty {2})", name, ItemId, ItemQuantity));
                return;
            }

            GameState.Instance.PlayerRpgState.Inventory.AddItem(ItemId, ItemQuantity);

            gameObject.SetActive(false);

            if (PickupEffect != null)
                Instantiate(PickupEffect, transform.position, Quaternion.identity, transform.root);
            
        }

        public void InteractableExecute(ActionInvokerData data)
        {
            if(UseInteractPickup && data.Activator is PlayerController)
            {
                GrantInventory();
            }
        }

        //persistence
        public override void SetExtraData(Dictionary<string, object> data)
        {
            ItemId = (string)data["ItemId"];
            ItemQuantity = Convert.ToInt32(data["ItemQuantity"]);
        }
        public override Dictionary<string, object> GetExtraData()
        {
            var data = new Dictionary<string, object>();

            data.Add("ItemId", ItemId);
            data.Add("ItemQuantity", ItemQuantity);

            return data;
        }

    }
}