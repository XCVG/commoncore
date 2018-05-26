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
        public Vector3 Position;
        public Vector3 Rotation;

        public override void OnActivate(GameObject activator)
        {
            if (CheckEligibility(activator))
            {
                WorldUtils.ChangeScene(Scene, SpawnPoint, Position, Rotation);
            }
        }
    }
}