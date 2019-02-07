using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Text;
using CommonCore.Messaging;

namespace CommonCore.RpgGame.State
{

    public class CampaignModel
    {
        //TODO need to flesh this out with accessors

        [JsonProperty]
        private Dictionary<string, string> Vars { get; set; } //generic data store
        [JsonProperty]
        private HashSet<string> Flags { get; set; } //fast and easy flag store
        [JsonProperty]
        private Dictionary<string, int> Quests { get; set; } //uses quest stages: negative values are completed, 0 is not started, positive are active

        public CampaignModel()
        {
            Vars = new Dictionary<string, string>();
            Flags = new HashSet<string>();
            Quests = new Dictionary<string, int>();
        }

        //***** Var/Flag accessors
        public string GetVar(string varName)
        {
            if (Vars.ContainsKey(varName))
                return Vars[varName];
            return null;
        }

        public T GetVar<T>(string varName)
        {
            //we may eventually do some fancy footwork to allow eg retrieving a string as an int, but not today
            var value = GetVar(varName);
            if (value == null)
                return default(T);
            else
                return (T)Convert.ChangeType(value, typeof(T));
        }

        public bool HasVar(string varName)
        {
            return Vars.ContainsKey(varName);
        }

        public KeyValuePair<string, string>[] GetAllVars()
        {
            return Vars.ToArray();
        }

        public void SetVar(string varName, string value)
        {
            Vars[varName] = value;
        }

        public string ListAllVars()
        {
            StringBuilder sb = new StringBuilder(Vars.Count * 32);
            foreach(var kvp in Vars)
            {
                sb.AppendFormat("{0}: {1}\n", kvp.Key, kvp.Value);
            }
            return sb.ToString();
        }

        public bool HasFlag(string flagName)
        {
            return Flags.Contains(flagName);
        }

        public string[] GetAllFlags()
        {
            return Flags.ToArray();
        }

        public void SetFlag(string flagName, bool flagState)
        {
            if(flagState)
            {
                if (!Flags.Contains(flagName))
                    Flags.Add(flagName);
            }
            else
            {
                if (Flags.Contains(flagName))
                    Flags.Remove(flagName);
            }
        }

        public void AddFlag(string flagName)
        {
            Flags.Add(flagName);
        }

        public void ClearFlag(string flagName)
        {
            Flags.Remove(flagName);
        }

        public void ToggleFlag(string flagName)
        {
            SetFlag(flagName, !HasFlag(flagName));
        }

        public string ListAllFlags()
        {
            StringBuilder sb = new StringBuilder(Flags.Count * 16);
            foreach (var flag in Flags)
            {
                sb.AppendLine(flag);
            }
            return sb.ToString();
        }
        

        //***** Quest accessors

        //quests don't actually trigger anything... yet.
        //it's not planned until maybe Downwarren (v4)

        public int GetQuestStage(string questName)
        {
            int stage = 0;
            Quests.TryGetValue(questName, out stage);
            return stage;
        }

        public void SetQuestStage(string questName, int questStage)
        {
            Quests[questName] = questStage;

            //honestly this is super hacky and I don't like it
            if(questStage < 0)
                QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgQuestEnded", "Quest", questName));
        }

        //StartQuest only starts a quest if it is not started
        public void StartQuest(string questName)
        {
            StartQuest(questName, 1);
        }

        public void StartQuest(string questName, int initialStage)
        {
            int oldStage = 0;
            Quests.TryGetValue(questName, out oldStage);
            if (oldStage == 0)
            {
                Quests[questName] = initialStage;

                //honestly this is super hacky and I don't like it
                QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgQuestStarted", "Quest", questName));
            }
            
        }

        public bool HasQuest(string questName)
        {
            return Quests.ContainsKey(questName);
        }

        public bool IsQuestStarted(string questName)
        {
            if(Quests.ContainsKey(questName))
            {
                return Quests[questName] != 0;
            }
            return false;
        }

        public bool IsQuestActive(string questName)
        {
            if (Quests.ContainsKey(questName))
            {
                return Quests[questName] > 0;
            }
            return false;
        }

        public bool IsQuestFinished(string questName)
        {
            if (Quests.ContainsKey(questName))
            {
                return Quests[questName] < 0;
            }
            return false;
        }

        public KeyValuePair<string, int>[] GetAllQuests()
        {
            return Quests.ToArray();
        }

        public string ListAllQuests()
        {
            StringBuilder sb = new StringBuilder(Quests.Count * 32);
            foreach (var kvp in Quests)
            {
                sb.AppendFormat("{0}: {1}\n", kvp.Key, kvp.Value);
            }
            return sb.ToString();
        }
    }
}