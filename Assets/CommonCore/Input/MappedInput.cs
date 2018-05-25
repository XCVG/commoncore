using CommonCore.DebugLog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        internal static void SetMapper(InputMapper newMapper)
        {
            CDebug.LogEx(string.Format("Set mapper to {0}", newMapper.GetType().Name), LogType.Log, typeof(MappedInput));
            Mapper = newMapper;
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