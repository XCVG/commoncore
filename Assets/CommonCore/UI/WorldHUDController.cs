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
        public Text MessageText;
        public ScrollRect MessageScrollRect;

        
        private QdmsMessageInterface MessageInterface;

        void Awake()
        {
            MessageInterface = new QdmsMessageInterface();
            Current = this;
        }

        void Start()
        {
            MessageText.text = string.Empty;
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
            if(message is HUDPushMessage)
            {
                MessageText.text = MessageText.text + "\n" + ((HUDPushMessage)message).Contents;
                Canvas.ForceUpdateCanvases();
                MessageScrollRect.verticalNormalizedPosition = 0;
            }
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