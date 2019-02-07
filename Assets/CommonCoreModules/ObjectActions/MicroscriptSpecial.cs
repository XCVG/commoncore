using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.RpgGame.State;

namespace CommonCore.ObjectActions
{

    public class MicroscriptSpecial : ActionSpecial
    {
        public EditorMicroscript[] Microscripts;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked)
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