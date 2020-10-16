using System;
using System.Collections.Generic;
using CommonCore.Config;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.State;
using CommonCore.State;

namespace CommonCore.RpgGame.Dialogue
{
    public class DialogueScene
    {
        public Dictionary<string, Frame> Frames { get; private set; }
        public string Default;
        public string Music;

        public DialogueScene(Dictionary<string, Frame> frames, string defaultFrame, string music)
        {
            Frames = frames;
            Default = defaultFrame;
            Music = music;
        }
    }

    public enum FrameType
    {
        ChoiceFrame, TextFrame
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

        public ChoiceNode(string next, string text)
        {
            Text = text;
            Next = next;
        }

        public ChoiceNode(string next, string text, Conditional showCondition, Conditional hideCondition, MicroscriptNode[] nextMicroscript, ConditionNode[] nextConditional, SkillCheckNode skillCheck)
            : this(next, text)
        {
            ShowCondition = showCondition;
            HideCondition = hideCondition;
            NextMicroscript = nextMicroscript;
            NextConditional = nextConditional;
            SkillCheck = skillCheck;
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
                    SkillType skill = (SkillType)Enum.Parse(typeof(SkillType), Target, true);
                    return player.DerivedStats.Skills[skill] * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerSkill;
                case SkillCheckTarget.Stat:
                    StatType stat = (StatType)Enum.Parse(typeof(StatType), Target, true);
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
        Center, Fill, Character, Battler
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

        public Frame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDirection, FrameImagePosition imagePosition, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
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

            if (nextConditional != null && nextConditional.Length > 0)
                NextConditional = (ConditionNode[])nextConditional.Clone();

            if (nextMicroscript != null && nextMicroscript.Length > 0)
                NextMicroscript = (MicroscriptNode[])nextMicroscript.Clone();
        }

        public string EvaluateConditional()
        {
            for(int i = NextConditional.Length-1; i >= 0; i--)
            {
                var nc = NextConditional[i];
                if (nc.Evaluate())
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
                mn.Execute();
            }
        }
    }

    public class BlankFrame : Frame
    {
        public BlankFrame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDir, FrameImagePosition imagePosition, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
            : base(background, image, next, music, nameText, text, nextText, cameraDir, imagePosition, nextConditional, nextMicroscript)
        {

        }
    }

    public class TextFrame : Frame
    {
        public TextFrame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDir, FrameImagePosition imagePosition, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
            : base(background, image, next, music, nameText, text, nextText, cameraDir, imagePosition, nextConditional, nextMicroscript)
        {
            
        }
    }

    public class ChoiceFrame: Frame
    {
        public readonly ChoiceNode[] Choices;

        public ChoiceFrame(string background, string image, string next, string music, string nameText, string text, string nextText, string cameraDir, FrameImagePosition imagePosition, ChoiceNode[] choices, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
            : base(background, image, next, music, nameText, text, nextText, cameraDir, imagePosition, nextConditional, nextMicroscript)
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

}