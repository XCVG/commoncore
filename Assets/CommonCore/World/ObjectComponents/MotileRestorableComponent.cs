using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    public class MotileRestorableComponent : RestorableComponent
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
