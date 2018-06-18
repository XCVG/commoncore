using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Messaging;
using CommonCore.StringSub;

namespace CommonCore.ObjectActions
{

    public class HUDMessageSpecial : ActionSpecial
    {
        public string Message;
        public bool UseSubstitution;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked)
                return;

            QdmsMessageBus.Instance.PushBroadcast(new HUDPushMessage(UseSubstitution ? Sub.Macro(Message) : Message));

            Locked = true;
        }
    }
}