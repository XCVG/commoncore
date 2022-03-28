using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.ObjectActions;
using CommonCore.World;

namespace CommonCore.RpgGame.ObjectActions
{

    /// <summary>
    /// Change Scene Action Special
    /// </summary>
    /// <remarks>
    /// <para>Probably should not be triggerable by things other than player because it always affects player</para>
    /// <para>Adapted from DoorInteractableComponent</para>
    /// </remarks>
    public class ChangeSceneSpecial : ActionSpecial
    {
        public string Scene;
        public string SpawnPoint;
        public bool UsePosition = false;
        public Vector3 Position;
        public Vector3 Rotation;

        private bool Locked = false;

        public override void Execute(ActionInvokerData data)
        {
            if (!Locked)
            {
                string spawnPointHack = SpawnPoint;
                if (UsePosition)
                    spawnPointHack = null;
                else if (string.IsNullOrEmpty(SpawnPoint))
                    spawnPointHack = string.Empty;

                WorldUtils.ChangeScene(Scene, spawnPointHack, Position, Rotation);
            }

            if(!Repeatable)
            {
                Locked = true;
            }
        }
    }
}


