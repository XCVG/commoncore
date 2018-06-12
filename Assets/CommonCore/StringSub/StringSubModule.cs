using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;
using SickDev.CommandSystem;

namespace CommonCore.StringSub
{

    /*
     * CommonCore String Substitution Module
     * Provides facilities to substitute strings, add macros, etc
     */
    public class StringSubModule : CCModule
    {
        internal static StringSubModule Instance { get; private set; }

        private Dictionary<string, Dictionary<string, string>> Strings;

        public StringSubModule()
        {
            Instance = this;

            //load all substitution lists
            Strings = new Dictionary<string, Dictionary<string, string>>();
            TextAsset[] tas = CCBaseUtil.LoadResources<TextAsset>("Strings/");
            foreach(TextAsset ta in tas)
            {
                try
                {
                    var lists = CCBaseUtil.LoadJson<Dictionary<string, Dictionary<string, string>>>(ta.text);
                    foreach(var list in lists)
                    {
                        //merge new lists onto old
                        if(Strings.ContainsKey(list.Key))
                        {
                            //list already exists, need to merge
                            var oldList = Strings[list.Key];
                            foreach(var item in list.Value)
                            {
                                oldList[item.Key] = item.Value;
                            }
                        }
                        else
                        {
                            //list doesn't exist, can just add
                            Strings.Add(list.Key, list.Value);
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError("Error loading string file: " + ta.name);
                    Debug.LogException(e);
                }
            }

            string statusString = string.Format("({0} files, {1} lists)", tas.Length, Strings.Count);

            Debug.Log("String Substitution module loaded!" + statusString);
        }

        internal string GetString(string baseString, string listName)
        {
            Dictionary<string, string> list = null;
            if (Strings.TryGetValue(listName, out list))
            {
                string newString = null;
                if (list.TryGetValue(baseString, out newString))
                {
                    return newString;
                }
            }

            CDebug.LogEx(string.Format("Missing string {0} in list {1}", baseString, listName), LogLevel.Verbose, this);

            return baseString;
        }

        [Command(alias = "Replace", className = "StringSub")]
        public static void CommandReplace(string baseString, string listName)
        {
            DevConsole.singleton.Log(Instance.GetString(baseString, listName));
        }

    }

    //basically just shorthand for accessing functionality a different way
    public static class Sub
    {
        public static string Replace(string baseString, string listName)
        {
            return StringSubModule.Instance.GetString(baseString, listName);
        }

        public static string Macro(string baseString)
        {
            return baseString; //TODO implement this
        }
    }


}