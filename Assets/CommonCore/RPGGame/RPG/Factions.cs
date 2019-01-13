using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;

namespace CommonCore.RpgGame.Rpg
{
    public enum PredefinedFaction
    {
        None = 0, Player, Neutral, Monster
    }

    public enum FactionRelationStatus
    {
        Neutral = 0, Friendly, Hostile
    }

    public class FactionModel
    {
        //relationships are defined as self->target
        private static Dictionary<string, Dictionary<string, FactionRelationStatus>> FactionTable;

        internal static void Load()
        {
            FactionTable = new Dictionary<string, Dictionary<string, FactionRelationStatus>>();

            //load predefined factions
            //difference between None and Neutral is that Monsters will attack Neutral
            //None is a true non-alignment, whereas Neutral is meant for non-hostile NPCs and such
            FactionTable.Add("None", new Dictionary<string, FactionRelationStatus>());
            FactionTable.Add("Player", new Dictionary<string, FactionRelationStatus>() {
                { "Monster", FactionRelationStatus.Hostile}
            });
            FactionTable.Add("Neutral", new Dictionary<string, FactionRelationStatus>());
            FactionTable.Add("Monster", new Dictionary<string, FactionRelationStatus>() {
                { "Neutral", FactionRelationStatus.Hostile },
                { "Player", FactionRelationStatus.Hostile }
            });

            //load factions from defs
            try
            {
                TextAsset ta = CoreUtils.LoadResource<TextAsset>("RPGDefs/factions");
                var newFactions = CoreUtils.LoadJson<Dictionary<string, Dictionary<string, FactionRelationStatus>>>(ta.text);

                foreach(var row in newFactions)
                {
                    if(FactionTable.ContainsKey(row.Key))
                    {
                        var destRow = FactionTable[row.Key];
                        foreach(var entry in row.Value)
                        {
                            destRow[entry.Key] = entry.Value;
                        }
                    }
                    else
                    {
                        FactionTable.Add(row.Key, row.Value);
                    }
                }
            }
            catch(Exception e)
            {
                CDebug.LogEx("Failed to load faction defs!", LogLevel.Error, typeof(FactionModel));
                CDebug.LogError(e);
            }
        }

        public static FactionRelationStatus GetRelation(string self, string target)
        {
            //a few simple rules:
            // -factions are always friendly with themselvs
            // -factions are always neutral toward factions they have no entry for
            // -factions are always neutral toward "None"
            // -otherwise, it's a lookup

            if (self == target || self == "None" || target == "None")
                return FactionRelationStatus.Friendly;

            var selfEntry = FactionTable.GetOrDefault(self);
            if(selfEntry != null)
            {
                return selfEntry.GetOrDefault(target, FactionRelationStatus.Neutral);
            }
            else
            {
                //no entry in table for our faction
                return FactionRelationStatus.Neutral;
            }
        }

        public static string GetFactionsList()
        {
            StringBuilder sb = new StringBuilder(FactionTable.Count * 100);

            foreach(var row in FactionTable)
            {
                sb.AppendLine(FactionTableRowToString(row));
            }

            return sb.ToString();
        }

        private static string FactionTableRowToString(KeyValuePair<string, Dictionary<string, FactionRelationStatus>> row)
        {
            StringBuilder sb = new StringBuilder(row.Key.Length * 32);

            sb.AppendFormat("{0} : ", row.Key);

            foreach(var entry in row.Value)
            {
                sb.AppendFormat("[{0}: {1}] ", entry.Key, entry.Value.ToString());
            }

            return sb.ToString();
        }
    }

}