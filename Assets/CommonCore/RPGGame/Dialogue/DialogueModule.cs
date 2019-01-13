using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;

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
            CDebug.LogEx("Dialogue module loaded!", LogLevel.Message, this);
        }

        private void LoadAll()
        {
            if (CoreParams.LoadPolicy != DataLoadPolicy.OnStart)
                return;

            TextAsset[] tas = CoreUtils.LoadResources<TextAsset>("Dialogue/");
            foreach(var ta in tas)
            {
                try
                {
                    LoadedDialogues.Add(ta.name, DialogueParser.LoadDialogueFromString(ta.name, ta.text));
                    CDebug.LogEx("Loaded dialogue " + ta.name, LogLevel.Verbose, this);
                }
                catch(Exception e)
                {
                    CDebug.LogException(e);
                }
            }

            TextAsset[] tasm = CoreUtils.LoadResources<TextAsset>("Monologue/");
            foreach (var ta in tasm)
            {
                try
                {
                    LoadedMonologues.Add(ta.name, MonologueParser.LoadMonologueFromString(ta.text));
                    CDebug.LogEx("Loaded monologue " + ta.name, LogLevel.Verbose, this);
                }
                catch (Exception e)
                {
                    CDebug.LogException(e);
                }
            }
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
    }
}