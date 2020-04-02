using CommonCore.Input;
using CommonCore.LockPause;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CommonCore
{
    /// <summary>
    /// Script component that provides a skippable timer, meant for cutscenes etc
    /// </summary>
    public class SkippableTimerScript : MonoBehaviour
    {
        [SerializeField, Tooltip("<= 0 means no timer")]
        private float TimeToWait = 0;
        [SerializeField]
        private bool HoldToSkip = true;
        [SerializeField]
        private bool UseMappedInput = true;
        [SerializeField]
        private bool UseUnityInput = true;
        [SerializeField]
        private float ButtonHoldTime = 3f;

        [SerializeField]
        private UnityEvent FinishedEvent = null;
        [SerializeField]
        private GameObject ContinueHintObject = null;

        private float HeldTime = 0;
        private float Elapsed = 0;

        private void Start()
        {
            if (TimeToWait <= 0)
                Elapsed = -1;
        }

        private void Update()
        {
            if(TimeToWait > 0)
                Elapsed += Time.deltaTime;

            if (GetSkipKeyDown())
            {
                ContinueHintObject.Ref()?.SetActive(true);
            }

            if (GetSkipKey())
            {
                HeldTime += Time.deltaTime;
            }
            else
            {
                HeldTime = 0;
            }

            if (HeldTime > ButtonHoldTime || Elapsed > TimeToWait)
            {
                FinishedEvent.Invoke();
            }
        }

        private bool GetSkipKey()
        {
            if (LockPauseModule.IsInputLocked())
                return false;

            if (UseUnityInput && (UnityEngine.Input.GetButton("Submit") || UnityEngine.Input.GetKey(KeyCode.Space)))
                return true;

            if (UseMappedInput && (MappedInput.GetButton(DefaultControls.Confirm) || MappedInput.GetButton(DefaultControls.Fire) || MappedInput.GetButton(DefaultControls.Use)))
                return true;

            return false;
        }

        private bool GetSkipKeyDown()
        {
            if (LockPauseModule.IsInputLocked())
                return false;

            if (UseUnityInput && (UnityEngine.Input.GetButtonDown("Submit") || UnityEngine.Input.GetKeyDown(KeyCode.Space)))
                return true;

            if (UseMappedInput && (MappedInput.GetButtonDown(DefaultControls.Confirm) || MappedInput.GetButtonDown(DefaultControls.Fire) || MappedInput.GetButtonDown(DefaultControls.Use)))
                return true;

            return false;
        }
    }
}