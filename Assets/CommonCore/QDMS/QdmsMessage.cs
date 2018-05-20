using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Messaging
{

    public enum QdmsFlag
    {

    }

    public abstract class QdmsMessage
    {
        public QdmsMessageInterface Sender { get; private set; }

        internal void SetSender(QdmsMessageInterface sender)
        {
            Sender = sender;
        }
    }

    public class QdmsKeyValueMessage : QdmsMessage
    {
        private readonly Dictionary<string, object> _Dictionary;

        public QdmsKeyValueMessage(Dictionary<string, object> values)
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
        public readonly QdmsFlag Flag;

        public QdmsFlagMessage(QdmsFlag flag)
        {
            Flag = flag;
        }
    }

}
