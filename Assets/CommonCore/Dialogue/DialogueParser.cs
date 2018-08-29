using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using CommonCore.State;

namespace CommonCore.Dialogue
{
    internal static class DialogueParser
    {

        public static DialogueScene LoadDialogue(string dialogueName)
        {
            TextAsset ta = CCBaseUtil.LoadResource<TextAsset>("Dialogue/" + dialogueName);
            return LoadDialogueFromString(dialogueName, ta.text);
        }

        public static DialogueScene LoadDialogueFromString(string dialogueName, string text)
        {
            JObject jo = JObject.Parse(text);
            //Debug.Log(jo);

            //parse root node (scene)
            string sBackground = string.Empty;
            string sImage = string.Empty;
            string sMusic = string.Empty;
            string sNext = string.Empty;
            string sText = string.Empty;
            string sName = string.Empty;
            if (jo["background"] != null)
                sBackground = jo["background"].Value<string>();
            if (jo["image"] != null)
                sImage = jo["image"].Value<string>();
            if (jo["music"] != null)
                sMusic = jo["music"].Value<string>();
            if (jo["default"] != null)
                sNext = jo["default"].Value<string>();
            if (jo["text"] != null)
                sText = jo["text"].Value<string>();
            if (jo["nameText"] != null)
                sName = jo["nameText"].Value<string>();
            Frame baseFrame = new Frame(sBackground, sImage, sNext, sMusic, sName, sText, null, null);

            //parse frames
            Dictionary<string, Frame> frames = new Dictionary<string, Frame>();
            frames.Add(dialogueName, baseFrame);
            JObject jf = (JObject)jo["frames"];
            foreach (var x in jf)
            {
                try
                {
                    string key = x.Key;
                    JToken value = x.Value;
                    Frame f = DialogueParser.ParseSingleFrame(value, baseFrame);
                    frames.Add(key, f);
                }
                catch (Exception e)
                {
                    Debug.Log("Failed to parse frame!");
                    Debug.LogException(e);
                }
            }

            return new DialogueScene(frames, sNext, sMusic);
        }

        public static Frame ParseSingleFrame(JToken jt, Frame baseFrame)
        {
            string background = baseFrame.Background;
            string image = baseFrame.Image;
            string next = baseFrame.Next;
            string music = baseFrame.Music;
            string nameText = baseFrame.NameText;
            string text = baseFrame.Text;
            string type = null;

            if (jt["background"] != null)
                background = jt["background"].Value<string>();
            if (jt["image"] != null)
                image = jt["image"].Value<string>();
            if (jt["next"] != null)
                next = jt["next"].Value<string>();
            if (jt["music"] != null)
                music = jt["music"].Value<string>();
            if (jt["nameText"] != null)
                nameText = jt["nameText"].Value<string>();
            if (jt["text"] != null)
                text = jt["text"].Value<string>();
            if (jt["type"] != null)
                type = jt["type"].Value<string>();

            //TODO load/parse conditionals and microscripts
            ConditionNode[] conditional = null;
            MicroscriptNode[] microscript = null;

            if (jt["conditional"] != null)
            {
                List<ConditionNode> cList = new List<ConditionNode>();
                JArray ja = (JArray)jt["conditional"];
                foreach (var x in ja)
                {
                    try
                    {
                        cList.Add(ParseConditionNode(x));
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                conditional = cList.ToArray();
            }

            if (jt["microscript"] != null)
            {
                //TODO parse microscripts
                List<MicroscriptNode> nList = new List<MicroscriptNode>();
                JArray ja = (JArray)jt["microscript"];
                foreach (var x in ja)
                {
                    try
                    {
                        nList.Add(ParseMicroscript(x));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }
                }
                microscript = nList.ToArray();
            }

            if (type == "choice")
            {
                //parse choices if choice frame
                List<ChoiceNode> choices = new List<ChoiceNode>();
                JArray ja = (JArray)jt["choices"];
                foreach (var x in ja)
                {
                    choices.Add(ParseChoiceNode(x));
                }
                return new ChoiceFrame(background, image, next, music, nameText, text, choices.ToArray(), conditional, microscript);
            }
            else if (type == "text")
            {
                return new TextFrame(background, image, next, music, nameText, text, conditional, microscript);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static ChoiceNode ParseChoiceNode(JToken jt)
        {
            string text = jt["text"].Value<string>();
            string next = jt["next"].Value<string>();

            MicroscriptNode[] microscripts = null;
            if (jt["microscript"] != null)
            {
                //TODO parse microscripts
                List<MicroscriptNode> nList = new List<MicroscriptNode>();
                JArray ja = (JArray)jt["microscript"];
                foreach (var x in ja)
                {
                    try
                    {
                        nList.Add(ParseMicroscript(x));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }
                }
                microscripts = nList.ToArray();
            }

            ConditionNode[] conditionals = null;
            if (jt["conditional"] != null)
            {
                List<ConditionNode> cList = new List<ConditionNode>();
                JArray ja = (JArray)jt["conditional"];
                foreach (var x in ja)
                {
                    try
                    {
                        cList.Add(ParseConditionNode(x));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }

                }
                conditionals = cList.ToArray();
            }

            Conditional showCondition = null;
            if (jt["showCondition"] != null)
            {
                showCondition = ParseSingleCondition(jt["showCondition"]);
            }

            Conditional hideCondition = null;
            if (jt["hideCondition"] != null)
            {
                hideCondition = ParseSingleCondition(jt["hideCondition"]);
            }

            return new ChoiceNode(next, text, showCondition, hideCondition, microscripts, conditionals);
        }

        public static ConditionNode ParseConditionNode(JToken jt)
        {
            //next and list of conditions
            string next = jt["next"].Value<string>();
            JArray ja = (JArray)jt["conditions"];
            List<Conditional> conditions = new List<Conditional>();
            foreach (var x in ja)
            {
                conditions.Add(ParseSingleCondition(x));
            }

            return new ConditionNode(next, conditions.ToArray());
        }

        public static Conditional ParseSingleCondition(JToken jt)
        {
            //types
            ConditionType type;
            string target;
            if (jt["flag"] != null)
            {
                type = ConditionType.Flag;
                target = jt["flag"].Value<string>();
            }
            else if (jt["noflag"] != null)
            {
                type = ConditionType.NoFlag;
                target = jt["noflag"].Value<string>();
            }
            else if (jt["variable"] != null)
            {
                type = ConditionType.Variable;
                target = jt["variable"].Value<string>();
            }
            else if (jt["affinity"] != null)
            {
                type = ConditionType.Affinity;
                target = jt["affinity"].Value<string>();
            }            
            else if (jt["quest"] != null)
            {
                type = ConditionType.Quest;
                target = jt["quest"].Value<string>();
            }
            else if (jt["item"] != null)
            {
                type = ConditionType.Item;
                target = jt["item"].Value<string>();
            }
            else if (jt["actorvalue"] != null)
            {
                type = ConditionType.ActorValue;
                target = jt["actorvalue"].Value<string>();
            }
            else
            {
                throw new NotSupportedException();
            }

            //options
            ConditionOption? option = null;
            IComparable optionValue = 0;
            if (type == ConditionType.Item)
            {
                //check for "consume"
                if (jt["consume"] != null)
                {
                    option = ConditionOption.Consume;
                    optionValue = Convert.ToInt32(jt["consume"].Value<bool>());
                }

            }
            else if (type == ConditionType.Affinity || type == ConditionType.Quest || type == ConditionType.Variable || type == ConditionType.ActorValue)
            {
                if (jt["greater"] != null)
                {
                    option = ConditionOption.Greater;
                    optionValue = (IComparable)CCBaseUtil.StringToNumericAuto(jt["greater"].Value<string>());
                }
                else if (jt["less"] != null)
                {
                    option = ConditionOption.Less;
                    optionValue = (IComparable)CCBaseUtil.StringToNumericAuto(jt["less"].Value<string>());
                }
                else if (jt["equal"] != null)
                {
                    option = ConditionOption.Equal;
                    optionValue = (IComparable)CCBaseUtil.StringToNumericAuto(jt["equal"].Value<string>());
                }
                else if (jt["greaterEqual"] != null)
                {
                    option = ConditionOption.GreaterEqual;
                    optionValue = (IComparable)CCBaseUtil.StringToNumericAuto(jt["greaterEqual"].Value<string>());
                }
                else if (jt["lessEqual"] != null)
                {
                    option = ConditionOption.LessEqual;
                    optionValue = (IComparable)CCBaseUtil.StringToNumericAuto(jt["lessEqual"].Value<string>());
                }
                else if (jt["started"] != null)
                {
                    option = ConditionOption.Started;
                    optionValue = Convert.ToInt32(jt["started"].Value<bool>());
                }
                else if (jt["finished"] != null)
                {
                    option = ConditionOption.Finished;
                    optionValue = Convert.ToInt32(jt["finished"].Value<bool>());
                }
            }

            return new Conditional(type, target, option, optionValue);
        }

        public static MicroscriptNode ParseMicroscript(JToken jt)
        {
            MicroscriptType type;
            string target;
            if (jt["flag"] != null)
            {
                type = MicroscriptType.Flag;
                target = jt["flag"].Value<string>();
            }
            else if (jt["item"] != null)
            {
                type = MicroscriptType.Item;
                target = jt["item"].Value<string>();
            }
            else if (jt["variable"] != null)
            {
                type = MicroscriptType.Variable;
                target = jt["variable"].Value<string>();
            }
            else if (jt["affinity"] != null)
            {
                type = MicroscriptType.Affinity;
                target = jt["affinity"].Value<string>();
            }
            else if (jt["quest"] != null)
            {
                type = MicroscriptType.Quest;
                target = jt["quest"].Value<string>();
            }
            else if (jt["actorvalue"] != null)
            {
                type = MicroscriptType.ActorValue;
                target = jt["actorvalue"].Value<string>();
            }
            else if(jt["exec"] != null)
            {
                type = MicroscriptType.Exec;
                target = jt["exec"].Value<string>();
            }
            else
            {
                throw new NotSupportedException();
            }

            MicroscriptAction action;
            object value = 0;
            if (jt["set"] != null)
            {
                action = MicroscriptAction.Set;
                if (type == MicroscriptType.Flag) //parse as boolean
                    value = Convert.ToInt32(jt["set"].Value<bool>());
                else //otherwise parse as number
                    value = jt["set"].Value<int>();
            }
            else if (jt["toggle"] != null)
            {
                action = MicroscriptAction.Toggle;
            }
            else if (jt["add"] != null)
            {
                action = MicroscriptAction.Add;
                value = CCBaseUtil.StringToNumericAuto(jt["add"].Value<string>());
            }
            else if (jt["give"] != null)
            {
                action = MicroscriptAction.Give;
                value = CCBaseUtil.StringToNumericAuto(jt["give"].Value<string>());
            }
            else if (jt["take"] != null)
            {
                action = MicroscriptAction.Take;
                value = CCBaseUtil.StringToNumericAuto(jt["take"].Value<string>());
            }
            else if (jt["start"] != null)
            {
                action = MicroscriptAction.Start;
                value = CCBaseUtil.StringToNumericAuto(jt["start"].Value<string>());
            }
            else if (jt["finish"] != null)
            {
                action = MicroscriptAction.Finish;
                value = CCBaseUtil.StringToNumericAuto(jt["finish"].Value<string>());
            }
            else
            {
                throw new NotSupportedException();
            }

            return new MicroscriptNode(type, target, action, value);
        }

        public static KeyValuePair<string, string> ParseLocation(string loc)
        {
            if (!loc.Contains("."))
                return new KeyValuePair<string, string>(null, loc);

            var arr = loc.Split('.');
            return new KeyValuePair<string, string>(arr[0], arr[1]);
        }
    }
}