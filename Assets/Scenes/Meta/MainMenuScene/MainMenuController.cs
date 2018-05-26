using CommonCore.State;
using CommonCore.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{

    public class MainMenuController : BaseMenuController
    {

        public override void Awake()
        {
            base.Awake();
        }

        // Use this for initialization
        public override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
        }

        public void OnClickContinue()
        {
            //for debugging purposes, load test scene directly
            SceneManager.LoadScene("TestScene");
        }

        public void OnClickNew()
        {
            //start a new game the normal way
            MetaState mgs = MetaState.Instance;
            mgs.Intents = new List<Intent>();
            mgs.LoadSave = null;
            mgs.NextScene = null;
            mgs.TransitionType = SceneTransitionType.NewGame;
            SceneManager.LoadScene("LoadingScene");
        }

        public void OnClickLoad()
        {
            //TODO show load panel
        }

        public void OnClickOptions()
        {
            //TODO show options panel
        }

        public void OnClickHelp()
        {
            //TODO show help panel
        }

        public void OnClickExit()
        {
            //cleanup will be called by hooks
            Application.Quit();
        }

    }
}