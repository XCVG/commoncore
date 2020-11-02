using System;
using UnityEngine;
using CommonCore.Scripting;
using CommonCore.State;

namespace CommonCore.RpgGame
{
    /// <summary>
    /// Callable scripts related to the RPG game system
    /// </summary>
    public static class RpgScripts
    {
        //for now, just some stuff for dialogue conditionals

        [CCScript(ClassName = "PlayerFlags", Name = "HasFlag", NeverPassExecutionContext = true)]
        public static bool HasPlayerFlag(string playerFlag)
        {
            return GameState.Instance.PlayerFlags.Contains(playerFlag);
        }

        [CCScript(ClassName = "PlayerFlags", Name = "NoFlag", NeverPassExecutionContext = true)]
        public static bool NoPlayerFlag(string playerFlag)
        {
            return !GameState.Instance.PlayerFlags.Contains(playerFlag);
        }

        [CCScript(ClassName = "SessionFlags", Name = "HasFlag", NeverPassExecutionContext = true)]
        public static bool HasSessionFlag(string sessionFlag)
        {
            return MetaState.Instance.SessionFlags.Contains(sessionFlag);
        }

        [CCScript(ClassName = "SessionFlags", Name = "NoFlag", NeverPassExecutionContext = true)]
        public static bool NoSessionFlag(string sessionFlag)
        {
            return !MetaState.Instance.SessionFlags.Contains(sessionFlag);
        }
    }
}