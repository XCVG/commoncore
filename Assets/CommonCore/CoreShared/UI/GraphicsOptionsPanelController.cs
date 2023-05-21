using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Config;
using UnityEngine.UI;
using System;

namespace CommonCore.UI
{

    /// <summary>
    /// Subpanel controller for extended graphics options
    /// </summary>
    public class GraphicsOptionsPanelController : ConfigSubpanelController
    {
        [Header("Elements"), SerializeField]
        private CanvasGroup CanvasGroup = null;
        [SerializeField]
        private Slider ShadowQualitySlider = null;
        [SerializeField]
        private Text ShadowQualityLabel = null;
        [SerializeField]
        private Slider ShadowDistanceSlider = null;
        [SerializeField]
        private Text ShadowDistanceLabel = null;
        [SerializeField]
        private Slider LightingQualitySlider = null;
        [SerializeField]
        private Text LightingQualityLabel = null;
        [SerializeField]
        private Slider MeshQualitySlider = null;
        [SerializeField]
        private Text MeshQualityLabel = null;
        [SerializeField]
        private Slider TextureScaleSlider = null;
        [SerializeField]
        private Text TextureScaleLabel = null;
        [SerializeField]
        private Slider AnisotropicFilteringSlider = null;
        [SerializeField]
        private Text AnisotropicFilteringLabel = null;
        [SerializeField]
        private Slider RenderingQualitySlider = null;
        [SerializeField]
        private Text RenderingQualityLabel = null;
        [SerializeField]
        private Slider LightReportingSlider = null;
        [SerializeField]
        private Text LightReportingLabel = null;

        [Header("Options"), SerializeField]
        private float ShadowDistanceSliderScale = 5f;

        public override void PaintValues()
        {
            IgnoreValueChanges = true;

            CanvasGroup.interactable = ConfigState.Instance.UseCustomVideoSettings;

            ShadowQualitySlider.value = (int)ConfigState.Instance.ShadowQuality;
            ShadowDistanceSlider.value = Mathf.RoundToInt(ConfigState.Instance.ShadowDistance / ShadowDistanceSliderScale);
            LightingQualitySlider.value = (int)ConfigState.Instance.LightingQuality;
            MeshQualitySlider.value = (int)ConfigState.Instance.MeshQuality;
            TextureScaleSlider.value = (int)(ConfigState.Instance.TextureScale);
            AnisotropicFilteringSlider.value = (int)ConfigState.Instance.AnisotropicFiltering;
            RenderingQualitySlider.value = (int)ConfigState.Instance.RenderingQuality;
            LightReportingSlider.value = (int)ConfigState.Instance.PlayerLightReporting;

            HandleShadowQualityChanged();
            HandleShadowDistanceChanged();
            HandleLightingQualityChanged();
            HandleMeshQualityChanged();
            HandleTextureScaleChanged();
            HandleAnisotropicFilteringChanged();
            HandleRenderingQualityChanged();
            HandleLightReportingChanged();

            IgnoreValueChanges = false;
        }

        public override void UpdateValues()
        {
            CanvasGroup.interactable = ConfigState.Instance.UseCustomVideoSettings;

            ConfigState.Instance.ShadowQuality = (Config.QualityLevel)(int)ShadowQualitySlider.value;
            ConfigState.Instance.ShadowDistance = ShadowDistanceSlider.value * ShadowDistanceSliderScale;
            ConfigState.Instance.LightingQuality = (Config.QualityLevel)(int)LightingQualitySlider.value;
            ConfigState.Instance.MeshQuality = (Config.QualityLevel)(int)MeshQualitySlider.value;
            ConfigState.Instance.TextureScale = (TextureScale)(int)TextureScaleSlider.value;
            ConfigState.Instance.AnisotropicFiltering = (AnisotropicFiltering)(int)AnisotropicFilteringSlider.value;
            ConfigState.Instance.RenderingQuality = (Config.QualityLevel)(int)RenderingQualitySlider.value;
            ConfigState.Instance.PlayerLightReporting = (PlayerLightReportingType)(int)LightReportingSlider.value;
        }


        //handlers to update text when sliders are moved
        public void HandleShadowQualityChanged()
        {
            MaybeSignalPendingChanges();
            ShadowQualityLabel.text = GetTextForQualityValue(ShadowQualitySlider.value);
        }

        public void HandleShadowDistanceChanged()
        {
            MaybeSignalPendingChanges();
            ShadowDistanceLabel.text = Mathf.RoundToInt((ShadowDistanceSlider.value * ShadowDistanceSliderScale)).ToString();
        }

        public void HandleLightingQualityChanged()
        {
            MaybeSignalPendingChanges();
            LightingQualityLabel.text = GetTextForQualityValue(LightingQualitySlider.value);
        }

        public void HandleMeshQualityChanged()
        {
            MaybeSignalPendingChanges();
            MeshQualityLabel.text = GetTextForQualityValue(MeshQualitySlider.value);
        }

        public void HandleTextureScaleChanged()
        {
            MaybeSignalPendingChanges();
            TextureScaleLabel.text = ((TextureScale)(int)TextureScaleSlider.value).ToString();
        }

        public void HandleAnisotropicFilteringChanged()
        {
            MaybeSignalPendingChanges();
            AnisotropicFilteringLabel.text = ((AnisotropicFiltering)(int)AnisotropicFilteringSlider.value).ToString();
        }

        public void HandleRenderingQualityChanged()
        {
            MaybeSignalPendingChanges();
            RenderingQualityLabel.text = GetTextForQualityValue(RenderingQualitySlider.value);
        }

        public void HandleLightReportingChanged()
        {
            MaybeSignalPendingChanges();
            LightReportingLabel.text = ((PlayerLightReportingType)(int)LightReportingSlider.value).ToString();
        }

        private void MaybeSignalPendingChanges()
        {
            base.HandleAnyChanged();
        }

        private string GetTextForQualityValue(float value)
        {
            var quality = (Config.QualityLevel)(int)value;
            return quality.ToString();
        }

    }
}