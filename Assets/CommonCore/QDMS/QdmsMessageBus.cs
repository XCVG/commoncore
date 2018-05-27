using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Messaging
{

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
                Debug.LogWarning("Message bus already exists!");

                _Instance = null;
            }

            Debug.Log("QDMS bus created!");
            Receivers = new List<QdmsMessageInterface>();
            _Instance = this;
        }

        ~QdmsMessageBus()
        {
            foreach(QdmsMessageInterface r in Receivers)
            {
                if (r != null)
                    r.Valid = false;
            }

            Debug.Log("QDMS bus destroyed!");
        }

        private List<QdmsMessageInterface> Receivers;

        internal void PushBroadcast(QdmsMessage msg) //internal doesn't work the way I thought it did, gah
        {
            foreach(QdmsMessageInterface r in Receivers)
            {
                try
                {
                    r.MessageQueue.Enqueue(msg);
                }
                catch(Exception e) //steamroll errors
                {
                    Debug.Log(e);
                }
            }
        }

        internal void RegisterReceiver(QdmsMessageInterface receiver)
        {
            Receivers.Add(receiver);
        }

        internal void UnregisterReceiver(QdmsMessageInterface receiver)
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
                if (r == null)
                    Receivers.RemoveAt(i);
            }
        }
        
    }
}