using CommonCore;
using CommonCore.Config;
using CommonCore.Scripting;
using CommonCore.State;
using CommonCore.StringSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    /// <summary>
    /// Controller for the main menu scene
    /// </summary>
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
            HandleCommandLineArgs();
        }

        public override void Update()
        {
            base.Update();
        }

        public void OnClickContinue()
        {
            var save = SaveUtils.GetLastSave();

            if (save != null)
                SharedUtils.LoadGame(save, false);
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
            CoreUtils.Quit();
        }

        public void CloseCurrentPanel()
        {
            if (CurrentPanel != null)
                CurrentPanel.SetActive(false);

            CurrentPanel = null;
        }

        public void HandleCommandLineArgs()
        {
            
            try
            {
                int scriptexecIndex = CoreParams.CommandLineArgs.IndexOf("-scriptexec", StringComparison.OrdinalIgnoreCase);
                if (scriptexecIndex >= 0)
                {
                    string scriptName = CoreParams.CommandLineArgs[scriptexecIndex + 1];
                    Debug.Log($"Running script specified from command line \"{scriptName}\"");

                    int argIndex = scriptexecIndex + 2;
                    List<string> arguments = new List<string>();
                    while (argIndex < CoreParams.CommandLineArgs.Count)
                    {
                        string possibleArg = CoreParams.CommandLineArgs[argIndex];
                        if (possibleArg.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                            break;
                        arguments.Add(possibleArg);
                        argIndex++;
                    }

                    ScriptingModule.Call(scriptName, new ScriptExecutionContext() { Caller = this }, arguments.ToArray());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to run script specified in command line ({e.GetType().Name}:{e.Message})");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }

            try
            {
                int loadsaveIndex = CoreParams.CommandLineArgs.IndexOf("-loadsave", StringComparison.OrdinalIgnoreCase);
                if (loadsaveIndex >= 0)
                {
                    string saveToLoad = CoreParams.CommandLineArgs[loadsaveIndex + 1];
                    if (!CoreParams.AllowSaveLoad)
                        Debug.LogWarning($"Loading {saveToLoad} (warning: save/load is not actually enabled for this game!)");
                    else
                        Debug.Log($"Loading {saveToLoad}");
                    SharedUtils.LoadGame(saveToLoad, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load save specified in command line ({e.GetType().Name}:{e.Message})");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }
        }

    }
}