using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.StringSub
{
    public interface IStringSubber
    {
        /// <summary>
        /// Patterns that this subber can match
        /// </summary>
        IEnumerable<string> MatchPatterns { get; }

        /// <summary>
        /// Method that substitutes strings for a pattern
        /// </summary>
        /// <param name="sequenceParts">sequence of pattern elements divided by :</param>
        string Substitute(string[] sequenceParts);

    }
}