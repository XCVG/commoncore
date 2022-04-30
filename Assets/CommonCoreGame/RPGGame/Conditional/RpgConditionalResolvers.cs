using CommonCore.Config;
using CommonCore.DelayedEvents;
using CommonCore.Scripting;
using CommonCore.State;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

namespace CommonCore.RpgGame.State
{

    /// <summary>
    /// Resolver for RPG conditionals
    /// </summary>
    /// <remarks>
    /// <para>Handles ActorValue and Item</para>
    /// </remarks>
    public class RpgConditionalResolver : ConditionalResolver
    {
        public RpgConditionalResolver(Conditional conditional) : base(conditional)
        {
            if (Conditional.Type == ConditionType.ActorValue || Conditional.Type == ConditionType.Item)
                CanResolve = true;
        }

        public override bool Resolve()
        {
            if (Conditional.Type == ConditionType.ActorValue)
            {
                try
                {
                    //we assume ActorValue is numeric here, which is probably safe
                    decimal av = GameState.Instance.PlayerRpgState.GetAV<decimal>(Conditional.Target);
                    return EvaluateValueWithOption(av);
                }
                catch (KeyNotFoundException)
                {
                    Debug.LogError($"{nameof(RpgConditionalResolver)}.Resolve failed: couldn't find ActorValue '{Conditional?.Target}'");
                    return false;
                }
            }
            else if(Conditional.Type == ConditionType.Item)
            {
                if (Conditional.Option == ConditionOption.Consume)
                    Debug.LogWarning($"{nameof(RpgConditionalResolver)}.Resolve warning: Consume item is not supported in CommonCore");

                int qty = GameState.Instance.PlayerRpgState.Inventory.CountItem(Conditional.Target);
                if (Conditional.Option == ConditionOption.Unknown || Conditional.Option == ConditionOption.Consume || Conditional.OptionValue == null)
                {                    
                    if (qty < 1)
                        return false;
                    else return true;
                }
                else
                {
                    return EvaluateValueWithOptionAsInt(qty);
                }
                
            }

            throw new NotSupportedException();                
        }

        private bool EvaluateValueWithOption(decimal value)
        {
            IComparable value0 = value; //target value
            IComparable value1 = Conditional.OptionValue; //our value (comparison value)

            if (value0 == null || value1 == null)
                return false; //assume null = false

            value1 = Convert.ToDecimal(value1);

            switch (Conditional.Option.Value)
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

        private bool EvaluateValueWithOptionAsInt(int value)
        {
            IComparable value0 = value; //target value
            IComparable value1 = Conditional.OptionValue; //our value (comparison value)

            if (value0 == null || value1 == null)
                return false; //assume null = false

            value0 = Convert.ToInt32(value0);
            value1 = Convert.ToInt32(value1);

            switch (Conditional.Option.Value)
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
    }

    /// <summary>
    /// Resolver for RPG microscripts
    /// </summary>
    /// <remarks>
    /// <para>Handles ActorValue, Item, and MapMarker</para>
    /// </remarks>
    public class RpgMicroscriptResolver : MicroscriptResolver
    {
        public RpgMicroscriptResolver(MicroscriptNode microscript) : base(microscript)
        {
            if (Microscript.Type == MicroscriptType.ActorValue || Microscript.Type == MicroscriptType.Item || Microscript.Type == MicroscriptType.MapMarker)
                CanResolve = true;
        }

        public override void Resolve()
        {
            switch (Microscript.Type)
            {
                case MicroscriptType.Item:
                    if (Microscript.Action == MicroscriptAction.Give)
                    {
                        GameState.Instance.PlayerRpgState.Inventory.AddItem(Microscript.Target, Convert.ToInt32(Microscript.Value));
                    }
                    else if (Microscript.Action == MicroscriptAction.Take)
                    {
                        GameState.Instance.PlayerRpgState.Inventory.UseItem(Microscript.Target, Convert.ToInt32(Microscript.Value));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.ActorValue:
                    if (Microscript.Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.PlayerRpgState.SetAV(Microscript.Target, Microscript.Value);
                    }
                    else if (Microscript.Action == MicroscriptAction.Add)
                    {
                        GameState.Instance.PlayerRpgState.ModAV(Microscript.Target, Microscript.Value);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.MapMarker:
                    if (Microscript.Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.MapMarkers[Microscript.Target] = (MapMarkerState)Enum.Parse(typeof(MapMarkerState), Microscript.Value.ToString(), true);
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