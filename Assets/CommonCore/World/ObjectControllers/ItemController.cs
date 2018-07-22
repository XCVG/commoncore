using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.Rpg;

namespace CommonCore.World
{
    //TODO: use-to-pickup instead of walkover-to-pickup
    //oh, it should be an interactable (or have an interactable)
    //and this should ideally be a setting (global+local override)
    public class ItemController : ThingController
    {

        public string ItemId;
        public int ItemQuantity;

        public bool GrantItem = true;

        public override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(ItemId) || ItemQuantity == 0)
                CDebug.LogWarning(string.Format("ItemController on {0} has invalid values (id {1}, qty {2})", name, ItemId, ItemQuantity));
        }

        void OnCollisionEnter(Collision collision)
        {
            OnTriggerEnter(collision.collider);
        }

        void OnTriggerEnter(Collider other)
        {
            var pc = other.GetComponent<PlayerController>();
            if(pc != null)
            {
                if (string.IsNullOrEmpty(ItemId) || ItemQuantity == 0)
                {
                    CDebug.LogError(string.Format("ItemController on {0} has invalid values (id {1}, qty {2})", name, ItemId, ItemQuantity));
                    return;
                }
                    
                GameState.Instance.PlayerRpgState.Inventory.AddItem(ItemId, ItemQuantity);

                gameObject.SetActive(false);
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