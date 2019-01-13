using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Input
{
    /*
     * Abstract base InputMapper
     * Functionality can be implemented by mappers with different backends
     */
    internal abstract class InputMapper
    {
        public abstract float GetAxis(string axis);
        public abstract bool GetButton(string button);
        public abstract bool GetButtonDown(string button);
        public abstract bool GetButtonUp(string button);
    }

    /*
     * Null InputMapper
     * Useless but there anyway
     */
    internal class NullInputMapper : InputMapper
    {
        public override float GetAxis(string axis)
        {
            return 0;
        }

        public override bool GetButton(string button)
        {
            return false;
        }

        public override bool GetButtonDown(string button)
        {
            return false;
        }

        public override bool GetButtonUp(string button)
        {
            return false;
        }
    }

    /*
     * Unity InputMapper
     * Simple passthrough to built-in input system
     */
    internal class UnityInputMapper : InputMapper
    {
        public override float GetAxis(string axis)
        {
            return UnityEngine.Input.GetAxis(axis);
        }

        public override bool GetButton(string button)
        {
            return UnityEngine.Input.GetButton(button);
        }

        public override bool GetButtonDown(string button)
        {
            return UnityEngine.Input.GetButtonDown(button);
        }

        public override bool GetButtonUp(string button)
        {
            return UnityEngine.Input.GetButtonUp(button);
        }
    }

}
