using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.UI;
using CommonCore.Config;
using CommonCore.Messaging;
using CommonCore.Scripting;
using System;

namespace CommonCore.RpgGame.UI
{
    public class GameplayOptionsPanelController : ConfigSubpanelController
    {
#pragma warning disable CS0649
        [SerializeField, Header("Difficulty")]
        private Slider DifficultySlider;
        [SerializeField]
        private Text DifficultyLabel;

        [SerializeField, Header("Other Settings")]
        private Toggle SubtitlesAlwaysToggle;
        [SerializeField]
        private Toggle SubtitlesForcedToggle;
        [SerializeField]
        private Toggle SubtitlesNeverToggle;
        
        [SerializeField]
        private Toggle CrosshairAlwaysToggle;
        [SerializeField]
        private Toggle CrosshairAutoToggle;
        [SerializeField]
        private Toggle CrosshairNeverToggle;

        [SerializeField]
        private Slider AutoaimSlider;
        [SerializeField]
        private Text AutoaimLabel;

        [SerializeField]
        private Toggle ShakeEffectToggle;
        [SerializeField]
        private Toggle BobEffectToggle;
        
        [SerializeField]
        private Toggle AdsToggleToggle;
        [SerializeField]
        private Toggle AdsHoldToggle;

        [SerializeField]
        private Toggle HitIndicatorsVisualToggle;
        [SerializeField]
        private Toggle HitIndicatorsAudioToggle;
#pragma warning restore CS0649

        //IGUI_GAMEPLAYOPTIONS

        public override void PaintValues()
        {
            ConfigState.Instance.AddCustomVarIfNotExists("GameplayConfig", () => new GameplayConfig());
            var gameplayConfig = ConfigState.Instance.CustomConfigVars["GameplayConfig"] as GameplayConfig;

            //paint 
            SetSubtitlesValue(ConfigState.Instance.Subtitles);

            DifficultySlider.value = (int)gameplayConfig.DifficultySetting;

            SetCrosshairValue(gameplayConfig.Crosshair);
            AutoaimSlider.value = (int)gameplayConfig.AimAssist;
            ShakeEffectToggle.isOn = gameplayConfig.ShakeEffects;
            BobEffectToggle.isOn = gameplayConfig.BobEffects;
            SetAdsValue(gameplayConfig.HoldAds);
            HitIndicatorsVisualToggle.isOn = gameplayConfig.HitIndicatorsVisual;
            HitIndicatorsAudioToggle.isOn = gameplayConfig.HitIndicatorsAudio;

            //setup sub-difficulty sliders
            SetupDifficultyParameterSliders();
        }

        public override void UpdateValues()
        {
            ConfigState.Instance.AddCustomVarIfNotExists("GameplayConfig", () => new GameplayConfig());
            var gameplayConfig = ConfigState.Instance.CustomConfigVars["GameplayConfig"] as GameplayConfig;

            ConfigState.Instance.Subtitles = GetSubtitlesValue();

            gameplayConfig.DifficultySetting = (DifficultyLevel)(int)DifficultySlider.value;
            
            gameplayConfig.Crosshair = GetCrosshairValue();
            gameplayConfig.AimAssist = (AimAssistState)(int)AutoaimSlider.value;
            gameplayConfig.ShakeEffects = ShakeEffectToggle.isOn;
            gameplayConfig.BobEffects = BobEffectToggle.isOn;
            gameplayConfig.HoldAds = AdsHoldToggle.isOn;
            gameplayConfig.HitIndicatorsVisual = HitIndicatorsVisualToggle.isOn;
            gameplayConfig.HitIndicatorsAudio = HitIndicatorsAudioToggle.isOn;

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("DifficultyChanged"));
        }

        private void SetupDifficultyParameterSliders()
        {
            //TODO setup slider ranges for difficulty

            //TODO paint sliders based on current or custom difficulty

            //TODO activate/deactivate difficulty sliders
        }

        public void HandleDifficultyChanged()
        {
            DifficultyLabel.text = ((DifficultyLevel)(int)DifficultySlider.value).ToString();

            SetupDifficultyParameterSliders();
        }

        public void HandleAutoaimChanged()
        {
            AutoaimLabel.text = ((AimAssistState)(int)AutoaimSlider.value).ToString();
        }

        private SubtitlesLevel GetSubtitlesValue()
        {
            if (SubtitlesAlwaysToggle.isOn)
                return SubtitlesLevel.Always;
            else if (SubtitlesForcedToggle.isOn)
                return SubtitlesLevel.ForcedOnly;
            else if (SubtitlesNeverToggle.isOn)
                return SubtitlesLevel.Never;

            throw new NotImplementedException();
        }

        private void SetSubtitlesValue(SubtitlesLevel state)
        {
            switch (state)
            {
                case SubtitlesLevel.Always:
                    SubtitlesAlwaysToggle.isOn = true;
                    break;
                case SubtitlesLevel.ForcedOnly:
                    SubtitlesForcedToggle.isOn = true;
                    break;
                case SubtitlesLevel.Never:
                    SubtitlesNeverToggle.isOn = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void SetAdsValue(bool value)
        {
            if (value)
                AdsHoldToggle.isOn = true;
            else
                AdsToggleToggle.isOn = true;
        }

        private CrosshairState GetCrosshairValue()
        {
            if (CrosshairAlwaysToggle.isOn)
                return CrosshairState.Always;
            else if (CrosshairAutoToggle.isOn)
                return CrosshairState.Auto;
            else if (CrosshairNeverToggle.isOn)
                return CrosshairState.Never;

            throw new NotImplementedException();
        }

        private void SetCrosshairValue(CrosshairState state)
        {
            switch (state)
            {
                case CrosshairState.Always:
                    CrosshairAlwaysToggle.isOn = true;
                    break;
                case CrosshairState.Auto:
                    CrosshairAutoToggle.isOn = true;
                    break;
                case CrosshairState.Never:
                    CrosshairNeverToggle.isOn = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}