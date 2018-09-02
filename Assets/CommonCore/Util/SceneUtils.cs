using System;
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
            /*
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            var scenes = new List<Scene>(sceneCount);
            for (int i = 0; i < sceneCount; i++)
            {
                try
                {
                    scenes.Add(SceneManager.GetSceneByBuildIndex(sceneCount));
                }
                catch(Exception e)
                {
                    //ignore it, we've gone over
                }
                
            }

            var sceneNames = new List<string>(sceneCount);

            foreach(Scene s in scenes)
            {
                sceneNames.Add(s.name);
            }

            return sceneNames.ToArray();
            */

            int sceneCount = SceneManager.sceneCountInBuildSettings;
            var scenes = new List<string>(sceneCount);
            for (int i = 0; i < sceneCount; i++)
            {
                try
                {
                    scenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
                }
                catch (Exception e)
                {
                    //ignore it, we've gone over or some stupid bullshit
                }

            }

            return scenes.ToArray();
            
        }
    }
}