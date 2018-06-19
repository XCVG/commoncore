using System;
using System.Collections.Generic;
using CommonCore.State;

namespace CommonCore.Dialogue
{
    internal enum FrameType
    {
        ChoiceFrame, TextFrame
    }

    internal class ChoiceNode
    {
        public readonly string Text;
        public readonly string Next;
        public readonly MicroscriptNode[] NextMicroscript;
        public readonly ConditionNode[] NextConditional;

        public readonly Condition ShowCondition;
        public readonly Condition HideCondition;

        public ChoiceNode(string next, string text)
        {
            Text = text;
            Next = next;
        }

        public ChoiceNode(string next, string text, Condition showCondition, Condition hideCondition, MicroscriptNode[] nextMicroscript, ConditionNode[] nextConditional)
            : this(next, text)
        {
            ShowCondition = showCondition;
            HideCondition = hideCondition;
            NextMicroscript = nextMicroscript;
            NextConditional = nextConditional;
        }

        public string EvaluateConditional()
        {
            for (int i = NextConditional.Length - 1; i >= 0; i--)
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

    internal class Frame
    {
        public readonly string Background;
        public readonly string Image;
        public readonly string Next;
        public readonly string Music;
        public readonly string NameText;
        public readonly string Text;
        public readonly ConditionNode[] NextConditional;
        public readonly MicroscriptNode[] NextMicroscript;

        public Frame(string background, string image, string next, string music, string nameText, string text, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
        {
            Background = background;
            Image = image;
            Next = next;
            Music = music;
            NameText = nameText;
            Text = text;

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

    internal class TextFrame : Frame
    {
        public TextFrame(string background, string image, string next, string music, string nameText, string text, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
            : base(background, image, next, music, nameText, text, nextConditional, nextMicroscript)
        {
            
        }
    }

    internal class ChoiceFrame: Frame
    {
        public readonly ChoiceNode[] Choices;

        public ChoiceFrame(string background, string image, string next, string music, string nameText, string text, ChoiceNode[] choices, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
            : base(background, image, next, music, nameText, text, nextConditional, nextMicroscript)
        {
            Choices = (ChoiceNode[])choices.Clone();
        }
    }

    internal class ConditionNode
    {
        public readonly string Next;
        public readonly Condition[] Conditions;

        public ConditionNode(string next, Condition[] conditions)
        {
            Next = next;
            Conditions = conditions;
        }

        public bool Evaluate()
        {
            if (Conditions == null || Conditions.Length == 0) //odd, but in the spec
                return true;

            foreach(Condition c in Conditions)
            {
                if (!c.Evaluate())
                    return false;
            }
            return true;
        }
    }

    internal enum ConditionType
    {
        Flag, NoFlag, Item, Variable, Affinity, Quest, ActorValue //Eval is obviously not supported, might be able to provide Emit instead
    }

    internal enum ConditionOption
    {
        Consume, Greater, Less, Equal, GreaterEqual, LessEqual, Started, Finished
    }

    internal class Condition
    {
        public readonly ConditionType Type;
        public readonly string Target;
        public readonly ConditionOption? Option;
        public readonly int OptionValue; //this should NOT be an int... TODO experiment with IComparable and reflection

        public Condition(ConditionType type, string target, ConditionOption? option, int optionValue)
        {
            Type = type;
            Target = target;
            Option = option;
            OptionValue = optionValue;
        }

        public bool Evaluate()
        {
            switch (Type)
            {
                case ConditionType.Flag:
                    return GameState.Instance.CampaignState.HasFlag(Target);
                case ConditionType.NoFlag:
                    return !GameState.Instance.CampaignState.HasFlag(Target);
                case ConditionType.Item:
                    int qty = GameState.Instance.PlayerRpgState.Inventory.CountItem(Target);
                    if (qty < 1)
                        return false;
                    else return true;                    
                case ConditionType.Variable:
                    if (GameState.Instance.CampaignState.HasVar(Target))
                        return EvaluateValueWithOption(GameState.Instance.CampaignState.GetVar<int>(Target));
                    else return false;                    
                case ConditionType.Affinity:
                    throw new NotImplementedException(); //could be supported, but isn't yet
                case ConditionType.Quest:
                    if (GameState.Instance.CampaignState.HasQuest(Target))
                        return EvaluateValueWithOption(GameState.Instance.CampaignState.GetQuestStage(Target));
                    else return false;
                case ConditionType.ActorValue:
                    try
                    {
                        int av = GameState.Instance.PlayerRpgState.GetAV<int>(Target);
                        return EvaluateValueWithOption(av);
                    }
                    catch(KeyNotFoundException)
                    {
                        return false;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private bool EvaluateValueWithOption(int value)
        {
            //technically out of spec but should be fine
            //probably the only instance that will work here but not with Katana
            switch (Option.Value)
            {
                case ConditionOption.Greater:
                    return value > OptionValue;
                case ConditionOption.Less:
                    return value < OptionValue;
                case ConditionOption.Equal:
                    return value == OptionValue;
                case ConditionOption.GreaterEqual:
                    return value >= OptionValue;
                case ConditionOption.LessEqual:
                    return value <= OptionValue;
                case ConditionOption.Started:
                    return value > 0;
                case ConditionOption.Finished:
                    return value < 0;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    internal enum MicroscriptType
    {
        Flag, Item, Variable, Affinity, Quest, ActorValue //eval is again not supported
    }

    internal enum MicroscriptAction
    {
        Set, Toggle, Add, Give, Take, Start, Finish
    }

    internal class MicroscriptNode //"directive" in Katana parlance
    {
        public readonly MicroscriptType Type;
        public readonly string Target;
        public readonly MicroscriptAction Action;
        public readonly int Value;

        public MicroscriptNode(MicroscriptType type, string target, MicroscriptAction action, int value)
        {
            Type = type;
            Target = target;
            Action = action;
            Value = value;
        }

        public void Execute()
        {
            switch (Type)
            {
                case MicroscriptType.Flag:
                    if(Action == MicroscriptAction.Toggle)
                    {
                        GameState.Instance.CampaignState.ToggleFlag(Target);
                    }
                    else if (Action == MicroscriptAction.Set)
                    {
                        bool sv = Convert.ToBoolean(Value);
                        GameState.Instance.CampaignState.SetFlag(Target, sv);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Item:
                    if(Action == MicroscriptAction.Give)
                    {
                        GameState.Instance.PlayerRpgState.Inventory.AddItem(Target, Value);
                    }
                    else if(Action == MicroscriptAction.Take)
                    {
                        GameState.Instance.PlayerRpgState.Inventory.UseItem(Target, Value);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Variable:
                    if(Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.CampaignState.SetVar(Target, Value.ToString());
                    }
                    else if(Action == MicroscriptAction.Add)
                    {
                        int oldVal = GameState.Instance.CampaignState.GetVar<int>(Target);
                        GameState.Instance.CampaignState.SetVar(Target, (oldVal + Value).ToString());
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Affinity:
                    throw new NotImplementedException();
                case MicroscriptType.Quest:
                    if(Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.CampaignState.SetQuestStage(Target, Value);
                    }
                    else if(Action == MicroscriptAction.Add)
                    {
                        GameState.Instance.CampaignState.SetQuestStage(Target, GameState.Instance.CampaignState.GetQuestStage(Target)+Value);
                    }
                    else if(Action == MicroscriptAction.Start)
                    {
                        GameState.Instance.CampaignState.StartQuest(Target);
                    }
                    else if(Action == MicroscriptAction.Finish)
                    {
                        if(GameState.Instance.CampaignState.IsQuestStarted(Target))
                            GameState.Instance.CampaignState.SetQuestStage(Target, Value);
                    }
                    break;
                case MicroscriptType.ActorValue:
                    if (Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.PlayerRpgState.SetAV<int>(Target, Value);
                    }
                    else if (Action == MicroscriptAction.Add)
                    {
                        GameState.Instance.PlayerRpgState.ModAV<int>(Target, Value);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

        }
    }

}