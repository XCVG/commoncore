using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using CommonCore;
using CommonCore.Config;
using CommonCore.Messaging;

/// <summary>
/// Tack-on script for toggling FXAA on a camera based on current ConfigState
/// </summary>
public class FXAAToggleTackon : MonoBehaviour
{
    [SerializeField, Tooltip("Check for changes to ConfigState after this tackon is started?")]
    private bool UseAggressiveConfigurationCheck = false;
    [SerializeField, Tooltip("Leave blank to attach to the camera on this GameObject")]
    private Camera AttachedCamera;

    private void OnEnable()
    {
        if (AttachedCamera == null)
            AttachedCamera = GetComponent<Camera>();

        ApplyFXAAState();
    }

    private void Update()
    {
        //TODO move this to messaging once we have support for calling delegates in QdmsMessageInterface

        if (UseAggressiveConfigurationCheck)
            ApplyFXAAState();
    }

    private void ApplyFXAAState()
    {
        if(AttachedCamera == null)
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

        processLayer.antialiasingMode = ConfigState.Instance.FxaaEnabled ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
    }
}
