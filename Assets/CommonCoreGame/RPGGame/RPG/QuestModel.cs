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

        private static void LoadLegacy()
        {
            //load quest defs from RPGDefs/rpg_quests.json

            try
            {
                CDebug.LogEx("Loading legacy quest defs!", LogLevel.Verbose, null);
                string data = CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/rpg_quests").text;
                var newDefs = JsonConvert.DeserializeObject<Dictionary<string, QuestDef>>(data, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
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
            foreach (TextAsset ta in tas)
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
                            Defs[itemName] = JsonConvert.DeserializeObject<QuestDef>(defJToken.ToString(), new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    CDebug.LogEx(e.ToString(), LogLevel.Verbose, null);
                    LoadErrorCount++;
                }
            }
        }

        public static QuestDef GetDef(string name)
        {
            if (!Defs.ContainsKey(name))
                return null;

            return Defs[name];
        }

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
        public readonly string NiceName;
        [JsonProperty]
        public readonly string Image;
        [JsonProperty]
        public readonly string Description;
        [JsonProperty]
        protected readonly Dictionary<int, string> Stages;
        [JsonProperty]
        public readonly bool Hidden;

        [JsonConstructor]
        public QuestDef(string niceName, string image, string description, Dictionary<int, string> stageText, bool hidden)
        {
            NiceName = niceName;
            Image = image;
            Description = description;
            Stages = stageText;
            Hidden = hidden;
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