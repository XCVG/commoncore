using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CommonCore.Console;

namespace CommonCore.Scripting
{

    public static class ScriptingConsoleIntegration
    {
        [Command(alias = "Call", className = "Scripting")]
        private static void Call(string script)
        {
            try
            {
                ScriptingModule.Call(script, new ScriptExecutionContext { Activator = null, Caller = null });
            }
            catch(Exception e)
            {
                ConsoleModule.WriteLine(string.Format("Error in script {0}\n{1}\n{2}", script, e.ToString(), e.StackTrace));
            }
        }

        [Command(alias = "Call", className = "Scripting")]
        private static void Call(string script, string arg0)
        {
            try
            {
                ScriptingModule.Call(script, new ScriptExecutionContext { Activator = null, Caller = null }, arg0);
            }
            catch (Exception e)
            {
                ConsoleModule.WriteLine(string.Format("Error in script {0}\n{1}\n{2}", script, e.ToString(), e.StackTrace));
            }
        }

        [Command(alias = "Call", className = "Scripting")]
        private static void Call(string script, string arg0, string arg1)
        {
            try
            {
                ScriptingModule.Call(script, new ScriptExecutionContext { Activator = null, Caller = null }, arg0, arg1);
            }
            catch (Exception e)
            {
                ConsoleModule.WriteLine(string.Format("Error in script {0}\n{1}\n{2}", script, e.ToString(), e.StackTrace));
            }
        }

        [Command(alias = "Call", className = "Scripting")]
        private static void Call(string script, string arg0, string arg1, string arg2)
        {
            try
            {
                ScriptingModule.Call(script, new ScriptExecutionContext { Activator = null, Caller = null }, arg0, arg1, arg2);
            }
            catch (Exception e)
            {
                ConsoleModule.WriteLine(string.Format("Error in script {0}\n{1}\n{2}", script, e.ToString(), e.StackTrace));
            }
        }

        [Command(alias = "CallForResult", className = "Scripting")]
        private static void CallForResult(string script)
        {
            try
            {
                var result = ScriptingModule.CallForResult(script, new ScriptExecutionContext { Activator = null, Caller = null });
                ConsoleModule.WriteLine($"Returned '{result?.ToString() ?? "null"}' [{result?.GetType().Name}]");
            }
            catch (Exception e)
            {
                ConsoleModule.WriteLine(string.Format("Error in script {0}\n{1}\n{2}", script, e.ToString(), e.StackTrace));
            }
        }

        [Command(alias = "CallForResult", className = "Scripting")]
        private static void CallForResult(string script, string arg0)
        {
            try
            {
                var result = ScriptingModule.CallForResult(script, new ScriptExecutionContext { Activator = null, Caller = null }, arg0);
                ConsoleModule.WriteLine($"Returned '{result?.ToString() ?? "null"}' [{result?.GetType().Name}]");
            }
            catch (Exception e)
            {
                ConsoleModule.WriteLine(string.Format("Error in script {0}\n{1}\n{2}", script, e.ToString(), e.StackTrace));
            }
        }

        [Command(alias = "CallForResult", className = "Scripting")]
        private static void CallForResult(string script, string arg0, string arg1)
        {
            try
            {
                var result = ScriptingModule.CallForResult(script, new ScriptExecutionContext { Activator = null, Caller = null }, arg0, arg1);
                ConsoleModule.WriteLine($"Returned '{result?.ToString() ?? "null"}' [{result?.GetType().Name}]");
            }
            catch (Exception e)
            {
                ConsoleModule.WriteLine(string.Format("Error in script {0}\n{1}\n{2}", script, e.ToString(), e.StackTrace));
            }
        }

        [Command(alias = "CallForResult", className = "Scripting")]
        private static void CallForResult(string script, string arg0, string arg1, string arg2)
        {
            try
            {
                var result = ScriptingModule.CallForResult(script, new ScriptExecutionContext { Activator = null, Caller = null }, arg0, arg1, arg2);
                ConsoleModule.WriteLine($"Returned '{result?.ToString() ?? "null"}' [{result?.GetType().Name}]");
            }
            catch (Exception e)
            {
                ConsoleModule.WriteLine(string.Format("Error in script {0}\n{1}\n{2}", script, e.ToString(), e.StackTrace));
            }
        }

        [Command(alias = "CallHooked", className = "Scripting")]
        private static void CallHooked(string hook)
        {
            if (Enum.TryParse<ScriptHook>(hook, out ScriptHook parsedHook))
                ScriptingModule.CallHooked(parsedHook, null, new object[] { });
            else
                ConsoleModule.WriteLine($"Failed to call hooked scripts because \"{hook}\" is not a valid built-in hook (were you looking for CallNamedHooked instead?)");
        }

        [Command(alias = "CallNamedHooked", className = "Scripting")]
        private static void CallNamedHooked(string hook)
        {
            ScriptingModule.CallNamedHooked(hook, null, new object[] { });
        }

        [Command(alias = "ListAll", className = "Scripting")]
        private static void ListAll()
        {
            
            var scripts = ScriptingModule.GetCallableMethods();
            StringBuilder sb = new StringBuilder(scripts.Count * 64);
            foreach (var s in scripts)
            {
                sb.AppendLine(string.Format("{0} : {1}", s.Key, s.Value.ToString()));
            }
            ConsoleModule.WriteLine(sb.ToString());
        }

    }
}