using CommonCore.Config;
using CommonCore.DelayedEvents;
using CommonCore.Scripting;
using CommonCore.State;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace CommonCore.RpgGame.State
{
    public enum ConditionType
    {
        Unknown, Flag, NoFlag, Item, Variable, Affinity, Quest, ActorValue, Exec //Eval is obviously not supported, we provide Exec instead
    }

    public enum ConditionOption
    {
        Unknown, Consume, Greater, Less, Equal, GreaterEqual, LessEqual, Started, Finished
    }    

    public class Conditional
    {
        public readonly ConditionType Type;
        public readonly string Target;
        public readonly ConditionOption? Option;
        public readonly IComparable OptionValue;

        public readonly JObject RawData;

        private ConditionalResolver Resolver;

        public Conditional(ConditionType type, string target, ConditionOption? option, IComparable optionValue, JObject rawData)
        {
            Type = type;
            Target = target;
            Option = option;
            OptionValue = optionValue;
            RawData = rawData;
        }

        public bool Evaluate()
        {
            switch (Type)
            {
                case ConditionType.Flag:
                    return GameState.Instance.CampaignState.HasFlag(Target);
                case ConditionType.NoFlag:
                    return !GameState.Instance.CampaignState.HasFlag(Target);
                case ConditionType.Variable:
                    if (GameParams.ForceCampaignVarNumericEvaluation)
                    {
                        if (GameState.Instance.CampaignState.HasVar(Target))
                            return EvaluateValueWithOption(GameState.Instance.CampaignState.GetVar<decimal>(Target)); //should work I hope
                        else return false;
                    }
                    else
                    {
                        if(GameState.Instance.CampaignState.HasVar(Target))
                            return EvaluateValueWithOption(GameState.Instance.CampaignState.GetVar<IComparable>(Target)); //fingers crossed
                        else return false;
                    }
                case ConditionType.Quest:
                    if (GameState.Instance.CampaignState.HasQuest(Target))
                        return EvaluateValueWithOption(GameState.Instance.CampaignState.GetQuestStage(Target));
                    else return false;
                case ConditionType.Exec:
                    try
                    {
                        object[] args = (Option == null) ? new object[] { } : new object[] { OptionValue };
                        return (bool)ScriptingModule.CallForResult(Target, new ScriptExecutionContext() { Caller = this }, args);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"Conditional.Evaluate failed: Script execution failed ({e.GetType().Name}:{e.Message})");
                        if(ConfigState.Instance.UseVerboseLogging)
                            Debug.LogException(e);
                        return false;
                    }
                default:
                    BindResolver();
                    if (Resolver != null)
                        return Resolver.Resolve();
                    throw new NotSupportedException("This conditional type is not natively supported and no resolver could be found");
            }
        }

        private bool EvaluateValueWithOption(IComparable value)
        {
            //"started" and "finished" will work with anything
            //technically out of spec but should be fine
            //probably the only instance that will work here but not with Katana

            //I expect this to completely shit the bed when types are different
            //yeah, it breaks *horribly*

            IComparable value0 = value; //target value
            IComparable value1 = OptionValue; //our value (comparison value)

            //handle this up here so we can coerce properly
            if (Option.Value == ConditionOption.Started || Option.Value == ConditionOption.Finished)
                value1 = 0;

            if (value0 == null || value1 == null)
                return false; //assume null = false

            if(System.Type.GetTypeCode(value0.GetType()) == TypeCode.UInt64 && System.Type.GetTypeCode(value1.GetType()) == TypeCode.UInt64)
            {
                //if both are ulong, compare as ulong
                value0 = Convert.ToUInt64(value0);
                value1 = Convert.ToUInt64(value1);
            }
            else if(TypeUtils.IsIntegerType(value0.GetType()) && TypeUtils.IsIntegerType(value1.GetType()) && System.Type.GetTypeCode(value0.GetType()) != TypeCode.UInt64 && System.Type.GetTypeCode(value1.GetType()) != TypeCode.UInt64)
            {
                //if both are integral and not ulong, compare as long
                value0 = Convert.ToInt64(value0);
                value1 = Convert.ToInt64(value1);
            }
            else if(TypeUtils.IsNumericType(value0.GetType()) && TypeUtils.IsNumericType(value1.GetType()))
            {
                //if both are numeric, compare as decimal
                value0 = Convert.ToDecimal(value0);
                value1 = Convert.ToDecimal(value1);
            }
            else if(value0.GetType() == typeof(string) || value1.GetType() == typeof(string))
            {
                //if one is a string, compare as string
                value0 = value0.ToString();
                value1 = value1.ToString();
            }
            //otherwise yolo it

            switch (Option.Value)
            {
                case ConditionOption.Greater:
                    return value0.CompareTo(value1) > 0;
                case ConditionOption.Less:
                    return value0.CompareTo(value1) < 0;
                case ConditionOption.Equal:
                    return value0.CompareTo(value1) == 0;
                case ConditionOption.GreaterEqual:
                    return value0.CompareTo(value1) >= 0;
                case ConditionOption.LessEqual:
                    return value0.CompareTo(value1) <= 0;
                case ConditionOption.Started:
                    return value0.CompareTo(value1) > 0;
                case ConditionOption.Finished:
                    return value0.CompareTo(value1) < 0;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Binds a ConditionalResolver (if an appropriate one exists) to this Conditional if one is not already bound
        /// </summary>
        public void BindResolver()
        {
            if(Resolver == null)
            {
                Resolver = ConditionalModule.Instance.GetResolverFor(this);
            }
        }

        /// <summary>
        /// Parses a condition from a JObject
        /// </summary>
        public static Conditional Parse(JObject jt)
        {
            //types
            ConditionType type = ConditionType.Unknown;
            string target = null;
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
            else if (jt["av"] != null)
            {
                type = ConditionType.ActorValue;
                target = jt["av"].Value<string>();
            }
            else if (jt["actorvalue"] != null)
            {
                type = ConditionType.ActorValue;
                target = jt["actorvalue"].Value<string>();
            }
            else if (jt["exec"] != null)
            {
                type = ConditionType.Exec;
                target = jt["exec"].Value<string>();
            }
            else
            {
                Debug.LogWarning($"[{nameof(Conditional)}.{nameof(Parse)}] Unsupported or unrecognized condition type");
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
            else if (type == ConditionType.Exec)
            {
                if (jt["arg"] != null)
                {
                    option = 0; //we just need it to be non-null
                    optionValue = (IComparable)TypeUtils.StringToNumericAuto(jt["arg"].Value<string>());
                }
            }
            else
            {
                if (jt["greater"] != null)
                {
                    option = ConditionOption.Greater;
                    optionValue = (IComparable)TypeUtils.StringToNumericAuto(jt["greater"].Value<string>());
                }
                else if (jt["less"] != null)
                {
                    option = ConditionOption.Less;
                    optionValue = (IComparable)TypeUtils.StringToNumericAuto(jt["less"].Value<string>());
                }
                else if (jt["equal"] != null)
                {
                    option = ConditionOption.Equal;
                    optionValue = (IComparable)TypeUtils.StringToNumericAuto(jt["equal"].Value<string>());
                }
                else if (jt["greaterEqual"] != null)
                {
                    option = ConditionOption.GreaterEqual;
                    optionValue = (IComparable)TypeUtils.StringToNumericAuto(jt["greaterEqual"].Value<string>());
                }
                else if (jt["lessEqual"] != null)
                {
                    option = ConditionOption.LessEqual;
                    optionValue = (IComparable)TypeUtils.StringToNumericAuto(jt["lessEqual"].Value<string>());
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

            return new Conditional(type, target, option, optionValue, jt);
        }
    }

    public enum MicroscriptType
    {
        Unknown, Flag, Item, Variable, Affinity, Quest, ActorValue, Exec, MapMarker //eval is again not supported
    }

    public enum MicroscriptAction
    {
        Unknown, Set, Toggle, Add, Give, Take, Start, Finish
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

        public readonly JObject RawData;

        private MicroscriptResolver Resolver;

        public MicroscriptNode(MicroscriptType type, string target, MicroscriptAction action, object value,
            DelayTimeType delayType, double delayTime, bool delayAbsolute, JObject rawData)
        {
            Type = type;
            Target = target;
            Action = action;
            Value = value;
            DelayType = delayType;
            DelayTime = delayTime;
            DelayAbsolute = delayAbsolute;
            RawData = rawData;
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
            MicroscriptNode dupNode = new MicroscriptNode(Type, Target, Action, Value, DelayTimeType.None, default, default, (JObject)RawData.DeepClone());
            //dupNode.Resolver = Resolver; //preserve early-bound resolver if exists
            //we could preserve resolver here, but resolver will be rebound on serialize/deserialize anyway
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
                case MicroscriptType.Variable:
                    if (Action == MicroscriptAction.Set || (Action == MicroscriptAction.Add && !GameState.Instance.CampaignState.HasVar(Target)))
                    {
                        if (GameParams.UseCampaignVarSimulatedLooseTyping)
                        {
                            GameState.Instance.CampaignState.SetVar<object>(Target, Value);
                        }
                        else
                        {
                            GameState.Instance.CampaignState.SetVarEx(Target, Value);
                        }
                    }
                    else if (Action == MicroscriptAction.Add)
                    {
                        if(GameParams.UseCampaignVarSimulatedLooseTyping)
                        {

                            //use dynamic operator +
                            //dynamic oldVal = GameState.Instance.CampaignState.GetVar<object>(Target);
                            //dynamic newVal = oldVal + (dynamic)Value;

                            object newVal = TypeUtils.AddValuesDynamic(GameState.Instance.CampaignState.GetVar<object>(Target), Value, false);
                            GameState.Instance.CampaignState.SetVar<object>(Target, newVal);

                        }
                        else
                        {

                            //use target value's operator +
                            //dynamic oldVal = GameState.Instance.CampaignState.GetVar<object>(Target);
                            //dynamic addedVal = TypeUtils.CoerceValue(Value, oldVal.GetType());
                            //dynamic newVal = oldVal + addedVal;

                            object newVal = TypeUtils.AddValuesDynamic(GameState.Instance.CampaignState.GetVar<object>(Target), Value, true);
                            GameState.Instance.CampaignState.SetVar<object>(Target, newVal);
                        }

                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
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
                        if (Value != null)
                            GameState.Instance.CampaignState.StartQuest(Target, Convert.ToInt32(Value));
                        else
                            GameState.Instance.CampaignState.StartQuest(Target);
                    }
                    else if (Action == MicroscriptAction.Finish)
                    {
                        if (GameState.Instance.CampaignState.IsQuestStarted(Target))
                            GameState.Instance.CampaignState.EndQuest(Target, Convert.ToInt32(Value));
                    }
                    break;
                case MicroscriptType.Exec:
                    Scripting.ScriptingModule.Call(Target, new Scripting.ScriptExecutionContext() { Caller = this }, Value);
                    break;
                default:
                    BindResolver();
                    if (Resolver != null)
                    {
                        Resolver.Resolve();
                        break;
                    }
                    throw new NotSupportedException("This microscript type is not natively supported and no resolver could be found");
            }
        }

        /// <summary>
        /// Binds a MicroscriptResolver (if an appropriate one exists) to this MicroscriptNode if one is not already bound
        /// </summary>
        public void BindResolver()
        {
            if (Resolver == null)
            {
                Resolver = ConditionalModule.Instance.GetResolverFor(this);
            }
        }

        /// <summary>
        /// Parses a microscript from a JObject
        /// </summary>
        public static MicroscriptNode Parse(JObject jt)
        {
            //parse type and target
            MicroscriptType type = MicroscriptType.Unknown;
            string target = null;
            MicroscriptAction action;
            object value = 0;

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
            else if (jt["av"] != null)
            {
                type = MicroscriptType.ActorValue;
                target = jt["av"].Value<string>();
            }
            else if (jt["actorvalue"] != null)
            {
                type = MicroscriptType.ActorValue;
                target = jt["actorvalue"].Value<string>();
            }
            else if (jt["exec"] != null)
            {
                type = MicroscriptType.Exec;
                target = jt["exec"].Value<string>();
                if (jt["arg"] != null)
                {
                    value = TypeUtils.StringToNumericAuto(jt["arg"].Value<string>());
                }
            }
            else
            {
                Debug.LogWarning($"[{nameof(MicroscriptNode)}.{nameof(Parse)}] Unsupported or unrecognized microscript type");
            }

            //parse action/value            
            if (jt["set"] != null)
            {
                action = MicroscriptAction.Set;
                if (type == MicroscriptType.Flag) //parse as boolean
                    value = Convert.ToInt32(jt["set"].Value<bool>());
                else //otherwise parse as number
                    value = TypeUtils.StringToNumericAuto(jt["set"].Value<string>());
            }
            else if (jt["toggle"] != null)
            {
                action = MicroscriptAction.Toggle;
            }
            else if (jt["add"] != null)
            {
                action = MicroscriptAction.Add;
                value = TypeUtils.StringToNumericAuto(jt["add"].Value<string>());
            }
            else if (jt["give"] != null)
            {
                action = MicroscriptAction.Give;
                value = TypeUtils.StringToNumericAuto(jt["give"].Value<string>());
            }
            else if (jt["take"] != null)
            {
                action = MicroscriptAction.Take;
                value = TypeUtils.StringToNumericAuto(jt["take"].Value<string>());
            }
            else if (jt["start"] != null)
            {
                action = MicroscriptAction.Start;
                value = TypeUtils.StringToNumericAuto(jt["start"].Value<string>());
            }
            else if (jt["finish"] != null)
            {
                action = MicroscriptAction.Finish;
                value = TypeUtils.StringToNumericAuto(jt["finish"].Value<string>());
            }
            else
            {
                if (type != MicroscriptType.Exec)
                    Debug.LogWarning($"[{nameof(MicroscriptNode)}.{nameof(Parse)}] Unacceptable or unrecognized action for microscript");

                action = MicroscriptAction.Unknown;
            }

            //parse delay, if applicable
            DelayTimeType delayType = DelayTimeType.None;
            double delayTime = default(double);
            bool delayAbsolute = false;
            if (jt["delay"] != null)
            {
                delayType = DelayTimeType.Game;
                delayTime = double.Parse(jt["delay"].Value<string>());
                if (jt["delayType"] != null)
                {
                    string delayTypeString = jt["delayType"].Value<string>();
                    switch (delayTypeString)
                    {
                        case "real":
                            delayType = DelayTimeType.Real;
                            break;
                        case "world":
                            delayType = DelayTimeType.World;
                            break;
                        case "game":
                            delayType = DelayTimeType.Game;
                            break;
                    }
                }
                if (jt["delayAbsolute"] != null)
                {
                    delayAbsolute = jt["delayAbsolute"].Value<bool>();
                }
            }

            return new MicroscriptNode(type, target, action, value, delayType, delayTime, delayAbsolute, jt);
        }
    }
}