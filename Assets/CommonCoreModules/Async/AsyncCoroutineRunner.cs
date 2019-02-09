using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Based on code from Unity3dAsyncAwaitUtil
// Copyright (c) Modest Tree Media Inc, Chris Leclair
// Licensed under the MIT license. See MTLICENSE file in the module folder for full license information.

namespace CommonCore.Async
{
    public class AsyncCoroutineRunner : MonoBehaviour
    {
        static AsyncCoroutineRunner _instance;

        internal static AsyncCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("AsyncCoroutineRunner")
                        .AddComponent<AsyncCoroutineRunner>();
                }

                return _instance;
            }
        }

        void Awake()
        {
            gameObject.hideFlags = HideFlags.DontSave;

            DontDestroyOnLoad(gameObject);
        }
    }
}
