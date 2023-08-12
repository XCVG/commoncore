using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.State
{
    [Serializable]
    public struct EditorConditional
    {
        public ConditionType Type;
        public string Target;
        public ConditionOption Option;
        public string OptionValue;

        [Tooltip("If set, all other options will be ignored")]
        public string CustomOverride;

        public Conditional Parse()
        {
            if (!string.IsNullOrEmpty(CustomOverride))
            {
                return Conditional.Parse((JObject)CoreUtils.ReadJson(CustomOverride));
            }
            else
            {
                if (Type == ConditionType.Unknown)
                    throw new ArgumentException("EditorConditional has Unknown type set");
            }

            ConditionOption? opt = null;
            if (Type == ConditionType.Item || Type == ConditionType.Quest || Type == ConditionType.ActorValue)
                opt = Option;

            IComparable val = (IComparable)TypeUtils.StringToNumericAuto(OptionValue);

            return new Conditional(Type, Target, opt, val, new JObject());
        }
    }
}