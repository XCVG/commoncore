using CommonCore.Config;
using CommonCore.DebugLog;
using CommonCore.StringSub;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommonCore.Input
{

    /// <summary>
    /// CommonCore Input Module. Initializes and manages mapped input system.
    /// </summary>
    public class InputModule : CCModule
    {

        public InputModule()
        {
            //set null mapper
            MappedInput.SetMapper(new NullInputMapper());

            //dump keycodes to file for future use
            if(CoreParams.IsDebug)
                DumpKeycodes();
        }

        public override void OnAllModulesLoaded()
        {
            //let's do this late in case mappers rely on other modules

            //find all mappers and set them in MappedInput
            var mapperTypes = CCBase.BaseGameTypes
                .Where(t => typeof(InputMapper).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Where(t => t != typeof(NullInputMapper))
                .ToArray();
            foreach (Type t in mapperTypes)
            {
                MappedInput.Mappers.Add(t.Name, t);
            }

            Log("Mappers: " + mapperTypes.ToNiceString());

            //attempt to set configured or default mapper
            try
            {
                if (!string.IsNullOrEmpty(ConfigState.Instance.InputMapper) && MappedInput.Mappers.ContainsKey(ConfigState.Instance.InputMapper))
                {
                    Log("Setting configured mapper " + ConfigState.Instance.InputMapper);
                    MappedInput.SetMapper(ConfigState.Instance.InputMapper);
                }
                else
                {
                    Log("Setting default mapper UnityInputMapper");
                    MappedInput.SetMapper(new UnityInputMapper());
                }

            }
            catch (Exception e)
            {
                LogError("Failed to load mapper!");
                LogException(e);
            }
        }

        /// <summary>
        /// Gets the mouse movement since the last frame
        /// </summary>
        public Vector2 GetMouseMovement()
        {
            //someday we'll provide platform-native mouse
            return new Vector2(UnityEngine.Input.GetAxisRaw("Mouse X"), UnityEngine.Input.GetAxisRaw("Mouse Y"));
        }

        /// <summary>
        /// Gets a collection of mappable axes
        /// </summary>
        /// <remarks>
        /// <para>For use by input mappers, and up to them to handle it</para>
        /// </remarks>
        public IReadOnlyCollection<string> GetMappableAxes()
        {
            //very much non-optimized

            List<string> mappableAxes = new List<string>();

            //get axes from DefaultControls
            {
                var fields = typeof(DefaultControls).GetFields();
                foreach(var field in fields)
                {
                    if (field.GetCustomAttribute<ControlIsAxisAttribute>() != null)
                        mappableAxes.Add((string)field.GetValue(null));
                }
            }

            //get axes from AdditionalAxes
            {
                mappableAxes.AddRange(CoreParams.AdditionalAxes);
            }

            //remove ignored controls
            {
                mappableAxes = mappableAxes.Except(CoreParams.HideControlMappings).ToList();
            }

            return mappableAxes.Distinct().ToList();
        }

        /// <summary>
        /// Gets a list of mappable buttons
        /// </summary>
        /// <remarks>For use by input mappers, and up to them to handle it</remarks>
        public List<string> GetMappableButtons()
        {
            //very much non-optimized

            List<string> mappableButtons = new List<string>();

            //get axes from DefaultControls
            {
                var fields = typeof(DefaultControls).GetFields();
                foreach (var field in fields)
                {
                    if (field.GetCustomAttribute<ControlIsAxisAttribute>() == null)
                        mappableButtons.Add((string)field.GetValue(null));
                }
            }

            //get axes from AdditionalButtons+
            {
                mappableButtons.AddRange(CoreParams.AdditionalButtons);
            }

            //remove ignored controls
            {
                mappableButtons = mappableButtons.Except(CoreParams.HideControlMappings).ToList();
            }

            return mappableButtons.Distinct().ToList();
        }

        private void DumpKeycodes()
        {            
            var keycodes = Enum.GetValues(typeof(KeyCode));
            StringBuilder sb = new StringBuilder(keycodes.Length * 16);
            foreach (int keycode in keycodes)
            {
                string keyname = Enum.GetName(typeof(KeyCode), keycode);
                sb.AppendLine($"{keyname},{keycode}");
            }
            string path = Path.Combine(CoreParams.PersistentDataPath, "keycodes.csv");
            File.WriteAllText(path, sb.ToString());
            Log($"Dumped {keycodes.Length} key codes to {path}");
        }

        /// <summary>
        /// Gets the nice name for a KeyCode
        /// </summary>
        /// <remarks>
        /// <para>Prefers, in order: a name from the CFG_KEYNAMES string list, the defined name for the value, the KeyCode number as a string</para>
        /// </remarks>
        public static string GetNameForKeyCode(KeyCode key)
        {
            string keyNumberString = ((int)key).ToString();
            if (Sub.Exists(keyNumberString, "CFG_KEYNAME"))
            {
                return Sub.Replace(keyNumberString, "CFG_KEYNAME");
            }

            return Enum.GetName(typeof(KeyCode), key) ?? keyNumberString;
        }

    }
}