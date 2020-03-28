using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CommonCore.Input
{
    /// <summary>
    /// Component for MappedInputModule/StandaloneInputModule that hooks into MappedInput system
    /// </summary>
    public class MappedInputComponent : BaseInput
    {
        //we leave the mouse and touch stuff alone, but hook axes and buttons to MappedInput backend

        [field: SerializeField]
        public bool UseDualInput { get; set; } = false;

        public override float GetAxisRaw(string axisName)
        {
            if (UseDualInput)
                return MappedInput.GetAxisRaw(axisName) + UnityEngine.Input.GetAxisRaw(axisName);

            return MappedInput.GetAxisRaw(axisName);
        }

        public override bool GetButtonDown(string buttonName)
        {
            if (UseDualInput)
                return MappedInput.GetButtonDown(buttonName) || UnityEngine.Input.GetButtonDown(buttonName);

            return MappedInput.GetButtonDown(buttonName);
        }
    }
}