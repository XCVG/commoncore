using CommonCore.State;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CommonCore.UI
{
    public class SystemPanelController : PanelController
    {
        public Text MessageText;

        public override void SignalPaint()
        {
            
        }

        public void OnClickSave()
        {
            if(!GameState.Instance.SaveLocked)
            {
                GameState.SerializeToFile("test_save");
                MessageText.text = "Saved successfully!";
            }
            else
            {
                //can't save!
                Debug.Log("Can't save here!");
                MessageText.text = "Can't save here!";
            }
        }

        public void OnClickExit()
        {
            Time.timeScale = 1;
            //BaseSceneController.Current.("MainMenuScene");
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}