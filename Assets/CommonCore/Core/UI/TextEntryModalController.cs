using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.DebugLog;
using CommonCore.StringSub;

namespace CommonCore.UI
{
    public struct TextEntryModalData
    {
        public string Heading { get; set; }
        public string Description { get; set; }
        public string Placeholder { get; set; }
        public string InitialText { get; set; }
        public bool AllowCancel { get; set; }
    }

    [CustomModal("TextEntryModal")]
    public class TextEntryModalController : CustomModalController<TextEntryModalData, string>
    {
        [Header("Elements")]
        public Text HeadingText;
        public Text DescriptionText;
        public Button ConfirmButton;
        public Text ConfirmButtonText;
        public Button CancelButton;
        public Text CancelButtonText;
        public InputField TextField;

        protected override string GetResult()
        {
            return TextField.text;
        }

        protected override void Init(TextEntryModalData data)
        {
            HeadingText.text = data.Heading;
            DescriptionText.text = data.Description;

            TextField.text = data.InitialText;
            ((Text)TextField.placeholder).text = data.Placeholder;

            if (!data.AllowCancel)
            {
                CancelButton.gameObject.SetActive(false);
            }

        }
    }
}