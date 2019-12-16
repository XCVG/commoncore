using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Scripting
{
    internal class ScriptingTest
    {
        [CCScript(ClassName = "Test", Name = "HelloWorld")]
        public static void TestMethod(ScriptExecutionContext context)
        {
            Debug.Log("Hello world!");
        }

        [CCScript(ClassName = "Test", Name = "NoArgs")]
        private static void NoArgsTest()
        {
            Debug.Log("Hello world! (no args)");
        }

        [CCScript(ClassName = "Test", Name = "ContextArg")]
        public static void ContextArgTest(ScriptExecutionContext context)
        {
            Debug.Log(context);
        }

        [CCScript(ClassName = "Test", Name = "SingleArg")]
        private static void SingleArgTest(ScriptExecutionContext context, string arg0)
        {
            Debug.Log(arg0);
        }

        [CCScript(ClassName = "Test", Name = "ReturnValue")]
        private static string ReturnValueTest()
        {
            return "ReturnValueTestResult";
        }
    }

    internal class ScriptingInstanceTest
    {
        public static ScriptingInstanceTest Instance { get; set; } = new ScriptingInstanceTest();

        [CCScript(ClassName = "Test", Name = "Instance", AutoResolveInstance = true)]
        private void InstanceTest()
        {
            Debug.Log($"Hello world! (from an instance [{this.GetHashCode()}])");
        }

        [CCScript(ClassName = "Test", Name = "NonStatic", AutoResolveInstance = false)]
        private void NonStaticTest()
        {
            Debug.Log($"Hello world! (from a non-autocreated instance [{this.GetHashCode()}])");
        }
    }

    internal static class ScriptingDelegateTest
    {
        [CCScript(ClassName = "Test", Name = "RegisterDelegate")]
        private static void RegisterTestDelegate()
        {
            Func<ScriptExecutionContext, string> func = (sec) => { Debug.Log("Hello world! (from an anonymous method)"); return "TestDelegateResult"; };
            ScriptingModule.Register(func, "Test", "Delegate");
        }
    }

    internal static class ScriptingHookTest
    {
        //None, AfterModulesLoaded, BeforeApplicationExit, OnGameStart, OnGameEnd, OnSceneLoad, AfterSceneLoad, OnSceneUnload, OnMainMenuOpen, OnIGUIMenuOpen, OnFrameUpdate

        [CCScript, CCScriptHook(Hook = ScriptHook.AfterModulesLoaded)]
        private static void AfterModulesLoaded(ScriptExecutionContext context)
        {
            Debug.Log($"AfterModulesLoaded\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.BeforeApplicationExit)]
        private static void BeforeApplicationExit(ScriptExecutionContext context)
        {
            Debug.Log($"BeforeApplicationExit\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.OnGameStart)]
        private static void OnGameStart(ScriptExecutionContext context)
        {
            Debug.Log($"OnGameStart\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.OnGameEnd)]
        private static void OnGameEnd(ScriptExecutionContext context)
        {
            Debug.Log($"OnGameEnd\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.OnSceneLoad)]
        private static void OnSceneLoad(ScriptExecutionContext context)
        {
            Debug.Log($"OnSceneLoad\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.AfterSceneLoad)]
        private static void AfterSceneLoad(ScriptExecutionContext context)
        {
            Debug.Log($"AfterSceneLoad\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.OnSceneUnload)]
        private static void OnSceneUnload(ScriptExecutionContext context)
        {
            Debug.Log($"OnSceneUnload\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.OnMainMenuOpen)]
        private static void OnMainMenuOpen(ScriptExecutionContext context)
        {
            Debug.Log($"OnMainMenuOpen\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.OnIGUIMenuOpen)]
        private static void OnIGUIMenuOpen(ScriptExecutionContext context)
        {
            Debug.Log($"OnIGUIMenuOpen\n{context}");
        }

        [CCScript(ClassName = "Test", Name = "HookOrExplicit"), CCScriptHook(Hook = ScriptHook.AfterSceneLoad, AllowExplicitCalls = true)]
        private static void HookOrExplicit(ScriptExecutionContext context)
        {
            Debug.Log($"HookOrExplicit\n{context}");
        }
    }
}