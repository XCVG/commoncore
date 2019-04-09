using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.DebugLog
{
    /// <summary>
    /// CommonCore Debug/Log Module. Provides some debug logging/misc services.
    /// </summary>
    [CCExplicitModule]
    public class DebugModule : CCModule
    {
        public DebugModule()
        {
            FPSCounter.Initialize();
        }

    }
}