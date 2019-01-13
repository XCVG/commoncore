using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;
using CommonCore.State;


namespace CommonCore.Rpg
{
    public static class RpgConsoleCommands //will probably break this up at some point, but maybe not until Balmora
    {

        //***** Player Manipulation

        [Command(className = "Player")]
        static void GetAV(string av)
        {
            try
            {
                var value = GameState.Instance.PlayerRpgState.GetAV<object>(av);
                DevConsole.singleton.Log(string.Format("{0} : {1}", av, value));
            }
            catch (Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

        [Command(className = "Player")]
        static void SetAV(string av, string value)
        {
            object convertedValue = CoreUtils.StringToNumericAuto(value);

            //we *should* now have the correct type in the box
            //DevConsole.singleton.Log(convertedValue.GetType().Name);
            try
            {
                if(convertedValue.GetType() == typeof(float))
                    GameState.Instance.PlayerRpgState.SetAV(av, (float)convertedValue);
                else if (convertedValue.GetType() == typeof(int))
                    GameState.Instance.PlayerRpgState.SetAV(av, (int)convertedValue);
                else if (convertedValue.GetType() == typeof(string))
                    GameState.Instance.PlayerRpgState.SetAV(av, (string)convertedValue);
                else
                    GameState.Instance.PlayerRpgState.SetAV(av, convertedValue);
            }
            catch(Exception e)
            {
                DevConsole.singleton.LogError(e.ToString() + e.StackTrace);
            }
        }

        [Command(className = "Player")]
        static void ModAV(string av, string value)
        {
            object convertedValue = CoreUtils.StringToNumericAuto(value);

            //we *should* now have the correct type in the box
            //DevConsole.singleton.Log(convertedValue.GetType().Name);
            try
            {
                if (convertedValue.GetType() == typeof(float))
                    GameState.Instance.PlayerRpgState.ModAV(av, (float)convertedValue);
                else if (convertedValue.GetType() == typeof(int))
                    GameState.Instance.PlayerRpgState.ModAV(av, (int)convertedValue);
                else if (convertedValue.GetType() == typeof(string))
                    GameState.Instance.PlayerRpgState.ModAV(av, (string)convertedValue);
                else
                    GameState.Instance.PlayerRpgState.ModAV(av, convertedValue);
            }
            catch (Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

        [Command(className = "Player")]
        static void LevelUp()
        {
            var player = GameState.Instance.PlayerRpgState;
            player.Experience = RpgValues.XPToNext(player.Level);
            player.CheckLevelUp();
        }

        //***** Faction manipulation

        [Command]
        static void ListFactions()
        {
            DevConsole.singleton.Log(FactionModel.GetFactionsList());
        }

        //***** MapMarker manipulation

        [Command]
        static void ListMapMarkers()
        {
            DevConsole.singleton.Log(GameState.Instance.MapMarkers.ToNiceString());
        }

        [Command]
        static void SetMapMarker(string marker, string markerState)
        {
            GameState.Instance.MapMarkers[marker] = (MapMarkerState)Enum.Parse(typeof(MapMarkerState), markerState);
        }

        //***** Inventory manipulation

        [Command]
        static void ListInventoryModels()
        {
            DevConsole.singleton.Log(InventoryModel.GetModelsList());
        }

        [Command]
        static void ListInventoryDefs()
        {
            DevConsole.singleton.Log(InventoryModel.GetDefsList());
        }

        [Command]
        static void ListSharedContainers()
        {

            var containersDict = GameState.Instance.ContainerState;
            StringBuilder sb = new StringBuilder(containersDict.Count * 64);
            foreach (var kvp in containersDict)
            {
                sb.AppendFormat("{0} ({1} items) \n", kvp.Key, kvp.Value.ListItems().Length);
            }

            DevConsole.singleton.Log(sb.ToString());
        }

        [Command]
        static void OpenSharedContainer(string container, string asShop)
        {
            var rContainer = GameState.Instance.ContainerState[container];
            bool bIsShop = Convert.ToBoolean(asShop);

            UI.ContainerModal.PushModal(GameState.Instance.PlayerRpgState.Inventory, rContainer, bIsShop, null);
        }

        [Command(className = "Player")]
        static void ListItems()
        {
            var items = GameState.Instance.PlayerRpgState.Inventory.GetItemsListActual();
            foreach(var item in items)
            {
                DevConsole.singleton.Log(item.ToString());
            }
        }

        [Command(className = "Player")]
        static void CountItem(string item)
        {
            try
            {
                var quantity = GameState.Instance.PlayerRpgState.Inventory.CountItem(item);
                DevConsole.singleton.Log(string.Format("{0}:{1}", item, quantity));
            }
            catch (Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

        [Command(className = "Player")]
        static void AddItem(string item)
        {
            AddItem(item, "1");
        }

        [Command(className = "Player")]
        static void AddItem(string item, string quantity)
        {
            try
            {
                GameState.Instance.PlayerRpgState.Inventory.AddItem(item, int.Parse(quantity));
            }
            catch (Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

        [Command(className = "Player")]
        static void RemoveItem(string item)
        {
            try
            {
                GameState.Instance.PlayerRpgState.Inventory.UseItem(item);
            }
            catch (Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

        [Command(className = "Player")]
        static void RemoveItem(string item, string quantity)
        {
            try
            {
                GameState.Instance.PlayerRpgState.Inventory.UseItem(item, int.Parse(quantity));
            }
            catch (Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

        //***** Quest model/def manipulation
        [Command]
        static void ListQuestDefs()
        {
            DevConsole.singleton.Log(QuestModel.GetDefsList());
        }

        [Command]
        static void GetQuestDef(string questDef)
        {
            DevConsole.singleton.Log(QuestModel.GetDef(questDef).ToLongString());
        }

        //***** Campaign manipulation

        [Command(className = "Campaign")]
        static void GetVar(string varName)
        {
            DevConsole.singleton.Log(GameState.Instance.CampaignState.GetVar(varName));
        }

        [Command(className = "Campaign")]
        static void SetVar(string varName, string newValue)
        {
            GameState.Instance.CampaignState.SetVar(varName, newValue);
        }

        [Command(className = "Campaign")]
        static void ListAllVars()
        {
            DevConsole.singleton.Log(GameState.Instance.CampaignState.ListAllVars());
        }

        [Command(className = "Campaign")]
        static void GetFlag(string flagName)
        {
            DevConsole.singleton.Log(GameState.Instance.CampaignState.HasFlag(flagName).ToString());
        }

        [Command(className = "Campaign")]
        static void SetFlag(string flagName, string flagState)
        {
            GameState.Instance.CampaignState.SetFlag(flagName, Convert.ToBoolean(flagState));
        }

        [Command(className = "Campaign")]
        static void ListAllFlags()
        {
            DevConsole.singleton.Log(GameState.Instance.CampaignState.ListAllFlags());
        }

        [Command(className = "Campaign")]
        static void GetQuestStage(string questName)
        {
            DevConsole.singleton.Log(GameState.Instance.CampaignState.GetQuestStage(questName).ToString());
        }

        [Command(className = "Campaign")]
        static void SetQuestStage(string questName, string questStage)
        {
            GameState.Instance.CampaignState.SetQuestStage(questName, Convert.ToInt32(questStage));
        }

        [Command(className = "Campaign")]
        static void ListAllQuests()
        {
            DevConsole.singleton.Log(GameState.Instance.CampaignState.ListAllQuests());
        }

        //***** Event manipulation
        [Command]
        static void DisplayTime()
        {
            string timeStr = string.Format("Real Time:  {0:F1}s\nGame Time:  {1:F1}s\nWorld Time: {2:F1}d, {3}s\n\nUnity Timescale: {4}\nWorld Timescale: {5}",
                GameState.Instance.WorldState.RealTimeElapsed,
                GameState.Instance.WorldState.GameTimeElapsed,
                GameState.Instance.WorldState.WorldDaysElapsed,
                GameState.Instance.WorldState.WorldSecondsElapsed,
                Time.timeScale,
                GameState.Instance.WorldState.WorldTimeScale);
            DevConsole.singleton.Log(timeStr);
        }

        [Command(className = "Events")]
        static void ListDelayedEvents()
        {
            DevConsole.singleton.Log(GameState.Instance.DelayedEvents.ToNiceString());
        }

        [Command(className = "Events")]
        static void ClearDelayedEvents()
        {
            GameState.Instance.DelayedEvents.Clear();
        }
    }
}