using CommonCore.Console;
using System;
using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// Static class for managing modal system
    /// </summary>
    public static class Modal //TODO modals should take out control locks
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

        private static Transform GetEphemeralOrUIRoot()
        {
            IngameMenuController imc = IngameMenuController.Current;
            if(imc != null)
            {
                GameObject erObj = imc.EphemeralRoot;
                if (erObj != null)
                    return erObj.transform;
            }

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