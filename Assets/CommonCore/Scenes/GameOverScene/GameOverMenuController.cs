using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.State;
using CommonCore;
using UnityEngine.UI;
using CommonCore.Audio;
using CommonCore.Scripting;

namespace CommonCore.UI
{

    /// <summary>
    /// Controller for the game over screen
    /// </summary>
    public class GameOverMenuController : BaseMenuController
    {
        [SerializeField, Header("Options")]
        private bool UseDirectReturn = true;
        [SerializeField]
        private bool ShowReturnButton = false;
        [SerializeField]
        private bool ShowReloadButton = false;
        [SerializeField]
        private string Music = "gameover";
        [SerializeField]
        private float MusicVolume = 1.0f;
        [SerializeField]
        private bool LoopMusic = true;
        [SerializeField]
        private string Background = "ENDGAMEPIC";

        [SerializeField, Header("References")]
        private Button ReturnButton = null;
        [SerializeField]
        private Button ReloadButton = null;
        [SerializeField]
        private RawImage BackgroundImage = null;

        public override void Start()
        {
            base.Start();

            //execute hook
            ScriptingModule.CallHooked(ScriptHook.OnGameOver, this);

            //set buttons
            if (ReturnButton != null)
            {
                ReturnButton.gameObject.SetActive(ShowReturnButton);
            }
            if (ReloadButton != null)
            {
                if (ShowReloadButton)
                {
                    ReloadButton.gameObject.SetActive(true);
                    if (!string.IsNullOrEmpty(SaveUtils.GetLastSave()))
                        ReloadButton.interactable = true;
                    else
                        ReloadButton.interactable = false;
                }
                else
                    ReloadButton.gameObject.SetActive(false);
            }

            //set music
            if (!string.IsNullOrEmpty(Music))
            {
                AudioPlayer.Instance.SetMusic(Music, MusicSlot.Ambient, MusicVolume, LoopMusic, false);
                AudioPlayer.Instance.StartMusic(MusicSlot.Ambient);
            }

            //set background
            SetBackground();
                        
        }

        private void SetBackground()
        {
            if (BackgroundImage == null || string.IsNullOrEmpty(Background))
                return;

            //attempt to load in order:
            //-sprite from UI/Backgrounds
            //-texture from UI/Backgrounds
            //-texture from DynamicTexture
            //-texture from Dialogue/bg
            Sprite spr = CoreUtils.LoadResource<Sprite>($"UI/Backgrounds/{Background}");
            if (spr != null)
            {
                BackgroundImage.texture = spr.texture;
                return;
            }
            Texture tex = CoreUtils.LoadResource<Texture2D>($"UI/Backgrounds/{Background}");
            if(tex != null)
            {
                BackgroundImage.texture = tex;
                return;
            }
            tex = CoreUtils.LoadResource<Texture2D>($"DynamicTexture/{Background}");
            if (tex != null)
            {
                BackgroundImage.texture = tex;
                return;
            }
            tex = CoreUtils.LoadResource<Texture2D>($"Dialogue/bg/{Background}"); //we do metagame a littl here
            if (tex != null)
            {
                BackgroundImage.texture = tex;
                return;
            }
        }

        public void HandleExitButtonClicked()
        {
            //clear data and continue
            SharedUtils.EndGame();
        }

        public void HandleReturnButtonClicked()
        {
            if (UseDirectReturn)
                SceneManager.LoadScene(MetaState.Instance.NextScene);
            else
                SharedUtils.ChangeScene(MetaState.Instance.NextScene);
        }

        public void HandleReloadButtonClicked()
        {
            string saveName = SaveUtils.GetLastSave();
            if(string.IsNullOrEmpty(saveName))
            {
                Modal.PushConfirmModal("There is no previous save to load", "Save Not Found", "Main Menu", "Close", null, (status, tag, result) => {
                    if (result)
                        SharedUtils.EndGame();
                });
            }
            else
            {
                SharedUtils.LoadGame(saveName, false);
            }
        }
    }
}