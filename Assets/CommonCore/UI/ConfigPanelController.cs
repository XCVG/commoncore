using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CommonCore.Config;
using CommonCore.UI;

namespace CommonCore.UI
{

    public class ConfigPanelController : MonoBehaviour
    {
        public Slider LookSpeedSlider;

        public Slider GraphicsQualitySlider;
        public Text GraphicsQualityLabel;
        public Toggle AntialiasingToggle;

        public Slider SoundVolumeSlider;
        public Slider MusicVolumeSlider;
        public Dropdown ChannelDropdown;

        void OnEnable()
        {
            PaintValues();
        }

        private void PaintValues()
        {
            LookSpeedSlider.value = ConfigState.Instance.LookSpeed;

            GraphicsQualitySlider.value = ConfigState.Instance.QualityLevel;
            AntialiasingToggle.isOn = ConfigState.Instance.FxaaEnabled;

            SoundVolumeSlider.value = ConfigState.Instance.SoundVolume;
            MusicVolumeSlider.value = ConfigState.Instance.MusicVolume;

            var cList = new List<string>(Enum.GetNames(typeof(AudioSpeakerMode)));
            ChannelDropdown.AddOptions(cList);
            ChannelDropdown.value = cList.IndexOf(AudioSettings.GetConfiguration().speakerMode.ToString());
        }

        public void OnQualitySliderChanged()
        {
            GraphicsQualityLabel.text = QualitySettings.names[(int)GraphicsQualitySlider.value];
        }


        public void OnClickConfirm()
        {
            UpdateValues();
            ConfigState.Save();
            ConfigModule.Apply();
            Modal.PushMessageModal("Applied settings changes!", null, null, OnConfirmed);
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
            ConfigState.Instance.FxaaEnabled = AntialiasingToggle.isOn;

            ConfigState.Instance.SoundVolume = SoundVolumeSlider.value;
            ConfigState.Instance.MusicVolume = MusicVolumeSlider.value;
            ConfigState.Instance.SpeakerMode = (AudioSpeakerMode)Enum.Parse(typeof(AudioSpeakerMode), ChannelDropdown.options[ChannelDropdown.value].text);
        }
    }
}