using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.Config;
using CommonCore.Messaging;

/// <summary>
/// Tack-on script for setting camera view distance based on config
/// </summary>
public class CameraSettingsTackon : MonoBehaviour
{
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

        ApplyCameraSettings();
    }

    private void Update()
    {
        if (UseAggressiveConfigurationCheck)
            ApplyCameraSettings();
    }

    private void HandleMessage(QdmsMessage message)
    {
        if (message is QdmsFlagMessage flagMessage && flagMessage.Flag == "ConfigChanged")
            ApplyCameraSettings();
    }

    private void ApplyCameraSettings()
    {
        AttachedCamera.farClipPlane = Mathf.Max(AttachedCamera.nearClipPlane, ConfigState.Instance.ViewDistance);
    }
}
