﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.Messaging
{
    /// <summary>
    /// When a message is to be delivered
    /// </summary>
    public enum QdmsMessageDelivery
    {
        Anytime, Immediate, EndOfFrame
    }

    /// <summary>
    /// Base type for QDMS messages
    /// </summary>
    public abstract class QdmsMessage
    {
        public QdmsMessageDelivery Delivery { get; internal set; } = QdmsMessageDelivery.Anytime;
        public object Sender { get; internal set; }
    }

    /// <summary>
    /// Basic message carrying a string flag; inherited types may have fixed flags
    /// </summary>
    public class QdmsFlagMessage : QdmsMessage
    {
        public readonly string Flag;

        public QdmsFlagMessage(string flag)
        {
            Flag = flag;
        }
    }

    /// <summary>
    /// Basic message containing data in key/value pairs, as well as a string flag
    /// </summary>
    public class QdmsKeyValueMessage : QdmsFlagMessage
    {
        private readonly IDictionary<string, object> _Dictionary = new Dictionary<string, object>();        

        public QdmsKeyValueMessage(string flag, IEnumerable<KeyValuePair<string, object>> values) : base(flag)
        {
            _Dictionary.AddRange(values);
        }        

        //shorthand constructor for single key/value
        public QdmsKeyValueMessage(string flag, string key, object value) : base(flag)
        {
            _Dictionary.Add(key, value);
        }

        [Obsolete] //the f'ed-up argument order of this is the worst thing ever
        public QdmsKeyValueMessage(Dictionary<string, object> values, string flag) : this(flag, values)
        {

        }

        public bool ContainsKey(string key) => _Dictionary.ContainsKey(key);

        [Obsolete("Use ContainsKey instead")]
        public bool HasValue(string key) => ContainsKey(key);

        public bool ContainsKeyForType<T>(string key)
        {
            object rawValue = null;
            bool exists = _Dictionary.TryGetValue(key, out rawValue);
            return (exists && rawValue is T);
        }

        [Obsolete("Use ContainsKeyForType instead")]
        public bool HasValue<T>(string key) => ContainsKeyForType<T>(key);

        public T GetItemOfType<T>(string key)
        {
            if (_Dictionary.TryGetValue(key, out object rawValue) && typeof(T).IsAssignableFrom(rawValue.GetType())) //still not quite right because implicit operators, but close enough for golf
                return (T)rawValue;
            throw new KeyNotFoundException($"An item with key {key} and type {typeof(T).Name} could not be found in this {nameof(QdmsKeyValueMessage)}");
        }

        [Obsolete("Use GetItemOfType instead (note slightly different semantics)")]
        public T GetValue<T>(string key)
        {
            if (_Dictionary.ContainsKey(key))
                return (T)_Dictionary[key];
            return default(T);
        }

        public object this[string i] => _Dictionary[i];

        [Obsolete("Use EnumerateEntries instead")]
        public IEnumerable<KeyValuePair<string, object>> EnumerateValues() => EnumerateEntries();

        public IEnumerable<KeyValuePair<string, object>> EnumerateEntries() => _Dictionary.ToArray();
    }



 

}
