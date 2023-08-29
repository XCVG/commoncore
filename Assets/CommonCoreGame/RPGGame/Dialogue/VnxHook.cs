using CommonCore;
using CommonCore.Config;
using CommonCore.RpgGame.Dialogue;
using CommonCore.Scripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vnx
{
    public static class VnxHook
    {
        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.AfterModulesLoaded)]
        private static void VnxInjectConfigPanel()
        {
            if(GameParams.DialogueUseVnx)
                ConfigModule.Instance.RegisterConfigPanel("VnxOptionsPanel", 400, (t) => GameObject.Instantiate(CoreUtils.LoadResource<GameObject>("UI/VnxOptionsPanel"), t));
        }
    }

}

