using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.UI;
using CommonCore.State;

namespace UI
{

    public class GameOverMenuController : BaseMenuController
    {

        public override void Start()
        {
            base.Start();

            //TODO display stats?
        }

        public void OnClickContinue()
        {
            //clear data and continue
            GameState.Reset();
            MetaState.Reset();
            GC.Collect();
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}