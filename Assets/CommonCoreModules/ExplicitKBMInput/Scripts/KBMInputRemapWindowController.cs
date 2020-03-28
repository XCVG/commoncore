using CommonCore;
using CommonCore.Config;
using CommonCore.Input;
using CommonCore.LockPause;
using CommonCore.StringSub;
using CommonCore.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.ExplicitKBMInput
{

    /// <summary>
    /// Controller for the input remapping window
    /// </summary>
    public class KBMInputRemapWindowController : PanelController
    {
        [SerializeField, Header("Prefabs")]
        private GameObject AxisMappingItemPrefab = null;
        [SerializeField]
        private GameObject ButtonMappingItemPrefab = null;

        [SerializeField, Header("Elements")]
        private RectTransform AxisScrollContent = null;
        [SerializeField]
        private RectTransform ButtonScrollContent = null;
        [SerializeField]
        private KBMMappingModalController MappingModal = null;

        [SerializeField, Header("Mouse Sensitivity")]
        private Slider MouseXSlider = null;
        [SerializeField]
        private Slider MouseYSlider = null;
        [SerializeField]
        private Slider MouseScrollSlider = null;


        private bool IgnoreSliderValueChanged = false; //probably not necessary but a good guard and antipattern

        private KBMInputMap InputMap;

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();

            LockPauseModule.LockControls(InputLockType.All, this);

            LoadControlMap();
            SetupControlList();
        }

        public override void SignalFinalUnpaint()
        {
            base.SignalFinalUnpaint();

            LockPauseModule.UnlockControls(this);
        }

        private void LoadControlMap()
        {
            //load the control map from ConfigState into local state
            InputMap = (KBMInputMap)KBMInputMap.CreateOrGetFromConfig().Clone(); //I think that's it lol
        }

        private void SetupControlList()
        {
            //layout all the controls from DefaultControls and AdditionalControls, setting up their mappings also

            //set up blank entries for everything that _should_ be defined in InputMap, but isn't
            var inputModule = CCBase.GetModule<InputModule>();
            var mappableAxes = inputModule.GetMappableAxes();
            var mappableButtons = inputModule.GetMappableButtons();
            //Debug.Log(mappableAxes.ToNiceString());
            //Debug.Log(mappableButtons.ToNiceString());
            {
                foreach (string axis in mappableAxes)
                {
                    if (!InputMap.AxisMappings.ContainsKey(axis))
                        InputMap.AxisMappings.Add(axis, new KBMInputMap.AxisMapping(KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, MouseAxis.Undefined, false));
                }

                foreach(string button in mappableButtons)
                {
                    if (!InputMap.ButtonMappings.ContainsKey(button))
                        InputMap.ButtonMappings.Add(button, new KBMInputMap.ButtonMapping(KeyCode.None, KeyCode.None, KeyCode.None));
                }
            }

            //start by painting the sliders
            IgnoreSliderValueChanged = true;
            MouseXSlider.value = InputMap.MouseXSensitivity;
            MouseYSlider.value = InputMap.MouseYSensitivity;
            MouseScrollSlider.value = InputMap.MouseScrollSensitivity;
            IgnoreSliderValueChanged = false;

            //then paint the axis list
            {
                AxisScrollContent.DestroyAllChildren();

                foreach (string axis in mappableAxes)
                {
                    var go = Instantiate(AxisMappingItemPrefab, AxisScrollContent);
                    var item = go.GetComponent<KBMAxisMappingItem>();
                    var mapping = InputMap.AxisMappings[axis];
                    item.SetupItem(axis, mapping, HandleMouseAxisChanged, HandleMouseInvertChanged, HandleAxisKeyRemappingStarted);
                }
            }

            //finally, paint the button list
            {
                ButtonScrollContent.DestroyAllChildren();

                foreach (string button in mappableButtons)
                {
                    var go = Instantiate(ButtonMappingItemPrefab, ButtonScrollContent);
                    var item = go.GetComponent<KBMButtonMappingItem>();
                    var mapping = InputMap.ButtonMappings[button];
                    item.SetupItem(button, mapping, HandleButtonKeyRemappingStarted);
                }
            }
        }

        private void SaveControlMap()
        {
            //commit our modified control map to ConfigState
            InputMap.SaveToConfig();
            if (MappedInput.GetCurrentMapper() is ExplicitKBMInputMapper kMapper)
                kMapper.ReloadControlMap();
            ConfigState.Save(); //do we want this? probably
        }

        public void HandleConfirmButtonClicked()
        {
            SaveControlMap();
            Destroy(gameObject);
        }

        public void HandleCancelButtonClicked()
        {
            Destroy(gameObject);
        }

        public void HandleMouseXSliderChanged(float value)
        {
            if (IgnoreSliderValueChanged || InputMap == null)
                return;

            InputMap.MouseXSensitivity = value;
        }

        public void HandleMouseYSliderChanged(float value)
        {
            if (IgnoreSliderValueChanged || InputMap == null)
                return;

            InputMap.MouseYSensitivity = value;
        }

        public void HandleMouseScrollSliderChanged(float value)
        {
            if (IgnoreSliderValueChanged || InputMap == null)
                return;

            InputMap.MouseScrollSensitivity = value;
        }

        //update the backing data for an axis's MouseAxis value
        private void HandleMouseAxisChanged(string axisName, MouseAxis value, Dropdown dropdown)
        {
            if(InputMap.AxisMappings.TryGetValue(axisName, out var axisMapping))
            {
                axisMapping.MouseAxis = value;
                InputMap.AxisMappings[axisName] = axisMapping; //since it's a value type, we need to insert our modified copy back
            }
            else
            {
                Debug.LogWarning($"Couldn't find a mapping for axis \"{axisName}\"");
            }
        }

        //updates the backing data for an axis's MouseInvert value
        private void HandleMouseInvertChanged(string axisName, bool value)
        {
            if (InputMap.AxisMappings.TryGetValue(axisName, out var axisMapping))
            {
                axisMapping.InvertMouse = value;
                InputMap.AxisMappings[axisName] = axisMapping; //since it's a value type, we need to insert our modified copy back
            }
            else
            {
                Debug.LogWarning($"Couldn't find a mapping for axis \"{axisName}\"");
            }
        }

        //open the "change mapping" window and do all that (for axis keys)
        private void HandleAxisKeyRemappingStarted(string axisName, KBMInputAxisKeyType keyType, Button button)
        {
            string displayName = $"{Sub.Replace(axisName, "CFG_MAPPINGS")} {((keyType == KBMInputAxisKeyType.PrimaryPositive || keyType == KBMInputAxisKeyType.SecondaryPositive) ? Sub.Replace("+", "EXPLICITKBMINPUT") : Sub.Replace("-", "EXPLICITKBMINPUT"))}";
            KeyCode oldKey = GetKeyForAxis(axisName, keyType);

            MappingModal.GetMapping(displayName, oldKey, (newKey) => {
                if (newKey.HasValue)
                {
                    //Debug.Log(newKey);

                    SetKeyOnAxis(newKey.Value, axisName, keyType);
                    button.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode(newKey.Value);
                }
            });
        }

        //open the "change mapping" window and do all that (for button keys)
        private void HandleButtonKeyRemappingStarted(string buttonName, KBMInputButtonKeyType keyType, Button button)
        {
            string displayName = Sub.Replace(buttonName, "CFG_MAPPINGS");
            KeyCode oldKey = GetKeyForButton(buttonName, keyType);

            MappingModal.GetMapping(displayName, oldKey, (newKey) => {
                if (newKey.HasValue)
                {
                   // Debug.Log(newKey);

                    SetKeyOnButton(newKey.Value, buttonName, keyType);
                    button.GetComponentInChildren<Text>().text = InputModule.GetNameForKeyCode(newKey.Value);
                }
            });
        }        

        private KeyCode GetKeyForButton(string button, KBMInputButtonKeyType keyType)
        {
            if(InputMap.ButtonMappings.TryGetValue(button, out var mapping))
            {
                switch (keyType)
                {
                    case KBMInputButtonKeyType.Primary:
                        return (KeyCode)mapping.Primary;
                    case KBMInputButtonKeyType.Secondary:
                        return (KeyCode)mapping.Secondary;
                    case KBMInputButtonKeyType.Tertiary:
                        return (KeyCode)mapping.Tertiary;
                    default:
                        throw new KeyNotFoundException();
                }
            }
            else
            {
                Debug.LogWarning($"Couldn't find a mapping for button \"{button}\"");
                return KeyCode.None;
            }
        }

        private void SetKeyOnButton(KeyCode key, string button, KBMInputButtonKeyType keyType)
        {
            if (InputMap.ButtonMappings.TryGetValue(button, out var mapping))
            {
                switch (keyType)
                {
                    case KBMInputButtonKeyType.Primary:
                        mapping.Primary = (int)key;
                        break;
                    case KBMInputButtonKeyType.Secondary:
                        mapping.Secondary = (int)key;
                        break;
                    case KBMInputButtonKeyType.Tertiary:
                        mapping.Tertiary = (int)key;
                        break;
                    default:
                        throw new KeyNotFoundException();
                }

                InputMap.ButtonMappings[button] = mapping; //value types, folks!
            }
            else
            {
                Debug.LogWarning($"Couldn't find a mapping for button \"{button}\"");
            }
        }

        private KeyCode GetKeyForAxis(string axis, KBMInputAxisKeyType keyType)
        {
            if(InputMap.AxisMappings.TryGetValue(axis, out var mapping))
            {
                switch (keyType)
                {
                    case KBMInputAxisKeyType.PrimaryPositive:
                        return (KeyCode)mapping.PrimaryPositive;
                    case KBMInputAxisKeyType.PrimaryNegative:
                        return (KeyCode)mapping.PrimaryNegative;
                    case KBMInputAxisKeyType.SecondaryPositive:
                        return (KeyCode)mapping.SecondaryPositive;
                    case KBMInputAxisKeyType.SecondaryNegative:
                        return (KeyCode)mapping.SecondaryNegative;
                    default:
                        throw new KeyNotFoundException();
                }
            }
            else
            {
                Debug.LogWarning($"Couldn't find a mapping for axis \"{axis}\"");
                return KeyCode.None;
            }
        }

        private void SetKeyOnAxis(KeyCode key, string axis, KBMInputAxisKeyType keyType)
        {
            if (InputMap.AxisMappings.TryGetValue(axis, out var mapping))
            {
                switch (keyType)
                {
                    case KBMInputAxisKeyType.PrimaryPositive:
                        mapping.PrimaryPositive = (int)key;
                        break;
                    case KBMInputAxisKeyType.PrimaryNegative:
                        mapping.PrimaryNegative = (int)key;
                        break;
                    case KBMInputAxisKeyType.SecondaryPositive:
                        mapping.SecondaryPositive = (int)key;
                        break;
                    case KBMInputAxisKeyType.SecondaryNegative:
                        mapping.SecondaryNegative = (int)key;
                        break;
                    default:
                        throw new KeyNotFoundException();
                }

                InputMap.AxisMappings[axis] = mapping; //value types, folks!
            }
            else
            {
                Debug.LogWarning($"Couldn't find a mapping for axis \"{axis}\"");
            }
        }

    }

    public delegate void MouseInvertChangedDelegate(string axisName, bool value);
    public delegate void MouseAxisChangedDelegate(string axisName, MouseAxis value, Dropdown dropdown);
    public delegate void AxisKeyRemapStartedDelegate(string axisName, KBMInputAxisKeyType keyType, Button button);
    public delegate void ButtonKeyRemapStartedDelegate(string axisName, KBMInputButtonKeyType keyType, Button button);

    public enum KBMInputButtonKeyType
    {
        Primary, Secondary, Tertiary
    }

    public enum KBMInputAxisKeyType
    {
        PrimaryPositive, PrimaryNegative, SecondaryPositive, SecondaryNegative
    }
}