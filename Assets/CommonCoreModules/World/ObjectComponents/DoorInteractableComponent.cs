using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    public class DoorInteractableComponent : InteractableComponent
    {
        public string Scene;
        public string SpawnPoint;
        public bool UsePosition = false;
        public Vector3 Position;
        public Vector3 Rotation;

        public string TransferEffect;

        public override void OnActivate(GameObject activator)
        {
            if (CheckEligibility(activator))
            {
                string spawnPointHack = SpawnPoint;
                if (UsePosition)
                    spawnPointHack = null;
                else if (string.IsNullOrEmpty(SpawnPoint))
                    spawnPointHack = string.Empty;

                if (!string.IsNullOrEmpty(TransferEffect))
                {
                    var targetTransform = activator.Ref()?.transform ?? transform;
                    WorldUtils.SpawnEffect(TransferEffect, targetTransform.position, targetTransform.rotation, null, false);
                }

                WorldUtils.ChangeScene(Scene, spawnPointHack, Position, Rotation);
            }
        }
    }
}