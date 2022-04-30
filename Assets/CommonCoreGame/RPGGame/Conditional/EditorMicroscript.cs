using CommonCore.DelayedEvents;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.State
{
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

        [Tooltip("If set, all other options will be ignored")]
        public string CustomOverride;

        public MicroscriptNode Parse()
        {
            if (!string.IsNullOrEmpty(CustomOverride))
            {
                return MicroscriptNode.Parse((JObject)CoreUtils.ReadJson(CustomOverride));
            }

            object val = TypeUtils.StringToNumericAuto(Value);
            return new MicroscriptNode(Type, Target, Action, val, DelayType, DelayTime, DelayAbsolute, new JObject());
        }

    }
}