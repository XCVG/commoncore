using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Console
{
    /// <summary>
    /// Interface representing a command console system
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Add a command to the console system
        /// </summary>
        void AddCommand(Delegate command, bool useClassName, string alias, string className, string description);

        /// <summary>
        /// Write a line of text to the console
        /// </summary>
        void WriteLine(string line);
    }
}