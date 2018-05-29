using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.State;

namespace CommonCore.World
{

    //a different kind of gross hack
    public class LocalRestorableComponent : DynamicRestorableComponent
    {
        public override RestorableData Save()
        {
            RestorableData data = base.Save();

            return data;
        }

        public override void Restore(RestorableData data)
        {
            base.Restore(data);

        }
    }
}