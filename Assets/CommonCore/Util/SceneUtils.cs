using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore
{

    public class SceneUtils
    {

        public static string[] GetSceneList()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            var scenes = new Scene[sceneCount];
            for (int i = 0; i < sceneCount; i++)
            {
                scenes[i] = SceneManager.GetSceneByBuildIndex(sceneCount);
            }

            var sceneNames = new string[sceneCount];

            for(int i = 0; i < sceneCount; i++)
            {
                sceneNames[i] = scenes[i].name;
            }

            return sceneNames;
        }
    }
}