using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CommonCore.AddonSupport
{

    internal class ProxyFieldAttribute : Attribute
    {
        public string TargetField { get; set; }
        public bool BindPrivate { get; set; }
        public string[] LinkToTypes { get; set; }
    }

    internal class ProxyExtensionDataAttribute : Attribute
    {

    }

    [Serializable]
    public struct ProxyExtensionTuple : ITuple
    {
        public string Key;
        public string Value;

        object ITuple.this[int index]
        {
            get
            {
                if (index == 0)
                    return Key;
                else if (index == 1)
                    return Value;
                else
                    throw new IndexOutOfRangeException();
            }
        }

        int ITuple.Length => 2;
    }

    internal class InvokeStaticProxyOptions
    {
        public IList<Type> ParameterMatchTypes { get; set; } = null;
        public bool MatchParameterTypes { get; set; } = false;
        public bool CoerceInputTypes { get; set; } = false;
        public Type OutputType { get; set; } = null;
    }

    internal class ProxyUtils
    {

        /// <summary>
        /// Invokes a static method on a type
        /// </summary>
        public static object InvokeStaticProxied(string typeFullName, string methodName, params object[] args)
        {
            if (args == null)
                args = new object[0];

            Type type = CCBase.BaseGameTypes
                .Where(t => t.FullName == typeFullName)
                .Single();

            MethodInfo method = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == methodName)
                .Where(m => m.GetParameters().Length == args.Length)
                //TODO match parameter types?
                .Single();

            //we don't do any type coercion (yet)

            return method.Invoke(null, args);
        }

        /// <summary>
        /// Invokes a static method on a type
        /// </summary>
        public static object InvokeStaticProxiedEx(string typeName, string methodName, InvokeStaticProxyOptions options, params object[] args)
        {
            if (args == null)
                args = new object[0];

            Type type = CCBase.BaseGameTypes
                .Where(t => t.FullName == typeName)
                .Single();

            MethodInfo method;
            if (options.MatchParameterTypes)
            {
                var signature = options.ParameterMatchTypes != null ? options.ParameterMatchTypes.ToArray() : args.Select(a => a.GetType()).ToArray();
                method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, signature, null);
            }
            else
            {
                method = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.Name == methodName)
                    .Where(m => m.GetParameters().Length == args.Length)
                    .Single();
            }

            object[] coercedArgs = args;
            if(options.CoerceInputTypes)
            {
                var mParams = method.GetParameters();
                for (int i = 0; i < args.Length; i++)
                {
                    coercedArgs[i] = TypeUtils.CoerceValue(args[i], mParams[i].ParameterType);
                }
            }

            object result = method.Invoke(null, coercedArgs);

            if(options.OutputType != null)
            {
                result = TypeUtils.CoerceValue(result, options.OutputType);
            }

            return result;
        }

        /// <summary>
        /// Sets proxied fields from a source object to a target object
        /// </summary>
        public static void SetProxyFields(object source, object target, bool throwOnError = false)
        {

            //main fields
            var sourceFields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttribute<ProxyFieldAttribute>() != null);

            foreach(var sourceInfo in sourceFields)
            {
                try
                {
                    var proxyAttribute = sourceInfo.GetCustomAttribute<ProxyFieldAttribute>();

                    if (proxyAttribute.LinkToTypes != null && proxyAttribute.LinkToTypes.Length > 0)
                    {
                        if (!proxyAttribute.LinkToTypes.Contains(target.GetType().Name)) //correct I hope
                            continue;
                    }

                    string fieldName = !string.IsNullOrEmpty(proxyAttribute.TargetField) ? proxyAttribute.TargetField : sourceInfo.Name;
                    var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                    var targetField = target.GetType().GetField(fieldName, bindingFlags);
                    if (targetField == null)
                        continue;

                    if (!proxyAttribute.BindPrivate && !targetField.IsPublic && targetField.GetCustomAttribute<SerializeField>() == null)
                        continue;
                    
                    object coercedValue = TypeUtils.CoerceValue(sourceInfo.GetValue(source), targetField.FieldType);
                    targetField.SetValue(target, coercedValue);
                    
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to bind {sourceInfo.Name} from {source.GetType().Name} to {target.GetType().Name}");
                    Debug.LogException(e);

                    if (throwOnError)
                        throw e;
                }

            }
                       
            //ProxyExtensionData
            try
            {
                var sourceExtensionDataMember = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.GetCustomAttribute<ProxyExtensionDataAttribute>() != null)
                    .SingleOrDefault();
                IEnumerable sourceExtensionData = (IEnumerable)sourceExtensionDataMember.GetValue(source); //is this legit?

                foreach(var element in sourceExtensionData)
                {
                    KeyValuePair<string, object> elementKvp;
                    if (element is KeyValuePair<string, object> kvp)
                        elementKvp = kvp;
                    else if (element is KeyValuePair<string, string> kvps)
                        elementKvp = new KeyValuePair<string, object>(kvps.Key, kvps.Value);
                    else if (element is ITuple tuple)
                        elementKvp = new KeyValuePair<string, object>((string)tuple[0], tuple[1]);
                    else
                        throw new InvalidCastException();

                    var targetField = target.GetType().GetField(elementKvp.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (targetField == null)
                        continue;

                    object coercedValue = TypeUtils.CoerceValue(elementKvp.Value, targetField.FieldType);
                    targetField.SetValue(target, coercedValue);
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to bind extension data from {source.GetType().Name} to {target.GetType().Name}");
                Debug.LogException(e);

                if (throwOnError)
                    throw e;

            }
        }
    }
}