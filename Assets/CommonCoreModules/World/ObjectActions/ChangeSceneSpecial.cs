using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Change Scene Action Special
    /// </summary>
    public class ChangeSceneSpecial : ActionSpecial
    {
        public string NextScene;
        public string SpawnPoint;
        public bool UsePosition = false;
        public Vector3 SpawnPosition;
        public Vector3 SpawnRotation;
        public string TransferEffect;

        private bool Locked = false;

        public void ChangeScene(BaseController activator)
        {
            string spawnPointHack = SpawnPoint;
            if (UsePosition)
                spawnPointHack = null;
            else if (string.IsNullOrEmpty(SpawnPoint))
                spawnPointHack = string.Empty;

            if(!string.IsNullOrEmpty(TransferEffect))
            {
                var targetTransform = activator.Ref()?.transform ?? transform;
                WorldUtils.SpawnEffect(TransferEffect, targetTransform.position, targetTransform.rotation, null, false);
            }

            WorldUtils.ChangeScene(NextScene, spawnPointHack, SpawnPosition, SpawnRotation);

            if (!Repeatable)
            {
                Locked = true;
            }
        }

        public override void Execute(ActionInvokerData data)
        {
            if (!AllowInvokeWhenDisabled && !isActiveAndEnabled)
                return;

            if (Locked)
                return;

            ChangeScene(data.Activator);
        }
    }
}