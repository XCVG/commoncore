using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;
using System.Linq;

namespace CommonCore.Input
{
    /*
     * Mapped Input static class
     * In-place Input replacement that provides redirection via Mappers
     * Based on the system in Firefighter VR, not what was in ARES
     */
    public static class MappedInput
    {
        private static InputMapper Mapper;

        internal static Dictionary<string, Type> Mappers { get; private set; } = new Dictionary<string, Type>();
        
        /// <summary>
        /// Available input mappers
        /// </summary>
        public static string[] AvailableMappers => Mappers.Keys.ToArray();

        /// <summary>
        /// Set the current input mapper to a different one
        /// </summary>
        public static void SetMapper(string newMapper)
        {
            if (!Mappers.ContainsKey(newMapper))
            {
                Debug.LogWarning($"[Input] Can't find mapper \"{newMapper}\"");
                throw new ArgumentOutOfRangeException();
            }

            Type newMapperType = Mappers[newMapper];

            if (Mapper != null && Mapper.GetType() == newMapperType)
                return;

            if (Mapper is IDisposable dMapper)
                dMapper.Dispose();

            Mapper = (InputMapper)Activator.CreateInstance(newMapperType);
        }

        internal static void SetMapper(InputMapper newMapper)
        {
            CDebug.LogEx(string.Format("[Input] Set mapper to {0}", newMapper.GetType().Name), LogLevel.Message, typeof(MappedInput));
            Mapper = newMapper;
        }

        /// <summary>
        /// Opens mapper configuration panel, if it exists
        /// </summary>
        public static void ConfigureMapper() //TODO options?
        {
            Mapper.Configure();
        }

        public static float GetAxis(string axis)
        {
            return Mapper.GetAxis(axis);
        }

        public static bool GetButton(string button)
        {
            return Mapper.GetButton(button);
        }

        public static bool GetButtonDown(string button)
        {
            return Mapper.GetButtonDown(button);
        }

        public static bool GetButtonUp(string button)
        {
            return Mapper.GetButtonUp(button);
        }
    }
}