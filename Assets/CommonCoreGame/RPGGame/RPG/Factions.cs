using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;
using CommonCore.State;
using Newtonsoft.Json;

namespace CommonCore.RpgGame.Rpg
{
    public enum PredefinedFaction
    {
        None = 0, Chaotic = 1, Player, Neutral, Monster
    }

    public enum FactionRelationStatus
    {
        Neutral = 0, Friendly, Hostile
    }

    public class FactionModel
    {
        //this is hacky as fuck because we decided to make FactionModel part of GameState in 3.x

        //relationships are defined as self->target
        [JsonProperty]
        private Dictionary<string, Dictionary<string, FactionRelationStatus>> FactionTable;

        [JsonIgnore]
        public static FactionModel BaseModel { get; private set; }

        internal static void Load()
        {
            BaseModel = new FactionModel();

            BaseModel.FactionTable = new Dictionary<string, Dictionary<string, FactionRelationStatus>>(StringComparer.OrdinalIgnoreCase);

            //load predefined factions
            //difference between None and Neutral is that Monsters will attack Neutral
            //None is a true non-alignment, whereas Neutral is meant for non-hostile NPCs and such
            BaseModel.FactionTable.Add("None", new Dictionary<string, FactionRelationStatus>(StringComparer.OrdinalIgnoreCase));
            BaseModel.FactionTable.Add("Player", new Dictionary<string, FactionRelationStatus>(StringComparer.OrdinalIgnoreCase) {
                { "Monster", FactionRelationStatus.Hostile}
            });
            BaseModel.FactionTable.Add("Neutral", new Dictionary<string, FactionRelationStatus>(StringComparer.OrdinalIgnoreCase));
            BaseModel.FactionTable.Add("Monster", new Dictionary<string, FactionRelationStatus>(StringComparer.OrdinalIgnoreCase) {
                { "Neutral", FactionRelationStatus.Hostile },
                { "Player", FactionRelationStatus.Hostile }
            });

            //load factions from defs
            try
            {
                TextAsset ta = CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/factions");
                LoadFactionDataFromAsset(ta);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load faction defs!");
                Debug.LogError(e);
            }
        }        

        internal static void LoadFromAddon(AddonLoadData data)
        {
            if(data.LoadedResources != null && data.LoadedResources.Count > 0)
            {
                if(data.LoadedResources.TryGetValue("Data/RPGDefs/factions", out var rh))
                {
                    if(rh.Resource is TextAsset ta)
                    {
                        LoadFactionDataFromAsset(ta);
                        Debug.Log("Loaded faction data from addon!");
                    }
                }
            }
        }

        private static void LoadFactionDataFromAsset(TextAsset ta)
        {
            var newFactions = CoreUtils.LoadJson<Dictionary<string, Dictionary<string, FactionRelationStatus>>>(ta.text);

            foreach (var row in newFactions)
            {
                if (BaseModel.FactionTable.ContainsKey(row.Key))
                {
                    var destRow = BaseModel.FactionTable[row.Key];
                    foreach (var entry in row.Value)
                    {
                        destRow[entry.Key] = entry.Value;
                    }
                }
                else
                {
                    BaseModel.FactionTable.Add(row.Key, new Dictionary<string, FactionRelationStatus>(row.Value, StringComparer.OrdinalIgnoreCase));
                }
            }
        }

        public static FactionModel CloneFactionModel(FactionModel original)
        {
            FactionModel newModel = new FactionModel();

            newModel.FactionTable = new Dictionary<string, Dictionary<string, FactionRelationStatus>>(StringComparer.OrdinalIgnoreCase);

            //deep copy faction model
            foreach(var outerKvp in original.FactionTable)
            {
                Dictionary<string, FactionRelationStatus> outerValue = new Dictionary<string, FactionRelationStatus>(StringComparer.OrdinalIgnoreCase);
                foreach(var innerKvp in outerKvp.Value)
                {
                    outerValue.Add(innerKvp.Key, innerKvp.Value);
                }
                newModel.FactionTable.Add(outerKvp.Key, outerValue);
            }

            return newModel;
        }           

        public FactionRelationStatus GetRelation(string self, string target)
        {
            //a few simple rules:
            // -factions are always friendly with themselvs
            // -factions are always neutral toward factions they have no entry for
            // -factions are always neutral toward "None"
            // -factions are always hostile toward "Chaotic"
            // -otherwise, it's a lookup

            if (self.Equals(target, StringComparison.OrdinalIgnoreCase))
                return FactionRelationStatus.Friendly;

            if (self.Equals("None", StringComparison.OrdinalIgnoreCase) || target.Equals("None", StringComparison.OrdinalIgnoreCase))
                return FactionRelationStatus.Neutral;

            if (self.Equals("Chaotic", StringComparison.OrdinalIgnoreCase) || target.Equals("Chaotic", StringComparison.OrdinalIgnoreCase))
                return FactionRelationStatus.Hostile;

            var selfEntry = FactionTable?.GetOrDefault(self);
            if (selfEntry != null)
            {
                return selfEntry.GetOrDefault(target, FactionRelationStatus.Neutral);
            }
            else
            {
                //no entry in table for our faction
                return FactionRelationStatus.Neutral;
            }
        }

        public void SetRelation(string self, string target, FactionRelationStatus relation)
        {
            if (self.Equals(target, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("cannot set a relation when self == target");

            var selfEntry = FactionTable?.GetOrDefault(self);
            if(selfEntry == null)
            {
                selfEntry = new Dictionary<string, FactionRelationStatus>();
                FactionTable[self] = selfEntry;
            }

            selfEntry[target] = relation;
        }

        public IEnumerable<string> EnumerateFactions()
        {
            HashSet<string> factions = new HashSet<string>();

            foreach (var outerKvp in FactionTable)
            {
                factions.Add(outerKvp.Key);
                foreach (var innerKvp in outerKvp.Value)
                {
                    factions.Add(innerKvp.Key);
                }
            }

            return factions;
        }

        public IEnumerable<(string self, string target, FactionRelationStatus relation)> EnumerateRelations()
        {
            List<(string self, string target, FactionRelationStatus relation)> relations = new List<(string self, string target, FactionRelationStatus relation)>();
            foreach (var outerKvp in FactionTable)
            {
                string self = outerKvp.Key;
                foreach(var innerKvp in outerKvp.Value)
                {
                    relations.Add((self, innerKvp.Key, innerKvp.Value));
                }
            }
            return relations;
        }        
    }

}