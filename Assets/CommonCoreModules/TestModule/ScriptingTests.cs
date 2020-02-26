using CommonCore.Scripting;
using System;
using UnityEngine;

namespace CommonCore.TestModule
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

        [CCScript(ClassName = "Test", Name = "SingleArgNoContext")]
        private static void SingleArgNoContextTest(string arg0)
        {
            Debug.Log(arg0);
        }

        [CCScript(ClassName = "Test", Name = "SingleArgCastableContext", NeverPassExecutionContext = true)]
        private static void SingleArgCastableContextTest(object arg0)
        {
            Debug.Log(arg0);
        }

        [CCScript(ClassName = "Test", Name = "TwoArg")]
        private static void TwoArgTest(ScriptExecutionContext context, string arg0, string arg1)
        {
            Debug.Log($"{arg0} {arg1}");
        }

        [CCScript(ClassName = "Test", Name = "TypedArg")]
        private static void TypedArgTest(ScriptExecutionContext context, int arg0)
        {
            Debug.Log($"Argument value: {arg0} ({arg0.GetType().Name})");
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

        [CCScript, CCScriptHook(Hook = ScriptHook.OnGameLoad)]
        private static void OnGameLoad(ScriptExecutionContext context)
        {
            Debug.Log($"OnGameLoad\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.OnSceneTransition)]
        private static void OnSceneTransition(ScriptExecutionContext context)
        {
            Debug.Log($"OnSceneTransition\n{context}");
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

        [CCScript, CCScriptHook(Hook = ScriptHook.OnPlayerSpawn)]
        private static void OnPlayerSpawn(ScriptExecutionContext context, GameObject player)
        {
            Debug.Log($"OnPlayerSpawn\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.AfterMainMenuCreate)]
        private static void AfterMainMenuCreate(ScriptExecutionContext context)
        {
            Debug.Log($"AfterMainMenuCreate\n{context}");
        }

        [CCScript, CCScriptHook(Hook = ScriptHook.AfterIGUIMenuCreate)]
        private static void AfterIGUIMenuCreate(ScriptExecutionContext context)
        {
            Debug.Log($"AfterIGUIMenuCreate\n{context}");
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