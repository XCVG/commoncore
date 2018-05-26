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
        public Vector3 SpawnPosition;
        public Vector3 SpawnRotation;

        public void ChangeScene()
        {
            WorldUtils.ChangeScene(NextScene, SpawnPoint, SpawnPosition, SpawnRotation);
        }

        public override void Execute(ActionInvokerData data)
        {
            ChangeScene();
        }
    }
}