using CommonCore.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Placeholder that will spawn an entity by name at run time
    /// </summary>
    public class EntityPlaceholder : MonoBehaviour, IPlaceholderComponent
    {

        [SerializeField, Tooltip("The entity to spawn")]
        private string FormID;
        [SerializeField, Tooltip("TID, leave empty to use auto TID or placeholder's TID")]
        private string ThingID;
        [SerializeField, Tooltip("If set and TID is empty, uses this placeholder's TID instead of auto TID")]
        private bool UsePlaceholderTID = true;

        [SerializeField]
        private bool DestroyPlaceholder = true;

        private bool SpawnedEntity = false;

        private void Start()
        {
            if(!SpawnedEntity)
                SpawnEntity();
        }

        public void SpawnEntity()
        {
            if (SpawnedEntity)
                return;

            //check for warning conditions
            if(string.IsNullOrEmpty(FormID))
            {
                Debug.LogError($"EntityPlaceholder on {gameObject.name}: No entity defined!");
                return;
            }

            if(string.IsNullOrEmpty(ThingID) && UsePlaceholderTID && !DestroyPlaceholder)
            {
                Debug.LogWarning($"EntityPlaceholder on {gameObject.name}: No TID given, and set to use placeholder TID without destroying placeholder (will result in objects with same TID!)");
            }

            string tid = string.IsNullOrEmpty(ThingID) ? (UsePlaceholderTID ? gameObject.name : null) : ThingID;
            try
            {
                //this looks like archaic C style error checking
                if (WorldUtils.SpawnEntity(FormID, tid, transform.position, transform.eulerAngles, transform.parent) == null)
                    Debug.LogError($"EntityPlaceholder on {gameObject.name}: Failed to spawn entity (unknown)");
            }
            catch(Exception e)
            {
                Debug.LogError($"EntityPlaceholder on {gameObject.name}: Failed to spawn entity ({e.GetType().Name})");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }

            SpawnedEntity = true;

            if (DestroyPlaceholder)
                Destroy(this.gameObject);
        }
    }
}