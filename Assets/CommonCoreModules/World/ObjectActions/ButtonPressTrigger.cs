using UnityEngine;
using System.Collections;
using CommonCore.Input;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Triggers an action when a button or key is pressed
    /// </summary>
    public class ButtonPressTrigger : ActionTrigger
    {
        [Header("Button Press Trigger Options")]
        public bool UseKeyInput;
        public string InputCode;

        void Start()
        {
            RestoreState();
        }

        void Update()
        {
            if (Triggered)
                return;

            bool inputPressed = false;

            if (UseKeyInput)
                inputPressed = UnityEngine.Input.GetKeyDown(InputCode);
            else
                inputPressed = MappedInput.GetButtonDown(InputCode);

            if (inputPressed)
            {
                ActionInvokerData d = new ActionInvokerData { Activator =  WorldUtils.GetPlayerController() };
                Special.Invoke(d);

                if (!Repeatable)
                {
                    Triggered = true;
                    SaveState();
                }
                    
            }

        }
    }
}