﻿using CommonCore.Audio;
using CommonCore.Console;
using CommonCore.DebugLog;
using CommonCore.LockPause;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.RpgGame.Dialogue
{
    public delegate void DialogueFinishedDelegate();

    public class DialogueInitiator
    {        
        /// <summary>
        /// Initiates dialogue, optionally running a callback method on completion
        /// </summary>
        public static void InitiateDialogue(string dialogue, bool pause, DialogueFinishedDelegate callback = null, string target = null)
        {
            DialogueController.CurrentDialogue = dialogue;
            DialogueController.CurrentCallback = callback;
            DialogueController.CurrentTarget = target;
            var prefab = CoreUtils.LoadResource<GameObject>("UI/DialogueSystem");
            var go = GameObject.Instantiate<GameObject>(prefab, CoreUtils.GetUIRoot());
            if (pause)
                LockPauseModule.PauseGame(PauseLockType.AllowCutscene, go);

        }

        /// <summary>
        /// Initiates dialogue and waits for completion (async/await variant)
        /// </summary>
        public static async Task RunDialogueAsync(string dialogue, bool pause, string target = null)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            InitiateDialogue(dialogue, pause, () => {
                tcs.SetResult(true);
            }, target);
            await tcs.Task;
        }

        /// <summary>
        /// Initiates dialogue and waits for completion (IEnumerator coroutine variant)
        /// </summary>
        public static IEnumerator RunDialogueCoroutine(string dialogue, bool pause, string target = null)
        {
            bool complete = false;
            InitiateDialogue(dialogue, pause, () => {
                complete = true;
            }, target);
            while (!complete)
                yield return null;
        }

        /// <summary>
        /// Sets the dynamic dialogue (experimental)
        /// </summary>
        public static void SetDynamicDialogue(DialogueScene scene)
        {
            var dialogueModule = CCBase.GetModule<DialogueModule>();
            dialogueModule.AddDialogue(DialogueModule.DynamicDialogueName, scene, true);
        }

        /// <summary>
        /// Clears the dynamic dialogue (experimental)
        /// </summary>
        public static void ClearDynamicDialogue()
        {
            var dialogueModule = CCBase.GetModule<DialogueModule>();
            dialogueModule.RemoveDialogue(DialogueModule.DynamicDialogueName);
        }

        [Command(alias = "Test", className = "Monologue")]
        static void TestMonologue(string monologue)
        {
            Monologue m = CCBase.GetModule<DialogueModule>().GetMonologue(monologue);
            ConsoleModule.WriteLine(m.GetLineRandom());
        }

        [Command(alias="TestStandalone", className="Dialogue")]
        static void TestDialogueStandalone(string dialogue)
        {
            DialogueController.CurrentDialogue = dialogue;
            CoreUtils.LoadScene("DialogueScene");
        }

        [Command(alias = "Test", className = "Dialogue")]
        static void TestDialogueInplace(string dialogue, string pause)
        {
            bool bPause = Convert.ToBoolean(pause);
            InitiateDialogue(dialogue, bPause, null);
        }

        [Command(alias = "Close", className = "Dialogue")]
        static void CloseDialogueSystems()
        {
            DialogueController.CurrentDialogue = null;
            DialogueController.CurrentCallback = null;

            var ds = GameObject.Find("DialogueSystem");
            if (ds != null)
            {
                var dc = ds.GetComponent<DialogueController>();
                dc.CloseDialogue();
            }

        }

        [Command(alias = "Purge", className = "Dialogue")]
        static void PurgeDialogueSystems()
        {
            DialogueController.CurrentDialogue = null;
            DialogueController.CurrentCallback = null;

            var ds = GameObject.Find("DialogueSystem");
            if (ds != null)
            {
                LockPauseModule.UnpauseGame(ds);
                GameObject.Destroy(ds);
            }

            var dc = GameObject.Find("DialogueCamera");
            if(dc != null)
                GameObject.Destroy(dc);

            AudioPlayer.Instance.ClearMusic(MusicSlot.Cinematic);            
            LockPauseModule.ForceCleanLocks(); //useless since objects aren't *yet* destroyed

        }

        [Command(alias = "DumpTrace", className = "Dialogue")]
        static void DumpDialogueTrace()
        {
            if (DialogueController.Trace != null)
                DebugUtils.JsonWrite(DialogueController.Trace, "dtrace");
        }
    }
}