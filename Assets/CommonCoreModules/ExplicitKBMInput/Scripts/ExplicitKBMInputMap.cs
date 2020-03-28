using CommonCore.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ExplicitKBMInput
{
    public enum MouseAxis
    {
        Undefined, Horizontal, Vertical, Scroll
    }

    public class KBMInputMap : ICloneable
    {
        //axis mapping, button mapping, mouse x/y/scroll sensitivity
        [JsonProperty]
        public Dictionary<string, AxisMapping> AxisMappings { get; private set; } = new Dictionary<string, AxisMapping>();
        [JsonProperty]
        public Dictionary<string, ButtonMapping> ButtonMappings { get; private set; } = new Dictionary<string, ButtonMapping>();
        public float MouseXSensitivity { get; set; } = 1.0f;
        public float MouseYSensitivity { get; set; } = 1.0f;
        public float MouseScrollSensitivity { get; set; } = 1.0f;

        public static KBMInputMap CreateOrGetFromConfig()
        {
            //try to get it first
            if (ConfigState.Instance.CustomConfigVars.ContainsKey("ExplicitKBMInputMap") && ConfigState.Instance.CustomConfigVars["ExplicitKBMInputMap"] is KBMInputMap savedInputMap)
            {
                Debug.Log("[ExplicitKBMInput] Loaded saved input map!");
                return savedInputMap;
            }

            //create new KBMInputMap, loading default if available and skipping if not
            KBMInputMap inputMap;

            var res = CoreUtils.LoadResource<TextAsset>("Modules/ExplicitKBMInput/DefaultControls");
            if(res != null)
            {                
                try
                {
                    inputMap = CoreUtils.LoadJson<KBMInputMap>(res.text);
                    Debug.Log("[ExplicitKBMInput] Loaded default input map!");
                }
                catch(Exception e)
                {
                    Debug.LogError("[ExplicitKBMInput] Failed to load default input map, inputs will start unmapped!");
                    Debug.LogException(e);
                    inputMap = new KBMInputMap();
                }
            }
            else
            {
                Debug.LogWarning("[ExplicitKBMInput] Couldn't find default input map, inputs will start unmapped!");
                inputMap = new KBMInputMap();
            }

            //save to config
            inputMap.SaveToConfig();           

            return inputMap;
        }

        public KBMInputMap()
        {
            //TODO setup some temporary mappings, just for testing
            AxisMappings.Add("NavigateX", new AxisMapping(KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.D, KeyCode.A, MouseAxis.Undefined, false));
            AxisMappings.Add("NavigateY", new AxisMapping(KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.W, KeyCode.S, MouseAxis.Undefined, false));
            ButtonMappings.Add("Submit", new ButtonMapping(KeyCode.Return, KeyCode.Space, KeyCode.E));
            ButtonMappings.Add("Cancel", new ButtonMapping(KeyCode.Escape, KeyCode.Backspace, KeyCode.None));
        }

        public object Clone()
        {
            KBMInputMap newMap = new KBMInputMap();
            newMap.AxisMappings = new Dictionary<string, AxisMapping>(AxisMappings);
            newMap.ButtonMappings = new Dictionary<string, ButtonMapping>(ButtonMappings);
            newMap.MouseScrollSensitivity = MouseScrollSensitivity;
            newMap.MouseXSensitivity = MouseXSensitivity;
            newMap.MouseYSensitivity = MouseYSensitivity;
            return newMap;
        }

        public void SaveToConfig()
        {
            ConfigState.Instance.CustomConfigVars["ExplicitKBMInputMap"] = this;
        }

        public struct AxisMapping
        {
            public int PrimaryPositive;
            public int PrimaryNegative;
            public int SecondaryPositive;
            public int SecondaryNegative;
            public MouseAxis MouseAxis;
            public bool InvertMouse;

            public AxisMapping (int primaryPositive, int primaryNegative, int secondaryPositive, int secondaryNegative, MouseAxis mouseAxis, bool invertMouse)
            {
                PrimaryPositive = primaryPositive;
                PrimaryNegative = primaryNegative;
                SecondaryPositive = secondaryPositive;
                SecondaryNegative = secondaryNegative;
                MouseAxis = mouseAxis;
                InvertMouse = invertMouse;
            }

            public AxisMapping(KeyCode primaryPositive, KeyCode primaryNegative, KeyCode secondaryPositive, KeyCode secondaryNegative, MouseAxis mouseAxis, bool invertMouse) :
                this((int)primaryPositive, (int)primaryNegative, (int)secondaryPositive, (int)secondaryNegative, mouseAxis, invertMouse)
            {

            }

        }

        public struct ButtonMapping
        {
            public int Primary;
            public int Secondary;
            public int Tertiary;

            public ButtonMapping(int primary, int secondary, int tertiary)
            {
                Primary = primary;
                Secondary = secondary;
                Tertiary = tertiary;
            }

            public ButtonMapping(KeyCode primary, KeyCode secondary, KeyCode tertiary) : this((int)primary, (int)secondary, (int)tertiary)
            {
                
            }
        }

    }
}