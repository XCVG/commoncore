using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SickDev.CommandSystem;
using CommonCore.LockPause;

namespace CommonCore.Dialogue
{
    public delegate void DialogueFinishedDelegate();

    public class DialogueInitiator
    {        
        public static void InitiateDialogue(string dialogue, bool pause, DialogueFinishedDelegate callback)
        {
            DialogueController.CurrentDialogue = dialogue;
            DialogueController.CurrentCallback = callback;
            var prefab = Resources.Load<GameObject>("UI/DialogueSystem");
            var go = GameObject.Instantiate<GameObject>(prefab, CCBaseUtil.GetWorldRoot());
            if (pause)
                LockPauseModule.PauseGame(PauseLockType.All, go);

        }

        [Command(alias = "Test", className = "Monologue")]
        static void TestMonologue(string monologue)
        {
            Monologue m = new Monologue(monologue);
            DevConsole.singleton.Log(m.GetLineRandom());
        }

        [Command(alias="TestStandalone", className="Dialogue")]
        static void TestDialogueStandalone(string dialogue)
        {
            DialogueController.CurrentDialogue = dialogue;
            SceneManager.LoadScene("DialogueScene");
        }

        [Command(alias = "Test", className = "Dialogue")]
        static void TestDialogueInplace(string dialogue, string pause)
        {
            bool bPause = Convert.ToBoolean(pause);
            InitiateDialogue(dialogue, bPause, null);
        }

        [Command(alias = "Purge", className = "Dialogue")]
        static void PurgeDialogueSystems()
        {
            DialogueController.CurrentDialogue = null;
            DialogueController.CurrentCallback = null;
            GameObject.Destroy(GameObject.Find("DialogueSystem"));
        }
    }
}