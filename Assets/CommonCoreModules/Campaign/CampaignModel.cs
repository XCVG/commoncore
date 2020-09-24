using CommonCore.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonCore.State
{

    public class CampaignModel
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        private Dictionary<string, object> Vars { get; set; } //generic data store
        [JsonProperty]
        private HashSet<string> Flags { get; set; } //fast and easy flag store
        [JsonProperty]
        private Dictionary<string, int> Quests { get; set; } //uses quest stages: negative values are completed, 0 is not started, positive are active

        public CampaignModel()
        {
            Vars = new Dictionary<string, object>();
            Flags = new HashSet<string>();
            Quests = new Dictionary<string, int>();
        }

        //***** Var/Flag accessors
        [Obsolete]
        public string GetVar(string varName)
        {
            if (Vars.ContainsKey(varName))
            {
                if(Vars[varName] is string s)
                    return s;
                return Vars[varName]?.ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets the value of a variable
        /// </summary>
        public T GetVar<T>(string varName)
        {
            //do some fancy footwork to coerce the value

            if (!Vars.TryGetValue(varName, out var value) || value == null) //return default on null values
                return default;

            if (typeof(T).IsAssignableFrom(value.GetType())) //if value is assignable to T, do it
                return (T)value;

            if (typeof(T) == typeof(string)) //if we want a string, make it a string
                return (T)(object)value.ToString();

            return TypeUtils.CoerceValue<T>(value);
        }

        /// <summary>
        /// Checks if a variable is already defined
        /// </summary>
        public bool HasVar(string varName)
        {
            return Vars.ContainsKey(varName);
        }

        [Obsolete]
        public void SetVar(string varName, object value)
        {
            Vars[varName] = value;
        }

        /// <summary>
        /// Sets the value of a variable, overriding its existing type
        /// </summary>
        public void SetVar<T>(string varName, T value)
        {
            Vars[varName] = value;
        }

        /// <summary>
        /// Sets the value of a variable, respecting its existing type
        /// </summary>
        public void SetVarEx<T>(string varName, T value)
        {
            if(!Vars.ContainsKey(varName) || Vars[varName] == null)
            {
                //if it doesn't exist, use SetVar
                SetVar(varName, value);
            }
            else
            {
                //if it does exist, coerce the type
                Vars[varName] = TypeUtils.CoerceValue(value, Vars[varName].GetType());
            }
        }

        public KeyValuePair<string, string>[] GetAllVars()
        {
            return Vars.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value?.ToString())).ToArray();
        }

        public string ListAllVars()
        {
            StringBuilder sb = new StringBuilder(Vars.Count * 32);
            foreach(var kvp in Vars)
            {
                sb.AppendFormat("{0}: {1} ({2})\n", kvp.Key, kvp.Value, kvp.Value.Ref()?.GetType().Name ?? "null");
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