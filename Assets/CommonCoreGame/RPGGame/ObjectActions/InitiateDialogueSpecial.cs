using CommonCore.ObjectActions;
using CommonCore.World;
using System;
using UnityEngine;
using CommonCore.RpgGame.Dialogue;

namespace CommonCore.RpgGame.ObjectActions
{

    /// <summary>
    /// Initiates a dialogue
    /// </summary>
    public class InitiateDialogueSpecial : ActionSpecial
    {
        //I can't believe we didn't have this

        public string Dialogue;
        public bool Pause;
        public bool TargetIsThis = true;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            DialogueInitiator.InitiateDialogue(Dialogue, Pause, null, TargetIsThis ? gameObject.name : null);

            if (!Repeatable)
                Locked = true;
        }

    }
}