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
using CommonCore.State;
using System.Text;
using CommonCore.Scripting;

namespace CommonCore.UI
{

    /// <summary>
    /// Controller for config panel
    /// </summary>
    public class ConfigPanelController : PanelController
    {
        [Header("Theme")]
        public bool ApplyTheme = true;

        [Header("Containers"), SerializeField]
        private RectTransform PanelContainer = null;
        [SerializeField]
        private Text StatusText = null;
        [SerializeField]
        private Text VersionText = null;

        [Header("Input")]
        public Dropdown InputDropdown;
        public Button ConfigureInputButton;
        public Slider LookSpeedSlider;
        public Toggle LookYToggle;

        [Header("Graphics")]
        public GameObject ResolutionGroup;
        public Dropdown ResolutionDropdown;
        public GameObject FullscreenGroup;
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
        public Slider BrightnessSlider;
        public Text BrightnessLabel;
        public GameObject MonitorGroup;
        public Dropdown MonitorDropdown;

        [Header("Sound")]
        public Slider SoundVolumeSlider;
        public Slider MusicVolumeSlider;
        public Dropdown ChannelDropdown;

        //a bit of a hack
        private bool IgnoreValueChanges = false;

        //backing data for dropdowns
        private List<string> InputMappers = null;
        private List<Vector2Int> Resolutions = null;

        //backing data for status text
        private bool PendingChanges = false;
        private bool PendingMoreOptionsOnApply = false;
        private bool PendingRequiresRestart = false;
        private bool CommittedRequiresRestart { get => (bool)MetaState.Instance.SessionData.GetOrDefault("ConfigRequiresRestart", false); set => MetaState.Instance.SessionData["ConfigRequiresRestart"] = value; }

        private bool AllowMonitorSelection => !CoreParams.IsEditor && (CoreParams.Platform == RuntimePlatform.LinuxPlayer || CoreParams.Platform == RuntimePlatform.OSXPlayer || CoreParams.Platform == RuntimePlatform.WindowsPlayer);

        private bool AllowResolutionSelection => !(CoreParams.Platform == RuntimePlatform.WebGLPlayer || SystemInfo.deviceType == DeviceType.Console);

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();

            ScriptingModule.CallHooked(ScriptHook.OnConfigPanelOpen, this);

            //initialize subpanels
            var subpanelBuilders = ConfigModule.Instance.SortedConfigPanelBuilders;
            foreach(var subpanelBuilder in subpanelBuilders)
            {
                var subpanelGO = subpanelBuilder(PanelContainer);
                subpanelGO.SetActive(true);

                if (ApplyTheme)
                {
                    ApplyThemeToElements(subpanelGO.transform);
                }

                var subpanelController = subpanelGO.GetComponent<ConfigSubpanelController>();
                if(subpanelController != null)
                {
                    subpanelController.SignalPendingChanges = SignalPendingChanges;
                }
            }

            ScriptingModule.CallHooked(ScriptHook.OnConfigPanelRendered, this);

        }

        public override void SignalPaint()
        {
            base.SignalPaint();

            PaintValues();
        }

        private void PaintValues()
        {
            IgnoreValueChanges = true;
            InputMappers = MappedInput.AvailableMappers.ToList();
            InputDropdown.ClearOptions();
            InputDropdown.AddOptions(InputMappers.Select(m => Sub.Replace(m, "CFG_MAPPERS")).ToList());
            int iIndex = InputMappers.IndexOf(ConfigState.Instance.InputMapper);
            InputDropdown.SetValueWithoutNotify(iIndex >= 0 ? iIndex : 0);
            ConfigureInputButton.interactable = iIndex >= 0; //enable configure button

            LookSpeedSlider.value = ConfigState.Instance.LookSpeed;
            LookYToggle.isOn = ConfigState.Instance.LookInvert;

            if (AllowResolutionSelection)
            {
                //Resolutions = new List<Resolution>(Screen.resolutions);
                Resolutions = GetDeduplicatedResolutionList(Screen.resolutions);
                ResolutionDropdown.ClearOptions();
                ResolutionDropdown.AddOptions(Resolutions.Select(r => $"{r.x} x {r.y}").ToList());
                int rIndex = Resolutions.IndexOf(ConfigState.Instance.Resolution);
                ResolutionDropdown.value = rIndex > 0 ? rIndex : Resolutions.Count - 1;
                FullscreenToggle.isOn = ConfigState.Instance.FullScreen;
            }
            else
            {
                ResolutionGroup.SetActive(false);
                FullscreenGroup.SetActive(false);
            }

            if (AllowMonitorSelection)
            {
                MonitorDropdown.ClearOptions();
                MonitorDropdown.AddOptions(Display.displays.Select((d, i) => $"Monitor {i}").ToList());
                int mIndex = PlayerPrefs.GetInt("UnitySelectMonitor", 0);
                MonitorDropdown.value = mIndex;
            }
            else
            {
                MonitorGroup.SetActive(false);
            }
            
            FramerateSlider.value = Math.Max(0, ConfigState.Instance.MaxFrames);
            VsyncSlider.value = ConfigState.Instance.VsyncCount;

            GraphicsQualitySlider.maxValue = QualitySettings.names.Length - 1;
            GraphicsQualitySlider.value = ConfigState.Instance.GraphicsQuality;
            GraphicsQualitySlider.interactable = true;

            AntialiasingQualitySlider.value = ConfigState.Instance.AntialiasingQuality;
            ViewDistanceSlider.value = ConfigState.Instance.ViewDistance;
            FovSlider.value = Mathf.RoundToInt(ConfigState.Instance.FieldOfView);
            EffectDwellSlider.value = Mathf.RoundToInt(ConfigState.Instance.EffectDwellTime);
            BrightnessSlider.value = Mathf.RoundToInt(ConfigState.Instance.Brightness * 100f);

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

            PaintStatusText();
            PaintVersionText();

            IgnoreValueChanges = false;
        }

        public void OnInputDropdownChanged()
        {
            ConfigureInputButton.interactable = false;
            SignalPendingChanges(PendingChangesFlags.MoreOptionsOnApply | PendingChangesFlags.DoNotSetPendingChanges);
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

        public void OnBrightnessSliderChanged()
        {
            BrightnessLabel.text = $"{(BrightnessSlider.value / 100f):F2}";
        }

        public void OnMonitorDropdownChanged()
        {
            if (IgnoreValueChanges)
                return;

            SignalPendingChanges(PendingChangesFlags.RequiresRestart);
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
                    PendingChanges = false;
                    PendingMoreOptionsOnApply = false;
                    PendingRequiresRestart = CommittedRequiresRestart;
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

        public void OnAnyChanged()
        {
            if (IgnoreValueChanges)
                return;

            SignalPendingChanges(PendingChangesFlags.None);
        }

        private void OnConfirmed(ModalStatusCode status, string tag)
        {
            PendingChanges = false;
            PendingMoreOptionsOnApply = false;
            if (PendingRequiresRestart)
                CommittedRequiresRestart = true;
            PendingRequiresRestart = CommittedRequiresRestart;

            string sceneName = SceneManager.GetActiveScene().name;
            if(sceneName == CoreParams.MainMenuScene)
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

            if(AllowResolutionSelection)
            {
                ConfigState.Instance.Resolution = Resolutions[ResolutionDropdown.value];
                ConfigState.Instance.FullScreen = FullscreenToggle.isOn;
            }            

            if (AllowMonitorSelection)
            {
                PlayerPrefs.SetInt("UnitySelectMonitor", MonitorDropdown.value);
            }

            ConfigState.Instance.MaxFrames = FramerateSlider.value > 0 ? Mathf.RoundToInt(FramerateSlider.value) : -1;
            ConfigState.Instance.VsyncCount = Mathf.RoundToInt(VsyncSlider.value);            

            ConfigState.Instance.GraphicsQuality = (int)GraphicsQualitySlider.value;
            ConfigState.Instance.AntialiasingQuality = (int)AntialiasingQualitySlider.value;
            ConfigState.Instance.ViewDistance = ViewDistanceSlider.value;
            ConfigState.Instance.FieldOfView = FovSlider.value;
            ConfigState.Instance.EffectDwellTime = EffectDwellSlider.value;
            ConfigState.Instance.Brightness = BrightnessSlider.value / 100f;

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

        private void SignalPendingChanges(PendingChangesFlags flags)
        {
            if(!flags.HasFlag(PendingChangesFlags.DoNotSetPendingChanges))
                PendingChanges = true;
            if (flags.HasFlag(PendingChangesFlags.MoreOptionsOnApply))
                PendingMoreOptionsOnApply = true;
            if (flags.HasFlag(PendingChangesFlags.RequiresRestart))
                PendingRequiresRestart = true;

            PaintStatusText();
        }

        private void PaintStatusText()
        {
            StringBuilder sb = new StringBuilder();

            if (PendingChanges)
                sb.AppendLine("There are unsaved settings changes");
            if (PendingMoreOptionsOnApply)
                sb.AppendLine("More options will be shown or unlocked when changes are applied");
            if (PendingRequiresRestart || CommittedRequiresRestart)
                sb.AppendLine("The game must be restarted to fully apply settings changes");

            StatusText.text = sb.ToString();
        }

        private void PaintVersionText()
        {
            VersionText.text = $"{CoreParams.GameName} {CoreParams.GameVersion.ToString(3)} C{CoreParams.VersionCode.ToString(3)} E{CoreParams.UnityVersion.ToString(3)}";
        }

    }
}