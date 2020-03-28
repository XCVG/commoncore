using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using CommonCore.World;

namespace CommonCore.ObjectActions
{
    public class ChangeSceneScript : ActionSpecial
    {
        public string NextScene;
        public string SpawnPoint;
        public bool UsePosition = false;
        public Vector3 SpawnPosition;
        public Vector3 SpawnRotation;

        public void ChangeScene()
        {
            string spawnPointHack = SpawnPoint;
            if (UsePosition)
                spawnPointHack = null;
            else if (string.IsNullOrEmpty(SpawnPoint))
                spawnPointHack = string.Empty;

            WorldUtils.ChangeScene(NextScene, spawnPointHack, SpawnPosition, SpawnRotation);
        }

        public override void Execute(ActionInvokerData data)
        {
            if (!AllowInvokeWhenDisabled && !isActiveAndEnabled)
                return;

            ChangeScene();
        }
    }
}