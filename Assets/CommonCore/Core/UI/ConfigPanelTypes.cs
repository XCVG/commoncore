using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{
    [Flags]
    public enum PendingChangesFlags
    {
        None = 0,
        MoreOptionsOnApply = 1,
        RequiresRestart = 2,
        DoNotSetPendingChanges = 4
    }
}