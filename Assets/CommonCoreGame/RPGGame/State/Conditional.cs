using CommonCore.DelayedEvents;
using CommonCore.Scripting;
using CommonCore.State;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.State
{
    public enum ConditionType
    {
        Flag, NoFlag, Item, Variable, Affinity, Quest, ActorValue, Exec //Eval is obviously not supported, we provide Exec instead
    }

    public enum ConditionOption
    {
        Consume, Greater, Less, Equal, GreaterEqual, LessEqual, Started, Finished
    }

    [Serializable]
    public struct EditorConditional
    {
        public ConditionType Type;
        public string Target;
        public ConditionOption Option;
        public string OptionValue;

        public Conditional Parse()
        {
            ConditionOption? opt = null;
            if (Type == ConditionType.Item || Type == ConditionType.Quest || Type == ConditionType.ActorValue)
                opt = Option;

            IComparable val = (IComparable)TypeUtils.StringToNumericAuto(OptionValue);

            return new Conditional(Type, Target, opt, val);
        }
    }

    public class Conditional
    {
        public readonly ConditionType Type;
        public readonly string Target;
        public readonly ConditionOption? Option;
        public readonly IComparable OptionValue;

        public Conditional(ConditionType type, string target, ConditionOption? option, IComparable optionValue)
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
                    catch (KeyNotFoundException)
                    {
                        Debug.LogError($"Conditional.Evaluate failed: couldn't find ActorValue '{Target}'");
                        return false;
                    }
                case ConditionType.Exec:
                    try
                    {
                        object[] args = (Option == null) ? new object[] { } : new object[] { OptionValue };
                        return (bool)ScriptingModule.CallForResult(Target, new ScriptExecutionContext() { Caller = this }, args);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"Conditional.Evaluate failed: Script execution failed {e}");
                        return false;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private bool EvaluateValueWithOption(IComparable value)
        {
            //technically out of spec but should be fine
            //probably the only instance that will work here but not with Katana
            switch (Option.Value)
            {
                case ConditionOption.Greater:
                    return value.CompareTo(OptionValue) > 0;
                case ConditionOption.Less:
                    return value.CompareTo(OptionValue) < 0;
                case ConditionOption.Equal:
                    return value.CompareTo(OptionValue) == 0;
                case ConditionOption.GreaterEqual:
                    return value.CompareTo(OptionValue) >= 0;
                case ConditionOption.LessEqual:
                    return value.CompareTo(OptionValue) <= 0;
                case ConditionOption.Started:
                    return value.CompareTo(0) > 0;
                case ConditionOption.Finished:
                    return value.CompareTo(0) < 0;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public enum MicroscriptType
    {
        Flag, Item, Variable, Affinity, Quest, ActorValue, Exec, MapMarker //eval is again not supported
    }

    public enum MicroscriptAction
    {
        Set, Toggle, Add, Give, Take, Start, Finish
    }

    [Serializable]
    public struct EditorMicroscript
    {
        public MicroscriptType Type;
        public string Target;
        public MicroscriptAction Action;
        public string Value;
        public DelayTimeType DelayType;
        public float DelayTime;
        public bool DelayAbsolute;

        public MicroscriptNode Parse()
        {
            object val = TypeUtils.StringToNumericAuto(Value);
            return new MicroscriptNode(Type, Target, Action, val, DelayType, DelayTime, DelayAbsolute);
        }

    }

    public class MicroscriptNode //"directive" in Katana parlance
    {
        public readonly MicroscriptType Type;
        public readonly string Target;
        public readonly MicroscriptAction Action;
        public readonly object Value;
        public readonly DelayTimeType DelayType;
        public readonly double DelayTime;
        public readonly bool DelayAbsolute;

        public MicroscriptNode(MicroscriptType type, string target, MicroscriptAction action, object value,
            DelayTimeType delayType, double delayTime, bool delayAbsolute)
        {
            Type = type;
            Target = target;
            Action = action;
            Value = value;
            DelayType = delayType;
            DelayTime = delayTime;
            DelayAbsolute = delayAbsolute;
        }

        public void Execute()
        {
            if (DelayType != DelayTimeType.None)
                ExecuteDelayed();
            else
                ExecuteImmediate();

        }

        private void ExecuteDelayed()
        {
            MicroscriptNode dupNode = new MicroscriptNode(Type, Target, Action, Value, DelayTimeType.None, default, default);
            DelayedEventScheduler.ScheduleEvent(dupNode, DelayType, DelayTime, DelayAbsolute);
        }

        private void ExecuteImmediate()
        {
            switch (Type)
            {
                case MicroscriptType.Flag:
                    if (Action == MicroscriptAction.Toggle)
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
                    if (Action == MicroscriptAction.Give)
                    {
                        GameState.Instance.PlayerRpgState.Inventory.AddItem(Target, Convert.ToInt32(Value));
                    }
                    else if (Action == MicroscriptAction.Take)
                    {
                        GameState.Instance.PlayerRpgState.Inventory.UseItem(Target, Convert.ToInt32(Value));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Variable:
                    if (Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.CampaignState.SetVar(Target, Value.ToString());
                    }
                    else if (Action == MicroscriptAction.Add)
                    {
                        decimal oldVal = Convert.ToDecimal(GameState.Instance.CampaignState.GetVar(Target));
                        GameState.Instance.CampaignState.SetVar(Target, (oldVal + Convert.ToDecimal(Value)).ToString()); //this is probably unsafe
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Affinity:
                    throw new NotImplementedException();
                case MicroscriptType.Quest:
                    if (Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.CampaignState.SetQuestStage(Target, Convert.ToInt32(Value));
                    }
                    else if (Action == MicroscriptAction.Add)
                    {
                        GameState.Instance.CampaignState.SetQuestStage(Target, GameState.Instance.CampaignState.GetQuestStage(Target) + Convert.ToInt32(Value));
                    }
                    else if (Action == MicroscriptAction.Start)
                    {
                        GameState.Instance.CampaignState.StartQuest(Target);
                    }
                    else if (Action == MicroscriptAction.Finish)
                    {
                        if (GameState.Instance.CampaignState.IsQuestStarted(Target))
                            GameState.Instance.CampaignState.SetQuestStage(Target, Convert.ToInt32(Value));
                    }
                    break;
                case MicroscriptType.ActorValue:
                    if (Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.PlayerRpgState.SetAV(Target, Value);
                    }
                    else if (Action == MicroscriptAction.Add)
                    {
                        GameState.Instance.PlayerRpgState.ModAV(Target, Value);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.MapMarker:
                    if (Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.MapMarkers[Target] = (MapMarkerState)Enum.Parse(typeof(MapMarkerState), Value.ToString(), true);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Exec:
                    Scripting.ScriptingModule.Call(Target, new Scripting.ScriptExecutionContext() { Caller = this }, Value);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}