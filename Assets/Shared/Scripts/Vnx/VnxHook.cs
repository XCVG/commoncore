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
            ConfigModule.Instance.RegisterConfigPanel("VnxOptionsPanel", 400, (t) => GameObject.Instantiate(CoreUtils.LoadResource<GameObject>("UI/VnxOptionsPanel"), t));
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, NamedHook = "DialogueOnOpen")]
        private static void VnxDialogueOnOpen(ScriptExecutionContext context)
        {
            VnxController.Instance.OnDialogueOpen(context.Caller as DialogueController);
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, NamedHook = "DialogueOnClose")]
        private static void VnxDialogueOnClose(ScriptExecutionContext context)
        {
            VnxController.Instance.OnDialogueClose();
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, NamedHook = "DialogueOnPresent")]
        private static void VnxDialogueOnPresent(ScriptExecutionContext context, Frame currentFrame)
        {
            VnxController.Instance.OnDialoguePresent(currentFrame);
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, NamedHook = "DialogueOnAdvance")]
        private static void VnxDialogueOnAdvance(ScriptExecutionContext context, KeyValuePair<string, string> nextLocation)
        {
            VnxController.Instance.OnDialogueAdvance(nextLocation);
        }
    }

}

