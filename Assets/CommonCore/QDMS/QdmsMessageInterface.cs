using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Messaging
{

    //not actually an interface
    public class QdmsMessageInterface
    {
        internal Queue<QdmsMessage> MessageQueue;
        public bool Valid { get; internal set; }

        public GameObject Attachment { get; internal set; }

        public QdmsMessageInterface(GameObject attachment) : this()
        {
            Attachment = attachment;
        }

        public QdmsMessageInterface()
        {
            MessageQueue = new Queue<QdmsMessage>();

            //register
            QdmsMessageBus.Instance.RegisterReceiver(this);

            Valid = true;
        }

        ~QdmsMessageInterface()
        {
            QdmsMessageBus.Instance.UnregisterReceiver(this);
        }

        public bool HasMessageInQueue
        {
            get
            {
                return MessageQueue.Count > 0;
            }
        }

        public int CountMessagesInQueue
        {
            get
            {
                return MessageQueue.Count;
            }
        }

        public QdmsMessage PopFromQueue()
        {
            if (MessageQueue.Count > 0)
                return MessageQueue.Dequeue();

            return null;
        }

        public void PushToBus(QdmsMessage msg)
        {
            msg.SetSender(this);
            QdmsMessageBus.Instance.PushBroadcast(msg);
        }

    }
}