using UnityEngine;
using System.Collections;
using System;
using CommonCore.Config;

namespace CommonCore.ObjectActions
{
    public class ActionSpecialSplitter : ActionSpecial
    {
        public bool ContinueOnError = true;
        public ActionSpecialEvent[] Specials;

        private bool Locked;
        
        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            foreach (ActionSpecialEvent sp in Specials)
            {
                try
                {
                    sp.Invoke(data);
                }
                catch(Exception e)
                {
                    Debug.LogError($"[{nameof(ActionSpecialSplitter)}] Failed to invoke ActionSpecial ({e.GetType().Name}: {e.Message})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);

                    if (!ContinueOnError)
                        throw;
                }
                
            }

            if (!Repeatable)
                Locked = true;
           
        }

    }
}