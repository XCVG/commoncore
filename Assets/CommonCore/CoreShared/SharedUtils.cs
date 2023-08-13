using CommonCore.Migrations;
using CommonCore.Scripting;
using CommonCore.State;
using CommonCore.UI;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore
{
    /// <summary>
    /// Functions for manipulating general game state
    /// </summary>
    public static class SharedUtils
    {

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to default start scene
        /// </summary>
        public static void StartGame()
        {
            StartGame(null);
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to specified scene
        /// </summary>
        public static void StartGame(string sceneOverride)
        {            
            MetaState.Instance.Clear();
            MetaState.Instance.NextScene = sceneOverride;
            MetaState.Instance.TransitionType = SceneTransitionType.NewGame;
            //GC.Collect();            
            CoreUtils.LoadScene(CoreParams.LoadingScene);            
        }

        /// <summary>
        /// Transitions to the GameOverScene, does not clear game data
        /// </summary>
        public static void ShowGameOver()
        {
            MetaState.Instance.NextScene = SceneManager.GetActiveScene().name; //in case we need it...
            CoreUtils.LoadScene(CoreParams.GameOverScene);
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to main menu scene
        /// </summary>
        public static void EndGame()
        {
            EndGame(null);
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to specified scene
        /// </summary>
        public static void EndGame(string sceneOverride)
        {
            //MetaState.Instance.Clear();
            MetaState.Instance.NextScene = sceneOverride;
            MetaState.Instance.TransitionType = SceneTransitionType.EndGame;
            //GC.Collect();
            TryGetSceneController().Ref()?.ExitScene();
            CoreUtils.LoadScene(CoreParams.LoadingScene);
        }

        /// <summary>
        /// Changes to a new scene, setting up state and calling transitions appropriately
        /// </summary>
        public static void ChangeScene(string scene)
        {
            ChangeScene(scene, false);
        }

        /// <summary>
        /// Changes to a new scene, setting up state and calling transitions appropriately
        /// </summary>
        public static void ChangeScene(string scene, bool skipLoading)
        {
            MetaState mgs = MetaState.Instance;
            mgs.PreviousScene = SceneManager.GetActiveScene().name;
            mgs.NextScene = scene;
            mgs.LoadSave = null;
            mgs.TransitionType = SceneTransitionType.ChangeScene;
            mgs.SkipLoadingScreen = skipLoading;

            TryGetSceneController().Ref()?.ExitScene(); //run scene exit routine if a scene controller exists

            SceneManager.LoadScene(CoreParams.LoadingScene); //TODO put loading scene name somewhere            
            
        }

        /// <summary>
        /// Loads a saved game and transitions to its scene
        /// </summary>
        /// <param name="saveName">The name of the save file</param>
        /// <remarks>This begins the loading process, but does not handle the actual loading of data</remarks>
        public static void LoadGame(string saveName, bool force)
        {
            if (!force && !CoreParams.AllowSaveLoad)
                throw new NotSupportedException("Save/Load is disabled in core params!");

            MetaState.Instance.Clear();
            MetaState mgs = MetaState.Instance;
            mgs.LoadSave = saveName;
            mgs.TransitionType = SceneTransitionType.LoadGame;

            SceneManager.LoadScene(CoreParams.LoadingScene);
        }

        /// <summary>
        /// Loads a save game to stte
        /// </summary>
        /// <param name="saveName">The save name, which may be an absolute or relative path, and may contain an extension</param>
        /// <remarks>This handles the actual loading of data</remarks>
        public static void LoadGameStateFromFile(string saveName)
        {
            string path = SaveUtils.GetCleanSavePath(saveName);

            var raw = (JObject)CoreUtils.ReadExternalJson(path);

            var newRaw = MigrationsManager.Instance.MigrateToLatest<GameState>(raw, true, out bool didMigrate);
            if (didMigrate)
            {
                Debug.Log($"Save file \"{path}\" was migrated successfully");
                if(CoreParams.UseSaveMigrationBackups || CoreParams.IsDebug)
                {
                    Directory.CreateDirectory(System.IO.Path.Combine(CoreParams.PersistentDataPath, "migrationbackups"));
                    File.Copy(path, Path.Combine(CoreParams.PersistentDataPath, "migrationbackups", $"{Path.GetFileNameWithoutExtension(path)}.migrated.{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}.json"), true);
                }
                CoreUtils.WriteExternalJson(path, raw);
            }

            ScriptingModule.CallHooked(ScriptHook.AfterSaveRead, null, newRaw ?? raw);

            GameState.DeserializeFromJObject(newRaw ?? raw);

            ScriptingModule.CallHooked(ScriptHook.AfterSaveDeserialize, null, GameState.Instance);

            PersistState.Instance.LastCampaignIdentifier = GameState.Instance.CampaignIdentifier;
            MetaState.Instance.NextScene = GameState.Instance.CurrentScene;
        }

        /// <summary>
        /// Saves the current state to file
        /// </summary>
        /// <param name="saveName">The name of the save file, with prefix and extension but without path</param>
        /// <param name="commit">Whether to commit or not</param>
        public static void SaveGame(string saveName, bool commit, bool force, SaveMetadata metadata)
        {
            if (!force && !CoreParams.AllowSaveLoad)
                throw new NotSupportedException("Save/Load is disabled in core params!");

            string savePath = SaveUtils.GetCleanSavePath(saveName);
            if(commit)
                BaseSceneController.Current.Commit();
            DateTime savePoint = DateTime.Now;
            GameState.Instance.UpdateDifficulty();

            ScriptingModule.CallHooked(ScriptHook.BeforeSaveSerialize, null, GameState.Instance);

            var jo = GameState.SerializeToJObject();
            if(metadata != null)
                jo["Metadata"] = CoreUtils.ConstructJson(metadata); //null check?

            ScriptingModule.CallHooked(ScriptHook.BeforeSaveWrite, null, jo);

            CoreUtils.WriteExternalJson(savePath, jo);
            File.SetCreationTime(savePath, savePoint);
        }

        /// <summary>
        /// Gets the scene controller (returns null on fail)
        /// </summary>
        public static BaseSceneController TryGetSceneController()
        {
            if (BaseSceneController.Current != null)
                return BaseSceneController.Current;

            // try grabbing WorldRoot
            Transform t = CoreUtils.GetWorldRoot();
            if (t != null)
            {
                BaseSceneController bsc = t.gameObject.GetComponent<BaseSceneController>();

                if (bsc != null)
                    return bsc;
            }

            return null;
        }

        /// <summary>
        /// Gets the scene controller (throws on fail)
        /// </summary>
        public static BaseSceneController GetSceneController()
        {
            BaseSceneController bsc = TryGetSceneController();

            if (bsc != null)
                return bsc;

            //still couldn't find it, throw an error
            Debug.LogError("Couldn't find SceneController");

            throw new NullReferenceException(); //not having a scene controller is fatal
        }

        /// <summary>
        /// Gets the HUD controller (returns null on fail)
        /// </summary>
        public static BaseHUDController TryGetHudController()
        {
            if (BaseHUDController.Current != null)
                return BaseHUDController.Current;

            // try searching UIRoot
            var uiRoot = CoreUtils.GetUIRoot();
            if(uiRoot != null)
            {
                foreach(Transform t in uiRoot)
                {
                    BaseHUDController bhc = t.GetComponent<BaseHUDController>();
                    if (bhc != null)
                        return bhc;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the HUD controller (throws on fail)
        /// </summary>
        public static BaseHUDController GetHudController()
        {
            BaseHUDController bhc = TryGetHudController();

            if (bhc != null)
                return bhc;

            //still couldn't find it, throw an error
            Debug.LogError("Couldn't find HudController");

            throw new NullReferenceException(); //not having a scene controller is fatal
        }

    }
}