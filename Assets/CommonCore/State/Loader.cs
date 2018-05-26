using System;
using UnityEngine;

namespace CommonCore.State
{
    //temporary helper class from ARES; will refactor it completely out
    //TODO: move into LoadingSceneController?
    public static class Loader
    {

        public static void CreateNewGame()
        {
            Debug.Log("Creating a new game!"); //doesn't do anything yet
            //TODO: initialize gamestate here
            //for debugging:
            MetaState.Instance.NextScene = "World_Ext_TestIsland";
        }

        public static void LoadGame()
        {
            Debug.Log("Loading game: " + MetaState.Instance.LoadSave);
        }

        public static void LoadSceneData()
        {
            //I guess we can put this here
        }

    }

}