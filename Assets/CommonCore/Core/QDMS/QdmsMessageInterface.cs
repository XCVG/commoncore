using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Messaging
{

    /// <summary>
    /// General-purpose message receiver, use via composition
    /// </summary>
    /// <remarks>
    /// Uses a message queue, which can be accessed directly. Alternatively, can attach delegates to handle messages via SubscribeReceiver and UnsubscribeReceiver.
    /// </remarks>
    public class QdmsMessageInterface: IQdmsMessageReceiver
    {
        internal Queue<QdmsMessage> MessageQueue;

        /// <summary>
        /// The GameObject this receiver is attached to (if it exists)
        /// </summary>
        public GameObject Attachment { get; private set; }

        /// <summary>
        /// Whether this interface has a GameObject attachment
        /// </summary>
        public bool HasAttachment { get; private set; }

        /// <summary>
        /// Whether to keep messages in the queue after handling them or not
        /// </summary>
        /// <remarks>
        /// Note that messages will always be kept in the queue if there are no subscribed actions/delegates available to handle them.
        /// </remarks>
        public bool KeepMessagesInQueue { get; set; } = false;

        private List<Action<QdmsMessage>> ReceiveActions = new List<Action<QdmsMessage>>();

        /// <summary>
        /// Create a message receiver interface
        /// </summary>
        /// <param name="attachment">The gameobject to attach to</param>
        public QdmsMessageInterface(GameObject attachment) : this()
        {
            Attachment = attachment;
            HasAttachment = true;
        }

        /// <summary>
        /// Create a message receiver interface
        /// </summary>
        public QdmsMessageInterface()
        {
            MessageQueue = new Queue<QdmsMessage>();

            //register
            QdmsMessageBus.Instance.RegisterReceiver(this);

        }

        ~QdmsMessageInterface()
        {
            QdmsMessageBus.Instance.UnregisterReceiver(this);
        }

        /// <summary>
        /// Whether there is a message in the queue or not
        /// </summary>
        public bool HasMessageInQueue
        {
            get
            {
                return MessageQueue.Count > 0;
            }
        }

        /// <summary>
        /// The number of messages in the queue
        /// </summary>
        public int MessagesInQueue
        {
            get
            {
                return MessageQueue.Count;
            }
        }

        /// <summary>
        /// Pop the last message from the queue.
        /// </summary>
        public QdmsMessage PopFromQueue()
        {
            if (MessageQueue.Count > 0)
                return MessageQueue.Dequeue();

            return null;
        }

        /// <summary>
        /// Push a message to the message bus.
        /// </summary>
        public void PushToBus(QdmsMessage msg)
        {
            if(msg.Sender == null)
                msg.SetSender(this);
            QdmsMessageBus.Instance.PushBroadcast(msg);
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <remarks>Interface implementation.</remarks>
        public void ReceiveMessage(QdmsMessage msg)
        {
            MessageQueue.Enqueue(msg);

            HandleMessage();
        }

        private void HandleMessage()
        {
            //if we have any receivers, fire them
            bool handledMessage = false;
            if(ReceiveActions.Count > 0)
            {
                var message = MessageQueue.Peek();

                foreach(var action in ReceiveActions)
                {
                    if (action != null)
                    {
                        try
                        {
                            action(message);
                            handledMessage = true;
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            }

            //if we handled the message and don't intend to keep it, dump it
            if (handledMessage && !KeepMessagesInQueue)
                MessageQueue.Dequeue();
        }

        /// <summary>
        /// Whether this receiver is valid or not.
        /// </summary>
        /// <remarks>Interface implementation.</remarks>
        public bool IsValid
        {
            get
            {
                return HasAttachment ? Attachment != null : true;
            }
        }

        /// <summary>
        /// Attaches a delegate to receive messages via this receiver
        /// </summary>
        public void SubscribeReceiver(Action<QdmsMessage> receiveAction)
        {
            ReceiveActions.Add(receiveAction);
        }

        /// <summary>
        /// Detatches a previously attached delegate
        /// </summary>
        public void UnsubscribeReceiver(Action<QdmsMessage> receiveAction)
        {
            if (ReceiveActions.Contains(receiveAction))
                ReceiveActions.Remove(receiveAction);
        }

    }
}