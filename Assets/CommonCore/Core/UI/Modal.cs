using CommonCore.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonCore.UI
{

    public enum ModalStatusCode
    {
        Undefined, Aborted, Complete
    }

    public delegate void MessageModalCallback(ModalStatusCode status, string tag);
    public delegate void QuantityModalCallback(ModalStatusCode status, string tag, int quantity);
    public delegate void ConfirmModalCallback(ModalStatusCode status, string tag, bool result);

    public readonly struct MessageModalResult
    {
        public readonly ModalStatusCode Status;
        
        public MessageModalResult(ModalStatusCode status)
        {
            Status = status;
        }
    }

    public readonly struct QuantityModalResult
    {
        public readonly ModalStatusCode Status;
        public readonly int Quantity;

        public QuantityModalResult(ModalStatusCode status, int quantity)
        {
            Status = status;
            Quantity = quantity;
        }
    }

    public readonly struct ConfirmModalResult
    {
        public readonly ModalStatusCode Status;
        public readonly bool Result;

        public ConfirmModalResult(ModalStatusCode status, bool result)
        {
            Status = status;
            Result = result;
        }
    }

    public readonly struct CustomModalResult<T>
    {
        public readonly ModalStatusCode Status;
        public readonly T Result;

        public CustomModalResult(ModalStatusCode status, T result)
        {
            Status = status;
            Result = result;
        }
    }

    /// <summary>
    /// Static class for managing modal system
    /// </summary>
    public static class Modal
    {
        private const string MessageModalPrefab = "UI/Modal_Message";
        private const string QuantityModalPrefab = "UI/Modal_Quantity";
        private const string ConfirmModalPrefab = "UI/Modal_Confirm";

        /// <summary>
        /// Pushes a message modal and invokes the callback when dismissed
        /// </summary>
        public static void PushMessageModal(string text, string heading, string tag, MessageModalCallback callback)
        {
            PushMessageModal(text, heading, tag, callback, false);
        }

        /// <summary>
        /// Pushes a message modal and invokes the callback when dismissed
        /// </summary>
        public static void PushMessageModal(string text, string heading, string tag, MessageModalCallback callback, bool ephemeral)
        {
            var go = GameObject.Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>(MessageModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CoreUtils.GetUIRoot());
            go.GetComponent<MessageModalController>().SetInitial(heading, text, null, tag, callback);
        }

        /// <summary>
        /// Pushes a message modal and allows it to be awaited
        /// </summary>
        public static async Task<MessageModalResult> PushMessageModalAsync(string text, string heading, bool ephemeral, CancellationToken? token)
        {
            //we could probably switch this to a TaskCompletionSource

            bool finished = false;
            ModalStatusCode status = ModalStatusCode.Undefined;

            MessageModalCallback callback = (s, t) => { finished = true; status = s; };

            var go = GameObject.Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>(MessageModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CoreUtils.GetUIRoot());
            go.GetComponent<MessageModalController>().SetInitial(heading, text, null, null, callback);

            while (!(finished || go == null))
            {
                if(token != null && token.Value.IsCancellationRequested)
                {
                    if (go != null)
                        UnityEngine.Object.Destroy(go);

                    break;
                }

                await Task.Yield();
            }

            if (status == ModalStatusCode.Undefined)
                status = ModalStatusCode.Aborted;

            return new MessageModalResult(status);
        }

        /// <summary>
        /// Pushes a quantity modal and invokes the callback when dismissed
        /// </summary>
        public static void PushQuantityModal(string heading, int min, int max, int initial, bool allowCancel, string tag, QuantityModalCallback callback)
        {
            PushQuantityModal(heading, min, max, initial, allowCancel, tag, callback, false);
        }

        /// <summary>
        /// Pushes a quantity modal and invokes the callback when dismissed
        /// </summary>
        public static void PushQuantityModal(string heading, int min, int max, int initial, bool allowCancel, string tag, QuantityModalCallback callback, bool ephemeral)
        {
            var go = GameObject.Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>(QuantityModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CoreUtils.GetUIRoot());
            go.GetComponent<QuantityModalController>().SetInitial(heading, min, max, initial, allowCancel, tag, callback);
        }

        /// <summary>
        /// Pushes a quantity modal and allows it to be awaited
        /// </summary>
        public static async Task<QuantityModalResult> PushQuantityModalAsync(string heading, int min, int max, int initial, bool allowCancel, bool ephemeral, CancellationToken? token)
        {
            bool finished = false;
            ModalStatusCode status = ModalStatusCode.Undefined;
            int quantity = initial;

            QuantityModalCallback callback = (s, t, q) => { finished = true; status = s; quantity = q; };

            var go = GameObject.Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>(QuantityModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CoreUtils.GetUIRoot());
            go.GetComponent<QuantityModalController>().SetInitial(heading, min, max, initial, allowCancel, null, callback);

            while (!(finished || go == null))
            {
                if (token != null && token.Value.IsCancellationRequested)
                {
                    if (go != null)
                        UnityEngine.Object.Destroy(go);

                    break;
                }

                await Task.Yield();
            }

            await Task.Yield(); //let the callback run, hopefully

            if (status == ModalStatusCode.Undefined)
                status = ModalStatusCode.Aborted;

            return new QuantityModalResult(status, quantity);
        }

        /// <summary>
        /// Pushes a confirm modal and invokes the callback when dismissed
        /// </summary>
        public static void PushConfirmModal(string text, string heading, string yesText, string noText, string tag, ConfirmModalCallback callback)
        {
            PushConfirmModal(text, heading, yesText, noText, tag, callback, false);
        }

        /// <summary>
        /// Pushes a confirm modal and invokes the callback when dismissed
        /// </summary>
        public static void PushConfirmModal(string text, string heading, string yesText, string noText, string tag, ConfirmModalCallback callback, bool ephemeral)
        {
            var go = GameObject.Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>(ConfirmModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CoreUtils.GetUIRoot());
            go.GetComponent<ConfirmModalController>().SetInitial(heading, text, yesText, noText, tag, callback);
        }

        /// <summary>
        /// Pushes a confirm modal and allows it to be awaited
        /// </summary>
        public static async Task<ConfirmModalResult> PushConfirmModalAsync(string text, string heading, string yesText, string noText, bool ephemeral, CancellationToken? token)
        {
            bool finished = false;
            ModalStatusCode status = ModalStatusCode.Undefined;
            bool result = false;

            ConfirmModalCallback callback = (s, t, r) => { finished = true; status = s; result = r; };

            var go = GameObject.Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>(ConfirmModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CoreUtils.GetUIRoot());
            go.GetComponent<ConfirmModalController>().SetInitial(heading, text, yesText, noText, null, callback);

            while (!(finished || go == null))
            {
                if (token != null && token.Value.IsCancellationRequested)
                {
                    if (go != null)
                        UnityEngine.Object.Destroy(go);

                    break;
                }

                await Task.Yield();
            }

            await Task.Yield(); //let the callback run, hopefully

            if (status == ModalStatusCode.Undefined)
                status = ModalStatusCode.Aborted;

            return new ConfirmModalResult(status, result);
        }

        /// <summary>
        /// Pushes a text entry modal and invokes the callback when dismissed
        /// </summary>
        public static void PushTextEntryModal(TextEntryModalData data, string tag, Action<ModalStatusCode, string, string> callback)
        {
            PushCustomModal<TextEntryModalController, TextEntryModalData, string>(data, tag, callback);
        }

        /// <summary>
        /// Pushes a text entry modal and invokes the callback when dismissed
        /// </summary>
        public static void PushTextEntryModal(TextEntryModalData data, string tag, Action<ModalStatusCode, string, string> callback, bool ephemeral)
        {
            PushCustomModal<TextEntryModalController, TextEntryModalData, string>(data, tag, callback, ephemeral);
        }

        /// <summary>
        /// Pushes a text entry modal and allows it to be awaited
        /// </summary>
        public static async Task<CustomModalResult<string>> PushTextEntryModalAsync(TextEntryModalData data, bool ephemeral, CancellationToken? token)
        {
            return await PushCustomModalAsync<TextEntryModalController, TextEntryModalData, string>(data, ephemeral, token);
        }

        /// <summary>
        /// Pushes a custom modal and invokes the callback when dismissed
        /// </summary>
        public static TModalController PushCustomModal<TModalController, TData, TResult>(TData data, string tag, Action<ModalStatusCode, string, TResult> callback) where TModalController : CustomModalController<TData, TResult>
        {
            return PushCustomModal<TModalController, TData, TResult>(null, data, tag, callback, false);
        }

        /// <summary>
        /// Pushes a custom modal and invokes the callback when dismissed
        /// </summary>
        public static TModalController PushCustomModal<TModalController, TData, TResult>(TData data, string tag, Action<ModalStatusCode, string, TResult> callback, bool ephemeral) where TModalController : CustomModalController<TData, TResult>
        {
            return PushCustomModal<TModalController, TData, TResult>(null, data, tag, callback, ephemeral);
        }

        /// <summary>
        /// Pushes a custom modal and invokes the callback when dismissed
        /// </summary>
        public static TModalController PushCustomModal<TModalController, TData, TResult>(string prefab, TData data, string tag, Action<ModalStatusCode, string, TResult> callback) where TModalController : CustomModalController<TData, TResult>
        {
            return PushCustomModal<TModalController, TData, TResult>(prefab, data, tag, callback, false);
        }

        /// <summary>
        /// Pushes a custom modal and invokes the callback when dismissed
        /// </summary>
        public static TModalController PushCustomModal<TModalController, TData, TResult>(string prefab, TData data, string tag, Action<ModalStatusCode, string, TResult> callback, bool ephemeral) where TModalController : CustomModalController<TData, TResult>
        {
            if(string.IsNullOrEmpty(prefab))
            {
                var attribute = typeof(TModalController).GetCustomAttribute<CustomModalAttribute>();
                if (attribute != null)
                    prefab = attribute.Prefab;

                if (string.IsNullOrEmpty(prefab))
                    throw new ArgumentException("Custom modal must have prefab from CustomModalAttribute if prefab is not specified!");
            }

            var go = GameObject.Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>("UI/Modals/" + prefab), ephemeral ? GetEphemeralOrUIRoot() : CoreUtils.GetUIRoot());
            TModalController modalController = go.GetComponent<TModalController>();
            modalController.SetInitial(data, tag, callback);
            return modalController;
        }

        /// <summary>
        /// Pushes a custom modal and allows it to be awaited
        /// </summary>
        public static async Task<CustomModalResult<TResult>> PushCustomModalAsync<TModalController, TData, TResult>(TData data, bool ephemeral, CancellationToken? token) where TModalController : CustomModalController<TData, TResult>
        {
            return await PushCustomModalAsync<TModalController, TData, TResult>(null, data, ephemeral, token);
        }

        /// <summary>
        /// Pushes a custom modal and allows it to be awaited
        /// </summary>
        public static async Task<CustomModalResult<TResult>> PushCustomModalAsync<TModalController, TData, TResult>(string prefab, TData data, bool ephemeral, CancellationToken? token) where TModalController : CustomModalController<TData, TResult>
        {
            bool finished = false;
            ModalStatusCode status = ModalStatusCode.Undefined;
            TResult result = default;

            Action<ModalStatusCode, string, TResult> callback = (s, t, r) => { finished = true; status = s; result = r; };

            var modalController = PushCustomModal<TModalController, TData, TResult>(prefab, data, null, callback, ephemeral);
            var go = modalController.gameObject;

            while (!(finished || go == null))
            {
                if (token != null && token.Value.IsCancellationRequested)
                {
                    if (go != null)
                        UnityEngine.Object.Destroy(go);

                    break;
                }

                await Task.Yield();
            }

            await Task.Yield(); //let the callback run, hopefully

            if (status == ModalStatusCode.Undefined)
                status = ModalStatusCode.Aborted;

            return new CustomModalResult<TResult>(status, result);
        }

        private static Transform GetEphemeralOrUIRoot()
        {
            GameObject erObj = GameObject.FindGameObjectWithTag("EphemeralRoot");
            if (erObj != null)
                return erObj.transform;

            return CoreUtils.GetUIRoot();
        }

    }

    public static class ModalCommandIntegration
    {
        [Command(alias = "TestMessageModal", className = "UI")]
        static void TestMessageModal()
        {
            TestMessageModal("Hurr Durr I'ma Sheep", "Test Message", "the tag formerly known as tag");
        }

        [Command(alias = "TestMessageModal", className = "UI")]
        static void TestMessageModal(string text, string heading, string tag)
        {
            Modal.PushMessageModal(text, heading, tag, TestMessageModalCallback);
        }

        static void TestMessageModalCallback(ModalStatusCode status, string tag)
        {
            ConsoleModule.WriteLine(string.Format("Message Modal Returned \"{0}\" [{1}]", tag, status));
        }
        
        [Command(alias = "TestQuantityModal", className = "UI")]
        static void TestQuantityModal()
        {
            Modal.PushQuantityModal("QtyTest", -1, 100, 1, false, "qty_test", TestQuantityModalCallback);
        }

        /*
        [Command(alias = "TestQuantityModal", className = "UI")] //breaks the console system for some reason
        static void TestQuantityModal(string heading, string min, string max, string initial, string allowCancel, string tag)
        {
            Modal.PushQuantityModal(heading, Convert.ToInt32(min), Convert.ToInt32(max), Convert.ToInt32(initial), Convert.ToBoolean(allowCancel), tag, TestMessageQuantityCallback);
        }
        */

        [Command(alias = "TestConfirmModal", className = "UI")]
        static void TestConfirmModal()
        {
            Modal.PushConfirmModal("If a tree falls in the forest and nobody is around to hear it, does it make a sound?", "ConfirmModalTest", "Yes", "No", "not a tag", TestConfirmModalCallback);
        }

        static void TestQuantityModalCallback(ModalStatusCode status, string tag, int quantity)
        {
            ConsoleModule.WriteLine(string.Format("Quantity Modal Returned \"{0}\",{2} [{1}]", tag, status, quantity));
        }

        static void TestConfirmModalCallback(ModalStatusCode status, string tag, bool result)
        {
            ConsoleModule.WriteLine(string.Format("Confirm Modal Returned \"{0}\",{2} [{1}]", tag, status, result));
        }


    }
}