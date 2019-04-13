using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CommonCore.Config;
using CommonCore.UI;
using CommonCore.Input;

namespace CommonCore.UI
{

    /// <summary>
    /// Controller for config panel
    /// </summary>
    public class ConfigPanelController : PanelController
    {
        //labels for antialiasing quality names; these match CommonCore defaults
        private static readonly string[] AntialiasingSettingNames = new string[] { "Off", "FXAA", "SMAA", "SMAA+" };

        public Slider LookSpeedSlider;

        public Slider GraphicsQualitySlider;
        public Text GraphicsQualityLabel;
        public Slider AntialiasingQualitySlider;
        public Text AntialiasingQualityLabel;

        public Slider SoundVolumeSlider;
        public Slider MusicVolumeSlider;
        public Dropdown ChannelDropdown;

        public Dropdown InputDropdown;

        public override void SignalPaint()
        {
            base.SignalPaint();

            PaintValues();
        }

        private void PaintValues()
        {
            LookSpeedSlider.value = ConfigState.Instance.LookSpeed;

            GraphicsQualitySlider.maxValue = QualitySettings.names.Length - 1;
            GraphicsQualitySlider.value = ConfigState.Instance.QualityLevel;
            AntialiasingQualitySlider.value = ConfigState.Instance.AntialiasingQuality;

            SoundVolumeSlider.value = ConfigState.Instance.SoundVolume;
            MusicVolumeSlider.value = ConfigState.Instance.MusicVolume;

            var cList = new List<string>(Enum.GetNames(typeof(AudioSpeakerMode)));
            ChannelDropdown.ClearOptions();
            ChannelDropdown.AddOptions(cList);
            ChannelDropdown.value = cList.IndexOf(AudioSettings.GetConfiguration().speakerMode.ToString());

            var iList = MappedInput.AvailableMappers.ToList();
            InputDropdown.ClearOptions();
            InputDropdown.AddOptions(iList);
            InputDropdown.value = iList.IndexOf(ConfigState.Instance.InputMapper);
        }

        public void OnQualitySliderChanged()
        {
            GraphicsQualityLabel.text = QualitySettings.names[(int)GraphicsQualitySlider.value];
        }

        public void OnAntialiasingSliderChanged()
        {
            AntialiasingQualityLabel.text = AntialiasingSettingNames[(int)AntialiasingQualitySlider.value];
        }

        public void OnClickConfirm()
        {
            UpdateValues();
            ConfigState.Save();
            ConfigModule.Apply();
            MappedInput.SetMapper(ConfigState.Instance.InputMapper); //we need to do this manually unfortunately...
            Modal.PushMessageModal("Applied settings changes!", "Changes Applied", null, OnConfirmed);
        }

        public void OnClickRevert()
        {
            Modal.PushConfirmModal("This will revert all unsaved settings changes", "Revert Changes", null, null, null, (status, tag, result) =>
            {
                if(status == ModalStatusCode.Complete && result)
                {
                    PaintValues();
                }
                else
                {

                }
            });
        }

        public void OnClickConfigureInput()
        {
            MappedInput.ConfigureMapper();
        }

        private void OnConfirmed(ModalStatusCode status, string tag)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if(sceneName == "MainMenuScene")
                SceneManager.LoadScene(sceneName);
        }

        private void UpdateValues()
        {
            ConfigState.Instance.LookSpeed = LookSpeedSlider.value;

            ConfigState.Instance.QualityLevel = (int)GraphicsQualitySlider.value;
            ConfigState.Instance.AntialiasingQuality = (int)AntialiasingQualitySlider.value;

            ConfigState.Instance.SoundVolume = SoundVolumeSlider.value;
            ConfigState.Instance.MusicVolume = MusicVolumeSlider.value;
            ConfigState.Instance.SpeakerMode = (AudioSpeakerMode)Enum.Parse(typeof(AudioSpeakerMode), ChannelDropdown.options[ChannelDropdown.value].text);
        }
    }
}