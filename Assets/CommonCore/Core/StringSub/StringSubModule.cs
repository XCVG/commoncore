using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CommonCore.DebugLog;
using System.Linq;
using CommonCore.Console;
using CommonCore.Config;

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

            Log("Finished loading!");
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            //load lists from AddonData
            if(data.LoadedResources != null && data.LoadedResources.Count > 0)
            {
                int numFiles = 0;
                var stringListKeys = data.LoadedResources.Keys
                    .Where(k => k.StartsWith("Data/Strings/"));
                foreach(string key in stringListKeys)
                {
                    var rh = data.LoadedResources[key];
                    if (rh.Resource is TextAsset ta)
                    {
                        try
                        {
                            LoadListFromTextAsset(ta);
                            numFiles++;
                        }
                        catch (Exception e)
                        {
                            LogError("Error loading string file: " + ta.name);
                            LogException(e);
                        }
                    }
                }

                string statusString = string.Format("({0} new files, {1} total lists)", numFiles, Strings.Count);
                Log("Loaded lists " + statusString);
            }

            //load subbers from AddonData
            if (data.LoadedAssemblies != null && data.LoadedAssemblies.Count > 0)
            {
                var subberTypes = data.LoadedAssemblies.SelectMany(a => a.GetTypes())
                    .Where((type) => type.GetInterfaces().Contains(typeof(IStringSubber)));
                LoadSubbersFromTypes(subberTypes);
            }
        }

        private void LoadLists()
        {
            //load all substitution lists
          
            TextAsset[][] textAssetArrays = CoreUtils.LoadResourcesVariants<TextAsset>("Data/Strings/");
            int numFiles = 0;

            foreach (var tas in textAssetArrays)
            {
                foreach (TextAsset ta in tas)
                {
                    try
                    {
                        LoadListFromTextAsset(ta);
                        numFiles++;
                    }
                    catch (Exception e)
                    {
                        LogError("StringSub: Error loading string file: " + ta.name);
                        LogException(e);
                    }
                }
                
            }

            string statusString = string.Format("({0} new files, {1} total lists)", numFiles, Strings.Count);
            Log("Loaded lists " + statusString);
        }

        private void LoadListFromTextAsset(TextAsset ta)
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

        private void LoadSubbers()
        {
            Type[] subberTypes = CCBase.BaseGameTypes
                .Where((type) => type.GetInterfaces().Contains(typeof(IStringSubber)))
                .ToArray();

            LoadSubbersFromTypes(subberTypes);

        }

        private void LoadSubbersFromTypes(IEnumerable<Type> subberTypes)
        {
            List<IStringSubber> newSubbers = new List<IStringSubber>();

            foreach (Type subberType in subberTypes)
            {
                try
                {
                    IStringSubber subber = (IStringSubber)Activator.CreateInstance(subberType);
                    Subbers.Add(subber);
                    newSubbers.Add(subber);

                    foreach (string pattern in subber.MatchPatterns)
                    {
                        if (SubMap.ContainsKey(pattern))
                            LogWarning(string.Format("StringSub: pattern \"{0}\" from {1} already registered by {2}", pattern, subber.GetType().Name, SubMap[pattern].Method.DeclaringType.Name));

                        SubMap[pattern] = subber.Substitute;
                    }
                }
                catch (Exception e)
                {
                    LogError("Failed to load subber " + subberType.Name);
                    LogException(e);
                }
            }

            if(newSubbers.Count > 0)
                Log("Loaded subbers " + newSubbers.Select(s => s.GetType().Name).ToNiceString());
        }

        internal string GetString(string baseString, string listName, bool suppressWarnings, bool ignoreKeyCase)
        {
            Dictionary<string, string> list = null;
            if (Strings.TryGetValue(listName, out list))
            {
                string newString = null;
                if (ignoreKeyCase)
                {
                    newString = list.GetIgnoreCase(baseString);
                    if(newString != null)
                        return newString;
                }
                else
                {
                    if (list.TryGetValue(baseString, out newString))
                    {
                        return newString;
                    }
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
            // general format is *:* where the first part is where to search
            // can add parameters with | but parsing this is deferred to the subbers

            string result = "?ERROR?";
            try
            {
                //hack to handle preserving Unity-compatible rich text color tags
                if(sequence.StartsWith("color", StringComparison.Ordinal))
                {
                    return $"<{sequence}>";
                }

                string[] sequenceParts = sequence.Split(':');

                switch (sequenceParts[0])
                {
                    case "(":
                        result = "<";
                        break;
                    case ")":
                        result = ">";
                        break;
                    case "l":
                        // l:*:* : Lookup (string substitution) List:String
                        result = GetString(sequenceParts[2], sequenceParts[1], false, false);
                        break;
                    case "/color":
                        result = "</color>"; //special handling for color closing tag
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
                        if (SubMap.ContainsKey(sequenceParts[0]))
                        {
                            result = SubMap[sequenceParts[0]].Invoke(sequenceParts);
                        }
                        else
                        {
                            result = string.Format("?SUBBER:{0}?", sequence);
                        }
                        break;
                }
            }
            catch(Exception e)
            {
                CDebug.LogEx($"Failed to handle macro ({e.GetType().Name}: {e.Message})", LogLevel.Verbose, this);
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

        [Command(alias = "ListMacros", className = "StringSub")]
        public static void CommandListMacros()
        {
            StringBuilder sb = new StringBuilder(80 * Instance.SubMap.Count);

            foreach(var key in Instance.SubMap.Keys)
            {
                sb.AppendLine($"<{key}>");
            }

            ConsoleModule.WriteLine(sb.ToString());
        }

        [Command(alias = "ListSubstitutions", className = "StringSub")]
        public static void CommandListSubstitutions()
        {
            StringBuilder sb = new StringBuilder(80 * Instance.Strings.Count);

            foreach(var list in Instance.Strings)
            {
                sb.AppendLine(list.Key);
                foreach(var item in list.Value)
                {
                    sb.AppendFormat("\t{0} : \"{1}\"\n", item.Key, item.Value);
                }
                sb.AppendLine();
            }

            ConsoleModule.WriteLine(sb.ToString());
        }

        [Command(alias = "Replace", className = "StringSub")]
        public static void CommandReplace(string baseString, string listName)
        {
            try
            {
                ConsoleModule.WriteLine(Instance.GetString(baseString, listName, false, false));
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
        public static string Replace(string baseString, string listName, bool ignoreCase)
        {
            return StringSubModule.Instance.GetString(baseString, listName, true, ignoreCase);
        }

        public static string Replace(string baseString, string listName)
        {
            return StringSubModule.Instance.GetString(baseString, listName, true, false);
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
                return "?FATALERROR?";
            }
                   
        }
    }


}