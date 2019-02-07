using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.DebugLog;
using CommonCore.StringSub;

namespace CommonCore.UI
{

    public class QuantityModalController : BaseMenuController
    {
        public Text HeadingText;
        public Button ConfirmButton;
        public Text ConfirmButtonText;
        public Button CancelButton;
        public Text CancelButtonText;
        public InputField QuantityField;

        private QuantityModalCallback Callback;

        private int Min;
        private int Max;
        private string ResultTag;
        private bool IsSet;

        public void SetInitial(string heading, int min, int max, int initial, bool allowCancel, string tag, QuantityModalCallback callback)
        {
            if (IsSet)
                CDebug.LogEx("QuantityModal was set more than once!", LogLevel.Warning, this);

            if (!string.IsNullOrEmpty(heading))
                HeadingText.text = Sub.Macro(heading);

            Min = min;
            Max = max;
            QuantityField.text = initial.ToString();

            if(!allowCancel)
            {
                CancelButton.gameObject.SetActive(false);
            }

            ResultTag = tag;

            if (callback == null)
                CDebug.LogEx("QuantityModal was passed null callback!", LogLevel.Warning, this);
            else
                Callback = callback;

            IsSet = true;
        }

        private int Quantity
        {
            get
            {
                return Convert.ToInt32(QuantityField.text).Clamp(Min, Max);
            }
        }

        public void OnStepClicked(int step)
        {
            int newVal = (Convert.ToInt32(QuantityField.text) + step).Clamp(Min, Max);
            QuantityField.text = newVal.ToString();
        }

        public void OnValueChanged(string newValue) //parameter is not getting passed
        {
            int val = Convert.ToInt32(QuantityField.text); //hacky fix
            int clampVal = val.Clamp(Min, Max);
            if (val != clampVal)
                QuantityField.text = clampVal.ToString();
        }

        public void OnContinueClicked()
        {
            Destroy(this.gameObject);
            if (Callback != null)
                Callback.Invoke(ModalStatusCode.Complete, ResultTag, Quantity);
        }

        public void OnCancelClicked()
        {
            Destroy(this.gameObject);
            if (Callback != null)
                Callback.Invoke(ModalStatusCode.Aborted, ResultTag, 0);
        }

    }
}