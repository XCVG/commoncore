using CommonCore.Audio;
using CommonCore.Console;
using CommonCore.LockPause;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.RpgGame.Dialogue
{
    public delegate void DialogueFinishedDelegate();

    public class DialogueInitiator
    {        
        public static void InitiateDialogue(string dialogue, bool pause, DialogueFinishedDelegate callback)
        {
            DialogueController.CurrentDialogue = dialogue;
            DialogueController.CurrentCallback = callback;
            var prefab = CoreUtils.LoadResource<GameObject>("UI/DialogueSystem");
            var go = GameObject.Instantiate<GameObject>(prefab, CoreUtils.GetWorldRoot());
            if (pause)
                LockPauseModule.PauseGame(PauseLockType.All, go);

        }

        [Command(alias = "Test", className = "Monologue")]
        static void TestMonologue(string monologue)
        {
            Monologue m = DialogueModule.GetMonologue(monologue);
            ConsoleModule.WriteLine(m.GetLineRandom());
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
    }
}