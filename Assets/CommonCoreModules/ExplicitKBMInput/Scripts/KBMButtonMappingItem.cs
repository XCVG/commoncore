using CommonCore.Input;
using CommonCore.StringSub;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.ExplicitKBMInput
{

    /// <summary>
    /// Script attached to an item in the button list of the input remap window
    /// </summary>
    public class KBMButtonMappingItem : MonoBehaviour
    {
        [SerializeField]
        private Text TitleText = null;
        [SerializeField]
        private Button Primary = null;
        [SerializeField]
        private Button Secondary = null;
        [SerializeField]
        private Button Tertiary = null;

        /// <summary>
        /// The button this mapping represents
        /// </summary>
        public string Button { get; private set; }

        private ButtonKeyRemapStartedDelegate KeyMappingStarted;

        public void SetupItem(string button, KBMInputMap.ButtonMapping mapping, ButtonKeyRemapStartedDelegate keyMappingStarted)
        {
            Button = button;
            KeyMappingStarted = keyMappingStarted;

            TitleText.text = Sub.Replace(Button, "CFG_MAPPINGS");
            Primary.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode((KeyCode)mapping.Primary);
            Secondary.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode((KeyCode)mapping.Secondary);
            Tertiary.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode((KeyCode)mapping.Tertiary);
        }

        public void HandlePrimaryClicked() => KeyMappingStarted(Button, KBMInputButtonKeyType.Primary, Primary);
        public void HandleSecondaryClicked() => KeyMappingStarted(Button, KBMInputButtonKeyType.Secondary, Secondary);
        public void HandleTertiaryClicked() => KeyMappingStarted(Button, KBMInputButtonKeyType.Tertiary, Tertiary);

    }
}