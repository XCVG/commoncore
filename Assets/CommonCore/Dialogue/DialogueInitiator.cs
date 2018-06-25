using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;
using UnityEngine.SceneManagement;

namespace CommonCore.Dialogue
{
    public delegate void DialogueFinishedDelegate();

    public class DialogueInitiator
    {        
        public static void InitiateDialogue(string dialogue, bool pause, DialogueFinishedDelegate callback)
        {
            DialogueController.CurrentDialogue = dialogue;
            DialogueController.CurrentCallback = callback;
            //TODO pause, have to rework the way pausing/unpausing works
            var prefab = Resources.Load<GameObject>("UI/DialogueSystem");
            GameObject.Instantiate<GameObject>(prefab, CCBaseUtil.GetWorldRoot());
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