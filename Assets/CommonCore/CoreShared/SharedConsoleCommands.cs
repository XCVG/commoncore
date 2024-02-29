using CommonCore;
using CommonCore.Console;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Basic console commands
/// </summary>
public static class SharedConsoleCommands
{
    //***** UTILITIES 
    [Command]
    static void PrintDataPath() //TODO move elsewhere
    {
        ConsoleModule.WriteLine(CoreParams.PersistentDataPath);
    }

    [Command]
    static void PrintScenePathList()
    {
        try
        {
            var sceneNames = CoreUtils.GetSceneList();
            var sceneRemaps = new Dictionary<string, string>(CoreUtils.EnumerateSceneOverrides());
            StringBuilder sb = new StringBuilder(sceneNames.Length * 16);
            foreach (var s in sceneNames)
            {
                if (sceneRemaps.TryGetValue(Path.GetFileNameWithoutExtension(s), out string remappedName))
                    sb.AppendLine($"{s} -> {remappedName}");
                else
                    sb.AppendLine(s);
            }
            ConsoleModule.WriteLine(sb.ToString());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }

    [Command]
    static void PrintSceneList()
    {
        try
        {
            var sceneNames = CoreUtils.GetSceneList();
            var sceneRemaps = new Dictionary<string, string>(CoreUtils.EnumerateSceneOverrides());
            StringBuilder sb = new StringBuilder(sceneNames.Length * 16);
            foreach (var s in sceneNames)
            {
                if (sceneRemaps.TryGetValue(Path.GetFileNameWithoutExtension(s), out string remappedName))
                    sb.AppendLine($"{Path.GetFileNameWithoutExtension(s)} -> {remappedName}");
                else
                    sb.AppendLine(Path.GetFileNameWithoutExtension(s));
            }
            ConsoleModule.WriteLine(sb.ToString());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }

    [Command]
    static void PrintCoreParams()
    {
        Dictionary<string, object> coreParams = new Dictionary<string, object>();
        var props = typeof(CoreParams).GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var prop in props)
            coreParams.Add(prop.Name, prop.GetValue(null));

        ConsoleModule.WriteLine(DebugUtils.JsonStringify(coreParams));
    }

    //***** LOAD/SAVE

    /// <summary>
    /// Loads a game from a file, loading and restoring the current scene from the save
    /// </summary>
    [Command]
    static void Load(string name)
    {
        SharedUtils.LoadGame(name, true);
    }

    /// <summary>
    /// Restores the saved game scene state to this scene
    /// </summary>
    [Command]
    static void Restore()
    {
        MetaState.Instance.TransitionType = SceneTransitionType.LoadGame; //?
        BaseSceneController bsc = SharedUtils.GetSceneController();
        bsc.Restore();
    }

    /// <summary>
    /// Commits this scene's state to game state
    /// </summary>
    [Command]
    static void Commit()
    {
        BaseSceneController bsc = SharedUtils.GetSceneController();
        bsc.Commit();
    }

    /// <summary>
    /// Commits this scene's state, then saves game state to file
    /// </summary>
    [Command]
    static void Save(string name)
    {
        SharedUtils.SaveGame(name, true, true, SaveUtils.CreateDefaultMetadata(name));
    }

    /// <summary>
    /// Creates a final save
    /// </summary>
    [Command]
    static void FinalSave()
    {
        SaveUtils.DoFinalSave();
    }

    //***** WARP

    /// <summary>
    /// Changes scenes
    /// </summary>
    [Command]
    static void Warp(string scene)
    {
        SharedUtils.ChangeScene(scene);
    }

    /// <summary>
    /// Changes scenes directly (unsafe)
    /// </summary>
    [Command]
    static void WarpDirect(string scene)
    {
        CoreUtils.LoadScene(scene);
    }

    /// <summary>
    /// Starts a new game (intended to be used from menu)
    /// </summary>
    [Command]
    static void StartGame()
    {
        SharedUtils.StartGame();
    }

    /// <summary>
    /// Starts a new game with scene override (intended to be used from menu)
    /// </summary>
    [Command]
    static void StartGame(string scene)
    {
        SharedUtils.StartGame(scene);
    }

    /// <summary>
    /// Reloads the current scene
    /// </summary>
    [Command]
    static void Reload()
    {
        if (GameState.Exists)
            SharedUtils.ChangeScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Ends the current game
    /// </summary>
    [Command]
    static void EndGame()
    {
        SharedUtils.EndGame();
    }

    /// <summary>
    /// Shows game over screen
    /// </summary>
    [Command]
    static void ShowGameOver()
    {
        SharedUtils.ShowGameOver();
    }

    /// <summary>
    /// Shows default main menu
    /// </summary>
    [Command]
    public static void ShowMainMenu()
    {
        var mainMenuController = CoreUtils.GetUIRoot().GetComponentInChildren<MainMenuController>();
        if (mainMenuController != null)
        {
            MetaState.Instance.SessionFlags.Add("ShowMainMenu");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            ConsoleModule.WriteLine($"{nameof(ShowMainMenu)} must be invoked from the main menu screen. Use {nameof(EndGame)} or {nameof(WarpDirect)} {CoreParams.MainMenuScene} to go to the main menu first.");
        }
    }

    //***** SCREENFADE

    /// <summary>
    /// Clears any screen fade
    /// </summary>
    [Command(alias = "Clear", className = "ScreenFader", useClassName = true)]
    static void ClearScreenFade()
    {
        ScreenFader.ClearFade();
    }
}
