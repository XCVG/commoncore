using CommonCore.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonCore.UI
{

    /// <summary>
    /// Attribute marking the prefab for a custom modal
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CustomModalAttribute : Attribute
    {
        public string Prefab { get; }

        public CustomModalAttribute(string prefab)
        {
            Prefab = prefab;
        }
    }

    /// <summary>
    /// Controller base class for a custom UI modal
    /// </summary>
    public abstract class CustomModalController<TData, TResult> : BaseMenuController
    {        
        protected Action<ModalStatusCode, string, TResult> Callback;

        protected string ResultTag;
        protected bool IsSet;

        public void SetInitial(TData data, string tag, Action<ModalStatusCode, string, TResult> callback)
        {
            if (IsSet)
                Debug.LogWarning($"Custom Modal ({GetType().Name}) was set more than once!");

            ResultTag = tag;

            Callback = callback;

            if (callback == null)
                Debug.Log($"Custom Modal ({GetType().Name}) was passed null callback!");
            else
                Callback = callback;

            Init(data);

            IsSet = true;
        }

        protected abstract void Init(TData data);

        protected abstract TResult GetResult();

        public void OnContinueClicked()
        {
            Destroy(this.gameObject);
            if (Callback != null)
                Callback.Invoke(ModalStatusCode.Complete, ResultTag, GetResult());
        }

        public void OnCancelClicked()
        {
            Destroy(this.gameObject);
            if (Callback != null)
                Callback.Invoke(ModalStatusCode.Aborted, ResultTag, GetResult());
        }
    }

}
