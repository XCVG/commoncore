﻿using CommonCore;
using CommonCore.Async;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.DebugLog;
using CommonCore.LockPause;
using CommonCore.RpgGame;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.UI;
using PseudoExtensibleEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Temporary "test" console commands for trying out various things
/// </summary>
public static class TestConsoleCommands
{
    [Command]
    public static void CCTestCommand()
    {
        Debug.Log("hello world");
    }

    [Command]
    public static void CCTestArgCommand(string mystring)
    {
        Debug.Log(mystring);
    }

    [Command]
    public static void DumpQualitySettings()
    {
        Dictionary<string, Dictionary<string, object>> allQSettings = new Dictionary<string, Dictionary<string, object>>();

        for(int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, true);
            Dictionary<string, object> qualitySettings = new Dictionary<string, object>();
            var props = typeof(QualitySettings).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach(var prop in props)
            {
                qualitySettings.Add(prop.Name, prop.GetValue(null));
            }
            allQSettings.Add(i.ToString(), qualitySettings);
        }

        DebugUtils.JsonWrite(allQSettings, "qualitysettings");
    }

    [Command]
    public static void DumpPxEnumMappings()
    {
        var pxEnumContext = PxEnum.CurrentContext;
        var mappings = (Dictionary<Type, List<Type>>)pxEnumContext.GetType().GetField("PseudoExtensionMap", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pxEnumContext);
        var mappedMappings = mappings.Select(m => new KeyValuePair<string, string[]>(m.Key.Name, m.Value.Select(v => v.Name).ToArray())).ToList();
        DebugUtils.JsonWrite(mappedMappings, "pxenummappings");
    }

    [Command]
    public static void DumpRpgItemModels()
    {
        var models = InventoryModel.EnumerateModels();
        DebugUtils.JsonWrite(models, "itemmodels");
    }

    [Command]
    public static void DumpCurrentDifficulty()
    {
        var diff = ConfigState.Instance.GetGameplayConfig().Difficulty;
        DebugUtils.JsonWrite(diff, "difficulty");
    }

    [Command]
    public static void TestWeakReferencePauseLock()
    {
        AsyncUtils.RunWithExceptionHandling(async () =>
        {
            object lockObject = new object();
            WeakReference untypedWeakReference = new WeakReference(lockObject);
            LockPauseModule.PauseGame(PauseLockType.AllowMenu, untypedWeakReference);
            Debug.Log("Untyped lock added!");

            await Task.Delay(3000);
            AsyncUtils.ThrowIfStopped();

            lockObject = null;
            CoreUtils.CollectGarbage(false);

            Debug.Log("Untyped lock released!");

            await Task.Yield();
            AsyncUtils.ThrowIfStopped();

            LockPauseModule.ForceCleanLocks();

            Debug.Log("Locks cleaned!");

            await Task.Delay(5000);
            AsyncUtils.ThrowIfStopped();

            IEnumerable typedLockObject = new string[] { "lol", "hi" };
            WeakReference<IEnumerable> typedWeakReference = new WeakReference<IEnumerable>(typedLockObject);

            LockPauseModule.PauseGame(PauseLockType.AllowMenu, typedWeakReference);
            Debug.Log("Typed lock added!");

            await Task.Delay(3000);
            AsyncUtils.ThrowIfStopped();

            typedLockObject = null;
            CoreUtils.CollectGarbage(false);

            Debug.Log("Typed lock released!");

            await Task.Yield();
            AsyncUtils.ThrowIfStopped();

            LockPauseModule.ForceCleanLocks();

            Debug.Log("Locks cleaned!");
        });
    }

    [Command]
    public static void TestPlayMusic()
    {
        var audioPlayer = CCBase.GetModule<AudioModule>().AudioPlayer;
        audioPlayer.PlayMusic("menu", MusicSlot.Override, 1.0f, true, false);
    }

    [Command]
    public static void TestTextEntryModal()
    {
        AsyncUtils.RunWithExceptionHandling(async () =>
        {
            var result = await Modal.PushTextEntryModalAsync(new TextEntryModalData() {
                AllowCancel = true,
                Heading = "text entry modal", 
                Description = "Lorem ipsum dolor sit amet. \n\nThe quick brown fox jumps over the lazy dog",
                InitialText = "default text",
                Placeholder = "or a placeholder"
            }, false, null);
            if(result.Status == ModalStatusCode.Complete)
            {
                await Modal.PushMessageModalAsync(result.Result, "Completed", false, null);
            }
            else
            {
                await Modal.PushMessageModalAsync("", "Cancelled", false, null);
            }
        });
    }

    [Command]
    public static void TestJsFunction()
    {
        JSCrossCall.CallJSFunction("console.log", "Hello from C#!");
    }

    [Command]
    public static void TestJsEvent()
    {
        JSCrossCall.TriggerCanvasEvent("test", new Dictionary<string, object>() { { "testData", "dogcow" } });
    }

}
