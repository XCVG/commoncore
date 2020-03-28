using CommonCore.Config;
using CommonCore.State;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CommonCore.UI
{

    /// <summary>
    /// Controller for the "system" panel on the ingame menu. Most is now delegated to sub-panel controllers
    /// </summary>
    public class SystemPanelController : PanelController
    {
        public Text MessageText;
        public GameObject ContainerPanel;
        public GameObject LoadPanel;
        public GameObject SavePanel;
        public GameObject ConfigPanel;
        public Button SaveButton;
        public Button LoadButton;

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();            
        }

        public override void SignalPaint()
        {
            HidePanels();
            SetButtonVisibility();
        }

        public void OnClickLoad()
        {
            if(LoadPanel.activeSelf)
            {
                HidePanels();
            }
            else
            {
                HidePanels();
                LoadPanel.SetActive(true);
            }
            
        }

        public void OnClickSave()
        {
            if (!GameState.Instance.SaveLocked)
            {
                if (SavePanel.activeSelf)
                {
                    HidePanels();
                }
                else
                {
                    HidePanels();
                    SavePanel.SetActive(true);
                }
            }
            else
            {
                Modal.PushMessageModal("You can't save here!", "Saving Disabled", null, null);
            }
        }

        [Obsolete]
        public void OnClickActualSave()
        {
            throw new NotImplementedException();

            /*
            if (!GameState.Instance.SaveLocked)
            {
                if(!string.IsNullOrEmpty(SaveInputField.text))
                {
                    SharedUtils.SaveGame(SaveInputField.text, true);
                    //BaseSceneController.Current.Commit();
                    //GameState.SerializeToFile(CoreParams.SavePath + @"\" + SaveInputField.text + ".json");
                    Modal.PushMessageModal("", "Saved Successfully", null, null);
                }
                else
                {
                    Modal.PushMessageModal("You need to enter a filename!", "Save Failed", null, null);
                }
                
            }
            else
            {
                //can't save!
                
                HidePanels();
            }
            */
        }

        public void OnClickConfig()
        {
            if (ConfigPanel.activeSelf)
            {
                HidePanels();
            }
            else
            {
                HidePanels();
                ConfigPanel.SetActive(true);
            }
        }

        public void OnClickExit()
        {
            Time.timeScale = ConfigState.Instance.DefaultTimescale; //needed?
            //BaseSceneController.Current.("MainMenuScene");
            SharedUtils.EndGame();
        }

        private void HidePanels()
        {
            foreach (Transform child in ContainerPanel.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        private void SetButtonVisibility()
        {
            if(!CoreParams.AllowSaveLoad)
            {
                SaveButton.gameObject.SetActive(false);
                LoadButton.gameObject.SetActive(false);
            }
            else if(!CoreParams.AllowManualSave)
            {
                SaveButton.gameObject.SetActive(false);
            }

            if(GameState.Instance.SaveLocked || GameState.Instance.ManualSaveLocked)
            {
                SaveButton.interactable = false;
            }
            else
            {
                SaveButton.interactable = true;
            }
        }
    }
}