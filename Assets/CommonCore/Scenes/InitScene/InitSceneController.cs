using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CommonCore;
using CommonCore.Config;

public class InitSceneController : MonoBehaviour
{

	void Update ()
    {
		if(CCBase.Initialized)
        {
            //ConfigModule.Apply();
            GameObject.Find("Text").GetComponent<Text>().text = "Loaded!";
            SceneManager.LoadScene("MainMenuScene");
        }
	}

    public void OnButtonClick()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
