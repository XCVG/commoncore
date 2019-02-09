using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Based on code from Unity3dAsyncAwaitUtil
// Copyright (c) Modest Tree Media Inc, Chris Leclair
// Licensed under the MIT license. See MTLICENSE file in the module folder for full license information.

namespace CommonCore.Async
{

    /// <summary>
    /// Asynchronous helper module.
    /// </summary>
    [CCEarlyModule]
    public class AsyncModule : CCModule
    {
        public AsyncModule()
        {
            SyncContextUtil.Install();

            AsyncCoroutineRunner.Instance.GetType(); //poke this to create the instance
        }
    }

    /// <summary>
    /// Synchronization Context utilities
    /// </summary>
    public static class SyncContextUtil
    {
        internal static void Install()
        {
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Thread ID of the main Unity thread
        /// </summary>
        public static int UnityThreadId
        {
            get; private set;
        }

        /// <summary>
        /// Main Unity game synchronization context
        /// </summary>
        public static SynchronizationContext UnitySynchronizationContext
        {
            get; private set;
        }
    }
}