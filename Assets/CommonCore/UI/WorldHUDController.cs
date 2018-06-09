using CommonCore.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{
    public class WorldHUDController : MonoBehaviour
    {
        public static WorldHUDController Current { get; private set; }

        public Text TargetText;

        
        private QdmsMessageInterface MessageInterface;

        void Awake()
        {
            MessageInterface = new QdmsMessageInterface();
            Current = this;
        }

        void Start()
        {

        }
        
        void Update()
        {
            
            while(MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }
        }

        private void HandleMessage(QdmsMessage message)
        {
            //TODO handle messages
        }

        internal void ClearTarget()
        {
            TargetText.text = string.Empty;
        }

        internal void SetTargetMessage(string message)
        {
            TargetText.text = message;
        }
    }
}