using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CommonCore.DebugLog;
using System.Linq;
using CommonCore.Console;

namespace CommonCore.StringSub
{

    /*
     * CommonCore String Substitution Module
     * Provides facilities to substitute strings, add macros, etc
     * Will become more useful as time goes on
     */
    public class StringSubModule : CCModule
    {
        internal static StringSubModule Instance { get; private set; }

        private Dictionary<string, Dictionary<string, string>> Strings = new Dictionary<string, Dictionary<string, string>>();
        private List<IStringSubber> Subbers = new List<IStringSubber>();
        private Dictionary<string, Func<string[], string>> SubMap = new Dictionary<string, Func<string[], string>>();

        public StringSubModule()
        {
            Instance = this;

            LoadLists();
            LoadSubbers();

            Log("StringSub: Finished loading!");
        }

        private void LoadLists()
        {
            //load all substitution lists

            TextAsset[][] textAssetArrays = CoreUtils.LoadDataResources<TextAsset>("Data/Strings/");

            foreach (var tas in textAssetArrays)
            {
                foreach (TextAsset ta in tas)
                {
                    try
                    {
                        var lists = CoreUtils.LoadJson<Dictionary<string, Dictionary<string, string>>>(ta.text);
                        foreach (var list in lists)
                        {
                            //merge new lists onto old
                            if (Strings.ContainsKey(list.Key))
                            {
                                //list already exists, need to merge
                                var oldList = Strings[list.Key];
                                foreach (var item in list.Value)
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
                    catch (Exception e)
                    {
                        LogError("StringSub: Error loading string file: " + ta.name);
                        LogException(e);
                    }
                }

                string statusString = string.Format("({0} files, {1} lists)", tas.Length, Strings.Count);
                Log("StringSub: Loaded lists " + statusString);
            }            
        }

        private void LoadSubbers()
        {
            Type[] subberTypes = CCBase.BaseGameTypes
                .Where((type) => type.GetInterfaces().Contains(typeof(IStringSubber)))
                .ToArray();

            foreach(Type subberType in subberTypes)
            {
                try
                {
                    IStringSubber subber = (IStringSubber)Activator.CreateInstance(subberType);
                    Subbers.Add(subber);

                    foreach(string pattern in subber.MatchPatterns)
                    {
                        if (SubMap.ContainsKey(pattern))
                            LogWarning(string.Format("StringSub: pattern \"{0}\" from {1} already registered by {2}", pattern, subber.GetType().Name, SubMap[pattern].Method.DeclaringType.Name));

                        SubMap[pattern] = subber.Substitute;
                    }
                }
                catch(Exception)
                {
                    LogError("StringSub: Failed to load subber " + subberType.Name);
                }
            }

            Log("StringSub: Loaded subbers " + Subbers.Select(s => s.GetType().Name).ToNiceString());

        }

        internal string GetString(string baseString, string listName, bool suppressWarnings)
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

            if(!suppressWarnings)
                CDebug.LogEx(string.Format("Missing string {0} in list {1}", baseString, listName), LogLevel.Verbose, this);

            return baseString;
        }

        internal string SubstituteMacros(string baseString)
        {
            //sanity check and quick reject
            if (!baseString.Contains("<"))
                return baseString;

            StringBuilder sb = new StringBuilder(baseString.Length * 2); //RAM is cheap, allocations are expensive

            //<> for substitution
            // <(>=< and <)>=> if you need those symbols            
            // TODO resilience and non-crashiness

            //advance pointer to next token
            int pointer = 0, lastPointer = 0;
            for (; pointer < baseString.Length; pointer++)
            {
                if(baseString[pointer] == '<')
                {
                    //we've hit the beginning of an escape sequence

                    //copy the string "so far"
                    sb.Append(baseString.Substring(lastPointer, pointer-lastPointer));

                    //advance to the end of the sequence
                    int newPointer = pointer + 1;
                    for (; baseString[newPointer] != '>'; newPointer++) { }
                    lastPointer = newPointer + 1;
                    string sequence = baseString.Substring(pointer + 1, newPointer-pointer-1);
                    pointer = newPointer;

                    //process and append escape sequence
                    sb.Append(GetMacro(sequence));
                }
                
            }

            //copy everything after the last escape sequence          
            sb.Append(baseString.Substring(lastPointer, pointer - lastPointer));

            return sb.ToString();
        }

        internal string GetMacro(string sequence)
        {
            // l:*:* : Lookup (string substitution) List:String
            // av:* : Player.GetAV
            // inv:* : inventory
            // cpf:* : Campaign Flag 
            // cpv:* : Campaign Variable 
            // cqs:* : Quest Stage 
            // general format is *:* where the first part is where to search
            // might eventually add parameters with | (deferred until at least Balmora)
            // TODO figure out inventory, etc
            // TODO (but this is for like... Balmora) switch to messaging or dynamic modules so we're not dependent on other modules

            string[] sequenceParts = sequence.Split(':');

            string result = "<ERROR>";
            switch (sequenceParts[0])
            {
                case "(":
                    result = "<";
                    break;
                case ")":
                    result = ">";
                    break;
                case "l":
                    result = GetString(sequenceParts[2], sequenceParts[1], false);
                    break;
                case "strong":
                    result = "<b>"; //handling dialogue written for proper html
                    break;
                case "/strong":
                    result = "</b>"; //handling dialogue written for proper html
                    break;
                case "em":
                    result = "<i>"; //handling dialogue written for proper html
                    break;
                case "/em":
                    result = "</i>"; //handling dialogue written for proper html
                    break;
                case "b":
                    result = "<b>"; //handling dialogue written for improper html
                    break;
                case "/b":
                    result = "</b>"; //handling dialogue written for improper html
                    break;
                case "i":
                    result = "<i>"; //handling dialogue written for improper html
                    break;
                case "/i":
                    result = "</i>"; //handling dialogue written for improper html
                    break;
                default:
                    if(SubMap.ContainsKey(sequenceParts[0]))
                    {
                        result = SubMap[sequenceParts[0]].Invoke(sequenceParts);
                    }
                    else
                    {
                        result = string.Format("<MISSING:{0}>", sequence);
                    }                    
                    break;
            }

            return result;
        }

        internal bool StringExists(string baseString, string listName)
        {
            Dictionary<string, string> list = null;
            if (Strings.TryGetValue(listName, out list))
            {
                return list.ContainsKey(baseString);
            }

            return false;
        }       

        [Command(alias = "Replace", className = "StringSub")]
        public static void CommandReplace(string baseString, string listName)
        {
            try
            {
                ConsoleModule.WriteLine(Instance.GetString(baseString, listName, false));
            }
            catch(Exception e)
            {
                ConsoleModule.WriteLine(e.ToString(), LogLevel.Error);
            }
        }

        [Command(alias = "Macro", className = "StringSub")]
        public static void CommandMacro(string baseString)
        {
            try
            {
                ConsoleModule.WriteLine(Instance.SubstituteMacros(baseString));
            }
            catch (Exception e)
            {
                ConsoleModule.WriteLine(e.ToString(), LogLevel.Error);
            }
        }

    }

    //basically just shorthand for accessing functionality a different way
    public static class Sub
    {
        public static string Replace(string baseString, string listName)
        {
            return StringSubModule.Instance.GetString(baseString, listName, true);
        }

        public static bool Exists(string baseString, string listName)
        {
            return StringSubModule.Instance.StringExists(baseString, listName);
        }

        public static string Macro(string baseString)
        {
            try
            {
                return StringSubModule.Instance.SubstituteMacros(baseString);
            }
            catch(Exception e) //eventually we won't need this
            {
                Debug.LogException(e);
                return "<<ERROR>>";
            }
                   
        }
    }


}