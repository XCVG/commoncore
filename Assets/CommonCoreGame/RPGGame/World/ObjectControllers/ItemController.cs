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
        public string PickupEffectName;
        public string DestroyEffectName;
        public string ItemId;
        public int ItemQuantity;
        public bool UseQuantityLimit = false;
        public bool AlwaysDestroyOnPickup = false;

        //we'll set these on the spec_item prefab as a "default"
        //overrides won't work consistently the way it's currently set up but that's okay for now
        public bool UseWalkoverPickup;
        public bool UseInteractPickup;

        protected override bool DeferComponentInitToSubclass => true;

        public override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(ItemId) || ItemQuantity == 0)
                Debug.LogWarning(string.Format("ItemController on {0} has invalid values (id {1}, qty {2})", name, ItemId, ItemQuantity));

            if(!UseInteractPickup)
            {
                //disable interactable (the whole thing is hacky)
                var iobj = transform.GetComponentInChildren<InteractableComponent>().Ref()?.gameObject;
                if (iobj != null)
                    iobj.gameObject.SetActive(false);
            }
            else
            {
                //set tooltip
                var iobj = transform.GetComponentInChildren<InteractableComponent>().Ref()?.gameObject;
                string itemName = null;
                var itemDef = InventoryModel.GetDef(ItemId);
                if (itemDef != null)
                    itemName = itemDef.NiceName;
                else
                    itemName = ItemId;
                iobj.GetComponent<InteractableComponent>().Tooltip = ItemQuantity > 1 ? string.Format("{0} [{1}]", itemName, ItemQuantity) : itemName;
            }

            TryExecuteOnComponents(component => component.Init(this));
            Initialized = true;
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

            if(UseQuantityLimit)
            {
                int remaining = GameState.Instance.PlayerRpgState.Inventory.AddItemsToQuantityLimit(ItemId, ItemQuantity);
                ItemQuantity = remaining;
            }
            else
            {
                GameState.Instance.PlayerRpgState.Inventory.AddItem(ItemId, ItemQuantity);
                ItemQuantity = 0;
            }

            if(AlwaysDestroyOnPickup || ItemQuantity == 0)
            {
                if (!string.IsNullOrEmpty(DestroyEffectName))
                    WorldUtils.SpawnEffect(DestroyEffectName, transform.position, transform.rotation, null, false);

                gameObject.SetActive(false);
            }
            

            if (PickupEffect != null)
                Instantiate(PickupEffect, transform.position, Quaternion.identity, transform.root);
            if (!string.IsNullOrEmpty(PickupEffectName))
                WorldUtils.SpawnEffect(PickupEffectName, transform.position, transform.rotation, null, false);
            
        }

        public void InteractableExecute(ActionInvokerData data)
        {
            if(UseInteractPickup && data.Activator is PlayerController)
            {
                GrantInventory();
            }
        }

        //persistence
        public override void RestoreEntityData(Dictionary<string, object> data)
        {
            base.RestoreEntityData(data);
            ItemId = (string)data["ItemId"];
            ItemQuantity = Convert.ToInt32(data["ItemQuantity"]);
        }
        public override Dictionary<string, object> CommitEntityData()
        {
            var data = base.CommitEntityData();

            data.Add("ItemId", ItemId);
            data.Add("ItemQuantity", ItemQuantity);

            return data;
        }

    }
}