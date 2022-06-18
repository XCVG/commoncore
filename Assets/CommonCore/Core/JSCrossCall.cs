using CommonCore.Messaging;
using CommonCore.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// Utility class for cross-calling into Javascript
    /// </summary>
    public static class JSCrossCall
    {
        //in theory it's possible to return values synchronously from a call to JS, though not (easily) from a call from JS to C# AFAICT
        //I'm not implementing that, I might do so in the future though (CS->JS will return synchronous, JS->CS will return a Promise)

        /// <summary>
        /// JavaScript thunk called from C#
        /// </summary>
        [DllImport("__Internal")]
        private static extern void CSCallThunk(string str);

        /// <summary>
        /// Thunk called from JavaScript
        /// </summary>
        internal static void JSCallThunk(string str)
        {
#if UNITY_WEBGL
            JSCallThunkData data = JsonConvert.DeserializeObject<JSCallThunkData>(str, CoreParams.DefaultJsonSerializerSettings); //should we use default settings here?
            if(data.Target == "PushBroadcastMessage")
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage(data.MessageFlag, data.MessageValues));
            }
            else if(data.Target == "CallScript")
            {
                ScriptingModule.Call(data.Target, new ScriptExecutionContext() { Caller = typeof(JSCrossCall) }, data.Args);
            }
#else
            throw new NotSupportedException($"{nameof(JSCrossCall)}.{nameof(JSCallThunk)} is not supported on non-WebGL platforms");
#endif

        }

        /// <summary>
        /// Calls a JavaScript function in the global scope
        /// </summary>
        public static void CallJSFunction(string functionName, params object[] args)
        {
            string dataStr = JsonConvert.SerializeObject(new { function = functionName,  args = args});
            CSCallThunk(dataStr);
        }

        /// <summary>
        /// Triggers an event on the canvas object this game is attached to
        /// </summary>
        public static void TriggerCanvasEvent(string eventName, IEnumerable<KeyValuePair<string, object>> additionalData)
        {
            string dataStr = JsonConvert.SerializeObject(new { @event = eventName, args = additionalData });
            CSCallThunk(dataStr);
        }
    }

    internal class JSCallThunkData
    {
        [JsonProperty(PropertyName = "target")]
        public string Target;

        [JsonProperty(PropertyName = "args")]
        public object[] Args;

        //for sending messages
        [JsonProperty(PropertyName = "messageFlag")]
        public string MessageFlag;
        [JsonProperty(PropertyName = "messageValues")]
        public Dictionary<string, object> MessageValues;
    }
}