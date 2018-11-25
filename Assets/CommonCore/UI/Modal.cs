using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;
using SickDev.DevConsole;

namespace CommonCore.UI
{

    public enum ModalStatusCode
    {
        Undefined, Aborted, Complete
    }

    public delegate void MessageModalCallback(ModalStatusCode status, string tag);
    public delegate void QuantityModalCallback(ModalStatusCode status, string tag, int quantity);
    public delegate void ConfirmModalCallback(ModalStatusCode status, string tag, bool result);

    public static class Modal //TODO modals should take out control locks
    {
        private const string MessageModalPrefab = "UI/Modal_Message";
        private const string QuantityModalPrefab = "UI/Modal_Quantity";
        private const string ConfirmModalPrefab = "UI/Modal_Confirm";

        public static void PushMessageModal(string text, string heading, string tag, MessageModalCallback callback)
        {
            PushMessageModal(text, heading, tag, callback, false);
        }

        public static void PushMessageModal(string text, string heading, string tag, MessageModalCallback callback, bool ephemeral)
        {
            var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(MessageModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CCBaseUtil.GetUIRoot());
            go.GetComponent<MessageModalController>().SetInitial(heading, text, null, tag, callback);
        }

        public static void PushQuantityModal(string heading, int min, int max, int initial, bool allowCancel, string tag, QuantityModalCallback callback)
        {
            PushQuantityModal(heading, min, max, initial, allowCancel, tag, callback, false);
        }

        public static void PushQuantityModal(string heading, int min, int max, int initial, bool allowCancel, string tag, QuantityModalCallback callback, bool ephemeral)
        {
            var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(QuantityModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CCBaseUtil.GetUIRoot());
            go.GetComponent<QuantityModalController>().SetInitial(heading, min, max, initial, allowCancel, tag, callback);
        }

        public static void PushConfirmModal(string text, string heading, string yesText, string noText, string tag, ConfirmModalCallback callback)
        {
            PushConfirmModal(text, heading, yesText, noText, tag, callback, false);
        }

        public static void PushConfirmModal(string text, string heading, string yesText, string noText, string tag, ConfirmModalCallback callback, bool ephemeral)
        {
            var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(ConfirmModalPrefab), ephemeral ? GetEphemeralOrUIRoot() : CCBaseUtil.GetUIRoot());
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

            return CCBaseUtil.GetUIRoot();
        }

    }

    public static class ModalCommandIntegration //TODO split this out properly
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
            SickDev.DevConsole.DevConsole.singleton.Log(string.Format("Message Modal Returned \"{0}\" [{1}]", tag, status));
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
            SickDev.DevConsole.DevConsole.singleton.Log(string.Format("Quantity Modal Returned \"{0}\",{2} [{1}]", tag, status, quantity));
        }

        static void TestConfirmModalCallback(ModalStatusCode status, string tag, bool result)
        {
            SickDev.DevConsole.DevConsole.singleton.Log(string.Format("Confirm Modal Returned \"{0}\",{2} [{1}]", tag, status, result));
        }


    }
}