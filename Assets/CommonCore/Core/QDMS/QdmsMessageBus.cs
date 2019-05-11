using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Messaging
{

    /// <summary>
    /// Message bus and messaging support module
    /// </summary>
    [CCExplicitModule]
    public class QdmsMessageBus: CCModule
    {
        public static QdmsMessageBus Instance
        {
            get
            {
                return _Instance;
            }
        }
        private static QdmsMessageBus _Instance;

        public QdmsMessageBus()
        {
            //Singleton-ish guard
            if(_Instance != null)
            {
                LogWarning("Message bus already exists!");

                _Instance = null;
            }

            Log("QDMS bus created!");
            Receivers = new List<IQdmsMessageReceiver>();
            _Instance = this;
        }

        public override void Dispose()
        {
            Log("QDMS bus destroyed!");

            _Instance = null;
        }

        private List<IQdmsMessageReceiver> Receivers;

        public void PushBroadcast(QdmsMessage msg)
        {
            PushBroadcast(msg, null);
        }

        public void PushBroadcast(QdmsMessage msg, object sender)
        {
            msg.Sender = sender;

            foreach(IQdmsMessageReceiver r in Receivers)
            {
                if (r != null && r.IsValid)
                {
                    try
                    {
                        r.ReceiveMessage(msg);
                    }
                    catch (Exception e) //steamroll errors
                    {
                        Debug.Log(e);
                    }
                }
            }
        }

        public void RegisterReceiver(IQdmsMessageReceiver receiver)
        {
            Receivers.Add(receiver);
        }

        public void UnregisterReceiver(IQdmsMessageReceiver receiver)
        {
            Receivers.Remove(receiver);
        }

        public static void ForceCreate()
        {
            Instance.GetType(); //hacky!
        }

        public static void ForcePurge()
        {
            _Instance = null;
        }

        public void ForceCleanup()
        {
            for(int i = Receivers.Count-1; i >= 0; i--)
            {
                var r = Receivers[i];
                if (r == null || !r.IsValid)
                    Receivers.RemoveAt(i);
            }
        }
        
    }
    public interface IQdmsMessageReceiver
    {
        bool IsValid { get; }
        void ReceiveMessage(QdmsMessage msg);
    }
}