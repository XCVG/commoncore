using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonCore.Config;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.State;
using CommonCore.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PseudoExtensibleEnum;
using UnityEngine;

namespace CommonCore.RpgGame.Dialogue
{
    public class DialogueScene
    {
        public static readonly string BaseFrameName = "_BaseFrame";

        public IReadOnlyDictionary<string, Frame> Frames { get; private set; }
        public string Default;
        public string Music;

        public DialogueScene(IReadOnlyDictionary<string, Frame> frames, string defaultFrame, string music)
        {
            Frames = frames;
            Default = defaultFrame;
            Music = music;
        }

        public Frame BaseFrame => Frames[BaseFrameName];
    }

    public class ChoiceNode
    {
        public readonly string Text;
        public readonly string Next;
        public readonly MicroscriptNode[] NextMicroscript;
        public readonly ConditionNode[] NextConditional;

        public readonly Conditional ShowCondition;
        public readonly Conditional HideCondition;

        public readonly SkillCheckNode SkillCheck;

        //TODO TraceSpeaker, TraceText, TraceIgnore, TraceShow
        public readonly string TraceSpeaker;
        public readonly string TraceText;
        public readonly bool TraceIgnore;
        public readonly bool TraceShow;

        public ChoiceNode(string next, string text)
        {
            Text = text;
            Next = next;
        }

        public ChoiceNode(string next, string text, Conditional showCondition, Conditional hideCondition, MicroscriptNode[] nextMicroscript, ConditionNode[] nextConditional, SkillCheckNode skillCheck, string traceSpeaker, string traceText, bool traceIgnore, bool traceShow)
            : this(next, text)
        {
            ShowCondition = showCondition;
            HideCondition = hideCondition;
            NextMicroscript = nextMicroscript;
            NextConditional = nextConditional;
            SkillCheck = skillCheck;
            TraceSpeaker = traceSpeaker;
            TraceText = traceText;
            TraceIgnore = traceIgnore;
            TraceShow = traceShow;
        }

        public string EvaluateConditional()
        {
            for (int i = NextConditional.Length - 1; i >= 0; i--)
            {
                var nc = NextConditional[i];
                bool ncResult = false;
                try
                {
                    ncResult = nc.Evaluate();
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
                if (ncResult)
                    return nc.Next;
            }
            return null;
        }

        public void EvaluateMicroscript()
        {
            if (NextMicroscript == null || NextMicroscript.Length < 1)
                return;

            foreach (MicroscriptNode mn in NextMicroscript)
            {
                try
                {
                    mn.Execute();
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

    }

    public class SkillCheckNode
    {
        public readonly SkillCheckType CheckType;
        public readonly SkillCheckComparison ComparisonType;
        public readonly SkillCheckTarget TargetType;        
        public readonly string Target;
        public readonly IComparable Value;
        public readonly string PassNext;
        public readonly string FailNext;
        public readonly bool AppendCheckText;

        public SkillCheckNode(SkillCheckType checkType, SkillCheckComparison comparisonType, SkillCheckTarget targetType, string target, IComparable value, string passNext, string failNext, bool appendCheckText)
        {
            CheckType = checkType;
            ComparisonType = comparisonType;
            TargetType = targetType;
            Target = target;
            Value = value;
            PassNext = passNext;
            FailNext = failNext;
            AppendCheckText = appendCheckText;
        }

        /// <summary>
        /// Does the skill check and returns what the next frame should be
        /// </summary>
        public string EvaluateSkillCheck()
        {
            if (Check())
                return PassNext;
            else
                return FailNext;
        }

        /// <summary>
        /// Does the skill check and returns if it passed or failed
        /// </summary>
        public bool Check()
        {
            if(CheckType == SkillCheckType.Soft)
            {
                var targetValue = GetValueOfTarget();

                float baseValue = Convert.ToSingle(targetValue);
                float neededValue = Convert.ToSingle(Value);

                if (GetOutcomeForCompareResult(baseValue.CompareTo(neededValue)))
                    return true;

                float passChance;
                if (ComparisonType == SkillCheckComparison.Less || ComparisonType == SkillCheckComparison.LessEqual)
                    passChance = neededValue / baseValue; //this might not be correct
                else
                    passChance = baseValue / neededValue;

                float chanceValue = UnityEngine.Random.Range(0f, 1f);

                return chanceValue <= passChance;
                
            }
            else if(CheckType == SkillCheckType.Hard)
            {
                var targetValue = GetValueOfTarget();
                int compareResult = TypeUtils.CompareNumericValues(targetValue, Value);
                return GetOutcomeForCompareResult(compareResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Checks if it is possible to pass the skill check
        /// </summary>
        public bool CheckIfPossible()
        {
            if(CheckType == SkillCheckType.Soft)
            {
                //check if target value > 0
                var targetValue = GetValueOfTarget();
                int compareResult = TypeUtils.CompareNumericValues(targetValue, 0);
                return compareResult > 0;
            }
            else if(CheckType == SkillCheckType.Hard)
            {
                return Check(); //must pass check to be possible
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the approximate pass chance of a soft skill check
        /// </summary>
        public float GetApproximatePassChance()
        {
            if(CheckType == SkillCheckType.Soft)
            {
                var targetValue = GetValueOfTarget();
                float baseValue = Convert.ToSingle(targetValue);
                float neededValue = Convert.ToSingle(Value);

                if (ComparisonType == SkillCheckComparison.Less || ComparisonType == SkillCheckComparison.LessEqual)
                    return neededValue / baseValue; //this might not be correct
                else
                    return baseValue / neededValue;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Tried to get the approximate pass chance of a hard skill check!");
                return CheckIfPossible() ? 1 : 0;
            }
        }

        /// <summary>
        /// Gets the value of the target of the check
        /// </summary>
        private IComparable GetValueOfTarget()
        {
            CharacterModel player = GameState.Instance.PlayerRpgState;

            switch (TargetType)
            {
                case SkillCheckTarget.Skill:
                    int skill = (int)PxEnum.Parse(typeof(SkillType), Target, true);
                    return player.DerivedStats.Skills[skill] * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerSkill;
                case SkillCheckTarget.Stat:
                    int stat = (int)PxEnum.Parse(typeof(StatType), Target, true);
                    return player.DerivedStats.Stats[stat] * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerSkill;
                case SkillCheckTarget.ActorValue:
                    return (IComparable)player.GetAV(Target);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Takes the result of a CompareTo and gets if it should be counted as pass or fail based on the comparison type
        /// </summary>
        private bool GetOutcomeForCompareResult(int compareResult)
        {
            switch (ComparisonType)
            {
                case SkillCheckComparison.GreaterEqual:
                    return compareResult >= 0;
                case SkillCheckComparison.Greater:
                    return compareResult > 0;
                case SkillCheckComparison.Less:
                    return compareResult < 0;
                case SkillCheckComparison.Equal:
                    return compareResult == 0;
                case SkillCheckComparison.LessEqual:
                    return compareResult <= 0;
                default:
                    throw new NotImplementedException();
            }
        }
        
    }

    public enum SkillCheckComparison
    {
        GreaterEqual, Greater, Less, Equal, LessEqual
    }

    public enum SkillCheckType
    {
        Unspecified, Hard, Soft
    }

    public enum SkillCheckTarget
    {
        Skill, Stat, ActorValue
    }

    public enum FrameImagePosition
    {
        Center, Fill, Character, Battler, CharacterBottom, Contain, Cover
    }

    public enum ChoicePanelHeight
    {
        Default, Full, Half, Variable, Fixed
    }

    public enum TraceDefaultSpeaker
    {
        None, PlayerLookup, PlayerName
    }

    /// <summary>
    /// Represents options for a frame
    /// </summary>
    public class FrameOptions : IReadOnlyDictionary<string, object>
    {
        //backing fields        
        private IDictionary<string, object> Options;

        //explicit fields

        public ChoicePanelHeight PanelHeight => TypeUtils.CoerceValue<ChoicePanelHeight>(Options.GetOrDefault(nameof(PanelHeight), ChoicePanelHeight.Default), false);

        public float PanelHeightPixels => TypeUtils.CoerceValue<float>(Options.GetOrDefault(nameof(PanelHeightPixels), 0), false);

        public bool HideNameText => TypeUtils.CoerceValue<bool>(Options.GetOrDefault(nameof(HideNameText), false), false);

        public string VoiceOverride => Options.GetOrDefault(nameof(VoiceOverride), null)?.ToString();

        public float? VoiceVolume => Options.ContainsKey(nameof(VoiceVolume)) ? (float?)TypeUtils.CoerceValue<float>(Options[nameof(VoiceVolume)]) : (float?)null;

        public IEnumerable<string> HideObjects => (Options.GetOrDefault(nameof(HideObjects), null) as IEnumerable)?.Cast<object>().Select(o => o.ToString()); //just... don't question this

        public bool AllowReturnFromShop => TypeUtils.CoerceValue<bool>(Options.GetOrDefault(nameof(AllowReturnFromShop), false), false);

        public bool? ShowImpossibleSkillChecks => Options.ContainsKey(nameof(ShowImpossibleSkillChecks)) ? TypeUtils.CoerceValue<bool>(Options[nameof(ShowImpossibleSkillChecks)]) : (bool?)null;

        public bool? AttemptImpossibleSkillChecks => Options.ContainsKey(nameof(AttemptImpossibleSkillChecks)) ? TypeUtils.CoerceValue<bool>(Options[nameof(AttemptImpossibleSkillChecks)]) : (bool?)null;

        public TraceDefaultSpeaker TraceDefaultSpeaker => TypeUtils.CoerceValue<TraceDefaultSpeaker>(Options.GetOrDefault(nameof(TraceDefaultSpeaker), default(TraceDefaultSpeaker)), false);
        public string TraceSpeaker => Options.GetOrDefault(nameof(TraceSpeaker), null)?.ToString();
        public bool TraceIgnore => TypeUtils.CoerceValue<bool>(Options.GetOrDefault(nameof(TraceIgnore), false), false);
        public string TraceText => Options.GetOrDefault(nameof(TraceText), null)?.ToString();
        public bool TraceIncludeChoices => TypeUtils.CoerceValue<bool>(Options.GetOrDefault(nameof(TraceIncludeChoices), false), false);
        public bool TraceIncludeNextText => TypeUtils.CoerceValue<bool>(Options.GetOrDefault(nameof(TraceIncludeNextText), false), false);
        public string TraceNextTextSpeaker => Options.GetOrDefault(nameof(TraceNextTextSpeaker), null)?.ToString();
        public string TraceNextTextText => Options.GetOrDefault(nameof(TraceNextTextText), null)?.ToString();

        //IReadOnlyDictionary implementation

        public object this[string key] => Options[key];        

        public int Count => Options.Count;

        public bool ContainsKey(string key) => Options.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Options.GetEnumerator();

        public bool TryGetValue(string key, out object value) => Options.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Options.Keys;

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Options.Values;

        public FrameOptions()
        {
            Options = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public FrameOptions(IEnumerable<KeyValuePair<string, object>> options)
        {
            Options = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            Options.AddRange(options);
        }

        public FrameOptions(FrameOptions baseFrameOptions, IEnumerable<KeyValuePair<string, object>> options)
        {
            Options = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            Options.AddRange(baseFrameOptions.Options); //note this does not deep copy!
            Options.AddRangeReplaceExisting(options); //and neither does this!
        }
    }

    public class FrameScripts
    {
        public string BeforePresent { get; private set; }
        public string OnPresent { get; private set; }
        public string OnChoice { get; private set; }
        public string OnUnpresent { get; private set; }

        public FrameScripts(FrameScripts baseScripts)
        {
            if(baseScripts != null)
            {
                BeforePresent = baseScripts.BeforePresent;
                OnPresent = baseScripts.OnPresent;
                OnChoice = baseScripts.OnChoice;
                OnUnpresent = baseScripts.OnUnpresent;
            }
        }

        public FrameScripts()
        {

        }
    }

    public class Frame
    {
        public readonly string Background;
        public readonly string Image;        
        public readonly string Next;
        public readonly string Music;
        public readonly string NameText;
        public readonly string Text;
        public readonly string NextText;
        public readonly string CameraDirection;
        public readonly FrameImagePosition ImagePosition;
        public readonly ConditionNode[] NextConditional;
        public readonly MicroscriptNode[] NextMicroscript;

        public readonly FrameOptions Options;
        public readonly FrameScripts Scripts;

        public readonly IReadOnlyDictionary<string, object> ExtraData;

        public readonly DialogueScene ParentScene;
        public readonly Frame BaseFrame;
        public readonly JToken RawData;

        public Frame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDirection, FrameImagePosition imagePosition, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript, FrameOptions options, FrameScripts scripts, DialogueScene parentScene, Frame baseFrame, JToken rawData, IReadOnlyDictionary<string, object> extraData)
        {
            Background = background;
            Image = image;
            Next = next;
            Music = music;
            NameText = nameText;
            Text = text;
            NextText = nextText;
            CameraDirection = cameraDirection;
            ImagePosition = imagePosition;

            //I'm not sure why we clone these but I'm not fixing it in Dialogue 1.5
            if (nextConditional != null && nextConditional.Length > 0)
                NextConditional = (ConditionNode[])nextConditional.Clone();

            if (nextMicroscript != null && nextMicroscript.Length > 0)
                NextMicroscript = (MicroscriptNode[])nextMicroscript.Clone();

            Options = options;
            Scripts = scripts;

            ParentScene = parentScene;
            BaseFrame = baseFrame;
            RawData = rawData;

            ExtraData = extraData ?? new Dictionary<string, object>();
        }

        public string EvaluateConditional(bool handleExceptions)
        {
            for(int i = NextConditional.Length-1; i >= 0; i--)
            {
                var nc = NextConditional[i];
                try
                {
                    if (nc.Evaluate())
                        return nc.Next;
                }
                catch(Exception e)
                {
                    if (!handleExceptions)
                        throw;

                    Debug.LogError($"Failed to evaluate conditional ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
                
            }
            return null;
        }

        public void EvaluateMicroscript(bool handleExceptions)
        {
            if (NextMicroscript == null || NextMicroscript.Length < 1)
                return;

            foreach (MicroscriptNode mn in NextMicroscript)
            {                
                try
                {
                    mn.Execute();
                }
                catch (Exception e)
                {
                    if (!handleExceptions)
                        throw;

                    Debug.LogError($"Failed to evaluate conditional ({e.GetType().Name})");
                    if(ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }
        }
    }

    public class BlankFrame : Frame
    {
        public BlankFrame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDir, FrameImagePosition imagePosition, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript, FrameOptions options, FrameScripts scripts, DialogueScene parentScene, Frame baseFrame, JToken rawData, IReadOnlyDictionary<string, object> extraData)
            : base(background, image, next, music, nameText, text, nextText, cameraDir, imagePosition, nextConditional, nextMicroscript, options, scripts, parentScene, baseFrame, rawData, extraData)
        {

        }
    }

    public class TextFrame : Frame
    {
        public readonly bool AllowSkip;
        public readonly float TimeToShow;
        public readonly bool UseTimer;

        public TextFrame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDir, FrameImagePosition imagePosition, bool allowSkip, float timeToShow, bool useTimer, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript, FrameOptions options, FrameScripts scripts, DialogueScene parentScene, Frame baseFrame, JToken rawData, IReadOnlyDictionary<string, object> extraData)
            : base(background, image, next, music, nameText, text, nextText, cameraDir, imagePosition, nextConditional, nextMicroscript, options, scripts, parentScene, baseFrame, rawData, extraData)
        {
            AllowSkip = allowSkip;
            TimeToShow = timeToShow;
            UseTimer = useTimer;
        }
    }

    public class ImageFrame : Frame
    {
        public readonly bool AllowSkip;
        public readonly bool HideSkip;
        public readonly float TimeToShow;
        public readonly bool UseTimer;

        public ImageFrame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDir, FrameImagePosition imagePosition, bool allowSkip, bool hideSkip, float timeToShow, bool useTimer, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript, FrameOptions options, FrameScripts scripts, DialogueScene parentScene, Frame baseFrame, JToken rawData, IReadOnlyDictionary<string, object> extraData)
            : base(background, image, next, music, nameText, text, nextText, cameraDir, imagePosition, nextConditional, nextMicroscript, options, scripts, parentScene, baseFrame, rawData, extraData)
        {
            AllowSkip = allowSkip;
            HideSkip = hideSkip;
            TimeToShow = timeToShow;
            UseTimer = useTimer;
        }
    }

    public class ChoiceFrame: Frame
    {
        public readonly ChoiceNode[] Choices;

        public ChoiceFrame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDir, FrameImagePosition imagePosition, ChoiceNode[] choices, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript, FrameOptions options, FrameScripts scripts, DialogueScene parentScene, Frame baseFrame, JToken rawData, IReadOnlyDictionary<string, object> extraData)
            : base(background, image, next, music, nameText, text, nextText, cameraDir, imagePosition, nextConditional, nextMicroscript, options, scripts, parentScene, baseFrame, rawData, extraData)
        {
            Choices = (ChoiceNode[])choices.Clone();
        }
    }



    public class ConditionNode
    {
        public readonly string Next;
        public readonly Conditional[] Conditions;

        public ConditionNode(string next, Conditional[] conditions)
        {
            Next = next;
            Conditions = conditions;
        }

        public bool Evaluate()
        {
            if (Conditions == null || Conditions.Length == 0) //odd, but in the spec
                return true;

            foreach(Conditional c in Conditions)
            {
                if (!c.Evaluate())
                    return false;
            }
            return true;
        }
    }

    public class DialogueTrace
    {
        [JsonProperty]
        public List<DialogueTraceNode> Nodes { get; private set; } = new List<DialogueTraceNode>();
    }

    public struct DialogueTraceNode
    {
        public string Path;
        public int? Choice;
        public string Speaker;
        public string Text;
        public bool Ignored;
    }

}