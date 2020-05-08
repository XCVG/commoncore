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
using CommonCore.StringSub;

namespace CommonCore.UI
{

    /// <summary>
    /// Controller for config panel
    /// </summary>
    public class ConfigPanelController : PanelController
    {

        [Header("Containers"), SerializeField]
        private RectTransform PanelContainer = null;

        [Header("Input")]
        public Dropdown InputDropdown;
        public Button ConfigureInputButton;
        public Slider LookSpeedSlider;
        public Toggle LookYToggle;

        [Header("Graphics")]
        public Dropdown ResolutionDropdown;
        public Toggle FullscreenToggle;
        public Slider FramerateSlider;
        public Text FramerateLabel;
        public Slider VsyncSlider;
        public Text VsyncLabel;
        public Slider GraphicsQualitySlider;
        public Text GraphicsQualityLabel;
        public Slider AntialiasingQualitySlider;
        public Text AntialiasingQualityLabel;
        public Slider ViewDistanceSlider;
        public Text ViewDistanceLabel;
        public Slider FovSlider;
        public Text FovLabel;
        public Slider EffectDwellSlider;
        public Text EffectDwellLabel;
        public Toggle ShowFpsToggle;

        [Header("Sound")]
        public Slider SoundVolumeSlider;
        public Slider MusicVolumeSlider;
        public Dropdown ChannelDropdown;

        //backing data for dropdowns
        private List<string> InputMappers = null;
        private List<Vector2Int> Resolutions = null;

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();

            //initialize subpanels
            var subpanels = ConfigModule.Instance.SortedConfigPanels;
            foreach(var subpanel in subpanels)
            {
                var subpanelGO = Instantiate(subpanel, PanelContainer);
                subpanelGO.SetActive(true);
            }
        }

        public override void SignalPaint()
        {
            base.SignalPaint();

            PaintValues();
        }

        private void PaintValues()
        {
            InputMappers = MappedInput.AvailableMappers.ToList();
            InputDropdown.ClearOptions();
            InputDropdown.AddOptions(InputMappers.Select(m => Sub.Replace(m, "CFG_MAPPERS")).ToList());
            int iIndex = InputMappers.IndexOf(ConfigState.Instance.InputMapper);
            InputDropdown.value = iIndex >= 0 ? iIndex : 0;
            ConfigureInputButton.interactable = iIndex >= 0; //enable configure button

            LookSpeedSlider.value = ConfigState.Instance.LookSpeed;
            LookYToggle.isOn = ConfigState.Instance.LookInvert;

            //Resolutions = new List<Resolution>(Screen.resolutions);
            Resolutions = GetDeduplicatedResolutionList(Screen.resolutions);
            ResolutionDropdown.ClearOptions();
            ResolutionDropdown.AddOptions(Resolutions.Select(r => $"{r.x} x {r.y}").ToList());
            int rIndex = Resolutions.IndexOf(ConfigState.Instance.Resolution);
            ResolutionDropdown.value = rIndex > 0 ? rIndex : Resolutions.Count - 1;

            FullscreenToggle.isOn = ConfigState.Instance.FullScreen;
            FramerateSlider.value = Math.Max(0, ConfigState.Instance.MaxFrames);
            VsyncSlider.value = ConfigState.Instance.VsyncCount;

            GraphicsQualitySlider.maxValue = QualitySettings.names.Length - 1;
            GraphicsQualitySlider.value = QualitySettings.GetQualityLevel();
            GraphicsQualitySlider.interactable = true;

            AntialiasingQualitySlider.value = ConfigState.Instance.AntialiasingQuality;
            ViewDistanceSlider.value = ConfigState.Instance.ViewDistance;
            FovSlider.value = Mathf.RoundToInt(ConfigState.Instance.FieldOfView);
            EffectDwellSlider.value = Mathf.RoundToInt(ConfigState.Instance.EffectDwellTime);

            ShowFpsToggle.isOn = ConfigState.Instance.ShowFps;

            SoundVolumeSlider.value = ConfigState.Instance.SoundVolume;
            MusicVolumeSlider.value = ConfigState.Instance.MusicVolume;

            var cList = new List<string>(Enum.GetNames(typeof(AudioSpeakerMode)));
            ChannelDropdown.ClearOptions();
            ChannelDropdown.AddOptions(cList);
            ChannelDropdown.value = cList.IndexOf(AudioSettings.GetConfiguration().speakerMode.ToString());
            

            //handle subpanels
            foreach (var subpanel in PanelContainer.GetComponentsInChildren<ConfigSubpanelController>())
            {
                try
                {
                    subpanel.PaintValues();
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to paint values for subpanel \"{subpanel.name}\"");
                    Debug.LogException(e);
                }
            }
        }

        public void OnInputDropdownChanged()
        {
            ConfigureInputButton.interactable = false;
        }

        public void OnFramerateSliderChanged()
        {
            int frameRate = (int)FramerateSlider.value;
            FramerateLabel.text = frameRate <= 0 ? "Unlimited" : frameRate.ToString();
        }

        public void OnVsyncSliderChanged()
        {
            int vsync = (int)VsyncSlider.value;
            switch (vsync)
            {
                case 0:
                    VsyncLabel.text = "Off";
                    break;
                case 1:
                    VsyncLabel.text = "On";
                    break;
                case 2:
                    VsyncLabel.text = "On (half refresh rate)";
                    break;
                default:
                    VsyncLabel.text = "???";
                    break;
            }
        }

        public void OnQualitySliderChanged()
        {
            GraphicsQualityLabel.text = QualitySettings.names[(int)GraphicsQualitySlider.value];
        }

        public void OnAntialiasingSliderChanged()
        {
            string lookupName = $"AntiAliasing{(int)AntialiasingQualitySlider.value}";
            AntialiasingQualityLabel.text = Sub.Replace(lookupName, "CFG");
        }

        public void OnViewDistanceSliderChanged()
        {
            ViewDistanceLabel.text = ((int)ViewDistanceSlider.value).ToString();
        }

        public void OnFovSliderChanged()
        {
            FovLabel.text = $"{FovSlider.value}°";
        }

        public void OnEffectDwellSliderChanged()
        {
            EffectDwellLabel.text = $"{EffectDwellSlider.value}s";
        }

        public void OnClickConfirm()
        {
            UpdateValues();
            ConfigModule.Apply();
            ConfigState.Save();            
            Modal.PushMessageModal("Applied settings changes!", "Changes Applied", null, OnConfirmed, true);
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
            }, true);
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
            else
                PaintValues();
        }

        private void UpdateValues()
        {
            //ConfigureInputButton.interactable = false;
            ConfigState.Instance.InputMapper = InputMappers[InputDropdown.value];
            ConfigState.Instance.LookSpeed = LookSpeedSlider.value;
            ConfigState.Instance.LookInvert = LookYToggle.isOn;

            ConfigState.Instance.Resolution = Resolutions[ResolutionDropdown.value];
            ConfigState.Instance.FullScreen = FullscreenToggle.isOn;
            ConfigState.Instance.MaxFrames = FramerateSlider.value > 0 ? Mathf.RoundToInt(FramerateSlider.value) : -1;
            ConfigState.Instance.VsyncCount = Mathf.RoundToInt(VsyncSlider.value);

            QualitySettings.SetQualityLevel( (int)GraphicsQualitySlider.value, true);
            ConfigState.Instance.AntialiasingQuality = (int)AntialiasingQualitySlider.value;
            ConfigState.Instance.ViewDistance = ViewDistanceSlider.value;
            ConfigState.Instance.FieldOfView = FovSlider.value;
            ConfigState.Instance.EffectDwellTime = EffectDwellSlider.value;

            ConfigState.Instance.ShowFps = ShowFpsToggle.isOn;

            ConfigState.Instance.SoundVolume = SoundVolumeSlider.value;
            ConfigState.Instance.MusicVolume = MusicVolumeSlider.value;
            ConfigState.Instance.SpeakerMode = (AudioSpeakerMode)Enum.Parse(typeof(AudioSpeakerMode), ChannelDropdown.options[ChannelDropdown.value].text);

            //handle subpanels
            foreach (var subpanel in PanelContainer.GetComponentsInChildren<ConfigSubpanelController>())
            {
                try
                {
                    subpanel.UpdateValues();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to update values from subpanel \"{subpanel.name}\"");
                    Debug.LogException(e);
                }
            }
        }

        private List<Vector2Int> GetDeduplicatedResolutionList(IEnumerable<Resolution> resolutions)
        {
            return resolutions.Select(r => new Vector2Int(r.width, r.height)).Distinct().ToList();
        }

    }
}