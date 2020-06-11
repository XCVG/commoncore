using CommonCore.Config;
using CommonCore.Messaging;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Component that handles zooming the camera in and out
    /// </summary>
    /// <remarks>
    /// <para>This takes over FOV handling from CameraSettingsTackon</para>
    /// <para>For now weapon ADS is the only thing that can zoom the camera in and out</para>
    /// </remarks>
    public class PlayerCameraZoomComponent : MonoBehaviour
    {
        private QdmsMessageInterface MessageInterface;

        [SerializeField, FormerlySerializedAs("AttachedCamera")]
        private Camera WorldCamera;
        [SerializeField]
        private Camera GunCamera;
        [SerializeField]
        private bool UseConfigViewAngle = true;

        private float? DefaultViewAngle;
        private float ADSZoomFactor = 1f;

        private Coroutine ADSZoomCoroutine = null;

        private void Awake()
        {
            MessageInterface = new QdmsMessageInterface(this.gameObject);
            MessageInterface.SubscribeReceiver(HandleMessage);
        }

        private void Start()
        {
            if (WorldCamera == null)
                WorldCamera = GetComponent<Camera>();

            if (GunCamera == null)
                Debug.LogWarning($"[{nameof(PlayerCameraZoomComponent)}] Gun camera must be set explicitly!");

            DefaultViewAngle = WorldCamera.fieldOfView;
            SetConfiguredViewAngle();
        }

        private void HandleMessage(QdmsMessage message)
        {
            if (message is ConfigChangedMessage)
            {
                SetConfiguredViewAngle();
            }
        }

        private void SetConfiguredViewAngle()
        {
            if (UseConfigViewAngle && ADSZoomCoroutine == null)
            {
                float viewAngle = ConfigState.Instance.FieldOfView;
                WorldCamera.fieldOfView = viewAngle;
                if (GunCamera)
                    GunCamera.fieldOfView = viewAngle;
            }
        }

        public void SetADSZoomFactor(float adsZoomFactor, float fadeTime)
        {           
            if (fadeTime == 0)
            {
                ADSZoomFactor = adsZoomFactor;
                UpdateCameraView();
            }
            else
            {
                if (ADSZoomCoroutine != null)
                    StopCoroutine(ADSZoomCoroutine);
                ADSZoomCoroutine = StartCoroutine(FadeADSZoom(adsZoomFactor, fadeTime));
            }
        }

        private IEnumerator FadeADSZoom(float targetZoomFactor, float fadeTime)
        {
            float baseZoom = ADSZoomFactor;
            float zoomDistance = targetZoomFactor - baseZoom;

            for(float elapsed = 0; elapsed < fadeTime; elapsed += Time.deltaTime)
            {
                float ratio = elapsed / fadeTime;

                ADSZoomFactor = baseZoom + (zoomDistance * ratio);
                UpdateCameraView();

                yield return null;
            }

            yield return null;

            ADSZoomFactor = targetZoomFactor;
            UpdateCameraView();

            ADSZoomCoroutine = null;
        }

        private void UpdateCameraView()
        {
            if(!DefaultViewAngle.HasValue)
                DefaultViewAngle = WorldCamera.fieldOfView;

            float viewAngle = UseConfigViewAngle ? ConfigState.Instance.FieldOfView : DefaultViewAngle.Value;

            viewAngle /= ADSZoomFactor;

            WorldCamera.fieldOfView = viewAngle;
            if (GunCamera)
                GunCamera.fieldOfView = viewAngle;
        }


    }

}