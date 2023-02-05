﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;

namespace CommonCore.ObjectActions
{

    public class ScriptExecuteSpecial : ActionSpecial
    {
        public string Script;
        public string[] ScriptArguments;
        public bool AutoconvertArguments;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            object[] args = ScriptArguments;
            if(AutoconvertArguments)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = TypeUtils.StringToNumericAuto((string)args[i]);
                }
            }

            ScriptingModule.Call(Script, new ScriptExecutionContext { Caller = this, Activator = data.Activator.Ref()?.gameObject }, args);

            if (!Repeatable)
                Locked = true;
        }

    }
}