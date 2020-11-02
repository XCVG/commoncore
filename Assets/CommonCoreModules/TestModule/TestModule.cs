using CommonCore.DebugLog;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CommonCore.TestModule
{

    /*
     * CommonCore Test Module
     * Just a random dummy module to test functionality
     */
    public class TestModule : CCModule
    {

        public TestModule()
        {
            Log("Test module loaded!");
        }

        public override void Dispose()
        {
            Log("Test module unloaded!");
        }

        public override void OnSceneLoaded()
        {
            Log("Test module: scene loaded!");
        }

        public override void OnSceneUnloaded()
        {
            Log("Test module: scene unloaded!");
        }

        public override void OnGameStart()
        {
            Log("Test module: game start!");
        }

        public override void OnGameEnd()
        {
            Log("Test module: game end!");
        }

        public override void OnAllModulesLoaded()
        {
            Log("Test module: all modules loaded!");

            //DebugUtils.TextWrite(CoreUtils.GetLoadedTypes().ToNiceString(), "types");
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Assemblies: ");
            foreach(var a in data.LoadedAssemblies)
            {
                sb.AppendFormat("\t{0}\n", a.FullName);
            }
            sb.AppendLine("Resources: ");
            foreach (var r in data.LoadedResources)
            {
                sb.AppendFormat("\t{0} ({1})\n", r.Key, r.Value.GetType().GetGenericArguments()[0]);
            }
            Log("Test module: addon loaded\n" + sb.ToString());
        }

        public override void OnAllAddonsLoaded()
        {
            Log("Test module: all addons loaded!");

            //DebugUtils.TextWrite(CoreUtils.GetLoadedTypes().ToNiceString(), "types");
        }

    }
}