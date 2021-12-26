using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.DebugLog;
using CommonCore.StringSub;

namespace CommonCore.UI
{
    public class TextEntryModalData
    {

    }

    public class TextEntryModalResult
    {

    }

    public class TextEntryModalController : CustomModalController<TextEntryModalData, TextEntryModalResult>
    {
        public Text HeadingText;
        public Button ConfirmButton;
        public Text ConfirmButtonText;
        public Button CancelButton;
        public Text CancelButtonText;
        public InputField QuantityField;

        protected override TextEntryModalResult GetResult()
        {
            throw new NotImplementedException();
        }

        protected override void Init(TextEntryModalData data)
        {
            throw new NotImplementedException();
        }
    }
}