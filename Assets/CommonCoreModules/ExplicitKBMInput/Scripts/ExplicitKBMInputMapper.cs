using CommonCore.Config;
using CommonCore.Input;
using CommonCore.StringSub;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CommonCore.ExplicitKBMInput
{

    /// <summary>
    /// Input mapper that supports remappable KB/M input
    /// </summary>
    public class ExplicitKBMInputMapper : InputMapper
    {
        private const float MouseMoveThreshold = 0.1f; //minimum mouse movement necessary for mouse movement to be considered a "key press"

        //I think we can get rid of this now that we init the mappers later
        private Func<Vector2> GetMouseMovement;

        private Dictionary<string, KBMInputMap.AxisMapping> AxisMappings;
        private Dictionary<string, KBMInputMap.ButtonMapping> ButtonMappings;
        private float MouseXSensitivity;
        private float MouseYSensitivity;
        private float MouseScrollSensitivity;

        private bool UseMapperWarnings;

        public ExplicitKBMInputMapper()
        {
            GetMouseMovement = CCBase.GetModule<InputModule>().GetMouseMovement; //why not?
            //GetMouseMovement = () => { return Vector2.zero; };
            ReloadControlMap();
        }

        public override void Configure()
        {
            UnityEngine.Object.Instantiate(CoreUtils.LoadResource<GameObject>("Modules/ExplicitKBMInput/KBMInputRemapWindow"), CoreUtils.GetUIRoot());
            EventSystem.current.Ref()?.SetSelectedGameObject(null); //deselect the configure button
        }

        public void ReloadControlMap()
        {
            //load the control mappings into our local (fast) data structure from the storage (slow) data structure
            var inputMap = KBMInputMap.CreateOrGetFromConfig();
            AxisMappings = inputMap.AxisMappings;
            ButtonMappings = inputMap.ButtonMappings;
            MouseXSensitivity = inputMap.MouseXSensitivity;
            MouseYSensitivity = inputMap.MouseYSensitivity;
            MouseScrollSensitivity = inputMap.MouseScrollSensitivity;

            UseMapperWarnings = !ConfigState.Instance.SuppressInputMapperWarnings;
        }

        public override float GetAxis(string axis)
        {
            if (AxisMappings.TryGetValue(axis, out var mapping))
            {
                int keyboardValue = 0;
                if (UnityEngine.Input.GetKey((KeyCode)mapping.PrimaryPositive))
                    keyboardValue += 1;
                if (UnityEngine.Input.GetKey((KeyCode)mapping.PrimaryNegative))
                    keyboardValue -= 1;
                if (UnityEngine.Input.GetKey((KeyCode)mapping.SecondaryPositive))
                    keyboardValue += 1;
                if (UnityEngine.Input.GetKey((KeyCode)mapping.SecondaryNegative))
                    keyboardValue -= 1;

                keyboardValue = MathUtils.Clamp(keyboardValue, -1, 1);

                float mouseValue = GetMouseAxis(mapping.MouseAxis, true) * (mapping.InvertMouse ? -1 : 1);

                //Debug.Log($"mouse: {mouseValue:F4}");

                return keyboardValue + mouseValue;
            }
            else if (UseMapperWarnings)
            {
                Debug.LogWarning($"[ExplicitKBMInputMapper] can't find mapping for axis \"{axis}\"");
            }


            return 0;
        }

        public override float GetAxisRaw(string axis)
        {
            if (AxisMappings.TryGetValue(axis, out var mapping))
            {
                int keyboardValue = 0;
                if (UnityEngine.Input.GetKey((KeyCode)mapping.PrimaryPositive))
                    keyboardValue += 1;
                if (UnityEngine.Input.GetKey((KeyCode)mapping.PrimaryNegative))
                    keyboardValue -= 1;
                if (UnityEngine.Input.GetKey((KeyCode)mapping.SecondaryPositive))
                    keyboardValue += 1;
                if (UnityEngine.Input.GetKey((KeyCode)mapping.SecondaryNegative))
                    keyboardValue -= 1;

                float mouseValue = GetMouseAxis(mapping.MouseAxis, false) * (mapping.InvertMouse ? -1 : 1);

                return keyboardValue + mouseValue;
            }
            else if(UseMapperWarnings)
            {
                Debug.LogWarning($"[ExplicitKBMInputMapper] can't find mapping for axis \"{axis}\"");
            }

            return 0;
        }

        public override bool GetButton(string button)
        {
            if(ButtonMappings.TryGetValue(button, out var mapping))
            {
                return UnityEngine.Input.GetKey((KeyCode)mapping.Primary) || UnityEngine.Input.GetKey((KeyCode)mapping.Secondary) || UnityEngine.Input.GetKey((KeyCode)mapping.Tertiary);
            }
            else if(AxisMappings.TryGetValue(button, out var axisMapping))
            {
                return UnityEngine.Input.GetKey((KeyCode)axisMapping.PrimaryPositive) || UnityEngine.Input.GetKey((KeyCode)axisMapping.PrimaryNegative) || UnityEngine.Input.GetKey((KeyCode)axisMapping.SecondaryPositive) || UnityEngine.Input.GetKey((KeyCode)axisMapping.SecondaryNegative) || Mathf.Abs(GetMouseAxis(axisMapping.MouseAxis, false)) > MouseMoveThreshold;
            }
            else if(UseMapperWarnings)
            {
                Debug.LogWarning($"[ExplicitKBMInputMapper] can't find mapping for button \"{button}\"");
            }

            return false;
        }

        public override bool GetButtonDown(string button)
        {
            if (ButtonMappings.TryGetValue(button, out var mapping))
            {
                return UnityEngine.Input.GetKeyDown((KeyCode)mapping.Primary) || UnityEngine.Input.GetKeyDown((KeyCode)mapping.Secondary) || UnityEngine.Input.GetKeyDown((KeyCode)mapping.Tertiary);
            }
            else if (AxisMappings.TryGetValue(button, out var axisMapping))
            {
                return UnityEngine.Input.GetKeyDown((KeyCode)axisMapping.PrimaryPositive) || UnityEngine.Input.GetKeyDown((KeyCode)axisMapping.PrimaryNegative) || UnityEngine.Input.GetKeyDown((KeyCode)axisMapping.SecondaryPositive) || UnityEngine.Input.GetKeyDown((KeyCode)axisMapping.SecondaryNegative) || Mathf.Abs(GetMouseAxis(axisMapping.MouseAxis, false)) > MouseMoveThreshold;
            }
            else if (UseMapperWarnings)
            {
                Debug.LogWarning($"[ExplicitKBMInputMapper] can't find mapping for button \"{button}\"");
            }

            return false;
        }

        public override bool GetButtonUp(string button)
        {
            if (ButtonMappings.TryGetValue(button, out var mapping))
            {
                return UnityEngine.Input.GetKeyUp((KeyCode)mapping.Primary) || UnityEngine.Input.GetKeyUp((KeyCode)mapping.Secondary) || UnityEngine.Input.GetKeyUp((KeyCode)mapping.Tertiary);
            }
            else if (AxisMappings.TryGetValue(button, out var axisMapping))
            {
                return UnityEngine.Input.GetKeyUp((KeyCode)axisMapping.PrimaryPositive) || UnityEngine.Input.GetKeyUp((KeyCode)axisMapping.PrimaryNegative) || UnityEngine.Input.GetKeyUp((KeyCode)axisMapping.SecondaryPositive) || UnityEngine.Input.GetKeyUp((KeyCode)axisMapping.SecondaryNegative) || Mathf.Abs(GetMouseAxis(axisMapping.MouseAxis, false)) < MouseMoveThreshold;
            }
            else if (UseMapperWarnings)
            {
                Debug.LogWarning($"[ExplicitKBMInputMapper] can't find mapping for button \"{button}\"");
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetMouseAxis(MouseAxis axis, bool applySensitivity)
        {
            switch (axis)
            {
                case MouseAxis.Horizontal:
                    return GetMouseMovement().x * (applySensitivity ? MouseXSensitivity : 1);
                case MouseAxis.Vertical:
                    return GetMouseMovement().y * (applySensitivity ? MouseYSensitivity : 1);
                case MouseAxis.Scroll:
                    return UnityEngine.Input.mouseScrollDelta.y * (applySensitivity ? MouseScrollSensitivity : 1);
                default:
                    return 0;
            }
        }

        public override MappingDescriptor GetDescriptorForAxis(string axis, AxisDirection direction)
        {
            if(AxisMappings.TryGetValue(axis, out var axisMapping))
            {
                var descriptors = new List<SingleMappingDescriptor>();
                if (axisMapping.MouseAxis != MouseAxis.Undefined)
                    descriptors.Add(new SingleMappingDescriptor((axisMapping.MouseAxis).ToString(), Sub.Replace(Enum.GetName(typeof(ExplicitKBMInput.MouseAxis), axisMapping.MouseAxis), "EXPLICITKBMINPUT_MOUSEAXIS"), Sub.Replace(Enum.GetName(typeof(ExplicitKBMInput.MouseAxis), axisMapping.MouseAxis), "EXPLICITKBMINPUT_MOUSEAXIS")));

                switch (direction)
                {
                    case AxisDirection.Any:
                        {
                            //TODO improve this, it'll be ugly
                            if (axisMapping.PrimaryPositive != 0)
                                descriptors.Add(new SingleMappingDescriptor(axisMapping.PrimaryPositive.ToString(), InputModule.GetNameForKeyCode((KeyCode)axisMapping.PrimaryPositive), InputModule.GetNameForKeyCode((KeyCode)axisMapping.PrimaryPositive)));
                            if (axisMapping.PrimaryNegative != 0)
                                descriptors.Add(new SingleMappingDescriptor(axisMapping.PrimaryNegative.ToString(), InputModule.GetNameForKeyCode((KeyCode)axisMapping.PrimaryNegative), InputModule.GetNameForKeyCode((KeyCode)axisMapping.PrimaryNegative)));
                            if (axisMapping.SecondaryPositive != 0)
                                descriptors.Add(new SingleMappingDescriptor(axisMapping.SecondaryPositive.ToString(), InputModule.GetNameForKeyCode((KeyCode)axisMapping.SecondaryPositive), InputModule.GetNameForKeyCode((KeyCode)axisMapping.SecondaryPositive)));
                            if (axisMapping.SecondaryNegative != 0)
                                descriptors.Add(new SingleMappingDescriptor(axisMapping.SecondaryNegative.ToString(), InputModule.GetNameForKeyCode((KeyCode)axisMapping.SecondaryNegative), InputModule.GetNameForKeyCode((KeyCode)axisMapping.SecondaryNegative)));
                        }
                        break;
                    case AxisDirection.Negative:
                        {
                            if (axisMapping.PrimaryNegative != 0)
                                descriptors.Add(new SingleMappingDescriptor(axisMapping.PrimaryNegative.ToString(), InputModule.GetNameForKeyCode((KeyCode)axisMapping.PrimaryNegative), InputModule.GetNameForKeyCode((KeyCode)axisMapping.PrimaryNegative)));
                            if (axisMapping.SecondaryNegative != 0)
                                descriptors.Add(new SingleMappingDescriptor(axisMapping.SecondaryNegative.ToString(), InputModule.GetNameForKeyCode((KeyCode)axisMapping.SecondaryNegative), InputModule.GetNameForKeyCode((KeyCode)axisMapping.SecondaryNegative)));
                        }
                        break;
                    case AxisDirection.Positive:
                        {
                            if(axisMapping.PrimaryPositive != 0)
                                descriptors.Add(new SingleMappingDescriptor(axisMapping.PrimaryPositive.ToString(), InputModule.GetNameForKeyCode((KeyCode)axisMapping.PrimaryPositive), InputModule.GetNameForKeyCode((KeyCode)axisMapping.PrimaryPositive)));
                            if (axisMapping.SecondaryPositive != 0)
                                descriptors.Add(new SingleMappingDescriptor(axisMapping.SecondaryPositive.ToString(), InputModule.GetNameForKeyCode((KeyCode)axisMapping.SecondaryPositive), InputModule.GetNameForKeyCode((KeyCode)axisMapping.SecondaryPositive)));
                        }
                        break;
                }

                return new MappingDescriptor(descriptors);
            }
            return new MappingDescriptor();
        }

        public override MappingDescriptor GetDescriptorForButton(string button)
        {
            if (ButtonMappings.TryGetValue(button, out var buttonMapping))
            {
                var descriptors = new List<SingleMappingDescriptor>();

                //should we run these checks or just return "none" when a mapping maps to nothing?
                if (buttonMapping.Primary != 0)
                    descriptors.Add(new SingleMappingDescriptor(buttonMapping.Primary.ToString(), InputModule.GetNameForKeyCode((KeyCode)buttonMapping.Primary), InputModule.GetNameForKeyCode((KeyCode)buttonMapping.Primary)));
                if (buttonMapping.Secondary != 0)
                    descriptors.Add(new SingleMappingDescriptor(buttonMapping.Secondary.ToString(), InputModule.GetNameForKeyCode((KeyCode)buttonMapping.Secondary), InputModule.GetNameForKeyCode((KeyCode)buttonMapping.Secondary)));
                if (buttonMapping.Tertiary != 0)
                    descriptors.Add(new SingleMappingDescriptor(buttonMapping.Tertiary.ToString(), InputModule.GetNameForKeyCode((KeyCode)buttonMapping.Tertiary), InputModule.GetNameForKeyCode((KeyCode)buttonMapping.Tertiary)));

                return new MappingDescriptor(descriptors);
            }
            return new MappingDescriptor();
        }
    }
}