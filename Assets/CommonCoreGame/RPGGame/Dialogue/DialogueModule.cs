using CommonCore.Console;
using CommonCore.DebugLog;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CommonCore.RpgGame.Dialogue
{

    public class DialogueModule : CCModule
    {
        private static DialogueModule Instance;

        private Dictionary<string, DialogueScene> LoadedDialogues;
        private Dictionary<string, Monologue> LoadedMonologues;

        public DialogueModule()
        {
            Instance = this;
            LoadedDialogues = new Dictionary<string, DialogueScene>();
            LoadedMonologues = new Dictionary<string, Monologue>();
            LoadAll();
        }

        private void LoadAll()
        {
            if (CoreParams.LoadPolicy != DataLoadPolicy.OnStart)
                return;

            int dialoguesLoaded = 0, monologuesLoaded = 0;

            TextAsset[] tas = CoreUtils.LoadResources<TextAsset>("Data/Dialogue/");
            foreach(var ta in tas)
            {
                try
                {
                    LoadedDialogues.Add(ta.name, DialogueParser.LoadDialogueFromString(ta.name, ta.text));
                    CDebug.LogEx("Loaded dialogue " + ta.name, LogLevel.Verbose, this);
                    dialoguesLoaded++;
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }

            TextAsset[] tasm = CoreUtils.LoadResources<TextAsset>("Data/Monologue/");
            foreach (var ta in tasm)
            {
                try
                {
                    LoadedMonologues.Add(ta.name, MonologueParser.LoadMonologueFromString(ta.text));
                    CDebug.LogEx("Loaded monologue " + ta.name, LogLevel.Verbose, this);
                    monologuesLoaded++;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Log($"Loaded {dialoguesLoaded} dialogues, {monologuesLoaded} monologues");
        }

        public override void Dispose()
        {
            base.Dispose();

            Instance = null;
        }

        internal static DialogueScene GetDialogue(string name)
        {
            DialogueScene ds;
            if(!Instance.LoadedDialogues.TryGetValue(name, out ds))
            {
                ds = DialogueParser.LoadDialogue(name);
                Instance.LoadedDialogues.Add(name, ds);
                CDebug.LogEx("Loaded new dialogue " + name, LogLevel.Verbose, Instance);
            }
            return ds;
        }

        internal static Monologue GetMonologue(string name)
        {
            Monologue m;
            if(!Instance.LoadedMonologues.TryGetValue(name, out m))
            {
                m = MonologueParser.LoadMonologue(name);
                Instance.LoadedMonologues.Add(name, m);
                CDebug.LogEx("Loaded new monologue " + name, LogLevel.Verbose, Instance);
            }
            return m;
        }

        [Command(alias = "ListAll", className = "Dialogue")]
        static void ListAllDialogues()
        {
            StringBuilder sb = new StringBuilder(Instance.LoadedDialogues.Count * 80);
            foreach (var dialogue in Instance.LoadedDialogues.Keys)
                sb.AppendLine(dialogue);
            ConsoleModule.WriteLine(sb.ToString());
        }

        [Command(alias = "ListAll", className = "Monologue")]
        static void ListAllMonologues()
        {
            StringBuilder sb = new StringBuilder(Instance.LoadedMonologues.Count * 80);
            foreach (var monologue in Instance.LoadedMonologues.Keys)
                sb.AppendLine(monologue);
            ConsoleModule.WriteLine(sb.ToString());
        }
    }
}