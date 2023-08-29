using CommonCore.Config;
using CommonCore.Scripting;
using UnityEngine;

namespace CommonCore.RpgGame.Dialogue
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

