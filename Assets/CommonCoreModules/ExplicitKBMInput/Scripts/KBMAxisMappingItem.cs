using CommonCore.Input;
using CommonCore.StringSub;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.ExplicitKBMInput
{

    /// <summary>
    /// Script attached to an item in the axis list of the input remap window
    /// </summary>
    /// <remarks>We do a little bit of conversion and stuff here but mostly just "kick stuff upstairs"</remarks>
    public class KBMAxisMappingItem : MonoBehaviour
    {
        [SerializeField]
        private Text TitleText = null;
        [SerializeField]
        private Button PrimaryPositive = null;
        [SerializeField]
        private Button PrimaryNegative = null;
        [SerializeField]
        private Button SecondaryPositive = null;
        [SerializeField]
        private Button SecondaryNegative = null;
        [SerializeField]
        private Dropdown MouseAxis = null;
        [SerializeField]
        private Toggle MouseInvert = null;
        
        /// <summary>
        /// The axis this mapping represents
        /// </summary>
        public string Axis { get; private set; }

        private List<MouseAxis> MouseAxisValues = new List<MouseAxis>();

        private bool IgnoreMouseAxisValueChanged = false;
        private bool IgnoreMouseInvertValueChanged = false;

        private MouseAxisChangedDelegate MouseAxisChanged;
        private MouseInvertChangedDelegate MouseInvertChanged;
        private AxisKeyRemapStartedDelegate KeyMappingStarted;

        /// <summary>
        /// Sets up this data row
        /// </summary>
        public void SetupItem(string axis, KBMInputMap.AxisMapping mapping, MouseAxisChangedDelegate mouseAxisChanged, MouseInvertChangedDelegate mouseInvertChanged, AxisKeyRemapStartedDelegate keyMappingStarted)
        {
            Axis = axis;
            MouseAxisChanged = mouseAxisChanged;
            MouseInvertChanged = mouseInvertChanged;
            KeyMappingStarted = keyMappingStarted;

            //setup mouseaxis dropdown           
            var allAxis = Enum.GetValues(typeof(ExplicitKBMInput.MouseAxis));
            List<string> options = new List<string>();
            foreach(int a in allAxis)
            {
                options.Add(Sub.Replace(Enum.GetName(typeof(ExplicitKBMInput.MouseAxis), a), "EXPLICITKBMINPUT_MOUSEAXIS"));
                MouseAxisValues.Add((ExplicitKBMInput.MouseAxis)a); //we can cast it because we know it maps
            }
            IgnoreMouseAxisValueChanged = true;
            MouseAxis.ClearOptions();
            MouseAxis.AddOptions(options);
            IgnoreMouseAxisValueChanged = false;

            //set values of all the things
            TitleText.text = Sub.Replace(Axis, "CFG_MAPPINGS");
            PrimaryPositive.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode((KeyCode)mapping.PrimaryPositive);
            PrimaryNegative.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode((KeyCode)mapping.PrimaryNegative);
            SecondaryPositive.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode((KeyCode)mapping.SecondaryPositive);
            SecondaryNegative.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode((KeyCode)mapping.SecondaryNegative);
            SetMouseAxis(MouseAxisValues.IndexOf(mapping.MouseAxis));
            SetMouseInvert(mapping.InvertMouse);
        }

        private void SetMouseAxis(int value)
        {
            IgnoreMouseAxisValueChanged = true;
            MouseAxis.value = value;
            IgnoreMouseAxisValueChanged = false;
        }

        private void SetMouseInvert(bool value)
        {
            IgnoreMouseInvertValueChanged = true;
            MouseInvert.isOn = value;
            IgnoreMouseInvertValueChanged = false;
        }

        public void HandleMouseAxisValueChanged()
        {
            if (IgnoreMouseAxisValueChanged)
                return;

            //fire the delegate
            var convertedValue = MouseAxisValues[MouseAxis.value];
            MouseAxisChanged(Axis, convertedValue, MouseAxis);
        }

        public void HandleMouseInvertValueChanged()
        {
            if (IgnoreMouseInvertValueChanged)
                return;

            //fire the delegate
            MouseInvertChanged(Axis, MouseInvert.isOn);
        }

        public void HandlePrimaryPositiveClicked() => KeyMappingStarted(Axis, KBMInputAxisKeyType.PrimaryPositive, PrimaryPositive);

        public void HandlePrimaryNegativeClicked() => KeyMappingStarted(Axis, KBMInputAxisKeyType.PrimaryNegative, PrimaryNegative);

        public void HandleSecondaryPositiveClicked() => KeyMappingStarted(Axis, KBMInputAxisKeyType.SecondaryPositive, SecondaryPositive);

        public void HandleSecondaryNegativeClicked() => KeyMappingStarted(Axis, KBMInputAxisKeyType.SecondaryNegative, SecondaryNegative);



    }

}