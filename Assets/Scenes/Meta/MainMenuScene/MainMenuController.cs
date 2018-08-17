using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CommonCore;
using CommonCore.State;
using CommonCore.UI;
using CommonCore.DebugLog;

namespace GameUI
{

    public class MainMenuController : BaseMenuController
    {
        [Header("Panel")]
        public GameObject CurrentPanel;
        public GameObject LoadPanel;
        public GameObject OptionsPanel;
        public GameObject HelpPanel;

        [Header("Special")]
        public GameObject MessageModal;
        public Text SystemText;

        public override void Awake()
        {
            base.Awake();
        }

        public override void Start()
        {
            base.Start();

            SystemText.text = string.Format("{0}\n{1} {2}\nCommonCore {3} {4}\nUnity {5}",
                Application.productName,
                CCParams.GameVersion, CCParams.GameVersionName,
                CCParams.VersionCode.ToString(), CCParams.VersionName,
                Application.unityVersion);
        }

        public override void Update()
        {
            base.Update();
        }

        public void OnClickContinue()
        {
            string savePath = CCParams.SavePath;
            DirectoryInfo saveDInfo = new DirectoryInfo(savePath);
            FileInfo saveFInfo = saveDInfo.GetFiles().OrderBy(f => f.CreationTime).Last();

            MetaState.Reset();
            MetaState mgs = MetaState.Instance;
            mgs.LoadSave = saveFInfo.Name;
            mgs.TransitionType = SceneTransitionType.LoadGame;

            SceneManager.LoadScene("LoadingScene");
        }

        public void OnClickNew()
        {
            MessageModal.SetActive(true);
        }      

        public void StartGame()
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
            //show load panel

            if(CurrentPanel != null)         
                CurrentPanel.SetActive(false);    

            if(CurrentPanel != LoadPanel)
            {
                CurrentPanel = LoadPanel;
                CurrentPanel.SetActive(true);
            }
            else
                CurrentPanel = null;
        }

        public void OnClickOptions()
        {
            //show options panel

            if (CurrentPanel != null)
                CurrentPanel.SetActive(false);

            if (CurrentPanel != OptionsPanel)
            {
                CurrentPanel = OptionsPanel;
                CurrentPanel.SetActive(true);
            }
            else
                CurrentPanel = null;
        }

        public void OnClickHelp()
        {
            //show help panel

            if (CurrentPanel != null)
                CurrentPanel.SetActive(false);

            if (CurrentPanel != HelpPanel)
            {
                CurrentPanel = HelpPanel;
                CurrentPanel.SetActive(true);
            }
            else
                CurrentPanel = null;
        }

        public void OnClickExit()
        {
            //cleanup will be called by hooks
            Application.Quit();
        }

    }
}