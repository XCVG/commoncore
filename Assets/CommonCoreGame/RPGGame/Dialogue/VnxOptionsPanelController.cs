using CommonCore.Config;
using CommonCore.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.RpgGame.Dialogue
{
    public class VnxOptionsPanelController : ConfigSubpanelController
    {
        public Slider TypeSpeedSlider;
        public Text TypeSpeedText;

        public Toggle AdvanceBeepToggle;
        public Toggle CharacterFadeToggle;

        public override void PaintValues()
        {
            IgnoreValueChanges = true;

            ConfigState.Instance.AddCustomVarIfNotExists("VnxConfig", () => new VnConfig());
            var vnConfig = ConfigState.Instance.CustomConfigVars["VnxConfig"] as VnConfig;

            TypeSpeedSlider.value = vnConfig.TypeOnSpeed * 25f;
            AdvanceBeepToggle.isOn = vnConfig.EnableAdvanceBeep;
            CharacterFadeToggle.isOn = vnConfig.AllowFade;

            IgnoreValueChanges = false;
        }

        public override void UpdateValues()
        {
            ConfigState.Instance.AddCustomVarIfNotExists("VnxConfig", () => new VnConfig());
            var vnConfig = ConfigState.Instance.CustomConfigVars["VnxConfig"] as VnConfig;

            vnConfig.TypeOnSpeed = CalculateTypeSpeed();
            vnConfig.EnableAdvanceBeep = AdvanceBeepToggle.isOn;
            vnConfig.AllowFade = CharacterFadeToggle.isOn;
        }

        public void HandleTypeSpeedChanged()
        {
            float val = CalculateTypeSpeed();
            if (val > 0)
                TypeSpeedText.text = (int)(val * 100f) + "%";
            else
                TypeSpeedText.text = "Disabled";
        }

        private float CalculateTypeSpeed()
        {
            return TypeSpeedSlider.value / 25f;
        }
    }
}


