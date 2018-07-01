using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.DebugLog;
using CommonCore.StringSub;

namespace CommonCore.UI
{

    public class MessageModalController : BaseMenuController
    {
        public Text HeadingText;
        public Text MainText;
        public Text ButtonText;

        private MessageModalCallback Callback;

        private string ResultTag;
        private bool IsSet;

        public void SetInitial(string heading, string text, string next, string tag, MessageModalCallback callback)
        {
            if (IsSet)
                CDebug.LogEx("MessageModal was set more than once!", LogLevel.Warning, this);

            if (!string.IsNullOrEmpty(heading))
                HeadingText.text = Sub.Macro(heading);

            if (!string.IsNullOrEmpty(text))
                MainText.text = Sub.Macro(text);

            if (!string.IsNullOrEmpty(next))
                ButtonText.text = Sub.Macro(next);

            ResultTag = tag;

            if (callback == null)
                CDebug.LogEx("MessageModal was passed null callback!", LogLevel.Warning, this);
            else
                Callback = callback;

            IsSet = true;
        }

        public void OnContinueClicked()
        {
            Destroy(this.gameObject);
            if(Callback != null)
                Callback.Invoke(ModalStatusCode.Complete, ResultTag);
        }

    }
}