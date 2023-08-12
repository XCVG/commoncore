using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CommonCore;
using CommonCore.Config;

public class InitSceneController : MonoBehaviour
{
    [SerializeField, Header("Display Options")]
    private Text StatusText = null;
    [SerializeField]
    private Animator LoadingAnimator = null;

    [SerializeField, Header("Strings")]
    private string StartingText = "Starting Up";
    [SerializeField]
    private string StartedText = "Loading Menu";
    [SerializeField]
    private string FailedText = "Fatal Error";

    [SerializeField, Tooltip("Enable legacy text animation used in CommonCore 4.x and earlier"), Header("Legacy Animation")]
    private bool UseLegacyAnimation = true;
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

    private void Start()
    {
        if (StatusText != null && !string.IsNullOrEmpty(StartingText))
            StatusText.text = StartingText;

        if (LoadingAnimator != null)
            LoadingAnimator.Play("Running");
    }

    void Update ()
    {
        if(LoadingDone && !CCBase.Failed)
        {
            SceneManager.LoadScene(CCBase.LoadSceneAfterInit);
        }

		if(CCBase.Initialized)
        {
            LoadingDone = true;
            if(UseLegacyAnimation)
                LoadingText.text = "Loaded!";
            if (StatusText != null && !string.IsNullOrEmpty(StartedText))
                StatusText.text = StartedText;
            if (LoadingAnimator != null)
                LoadingAnimator.Play("Stopped");
        }
        else if(CCBase.Failed)
        {
            LoadingDone = true;
            if (UseLegacyAnimation)
                LoadingText.text = "Fatal Error!";
            if (StatusText != null && !string.IsNullOrEmpty(FailedText))
                StatusText.text = FailedText;
            if (LoadingAnimator != null)
                LoadingAnimator.Play("Stopped");
        }
	}

    private void LateUpdate()
    {
        if (LoadingDone)
            return;

        if(UseLegacyAnimation && AnimateText)
        {
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
    }

    public void OnButtonClick()
    {
        SceneManager.LoadScene(CCBase.LoadSceneAfterInit);
    }
}
