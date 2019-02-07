using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.LockPause
{

    public partial class LockPauseModule //yes, I have a strategy
    {
        [Command(alias ="ClearLocks", useClassName = false)]
        public static void ForceClearAll()
        {
            ForceClearLocks();
        }

        [Command(alias = "ListLocks", useClassName = false)]
        public static void ListLocks()
        {
            StringBuilder sb = new StringBuilder(255);

            sb.AppendLine(string.Format("Pause state: {0}", Instance.PauseLockState == null ? "null" : Instance.PauseLockState.Value.ToString()));
            if(Instance.PauseLocks.Count > 0)
            {
                foreach(var pl in Instance.PauseLocks)
                {
                    sb.AppendLine(pl.ToString());
                }
            }

            sb.AppendLine();

            sb.AppendLine(string.Format("Inputlock state: {0}", Instance.InputLockState == null ? "null" : Instance.InputLockState.Value.ToString()));
            if (Instance.InputLocks.Count > 0)
            {
                foreach (var il in Instance.InputLocks)
                {
                    sb.AppendLine(il.ToString());
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}