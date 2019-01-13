using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using CommonCore.State;
using CommonCore.DebugLog;
using CommonCore.RpgGame.Rpg; //TODO split this into dependent and non-dependent classes
using CommonCore.RpgGame.State;
using CommonCore.RpgGame.World;

namespace CommonCore.World
{
    /// <summary>
    /// Utility class for world manipulation (dependent on current game code)
    /// </summary>
    public static class GameWorldUtils //TODO move this into something sane
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


        //TODO put a generic version of these in GameUtils
        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation, bool skipLoading, string objectOverride)
        {
            MetaState.Instance.LoadingScreenPropOverride = objectOverride;
            ChangeScene(scene, spawnPoint, position, rotation, skipLoading);
        }

        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation, bool skipLoading)
        {
            MetaState.Instance.SkipLoadingScreen = skipLoading;
            ChangeScene(scene, spawnPoint, position, rotation);
        }

        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation)
        {
            MetaState mgs = MetaState.Instance;
            mgs.PreviousScene = SceneManager.GetActiveScene().name;
            mgs.NextScene = scene;
            if (spawnPoint != null)
                mgs.PlayerIntent = new PlayerSpawnIntent(spawnPoint); //handle string.Empty as default spawn point
            else
                mgs.PlayerIntent = new PlayerSpawnIntent(position, Quaternion.Euler(rotation));            
            mgs.LoadSave = null;
            mgs.TransitionType = SceneTransitionType.ChangeScene;
            WorldUtils.GetSceneController().ExitScene();
        }

        //spawn object methods


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

        //hacky inventory hack
        public static void DropItem(InventoryItemModel item, int quantity, Vector3 position)
        {
            string spawnName = "spec_item";

            if(!string.IsNullOrEmpty(item.WorldModel))
            {
                var ent = CoreUtils.LoadResource<GameObject>("entities/" + item.WorldModel); //yup, you can't check this, you have to try it
                if (ent != null)
                    spawnName = item.WorldModel;
            }

            var go = WorldUtils.SpawnObject(spawnName, "inv_drop_" + GameState.Instance.NextUID, position, Vector3.zero, null); //TODO switch to one that works with loaded prefab
            var ic = go.GetComponent<ItemController>();
            ic.ItemId = item.Name;
            ic.ItemQuantity = quantity;
        }

        //a stupid place to put this, but not as stupid as the last place
        public static float CalculateDamage(float Damage, float Pierce, float Threshold, float Resistance) //this is a dumb spot and we will move it later
        {
            float d1 = Damage * ((100f - Mathf.Min(Resistance, 99f)) / 100f);
            float dt = Mathf.Max(0, Threshold - Pierce);
            float d2 = Mathf.Max(d1 - dt, Damage * 0.1f);
            if (CoreParams.UseRandomDamage)
                d2 *= UnityEngine.Random.Range(0.75f, 1.25f);
            return d2;
        }

    }
}
