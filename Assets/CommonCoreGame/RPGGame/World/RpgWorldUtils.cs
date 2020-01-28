using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using CommonCore.State;
using CommonCore.DebugLog;
using CommonCore.World;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.State;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Utility class for world/object manipulation specific to RpgGame package
    /// </summary>
    public static class RpgWorldUtils //TODO try to find a better name
    {
        /// <summary>
        /// Gets the current player controller
        /// </summary>
        /// <returns></returns>
        public static PlayerController GetPlayerController()
        {
            //TODO split into Get and TryGet

            PlayerController pc = WorldUtils.GetPlayerObject()?.GetComponent<PlayerController>(); //should be safe because GetPlayerObject returns true null
            if (pc != null)
            {
                return pc;
            }
            else
            {
                Debug.LogWarning("Couldn't find PlayerController!");
                return null;
            }
        }

        /// <summary>
        /// Changes the scene, overriding the prop shown on the loading screen (see <see cref="WorldUtils.ChangeScene(string, string, Vector3, Vector3, bool)"/>)
        /// </summary>
        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation, bool skipLoading, string objectOverride)
        {
            //MetaState.Instance.LoadingScreenPropOverride = objectOverride;
            WorldUtils.ChangeScene(scene, spawnPoint, position, rotation, skipLoading);
        }

        /// <summary>
        /// Checks if a target is "alive"- exists and had nonzero HP
        /// </summary>
        public static bool TargetIsAlive(Transform target)
        {
            if (target == null)
                return false;

            var bc = target.GetComponent<BaseController>();
            if (bc == null)
                return target.gameObject.activeInHierarchy;

            bool healthPass = true;
            if (bc is PlayerController)
                healthPass = GameState.Instance.PlayerRpgState.HealthFraction > 0;
            else if (bc is ActorController)
                healthPass = ((ActorController)bc).Health > 0;

            return target.gameObject.activeInHierarchy && healthPass;
        }

        /// <summary>
        /// HACK "drops" an inventory item; creates a spec_item object with that item
        /// </summary>
        public static void DropItem(InventoryItemModel item, int quantity, Vector3 position)
        {
            string spawnName = "spec_item";

            if(!string.IsNullOrEmpty(item.WorldModel))
            {
                var ent = CoreUtils.LoadResource<GameObject>("Entities/" + item.WorldModel); //yup, you can't check this, you have to try it
                if (ent != null)
                    spawnName = item.WorldModel;
            }

            var go = WorldUtils.SpawnEntity(spawnName, "inv_drop_" + GameState.Instance.NextUID, position, Vector3.zero, null); //TODO switch to one that works with loaded prefab
            var ic = go.GetComponent<ItemController>();
            ic.ItemId = item.Name;
            ic.ItemQuantity = quantity;
        }

        

    }
}
