using CommonCore.Config;
using CommonCore.LockPause;
using CommonCore.Messaging;
using CommonCore.StringSub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    /// <summary>
    /// Basic HUD controller, providing message box and subtitle support
    /// </summary>
    public class BaseHUDController : MonoBehaviour
    {
        public static BaseHUDController Current { get; protected set; }
        
        public Text MessageText;
        public ScrollRect MessageScrollRect;

        public Text SubtitleText;
        private float SubtitleTimer;
        private int SubtitlePriority = int.MinValue;

        private Vector2? BaseResolution = null;

        protected QdmsMessageInterface MessageInterface;


        private void Awake()
        {
            MessageInterface = new QdmsMessageInterface(this);
            Current = this;
        }

        protected virtual void Start()
        {
            ApplyScale();
            MessageText.text = string.Empty;
            UpdateSubtitles();
        }

        protected virtual void Update()
        {
            while (MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }

            UpdateSubtitles();
        }

        /// <summary>
        /// Handles a received message
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <returns>If the message was handled</returns>
        protected virtual bool HandleMessage(QdmsMessage message)
        {
            if (message is SubtitleMessage)
            {
                SubtitleMessage subMessage = (SubtitleMessage)message;
                if (subMessage.Priority >= SubtitlePriority)
                {
                    SubtitlePriority = subMessage.Priority;
                    SubtitleTimer = subMessage.HoldTime;
                    SubtitleText.text = subMessage.UseSubstitution ? Sub.Macro(subMessage.Contents) : subMessage.Contents;
                }
                return true;
            }
            else if (message is HUDClearMessage)
            {
                ClearHudMessages();
            }
            else if (message is HUDPushMessage)
            {
                AppendHudMessage(Sub.Macro(((HUDPushMessage)message).Contents));
                return true;
            }
            else if (message is QdmsFlagMessage fm && fm.Flag == "ConfigChanged")
            {
                ApplyScale();
            }

            return false;
        }

        private void UpdateSubtitles()
        {

            if (SubtitleTimer <= 0)
            {
                SubtitleText.text = string.Empty;
                SubtitlePriority = int.MinValue;
            }
            else
            {
                if((LockPauseModule.GetPauseLockState() ?? PauseLockType.AllowCutscene) >= PauseLockType.AllowCutscene)
                    SubtitleTimer -= Time.unscaledDeltaTime * (Mathf.Approximately(Time.timeScale, 0) ? 1 : Time.timeScale);
            }
        }

        protected void AppendHudMessage(string newMessage)
        {
            if (MessageText == null) //strictly speaking, it's optional
                return;

            MessageText.text = MessageText.text + "\n" + newMessage;
            Canvas.ForceUpdateCanvases();
            MessageScrollRect.verticalNormalizedPosition = 0;
        }

        protected void ClearHudMessages()
        {
            if (MessageText == null) //strictly speaking, it's optional
                return;

            MessageText.text = string.Empty;
            Canvas.ForceUpdateCanvases();
            MessageScrollRect.verticalNormalizedPosition = 0;
        }

        protected void ApplyScale()
        {
            float scale = ConfigState.Instance.HudScale;
            var scaler = GetComponent<CanvasScaler>();

            if(!Mathf.Approximately(scale, 1) && (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize))
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot apply HUD scale because scaler is not in correct mode (expected {CanvasScaler.ScaleMode.ScaleWithScreenSize}, got {scaler.Ref()?.uiScaleMode.ToString() ?? "null"})");
                return;
            }

            if(!BaseResolution.HasValue)
            {
                BaseResolution = scaler.referenceResolution;                
            }

            if(!Mathf.Approximately(scale, 1))
            {
                scaler.referenceResolution = BaseResolution.Value * scale;
                Debug.Log($"[{GetType().Name}] Set HUD scale to {scale:F4} (virtual resolution {scaler.referenceResolution.x:F2}x{scaler.referenceResolution.y:F2})");
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}