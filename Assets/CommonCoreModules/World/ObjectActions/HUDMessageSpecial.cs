using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Messaging;
using CommonCore.StringSub;
using CommonCore.UI;

namespace CommonCore.ObjectActions
{

    public class HUDMessageSpecial : ActionSpecial
    {
        public string Message;
        public bool UseSubstitution;
        public string[] Tags;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            QdmsMessageBus.Instance.PushBroadcast(new HUDPushMessage(UseSubstitution ? Sub.Macro(Message) : Message, Tags));

            if (!Repeatable)
                Locked = true;
        }
    }
}