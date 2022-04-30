using CommonCore.DebugLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{

    public class QuestModel
    {
        private static Dictionary<string, QuestDef> Defs;

        private static int LoadErrorCount;

        internal static void Load()
        {
            LoadErrorCount = 0;

            Defs = new Dictionary<string, QuestDef>();
            LoadLegacy();
            LoadAll();

            CDebug.LogEx(string.Format("Loaded quests ({0} defs, {1} errors)", Defs.Count, LoadErrorCount), LogLevel.Message, null);
        }

        internal static void LoadFromAddon(AddonLoadData data)
        {
            if (data.LoadedResources != null && data.LoadedResources.Count > 0)
            {
                var questAssets = data.LoadedResources
                    .Where(kvp => kvp.Key.StartsWith("Data/Quests/"))
                    .Where(kvp => kvp.Value.Resource is TextAsset)
                    .Select(kvp => (TextAsset)kvp.Value.Resource);

                LoadDefsFromAssets(questAssets, out var defCount, out var loadErrorCount);

                CDebug.LogEx(string.Format("Loaded quests from addon ({0} defs, {1} errors)", defCount, loadErrorCount), LogLevel.Message, null);
            }
        }

        private static void LoadLegacy()
        {
            //load quest defs from RPGDefs/rpg_quests.json

            try
            {
                CDebug.LogEx("Loading legacy quest defs!", LogLevel.Verbose, null);
                string data = CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/rpg_quests").text;
                var newDefs = JsonConvert.DeserializeObject<Dictionary<string, QuestDef>>(data, CoreParams.DefaultJsonSerializerSettings);
                newDefs.ToList().ForEach(x => Defs[x.Key] = x.Value);

            }
            catch(Exception e)
            {
                CDebug.LogEx(e.ToString(), LogLevel.Verbose, null);
                LoadErrorCount++;
            }

        }        

        private static void LoadAll()
        {
            //load quest defs (and in the future other things?) from files in Quests/
            CDebug.LogEx("Loading new style quest defs!", LogLevel.Verbose, null);

            TextAsset[] tas = CoreUtils.LoadResources<TextAsset>("Data/Quests/");
            LoadDefsFromAssets(tas, out _, out var loadErrorCount);
            LoadErrorCount += loadErrorCount;
        }

        private static void LoadDefsFromAssets(IEnumerable<TextAsset> questAssets, out int defCount, out int loadErrorCount)
        {
            defCount = 0;
            loadErrorCount = 0;

            foreach (TextAsset ta in questAssets)
            {
                try
                {
                    JObject outerJObject = JObject.Parse(ta.text); //this contains one or more items
                    foreach (JProperty itemJProperty in outerJObject.Properties())
                    {
                        string itemName = itemJProperty.Name;
                        JToken itemJToken = itemJProperty.Value;

                        //parse def
                        JToken defJToken = itemJToken["def"];
                        if (defJToken != null)
                        {
                            Defs[itemName] = JsonConvert.DeserializeObject<QuestDef>(defJToken.ToString(), CoreParams.DefaultJsonSerializerSettings);
                            defCount++;
                        }
                    }
                }
                catch (Exception e)
                {
                    CDebug.LogEx(e.ToString(), LogLevel.Verbose, null);
                    loadErrorCount++;
                }
            }
        }

        /// <summary>
        /// Checks if a quest definition exists
        /// </summary>
        public static bool HasDef(string name)
        {
            return Defs.ContainsKey(name);
        }

        /// <summary>
        /// Gets a quest definition
        /// </summary>
        public static QuestDef GetDef(string name)
        {
            if (!Defs.ContainsKey(name))
                return null;

            return Defs[name];
        }

        /// <summary>
        /// Adds a quest definition, optionally overwriting
        /// </summary>
        public static void AddDef(string name, QuestDef def, bool overwrite = true)
        {
            if (overwrite || !Defs.ContainsKey(name))
            {
                Defs[name] = def;
            }
            else
            {
                throw new InvalidOperationException("A quest definition by that name already exists");
            }
        }

        /// <summary>
        /// Returns an enumerable collection of all quest definitions
        /// </summary>
        public static IEnumerable<KeyValuePair<string, QuestDef>> EnumerateDefs()
        {
            return Defs.ToArray();
        }

        /// <summary>
        /// Gets the nice name of a quest, or its plain name if the nice name isn't available
        /// </summary>
        public static string GetNiceName(string name)
        {
            var qd = GetDef(name);
            if(qd != null)
            {
                var niceName = qd.NiceName;
                if (!string.IsNullOrEmpty(niceName))
                    return niceName;
            }

            return name;
        }

        /// <summary>
        /// Gets a nicely formatted list of quest definitions
        /// </summary>
        public static string GetDefsList()
        {
            StringBuilder sb = new StringBuilder(Defs.Count * 64);

            foreach (var kvp in Defs) //YES I WAS SMART ENOUGH TO USE VAR
            {
                sb.AppendFormat("{0}: {1} \n", kvp.Key, kvp.Value.ToString());
            }

            return sb.ToString();
        }
    }

    public class QuestDef
    {
        [JsonProperty]
        public string NiceName { get; protected set; }
        [JsonProperty]
        public string Image { get; protected set; }
        [JsonProperty]
        public string Description { get; protected set; }
        [JsonProperty]
        protected Dictionary<int, string> Stages { get; set; } = new Dictionary<int, string>();
        [JsonProperty]
        public bool Hidden { get; protected set; }

        [JsonProperty]
        public IReadOnlyDictionary<string, object> ExtraData { get; private set; } = new Dictionary<string, object>();

        public QuestDef(string niceName, string image, string description, Dictionary<int, string> stageText, bool hidden, IEnumerable<KeyValuePair<string, object>> extraData)
        {
            NiceName = niceName;
            Image = image;
            Description = description;
            Stages = stageText;
            Hidden = hidden;
            ExtraData = extraData.ToDictionary(x => x.Key, x => x.Value);
        }

        public string GetStageText(int stage)
        {
            if (Stages.ContainsKey(stage))
                return Stages[stage];
            else
                return null;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", NiceName, Description);
        }

        public string ToLongString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0} : {1} [image: {2}]\n", NiceName, Description, Image);
            foreach(var kvp in Stages)
            {
                sb.AppendFormat("\t{0}: {1}\n", kvp.Key, kvp.Value);
            }

            return sb.ToString();
        }
    }
}