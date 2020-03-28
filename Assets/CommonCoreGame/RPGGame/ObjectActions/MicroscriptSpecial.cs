using CommonCore.ObjectActions;
using CommonCore.RpgGame.State;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.ObjectActions
{

    /// <summary>
    /// Runs a microscript
    /// </summary>
    public class MicroscriptSpecial : ActionSpecial
    {
        public EditorMicroscript[] Microscripts;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            ExecuteMicroscripts();

            if (!Repeatable)
                Locked = true;
        }

        private void ExecuteMicroscripts()
        {
            foreach(EditorMicroscript em in Microscripts)
            {
                try
                {
                    MicroscriptNode m = em.Parse();
                    m.Execute();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

    }
}