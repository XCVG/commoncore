using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CommonCore.Messaging
{
    [Serializable]
    public class QdmsMessageEvent : UnityEvent<QdmsMessage>
    {

    }

    /// <summary>
    /// Component that fires UnityEvent when QdmsMessage is received
    /// </summary>
    public class QdmsMessageComponent : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        private QdmsMessageEvent MessageEvent;
#pragma warning restore CS0649
        [SerializeField]
        private bool ReceiveIfInactive = true;
        [SerializeField, Tooltip("Translates to Unity messages (BroadcastMessage). Experimental")]
        private bool TranslateMessages = false;

        private QdmsMessageInterface MessageInterface;        

        void Awake()
        {
            MessageInterface = new QdmsMessageInterface(gameObject);
            MessageInterface.ReceiveIfAttachmentInactive = ReceiveIfInactive;

            if (MessageEvent.GetPersistentEventCount() > 0)
                MessageInterface.SubscribeReceiver(HandleMessage);
        }

        private void HandleMessage(QdmsMessage message)
        {
            MessageEvent.Invoke(message);

            if (TranslateMessages)
                TranslateAndBroadcast(message);
        }

        private void TranslateAndBroadcast(QdmsMessage message)
        {
            string messageName;
            object messageArg = null;

            if (message is QdmsFlagMessage flagMessage)
            {
                messageName = flagMessage.Flag;
                if (message is QdmsKeyValueMessage kvm)
                    messageArg = kvm.EnumerateValues().ToDictionary(k => k.Key, v => v.Value);
            }
            else
            {
                messageName = message.GetType().Name;
                if (messageName.EndsWith("message", StringComparison.OrdinalIgnoreCase))
                    messageName = messageName.Substring(0, messageName.LastIndexOf("message", StringComparison.OrdinalIgnoreCase));
                messageArg = message;
            }

            BroadcastMessage(messageName, messageArg, SendMessageOptions.DontRequireReceiver);
        }

    }
}