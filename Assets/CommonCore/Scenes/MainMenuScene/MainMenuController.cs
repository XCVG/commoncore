using CommonCore;
using CommonCore.Scripting;
using CommonCore.UI;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
                Application.version, CoreParams.GameVersionName,
                CoreParams.VersionCode.ToString(), CoreParams.VersionName,
                Application.unityVersion);

            ScriptingModule.CallHooked(ScriptHook.AfterMainMenuCreate, this);
        }

        public override void Update()
        {
            base.Update();
        }

        public void OnClickContinue()
        {
            string savePath = CoreParams.SavePath;
            DirectoryInfo saveDInfo = new DirectoryInfo(savePath);
            FileInfo saveFInfo = saveDInfo.GetFiles().OrderBy(f => f.CreationTime).Last();

            SharedUtils.LoadGame(saveFInfo.Name);
        }

        public void OnClickNew()
        {
            MessageModal.SetActive(true);
        }      

        public void StartGame()
        {
            //start a new game the normal way
            SharedUtils.StartGame();
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