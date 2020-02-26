﻿using CommonCore.Config;
using CommonCore.StringSub;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommonCore.State
{

    public enum SaveGameType
    {
        Manual, Quick, Auto
    }

    /// <summary>
    /// Struct representing a save file
    /// </summary>
    public struct SaveGameInfo
    {
        public string NiceName { get; set; }
        public string FileName { get; set; }
        public SaveGameType Type { get; set; }        
        public DateTime Date { get; set; }

        public string Location { get; set; }
        public byte[] ImageData { get; set; } 
        public string CampaignHash { get; set; }

        /// <summary>
        /// Create a SaveGameInfo from parameters
        /// </summary>
        public SaveGameInfo(string niceName, string fileName, SaveGameType type, DateTime date)
        {
            NiceName = niceName;
            FileName = fileName;
            Type = type;            
            Date = date;

            ImageData = null;
            Location = null;
            CampaignHash = null;
        }

        /// <summary>
        /// Create a SaveGameInfo from a file via FileInfo object
        /// </summary>
        public SaveGameInfo(FileInfo fileInfo)
        {
            SaveGameType type = SaveGameType.Manual;
            string niceName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            if (niceName.Contains("_"))
            {
                //split nicename
                string[] splitName = niceName.Split('_');
                if (splitName.Length >= 2)
                {
                    niceName = niceName.Substring(niceName.IndexOf('_') + 1);
                    if (splitName[0] == "q")
                    {
                        niceName = $"{Sub.Replace("Quicksave", "IGUI_SAVE")}";
                        type = SaveGameType.Quick;
                    }
                    else if (splitName[0] == "a")
                    {
                        niceName = $"{Sub.Replace("Autosave", "IGUI_SAVE")} {(splitName.Length > 2 ? splitName[2] : "?")}";
                        type = SaveGameType.Auto;
                    }
                    else if (splitName[0] == "m")
                        type = SaveGameType.Manual;
                    else
                        niceName = Path.GetFileNameWithoutExtension(fileInfo.Name); //undo our oopsie if it turns out someone is trolling with prefixes
                }

            }

            NiceName = niceName;
            FileName = fileInfo.Name;
            Type = type;
            Date = fileInfo.CreationTime;

            ImageData = null;
            Location = null;
            CampaignHash = null;
        }

        /// <summary>
        /// Loads metadata from the save file
        /// </summary>
        public void LoadMetadata()
        {
            //nop for now
        }
    }

    public static class SaveUtils
    {
        public static string GetSafeName(string name)
        {
            //we play it *very* safe, allowing only Latin alphanumeric characters
            //and we truncate filenames to 30 characters

            StringBuilder cleanName = new StringBuilder(name.Length);

            foreach(char c in name)
            {
                if (c < 128 && char.IsLetterOrDigit(c))
                    cleanName.Append(c);
            }

            return cleanName.ToString(0, Math.Min(30, cleanName.Length));
        }

        //all these should be "safe" (log errors instead of throwing) and check conditions themselves

        /// <summary>
        /// Creates a quicksave, if allowed to do so
        /// </summary>
        public static void DoQuickSave()
        {
            try
            {                
                if (GameState.Instance.SaveLocked)
                    return;

                //quicksave format will be q_<hash>
                //since we aren't supporting campaign-unique autosaves yet, hash will just be 0

                string campaignId = "0";
                string saveFileName = $"q_{campaignId}.json";
                string saveFilePath = Path.Combine(CoreParams.SavePath, saveFileName);

                SharedUtils.SaveGame(saveFileName, true);                

                //Debug.LogWarning("Quicksave!");
            }
            catch(Exception e)
            {
                Debug.LogError($"Quicksave failed! ({e.GetType().Name})");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Loads the quicksave if it exists
        /// </summary>
        public static void DoQuickLoad()
        {
            try
            {
                //quicksave format will be q_<hash>
                //since we aren't supporting campaign-unique autosaves yet, hash will just be 0

                string campaignId = "0";
                string saveFileName = $"q_{campaignId}.json";
                string saveFilePath = Path.Combine(CoreParams.SavePath, saveFileName);

                if(File.Exists(saveFilePath))
                {
                    SharedUtils.LoadGame(saveFileName);
                }
                else
                {
                    Debug.Log("Quickload failed (doesn't exist)");
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"Quickload failed! ({e.GetType().Name})");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Creates an autosave, if allowed to do so
        /// </summary>
        public static void DoAutoSave()
        {
            DoAutoSave(false);
        }

        /// <summary>
        /// Creates an autosave
        /// </summary>
        public static void DoAutoSave(bool force)
        {
            try
            {
                if (GameState.Instance.SaveLocked && !force)
                    return; //don't autosave if we're not allowed to

                if (ConfigState.Instance.AutosaveCount <= 0 && !force)
                    return; //don't autosave if we've disabled it

                //autosave format will be a_<hash>_<index>
                //since we aren't supporting campaign-unique autosaves, yet, hash will just be 0
                string campaignId = "0";
                string filterString = $"a_{campaignId}_";

                string savePath = CoreParams.SavePath;
                DirectoryInfo saveDInfo = new DirectoryInfo(savePath);
                FileInfo[] savesFInfo = saveDInfo.GetFiles().Where(f => f.Name.StartsWith(filterString)).OrderBy(f => f.Name).Reverse().ToArray();

                //Debug.Log(savesFInfo.Select(f => f.Name).ToNiceString());

                int highestSaveId = 1;
                foreach(var saveFI in savesFInfo)
                {
                    try
                    {
                        var nameParts = Path.GetFileNameWithoutExtension(saveFI.Name).Split('_');
                        int saveId = int.Parse(nameParts[2]);
                        if (saveId > highestSaveId)
                            highestSaveId = saveId;
                    }
                    catch(Exception)
                    {
                        Debug.LogError($"Found an invalid save file: {saveFI.Name}");
                    }
                }

                //save this autosave
                string newSaveName = $"a_{campaignId}_{highestSaveId + 1}.json";
                SharedUtils.SaveGame(newSaveName, false);

                //remove old autosaves
                int numAutosaves = savesFInfo.Length + 1;
                for(int i = savesFInfo.Length - 1; i >= 0 && numAutosaves > ConfigState.Instance.AutosaveCount; i--)
                {
                    var saveFI = savesFInfo[i];
                    try
                    {
                        File.Delete(Path.Combine(CoreParams.SavePath, saveFI.Name));
                        numAutosaves--;
                    }
                    catch(Exception)
                    {
                        Debug.LogError($"Failed to delete save file: {saveFI.Name}");
                    }
                }

                Debug.Log($"Autosave complete ({newSaveName})"); //TODO autosave indication
            }
            catch(Exception e)
            {
                Debug.LogError($"An error occurred during the autosave process ({e.GetType().Name})");
                Debug.LogException(e);
            }
        }


    }

}