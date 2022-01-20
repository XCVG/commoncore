using CommonCore.Scripting;
using CommonCore.StringSub;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.Experimental
{

    /// <summary>
    /// String subber providing integration with the scripting system
    /// </summary>
    /// <remarks>
    /// <para>Reserves the "s" selector</para>
    /// </remarks>
    public class ScriptStringSubber : IStringSubber
    {
        public IEnumerable<string> MatchPatterns => new string[] { "s" };

        public string Substitute(string[] sequenceParts)
        {
            if (sequenceParts.Length < 2)
                throw new ArgumentException("sequenceParts must have length >= 2", nameof(sequenceParts));

            var scriptName = sequenceParts[1];

            object[] args = new object[] { };

            if(sequenceParts.Length >= 3)
            {
                var rawArgs = sequenceParts[2].Split('|');
                args = rawArgs.Select(s => TypeUtils.StringToNumericAutoDouble(s)).ToArray();
            }

            return ScriptingModule.CallForResult(scriptName, new ScriptExecutionContext() { Caller = this }, args)?.ToString();
        }
    }
}