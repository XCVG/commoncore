using System;
using System.Collections;
using System.Collections.Generic;
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
        }

    }
}