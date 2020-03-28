using CommonCore.Config;
using CommonCore.Messaging;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace CommonCore.Integrations.UnityPostProcessingV2
{

    /// <summary>
    /// Tack-on script for toggling antialiasing on a camera based on current ConfigState
    /// </summary>
    /// <remarks>
    /// <para>The name is because originally it *only* supported FXAA.</para>
    /// </remarks>
    public class FXAAToggleTackon : MonoBehaviour
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

        private void Awake()
        {
            MessageInterface = new QdmsMessageInterface(this.gameObject);
            MessageInterface.SubscribeReceiver(HandleMessage);
        }

        private void OnEnable()
        {
            if (AttachedCamera == null)
                AttachedCamera = GetComponent<Camera>();

            ApplyFXAAState();
        }

        private void Update()
        {
            if (UseAggressiveConfigurationCheck)
                ApplyFXAAState();
        }

        private void HandleMessage(QdmsMessage message)
        {
            if (message is ConfigChangedMessage)
                ApplyFXAAState();
        }

        private void ApplyFXAAState()
        {
            if (AttachedCamera == null)
            {
                Debug.LogError($"FXAAToggleTackon on {this.gameObject.name} has no attached camera!");
                return;
            }

            PostProcessLayer processLayer = AttachedCamera.GetComponent<PostProcessLayer>();

            if (processLayer == null)
            {
                Debug.LogError($"FXAAToggleTackon on {this.gameObject.name} has no attached PostProcessLayer");
                return;
            }

            int configuredQuality = ConfigState.Instance.AntialiasingQuality;
            if (configuredQuality >= AntialiasingSettings.Length)
                configuredQuality = AntialiasingSettings.Length - 1;

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
        }

        private class AntialiasingSetting
        {
            public PostProcessLayer.Antialiasing Mode;
            public object Quality;
        }
    }
}