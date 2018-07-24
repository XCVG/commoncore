﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Messaging
{

    public abstract class QdmsMessage
    {
        public QdmsMessageInterface Sender { get; private set; }

        internal void SetSender(QdmsMessageInterface sender)
        {
            Sender = sender;
        }
    }

    public class QdmsKeyValueMessage : QdmsFlagMessage
    {
        private readonly Dictionary<string, object> _Dictionary;

        public QdmsKeyValueMessage(Dictionary<string, object> values, string flag): base(flag)
        {
            _Dictionary = new Dictionary<string, object>();

            foreach(var p in values)
            {
                _Dictionary.Add(p.Key, p.Value);
            }
        }

        public T GetValue<T>(string key)
        {
            if (_Dictionary.ContainsKey(key))
                return (T)_Dictionary[key];
            return default(T);
        }

        public Type GetType(string key)
        {
            if (_Dictionary.ContainsKey(key))
                return _Dictionary[key].GetType();

            return null;
        }
    }

    public class QdmsFlagMessage : QdmsMessage
    {
        public readonly string Flag;

        public QdmsFlagMessage(string flag)
        {
            Flag = flag;
        }
    }

    public class HUDPushMessage : QdmsMessage
    {
        public readonly string Contents;

        public HUDPushMessage(string contents) : base()
        {
            Contents = contents;
        }
    }

}
