using CommonCore.Config;
using CommonCore.Console;
using CommonCore.DebugLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommonCore.RpgGame.Dialogue
{

    /// <summary>
    /// Module that handles loading
    /// </summary>
    public class DialogueModule : CCModule
    {
        //why is this separate from RPGModule? Probably a legacy of Garlic Gang to be honest

        //private static DialogueModule Instance;

        public static readonly string DynamicDialogueName = "_dynamicPreload";

        private Dictionary<string, DialogueScene> LoadedDialogues;
        private Dictionary<string, Monologue> LoadedMonologues;

        public DialogueModule()
        {
            //Instance = this;
            LoadedDialogues = new Dictionary<string, DialogueScene>();
            LoadedMonologues = new Dictionary<string, Monologue>();
            LoadAll();
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            if (CoreParams.LoadPolicy != DataLoadPolicy.OnStart)
                return;

            if (data.LoadedResources != null && data.LoadedResources.Count > 0)
            {
                var dialogueAssets = data.LoadedResources
                    .Where(kvp => kvp.Key.StartsWith("Data/Dialogue/"))
                    .Where(kvp => kvp.Value.Resource.Ref() != null)
                    .Where(kvp => kvp.Value.Resource is TextAsset)
                    .Select(kvp => (TextAsset)kvp.Value.Resource);

                var monologueAssets = data.LoadedResources
                    .Where(kvp => kvp.Key.StartsWith("Data/Monologue/"))
                    .Where(kvp => kvp.Value.Resource.Ref() != null)
                    .Where(kvp => kvp.Value.Resource is TextAsset)
                    .Select(kvp => (TextAsset)kvp.Value.Resource);

                LoadFromTextAssets(dialogueAssets, monologueAssets);
            }
        }

        private void LoadAll()
        {
            if (CoreParams.LoadPolicy != DataLoadPolicy.OnStart)
                return;

            TextAsset[] tas = CoreUtils.LoadResources<TextAsset>("Data/Dialogue/");
            TextAsset[] tasm = CoreUtils.LoadResources<TextAsset>("Data/Monologue/");
            LoadFromTextAssets(tas, tasm);
        }

        private void LoadFromTextAssets(IEnumerable<TextAsset> dialogueAssets, IEnumerable<TextAsset> monologueAssets)
        {
            int dialoguesLoaded = 0, monologuesLoaded = 0;

            foreach (var ta in dialogueAssets)
            {
                try
                {
                    LoadedDialogues[ta.name] = DialogueParser.LoadDialogueFromString(ta.name, ta.text);
                    if(ConfigState.Instance.UseVerboseLogging)
                        CDebug.LogEx("Loaded dialogue " + ta.name, LogLevel.Verbose, this);
                    dialoguesLoaded++;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            foreach (var ta in monologueAssets)
            {
                try
                {
                    LoadedMonologues[ta.name] = MonologueParser.LoadMonologueFromString(ta.text);
                    if (ConfigState.Instance.UseVerboseLogging)
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

        /// <summary>
        /// Checks if a dialogue exists
        /// </summary>
        public bool HasDialogue(string name) => LoadedDialogues.ContainsKey(name);

        /// <summary>
        /// Gets a dialogue by name
        /// </summary>
        public DialogueScene GetDialogue(string name)
        {
            DialogueScene ds;
            if (!LoadedDialogues.TryGetValue(name, out ds))
            {
                ds = DialogueParser.LoadDialogue(name);
                LoadedDialogues.Add(name, ds);
                CDebug.LogEx("Loaded new dialogue " + name, LogLevel.Verbose, this);
            }
            return ds;
        }

        /// <summary>
        /// Adds a dialogue, optionally overwriting
        /// </summary>
        public void AddDialogue(string name, DialogueScene dialogue, bool overwrite = true)
        {
            if(overwrite || !LoadedDialogues.ContainsKey(name))
            {
                LoadedDialogues[name] = dialogue;
            }
            else
            {
                throw new InvalidOperationException($"A dialogue called {name} already exists");
            }
        }

        /// <summary>
        /// Removes a dialogue
        /// </summary>
        public bool RemoveDialogue(string name)
        {
            return LoadedDialogues.Remove(name);
        }

        /// <summary>
        /// Removes a dialogue
        /// </summary>
        public bool RemoveDialogue(DialogueScene dialogue)
        {
            var key = LoadedDialogues.GetKeyForValue(dialogue);
            if (!string.IsNullOrEmpty(key))
                return LoadedDialogues.Remove(key);
            return false;
        }

        /// <summary>
        /// Returns an enumerable collection of all dialogues
        /// </summary>
        public IEnumerable<KeyValuePair<string, DialogueScene>> EnumerateDialogues()
        {
            return LoadedDialogues.ToArray();
        }

        /// <summary>
        /// Checks if a monologue exists
        /// </summary>
        public bool HasMonologue(string name) => LoadedMonologues.ContainsKey(name);

        /// <summary>
        /// Gets a monologue by name
        /// </summary>
        public Monologue GetMonologue(string name)
        {
            Monologue m;
            if (!LoadedMonologues.TryGetValue(name, out m))
            {
                m = MonologueParser.LoadMonologue(name);
                LoadedMonologues.Add(name, m);
                CDebug.LogEx("Loaded new monologue " + name, LogLevel.Verbose, this);
            }
            return m;
        }

        /// <summary>
        /// Adds a monologue, optionally overwriting
        /// </summary>
        public void AddMonologue(string name, Monologue monologue, bool overwrite = true)
        {
            if (overwrite || !LoadedMonologues.ContainsKey(name))
            {
                LoadedMonologues[name] = monologue;
            }
            else
            {
                throw new InvalidOperationException("A monologue by that name already exists");
            }
        }

        /// <summary>
        /// Removes a monologue
        /// </summary>
        public bool RemoveMonologue(string name)
        {
            return LoadedMonologues.Remove(name);
        }

        /// <summary>
        /// Removes a monologue
        /// </summary>
        public bool RemoveMonologue(Monologue monologue)
        {
            var key = LoadedMonologues.GetKeyForValue(monologue);
            if (!string.IsNullOrEmpty(key))
                return LoadedMonologues.Remove(key);
            return false;
        }

        /// <summary>
        /// Returns an enumerable collection of all monologues
        /// </summary>
        public IEnumerable<KeyValuePair<string, Monologue>> EnumerateMonologues()
        {
            return LoadedMonologues.ToArray();
        }

        [Command(alias = "ListAll", className = "Dialogue")]
        static void ListAllDialogues()
        {
            var dialogueModule = CCBase.GetModule<DialogueModule>();
            StringBuilder sb = new StringBuilder(dialogueModule.LoadedDialogues.Count * 80);
            foreach (var dialogue in dialogueModule.LoadedDialogues.Keys)
                sb.AppendLine(dialogue);
            ConsoleModule.WriteLine(sb.ToString());
        }

        [Command(alias = "ListAll", className = "Monologue")]
        static void ListAllMonologues()
        {
            var dialogueModule = CCBase.GetModule<DialogueModule>();
            StringBuilder sb = new StringBuilder(dialogueModule.LoadedMonologues.Count * 80);
            foreach (var monologue in dialogueModule.LoadedMonologues.Keys)
                sb.AppendLine(monologue);
            ConsoleModule.WriteLine(sb.ToString());
        }
    }
}