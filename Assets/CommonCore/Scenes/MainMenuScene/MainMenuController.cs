using CommonCore;
using CommonCore.Scripting;
using CommonCore.State;
using CommonCore.StringSub;
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

        [Header("Buttons")]
        public Button ContinueButton;
        public Button LoadButton;

        [Header("Special")]
        public GameObject MessageModal;
        public Text SystemText;

        [Header("Options"), SerializeField]
        private bool ShowMessageModal = true;

        public override void Awake()
        {
            base.Awake();
        }

        public override void Start()
        {
            base.Start();

            SystemText.text = CoreParams.GetShortSystemText();

            if(!CoreParams.AllowSaveLoad)
            {
                ContinueButton.gameObject.SetActive(false);
                LoadButton.gameObject.SetActive(false);
            }

            ScriptingModule.CallHooked(ScriptHook.AfterMainMenuCreate, this);
        }

        public override void Update()
        {
            base.Update();
        }

        public void OnClickContinue()
        {
            var save = SaveUtils.GetLastSave();

            if (save != null)
                SharedUtils.LoadGame(save);
            else
                Modal.PushMessageModal(Sub.Replace("ContinueNoSaveMessage", "IGUI_SAVE"), Sub.Replace("ContinueNoSaveHeading", "IGUI_SAVE"), null, null);
        }

        public void OnClickNew()
        {
            if (ShowMessageModal)
                MessageModal.SetActive(true);
            else
                StartGame();
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