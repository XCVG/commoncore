using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.DebugLog
{
    /// <summary>
    /// Controller for a dirt-simple, bog-standard FPS counter
    /// </summary>
    public sealed class FPSCounter : MonoBehaviour
    {
        private const string PrefabPath = @"UI/FPSCounter";
        private const int BufferLength = 60;

        public static FPSCounter Instance { get; private set; }

        [SerializeField]
        private Text DisplayText;

        private float[] FpsBuffer = new float[BufferLength];

        /// <summary>
        /// Creates an FPS counter instance
        /// </summary>
        internal static void Initialize()
        {
            if(Instance != null)
            {
                Debug.LogWarning("[Debug] Tried to initialize FPS Counter, but FPS counter already exists!");
                return;
            }

            try
            {
                var go = Instantiate(CoreUtils.LoadResource<GameObject>(PrefabPath));                
            }
            catch(Exception e)
            {
                Debug.LogError("[Debug] Failed to initialize FPS Counter!");
                Debug.LogException(e);
            }
        }

        //TODO need a way to turn it on and off

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }

    }
}