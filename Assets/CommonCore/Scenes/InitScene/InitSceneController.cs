using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CommonCore;
using CommonCore.Config;

public class InitSceneController : MonoBehaviour
{
    [SerializeField]
    private bool AnimateText = true;
    [SerializeField]
    private Text LoadingText = null;
    [SerializeField]
    private int MaxLoadingDots = 3;
    [SerializeField]
    private float LoadingDotDelay = 0.5f;

    private int LoadingDots = 0;
    private float Elapsed = 0;
    private bool LoadingDone = false;

	void Update ()
    {
		if(CCBase.Initialized)
        {
            LoadingDone = true;
            LoadingText.text = "Loaded!";
            SceneManager.LoadScene(CCBase.LoadSceneAfterInit);
        }
        else if(CCBase.Failed)
        {
            LoadingDone = true;
            LoadingText.text = "Fatal Error!";
        }
	}

    private void LateUpdate()
    {
        if (LoadingDone || !AnimateText)
            return;

        Elapsed += Time.deltaTime;

        if (Elapsed > LoadingDotDelay)
        {
            Elapsed = 0;

            //crude as heck but it'll work for now
            LoadingDots++;
            if (LoadingDots > MaxLoadingDots)
                LoadingDots = 0;
            string dots = string.Empty;
            for (int i = 0; i < LoadingDots; i++)
                dots = dots + ".";
            LoadingText.text = $"Loading{dots}";
        }
    }

    public void OnButtonClick()
    {
        SceneManager.LoadScene(CCBase.LoadSceneAfterInit);
    }
}
