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
        private Queue<QdmsMessage> MessageQueue;

        /// <summary>
        /// The Object this receiver is attached to (if it exists)
        /// </summary>
        public UnityEngine.Object Attachment { get; private set; }

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

        /// <summary>
        /// Whether to receive message when the attached UnityEngine.Object is inactive
        /// </summary>
        /// <remarks>
        /// This will have no effect if this receiver does not have an attachment or is not of a type that can be considered inactive
        /// </remarks>
        public bool ReceiveIfAttachmentInactive { get; set; } = true;

        /// <summary>
        /// Whether this message receiver is valid
        /// </summary>
        /// <remarks>Actual validity reported to bus is "has no attachment or attachment exists AND this is valid"</remarks>
        public bool IsValid { get; set; } = true;        

        private List<Action<QdmsMessage>> ReceiveActions = new List<Action<QdmsMessage>>();

        /// <summary>
        /// Create a message receiver interface
        /// </summary>
        /// <param name="attachment">The gameobject to attach to</param>
        public QdmsMessageInterface(UnityEngine.Object attachment) : this()
        {
            Attachment = attachment;
            HasAttachment = true;
        }

        /// <summary>
        /// Create a message receiver interface
        /// </summary>
        /// <remarks>If you use this directly (no attachment) you must set IsValid to false when you are done with the receiver!</remarks>
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
                msg.Sender = this;
            QdmsMessageBus.Instance.PushBroadcast(msg);
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <remarks>Interface implementation.</remarks>
        public void ReceiveMessage(QdmsMessage msg)
        {
            if(HasAttachment && !ReceiveIfAttachmentInactive)
            {
                if (Attachment == null)
                    return;
                if (Attachment is GameObject go && !go.activeInHierarchy)
                    return;
                if (Attachment is Behaviour b && !b.isActiveAndEnabled)
                    return;
                if (Attachment is Component c && c.gameObject != null && !c.gameObject.activeInHierarchy)
                    return;
            }

            bool handledMessage = HandleMessage(msg);

            if(!handledMessage || KeepMessagesInQueue)
                MessageQueue.Enqueue(msg);
            
        }

        private bool HandleMessage(QdmsMessage message)
        {
            //if we have any receivers, fire them
            bool handledMessage = false;
            if(ReceiveActions.Count > 0)
            {
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

            return handledMessage;
        }

        /// <summary>
        /// Interface implementation; if this receiver is valid or not
        /// </summary>
        bool IQdmsMessageReceiver.IsValid
        {
            get
            {
                return (HasAttachment ? Attachment != null : true) && this.IsValid;
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