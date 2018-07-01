using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.DebugLog;
using CommonCore.StringSub;

namespace CommonCore.UI
{

    public class ConfirmModalController : BaseMenuController
    {
        public Text HeadingText;
        public Text MainText;
        public Text ContinueButtonText;
        public Text CancelButtonText;

        private ConfirmModalCallback Callback;

        private string ResultTag;
        private bool IsSet;

        public void SetInitial(string heading, string text, string continueText, string cancelText, string tag, ConfirmModalCallback callback)
        {
            if (IsSet)
                CDebug.LogEx("ConfirmModal was set more than once!", LogLevel.Warning, this);

            if (!string.IsNullOrEmpty(heading))
                HeadingText.text = Sub.Macro(heading);

            if (!string.IsNullOrEmpty(text))
                MainText.text = Sub.Macro(text);

            if (!string.IsNullOrEmpty(continueText))
                ContinueButtonText.text = Sub.Macro(continueText);

            if (!string.IsNullOrEmpty(cancelText))
                CancelButtonText.text = Sub.Macro(cancelText);

            ResultTag = tag;

            if (callback == null)
                CDebug.LogEx("ConfirmModal was passed null callback!", LogLevel.Warning, this);
            else
                Callback = callback;

            IsSet = true;
        }

        public void OnContinueClicked()
        {
            Destroy(this.gameObject);
            if (Callback != null)
                Callback.Invoke(ModalStatusCode.Complete, ResultTag, true);
        }

        public void OnCancelClicked()
        {
            Destroy(this.gameObject);
            if (Callback != null)
                Callback.Invoke(ModalStatusCode.Complete, ResultTag, false);
        }
    }
}