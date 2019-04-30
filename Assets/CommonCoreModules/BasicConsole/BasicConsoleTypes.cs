using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace CommonCore.BasicConsole
{

    /// <summary>
    /// A multi-line message to be displayed in the console
    /// </summary>
    internal class ConsoleMessage //should this be a struct?
    {
        private const int LineLength = 200;

        public ImmutableArray<string> Lines { get; private set; }
        public bool Expanded { get; set; }
        public int ShownLines { get => Expanded ? Lines.Length : 1; }
        public LogType LogType { get; private set; }

        public ConsoleMessage(string message, LogType logType)
        {
            //split message and fill Lines

            if (message.Contains("\n"))
            {
                //message has multiple lines
                string[] splitMessage = message.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                //check for any long lines
                bool hasLongLines = false;
                foreach(string seg in splitMessage)
                {
                    if(seg.Length > LineLength)
                    {
                        hasLongLines = true;
                        break;
                    }
                }

                //handling is different for long lines or only short lines
                if(hasLongLines)
                {
                    //need to recursively split up lines
                    List<string> tempLines = new List<string>();
                    foreach(string line in splitMessage)
                    {
                        if (line.Length > LineLength)
                            SplitLines(line, tempLines);
                        else
                            tempLines.Add(line);
                    }
                    Lines = tempLines.ToImmutableArray();
                }
                else
                {
                    //can just use our array as is
                    Lines = splitMessage.ToImmutableArray();
                }

            }
            else
            {
                if (message.Length < LineLength)
                {
                    //message will fit in single line (most common and cheapest case)
                    Lines = (new string[] { message }).ToImmutableArray();
                }
                else
                {
                    List<string> tempLines = new List<string>();
                    //message needs to be split, but has no newlines of its own
                    SplitLines(message, tempLines);
                    Lines = tempLines.ToImmutableArray();
                }
            }

            LogType = logType;
        }

        private static void SplitLines(string message, List<string> tempLines)
        {
            int idx = 0;
            while (idx < message.Length)
            {
                int charsToTake = Math.Min(message.Length - idx, LineLength); //watch for off-by-one!
                tempLines.Add(message.Substring(idx, charsToTake));
                idx += charsToTake;
            }
        }
    }
}