using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.Messaging;
using CommonCore.StringSub;
using CommonCore.State;
using CommonCore.Rpg;

namespace CommonCore.UI
{
    public class WorldHUDController : MonoBehaviour
    {
        public static WorldHUDController Current { get; private set; }

        public Text TargetText;
        public Text MessageText;
        public ScrollRect MessageScrollRect;

        public Slider HealthSlider;
        public Text HealthText;
        
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
            //this is all slow and dumb and temporary... which means it'll probably be untouched until Ferelden

            while(MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }

            UpdateStatusDisplays();
        }

        private void HandleMessage(QdmsMessage message)
        {
            if(message is HUDPushMessage)
            {
                MessageText.text = MessageText.text + "\n" + Sub.Macro(((HUDPushMessage)message).Contents);
                Canvas.ForceUpdateCanvases();
                MessageScrollRect.verticalNormalizedPosition = 0;
            }
        }

        private void UpdateStatusDisplays()
        {
            var player = GameState.Instance.PlayerRpgState;
            HealthText.text = player.Health.ToString("f0");
            HealthSlider.value = player.HealthFraction;
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