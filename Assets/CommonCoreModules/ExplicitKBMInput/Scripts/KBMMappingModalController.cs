using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CommonCore.Input;
using CommonCore.StringSub;
using System;

namespace CommonCore.ExplicitKBMInput
{

    /// <summary>
    /// Controller for the popup in the KBM input mapper config page that lets you actually pick the key
    /// </summary>
    public class KBMMappingModalController : MonoBehaviour
    {

        [SerializeField]
        private Text MappingText = null;
        [SerializeField]
        private Text CurrentMappingText = null;

        private KBMButtonMappingCallback Callback;
        private Array KeyCodes;

        private void Awake()
        {
            KeyCodes = Enum.GetValues(typeof(KeyCode));
        }

        /// <summary>
        /// Call this, passing in a callback, to get a new key mapping
        /// </summary>
        public void GetMapping(string displayName, KeyCode oldKey, KBMButtonMappingCallback callback)
        {
            Callback = callback;
            gameObject.SetActive(true); //activate our panel

            //setup text boxen
            MappingText.text = displayName;
            CurrentMappingText.text = InputModule.GetNameForKeyCode(oldKey);

            //clear selection
            EventSystem.current.Ref()?.SetSelectedGameObject(null);
        }

        private void Update()
        {
            //listen for keycodes
            //yes, this is the least shitty way of doing it
            foreach (KeyCode keycode in KeyCodes)
            {
                if (keycode == KeyCode.Tilde || keycode == KeyCode.BackQuote || keycode >= KeyCode.JoystickButton0)
                    continue;

                if (UnityEngine.Input.GetKeyDown(keycode))
                {
                    if(keycode == KeyCode.Escape)
                    {
                        CancelMappingAndReturn();
                        break;
                    }
                    else if(keycode == KeyCode.Backspace)
                    {
                        ClearMappingAndReturn();
                        break;
                    }
                    else
                    {
                        SetMappingAndReturn(keycode);
                        break;
                    }
                }
            }
        }

        private void SetMappingAndReturn(KeyCode key)
        {
            if (Callback != null)
            {
                Callback(key);
                Callback = null;
            }

            gameObject.SetActive(false);
        }

        private void CancelMappingAndReturn()
        {
            if(Callback != null)
            {
                Callback(null);
                Callback = null;
            }

            gameObject.SetActive(false);
        }

        private void ClearMappingAndReturn()
        {
            if (Callback != null)
            {
                Callback(KeyCode.None);
                Callback = null;
            }

            gameObject.SetActive(false);
        }

        public void HandleCancelButtonClicked()
        {
            CancelMappingAndReturn();
        }

        public void HandleClearButtonClicked()
        {
            ClearMappingAndReturn();
        }
        
    }

    public delegate void KBMButtonMappingCallback(KeyCode? newKeyCode);
}