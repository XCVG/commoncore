using CommonCore.Config;
using CommonCore.Messaging;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace CommonCore.Integrations.UnityPostProcessingV2
{

    /// <summary>
    /// Tack-on script for toggling antialiasing and other postprocessing on a camera based on current ConfigState
    /// </summary>
    /// <remarks>
    /// <para>The name is because originally it *only* supported FXAA.</para>
    /// </remarks>
    public class PostProcessingV2ConfigTackon : MonoBehaviour
    {
        //DO NOT CHANGE THESE UNLESS YOU KNOW WHAT YOU'RE DOING
        private static readonly AntialiasingSetting[] AntialiasingSettings = new AntialiasingSetting[] {
            new AntialiasingSetting() { Mode = PostProcessLayer.Antialiasing.None },
            new AntialiasingSetting() { Mode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing },
            new AntialiasingSetting() { Mode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing, Quality = SubpixelMorphologicalAntialiasing.Quality.Low },
            new AntialiasingSetting() { Mode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing, Quality = SubpixelMorphologicalAntialiasing.Quality.High }
        };

        private QdmsMessageInterface MessageInterface;

        [SerializeField, Tooltip("Check for changes to ConfigState after this tackon is started?")]
        private bool UseAggressiveConfigurationCheck = false;
        [SerializeField, Tooltip("Leave blank to attach to the camera on this GameObject")]
        private Camera AttachedCamera;

        private static PostProcessVolume Volume;

        private void Awake()
        {
            MessageInterface = new QdmsMessageInterface(this.gameObject);
            MessageInterface.SubscribeReceiver(HandleMessage);

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnEnable()
        {
            if (AttachedCamera == null)
                AttachedCamera = GetComponent<Camera>();

            ApplyConfig();
        }

        private void Update()
        {
            if (UseAggressiveConfigurationCheck)
                ApplyConfig();
        }

        private void OnSceneUnloaded(Scene current)
        {
            if (Volume != null)
                RuntimeUtilities.DestroyVolume(Volume, true, true);
        }

        private void HandleMessage(QdmsMessage message)
        {
            if (message is ConfigChangedMessage)
                ApplyConfig();
        }

        private void ApplyConfig()
        {
            if (AttachedCamera == null)
            {
                Debug.LogError($"{nameof(PostProcessingV2ConfigTackon)} on {this.gameObject.name} has no attached camera!");
                return;
            }

            PostProcessLayer processLayer = AttachedCamera.GetComponent<PostProcessLayer>();

            if (processLayer == null)
            {
                Debug.LogError($"{nameof(PostProcessingV2ConfigTackon)} on {this.gameObject.name} has no attached PostProcessLayer");
                return;
            }

            int configuredQuality = ConfigState.Instance.AntialiasingQuality;
            if (configuredQuality >= AntialiasingSettings.Length)
                configuredQuality = AntialiasingSettings.Length - 1;
            else if (configuredQuality < 0)
                configuredQuality = 0;

            //apply mode
            processLayer.antialiasingMode = AntialiasingSettings[configuredQuality].Mode;

            //attempt to apply quality
            object aaQuality = AntialiasingSettings[configuredQuality].Quality;
            if (aaQuality != null)
            {
                switch (processLayer.antialiasingMode)
                {
                    case PostProcessLayer.Antialiasing.FastApproximateAntialiasing:
                        processLayer.fastApproximateAntialiasing.fastMode = (bool)aaQuality;
                        break;
                    case PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing:
                        processLayer.subpixelMorphologicalAntialiasing.quality = (SubpixelMorphologicalAntialiasing.Quality)aaQuality;
                        break;
                }
            }

            //create volume if not exists
            if(Volume == null)
            {
                Volume = PostProcessManager.instance.QuickVolume(22, 0);
                Volume.isGlobal = true;
            }

            //create color grading if not exists
            var profile = Volume.profile;
            if(!profile.HasSettings<ColorGrading>())
            {
                var newCg = ScriptableObject.CreateInstance<ColorGrading>();
                newCg.gradingMode.Override(GradingMode.HighDefinitionRange);
                newCg.enabled.Override(true);
                profile.AddSettings(newCg);
            }

            var colorGrading = profile.GetSetting<ColorGrading>();
            if(colorGrading.gradingMode.GetValue<GradingMode>() == GradingMode.HighDefinitionRange)
            {
                float rawBrightness = ConfigState.Instance.Brightness;
                float scaledBrightness = rawBrightness >= 1f ? ((rawBrightness - 1f) * 2f) : ((1f - rawBrightness) * -5f);
                colorGrading.postExposure.Override(scaledBrightness);
            }
            else if (colorGrading.gradingMode.GetValue<GradingMode>() == GradingMode.LowDefinitionRange)
            {
                float rawBrightness = ConfigState.Instance.Brightness;
                float scaledBrightness = rawBrightness >= 1f ? (MathUtils.ScaleRange(rawBrightness, 1f, 4f, 0, 100f)) : (MathUtils.ScaleRange(rawBrightness, 0, 1, -100f, 0));
                colorGrading.brightness.Override(scaledBrightness);
            }
        }

        private class AntialiasingSetting
        {
            public PostProcessLayer.Antialiasing Mode;
            public object Quality;
        }
    }
}