using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;

namespace CommonCore.LockPause
{

    public static class LockPauseConsoleIntegration
    {
        [Command(alias ="ForceUnlock", useClassName = false)]
        static void ForceClearAll()
        {
            LockPauseModule.ForceClearLocks();
        }
    }
}