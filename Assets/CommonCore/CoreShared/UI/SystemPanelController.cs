using CommonCore.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CommonCore.UI
{
    public class SystemPanelController : PanelController
    {
        public Text MessageText;
        public GameObject ContainerPanel;
        public GameObject LoadPanel;
        public GameObject SavePanel;
        public GameObject ConfigPanel;
        public InputField SaveInputField;

        public override void SignalPaint()
        {
            HidePanels();
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

        public void OnClickActualSave()
        {
            if (!GameState.Instance.SaveLocked)
            {
                if(!string.IsNullOrEmpty(SaveInputField.text))
                {
                    BaseSceneController.Current.Commit();
                    GameState.SerializeToFile(CoreParams.SavePath + @"\" + SaveInputField.text + ".json");
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
            Time.timeScale = 1;
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
    }
}