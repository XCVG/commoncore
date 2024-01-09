using CommonCore;
using CommonCore.Config;
using CommonCore.Scripting;
using CommonCore.StringSub;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.State
{

    public class SaveNotAllowedException : Exception
    {
        public SaveNotAllowedException() : base("Saving is not allowed")
        {

        }

        public SaveNotAllowedException(string message) : base(message)
        {

        }
    }

    public enum SaveGameType
    {
        Manual, Quick, Auto
    }

    /// <summary>
    /// Struct representing a save file
    /// </summary>
    /// <remarks>
    /// <para>Used by save and load panels</para>
    /// <para>Not what's actually stored, that's <see cref="SaveMetadata"/></para>
    /// </remarks>
    public struct SaveGameInfo
    {
        public string ShortName { get; set; }
        public string FileName { get; set; }
        public SaveGameType Type { get; set; }        
        public DateTime Date { get; set; }

        /// <summary>
        /// Create a SaveGameInfo from a file via FileInfo object
        /// </summary>
        public SaveGameInfo(FileInfo fileInfo)
        {
            SaveGameType type = SaveGameType.Manual;
            string shortName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            if (shortName.Contains("_"))
            {
                //split nicename
                string[] splitName = shortName.Split('_');
                if (splitName.Length >= 2)
                {
                    shortName = shortName.Substring(shortName.IndexOf('_') + 1);
                    if (splitName[0] == "q")
                    {
                        shortName = $"{Sub.Replace("Quicksave", "IGUI_SAVE")}";
                        type = SaveGameType.Quick;
                    }
                    else if (splitName[0] == "a")
                    {
                        shortName = $"{Sub.Replace("Autosave", "IGUI_SAVE")} {(splitName.Length > 2 ? splitName[2] : "?")}";
                        type = SaveGameType.Auto;
                    }
                    else if (splitName[0] == "m")
                        type = SaveGameType.Manual;
                    else
                        shortName = Path.GetFileNameWithoutExtension(fileInfo.Name); //undo our oopsie if it turns out someone is trolling with prefixes
                }

            }

            ShortName = shortName;
            FileName = fileInfo.Name;
            Type = type;
            Date = fileInfo.CreationTime;

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

        /// <summary>
        /// Gets the last save file, or null if it doesn't exist
        /// </summary>
        public static string GetLastSave()
        {
            string savePath = CoreParams.SavePath;
            DirectoryInfo saveDInfo = new DirectoryInfo(savePath);
            var files = saveDInfo.GetFiles();

            if (files == null || files.Length == 0)
            {
                return null;
            }

            FileInfo saveFInfo = files.OrderBy(f => f.CreationTime).Last();
            if (saveFInfo != null)
                return saveFInfo.Name;
            else
                return null;
        }

        /// <summary>
        /// Gets the clean save path given a save name which may or may not contain a path and may or may not contain an extension
        /// </summary>
        public static string GetCleanSavePath(string saveName)
        {
            string path;
            if (Path.IsPathRooted(saveName))
                path = saveName;
            else
                path = Path.Combine(CoreParams.SavePath, saveName);

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
                path = path + ".json";

            return path;
        }

        /// <summary>
        /// Creates default metadata with specified nicename, drawing other info from game state and thumbnail generation script
        /// </summary>
        public static SaveMetadata CreateDefaultMetadata(string niceName)
        {
            var sm = new SaveMetadata()
            {
                NiceName = niceName,
                CampaignIdentifier = GameState.Instance.CampaignIdentifier,
                LocationRaw = SceneManager.GetActiveScene().name,
                CompanyName = CoreParams.CompanyName,
                GameName = CoreParams.GameName,
                GameVersion = CoreParams.GetCurrentVersion(),
                CampaignStartDate = GameState.Instance.CampaignStartDate,
                LastSaveDate = DateTime.Now
            };

            sm.Location = Sub.Exists(sm.LocationRaw, "LOCATION") ? Sub.Replace(sm.LocationRaw, "LOCATION") : null;

            //very loosely coupled thumbnail
            try
            {
                if (ScriptingModule.CheckScript("Save.GetSaveThumbnail"))
                    sm.ThumbnailImage = (byte[])ScriptingModule.CallForResult("Save.GetSaveThumbnail", new ScriptExecutionContext());
            }
            catch(Exception e)
            {
                Debug.LogError($"An error occurred when trying to get the save thumbnail ({e.GetType().Name}: {e.Message})");
                if(ConfigState.Instance.UseVerboseLogging)                
                    Debug.LogException(e);
                
            }

            return sm;
        }

        /// <summary>
        /// Loads save metadata from a save file
        /// </summary>
        public static SaveMetadata LoadSaveMetadata(string saveName)
        {
            string path = SaveUtils.GetCleanSavePath(saveName);

            var jo = (JObject)CoreUtils.ReadExternalJson(path);

            if(!jo["Metadata"].IsNullOrEmpty())
            {
                return CoreUtils.InterpretJson<SaveMetadata>(jo["Metadata"]);
            }

            return null;
        }

        //all non-Ex should be "safe" (log errors instead of throwing) and check conditions themselves

        /// <summary>
        /// Creates a quicksave, if allowed to do so, displaying an indicator and suppressing exceptions
        /// </summary>
        /// <remarks>Defaults to commit=true</remarks>
        public static void DoQuickSave()
        {
            DoQuickSave(true);
        }

        /// <summary>
        /// Creates a quicksave, if allowed to do so, displaying an indicator and suppressing exceptions
        /// </summary>
        public static void DoQuickSave(bool commit)
        {
            try
            {
                DoQuickSaveEx(commit, false);
                ShowSaveIndicator(SaveStatus.Success);
            }
            catch (Exception e)
            {
                if (e is SaveNotAllowedException)
                {
                    Debug.Log("Quicksave failed: save not allowed");
                    ShowSaveIndicator(SaveStatus.Blocked);
                }
                else
                {
                    Debug.LogError($"An error occurred during the quicksave process ({e.GetType().Name})");
                    Debug.LogException(e);
                    ShowSaveIndicator(SaveStatus.Error);
                }
            }
        }

        /// <summary>
        /// Creates a quicksave
        /// </summary>
        /// <remarks>Note that this does not display the indicator and can throw exceptions</remarks>
        public static void DoQuickSaveEx(bool commit, bool force)
        {

            if (!CoreParams.AllowSaveLoad)
                throw new NotSupportedException();

            if(!force && (GameState.Instance.SaveLocked || GameState.Instance.ManualSaveLocked || !CoreParams.AllowManualSave))
                    throw new SaveNotAllowedException();

            //quicksave format will be q_<hash>
            //since we aren't supporting campaign-unique autosaves yet, hash will just be 0

            string campaignId = "0";
            string saveFileName = $"q_{campaignId}";
            //string saveFilePath = Path.Combine(CoreParams.SavePath, saveFileName);

            SharedUtils.SaveGame(saveFileName, true, false, CreateDefaultMetadata("Quicksave"));

            Debug.Log($"Quicksave complete ({saveFileName})");

            //Debug.LogWarning("Quicksave!");

        }

        /// <summary>
        /// Loads the quicksave if it exists
        /// </summary>
        public static void DoQuickLoad()
        {
            try
            {
                if (!CoreParams.AllowSaveLoad)
                    return;

                //quicksave format will be q_<hash>
                //since we aren't supporting campaign-unique autosaves yet, hash will just be 0

                string campaignId = "0";
                string saveFileName = $"q_{campaignId}.json";
                string saveFilePath = Path.Combine(CoreParams.SavePath, saveFileName);

                if(File.Exists(saveFilePath))
                {
                    if(CoreParams.EnforceQuicksaveVersionMatching)
                    {
                        var metadata = SaveUtils.LoadSaveMetadata(saveFilePath);
                        if(metadata == null || metadata.GameVersion.GameVersion > CoreParams.GameVersion)
                        {
                            Debug.LogWarning("Quickload failed (version mismatch)");
                            ShowSaveIndicator(SaveStatus.LoadError);
                            return;
                        }                       
                    }

                    SharedUtils.LoadGame(saveFileName, false);
                }
                else
                {
                    Debug.Log("Quickload failed (doesn't exist)");
                    ShowSaveIndicator(SaveStatus.LoadMissing);
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"Quickload failed! ({e.GetType().Name})");
                Debug.LogException(e);
                ShowSaveIndicator(SaveStatus.LoadError);
            }
        }

        /// <summary>
        /// Loads the quicksave if it exists
        /// </summary>
        /// <remarks>Note that this does not display the indicator and can throw exceptions</remarks>
        public static void DoQuickLoadEx()
        {

            if (!CoreParams.AllowSaveLoad)
                throw new NotSupportedException();

            //quicksave format will be q_<hash>
            //since we aren't supporting campaign-unique autosaves yet, hash will just be 0

            string campaignId = "0";
            string saveFileName = $"q_{campaignId}.json";
            string saveFilePath = Path.Combine(CoreParams.SavePath, saveFileName);

            if (File.Exists(saveFilePath))
            {
                SharedUtils.LoadGame(saveFileName, false);
            }
            else
            {
                throw new FileNotFoundException("Quickload file could not be found");
            }

        }

        /// <summary>
        /// Creates an autosave, if allowed to do so, displaying an indicator and suppressing exceptions
        /// </summary>
        /// <remarks>Defaults to commit=false</remarks>
        public static void DoAutoSave()
        {
            DoAutoSave(false);
        }

        /// <summary>
        /// Creates an autosave, if allowed to do so, displaying an indicator and suppressing exceptions
        /// </summary>
        public static void DoAutoSave(bool commit)
        {
            try
            {
                DoAutoSaveEx(commit, false);

                ShowSaveIndicator(SaveStatus.Success);

            }
            catch (Exception e)
            {
                if (e is SaveNotAllowedException)
                {
                    Debug.Log("Autosave failed: save not allowed");
                    ShowSaveIndicator(SaveStatus.Blocked);
                }
                else
                {
                    Debug.LogError($"An error occurred during the autosave process ({e.GetType().Name})");
                    Debug.LogException(e);
                    ShowSaveIndicator(SaveStatus.Error);
                }
            }
        }

        /// <summary>
        /// Creates an autosave
        /// </summary>
        /// <remarks>Note that this does not display the indicator and can throw exceptions</remarks>
        public static void DoAutoSaveEx(bool commit, bool force)
        {
            if (!CoreParams.AllowSaveLoad)
            {
                if (force) //you are not allowed to force a save if it's globally disabled; the assumption is that if it's globally disabled, it won't work at all
                    throw new NotSupportedException("Save/Load is disabled in core params!");

                throw new SaveNotAllowedException();
            }

            if (GameState.Instance.SaveLocked && !force)
                throw new SaveNotAllowedException(); //don't autosave if we're not allowed to

            if (ConfigState.Instance.AutosaveCount <= 0 && !force)
                throw new SaveNotAllowedException(); //don't autosave if we've disabled it

            //autosave format will be a_<hash>_<index>
            //since we aren't supporting campaign-unique autosaves, yet, hash will just be 0
            string dummyCampaignId = "0";
            string filterString = $"a_{dummyCampaignId}_";

            string savePath = CoreParams.SavePath;
            DirectoryInfo saveDInfo = new DirectoryInfo(savePath);
            FileInfo[] savesFInfo = saveDInfo.GetFiles().Where(f => f.Name.StartsWith(filterString)).OrderBy(f => f.Name).Reverse().ToArray();

            //Debug.Log(savesFInfo.Select(f => f.Name).ToNiceString());

            List<int> knownSaveIds = new List<int>();
            int highestSaveId = 1;
            foreach(var saveFI in savesFInfo)
            {
                try
                {
                    var nameParts = Path.GetFileNameWithoutExtension(saveFI.Name).Split('_');
                    int saveId = int.Parse(nameParts[2]);
                    knownSaveIds.Add(saveId);
                    if (saveId > highestSaveId)
                        highestSaveId = saveId;
                }
                catch(Exception)
                {
                    Debug.LogWarning($"Found an invalid save file: {saveFI.Name}");
                }
            }

            //save this autosave
            string newSaveName = $"a_{dummyCampaignId}_{highestSaveId + 1}";
            var metadata = CreateDefaultMetadata($"Autosave {highestSaveId + 1}");
            SharedUtils.SaveGame(newSaveName, commit, false, metadata);

            //remove old autosaves
            string currentCampaignId = metadata.CampaignIdentifier;
            HashSet<string> keptCampaignIds = new HashSet<string>();
            int keptSavesForCurrentCampaign = 0;
            foreach(FileInfo saveF in savesFInfo)
            {
                try
                {
                    if (CoreParams.UseCampaignIdentifier && CoreParams.KeepAutosaveForEachCampaign)
                    {
                        //read metadata, if it's not this campaign id then do a check against keptcampaignids and continue the loop
                        var jo = (JObject)CoreUtils.ReadExternalJson(saveF.FullName);

                        if (!jo["Metadata"].IsNullOrEmpty())
                        {
                            var saveMetadata = CoreUtils.InterpretJson<SaveMetadata>(jo["Metadata"]);
                            if (saveMetadata.CampaignIdentifier != currentCampaignId)
                            {
                                if(!string.IsNullOrEmpty(saveMetadata.CampaignIdentifier))
                                {
                                    if(keptCampaignIds.Contains(saveMetadata.CampaignIdentifier))
                                    {
                                        File.Delete(saveF.FullName);
                                    }
                                    else
                                    {
                                        keptCampaignIds.Add(saveMetadata.CampaignIdentifier);
                                    }
                                }
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //keep n saves for current campaign
                    if (keptSavesForCurrentCampaign >= ConfigState.Instance.AutosaveCount)
                    {
                        File.Delete(saveF.FullName);
                    }
                    else
                    {
                        keptSavesForCurrentCampaign++;
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error handling save file: {saveF.FullName} ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }

            Debug.Log($"Autosave complete ({newSaveName})");

        }

        /// <summary>
        /// Creates a full finalsave, displaying an indicator and suppressing exceptions
        /// </summary>
        public static void DoFinalSave()
        {
            try
            {
                DoFinalSaveEx();
                ShowSaveIndicator(SaveStatus.Success);
            }
            catch (Exception e)
            {

                Debug.LogError($"An error occurred during the finalsave process ({e.GetType().Name})");
                Debug.LogException(e);
                ShowSaveIndicator(SaveStatus.Error);
                
            }
        }

        /// <summary>
        /// Creates a full finalsave
        /// </summary>
        /// <remarks>
        /// <para>Ignores all restrictions on saving; finalsaves are special</para>
        /// <para>Does not commit scene state before saving</para>
        /// </remarks>
        public static void DoFinalSaveEx()
        {
            string finalSaveName = $"finalsave_{DateTime.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture)}.json";
            string savePath = CoreParams.FinalSavePath + Path.DirectorySeparatorChar + finalSaveName;
            DateTime savePoint = DateTime.Now;
            var jobject = JObject.FromObject(GameState.Instance, JsonSerializer.CreateDefault(CoreParams.DefaultJsonSerializerSettings));
            jobject.Add("FinalSaveDate", JToken.FromObject(savePoint));
            jobject.Add("FinalSaveIdentifier", Guid.NewGuid().ToString("N"));
            File.WriteAllText(savePath, JsonConvert.SerializeObject(jobject, Formatting.Indented, CoreParams.DefaultJsonSerializerSettings));
            File.SetCreationTime(savePath, savePoint);
            Debug.Log($"Finalsave complete ({finalSaveName})");
        }

        private enum SaveStatus
        {
            Undefined, Success, Error, Blocked, LoadError, LoadMissing
        }

        private static void ShowSaveIndicator(SaveStatus status)
        {
            string path = $"UI/SaveIndicators/SaveIndicator{status.ToString()}";
            var prefab = CoreUtils.LoadResource<GameObject>(path);
            if(prefab != null)
            {
                UnityEngine.Object.Instantiate(prefab);
            }
            else
            {
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogWarning($"{nameof(ShowSaveIndicator)} couldn't find prefab for \"{path}\"");
            }
        }

    }

}