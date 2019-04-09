using CommonCore.Config;
using CommonCore.Messaging;
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
        private const int BufferLength = 120;

        public static FPSCounter Instance { get; private set; }

        [SerializeField]
        private Text DisplayText;

        private QdmsMessageInterface MessageInterface;

        //inspired by https://catlikecoding.com/unity/tutorials/frames-per-second/
        private float[] FpsSamples = new float[BufferLength];
        private int FpsSampleIndex = 0;

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

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            Instance = this;
        }

        private void Start()
        {
            MessageInterface = new QdmsMessageInterface(this.gameObject);
            MessageInterface.SubscribeReceiver(HandleMessageReceived);
            SetStateFromConfig();
        }

        private void Update()
        {
            //update index
            FpsSampleIndex++;
            if (FpsSampleIndex >= FpsSamples.Length)
                FpsSampleIndex = 0;

            //update current
            FpsSamples[FpsSampleIndex] = (1f / Time.unscaledDeltaTime);

            //calculate average
            float sumFps = 0;
            foreach (float sample in FpsSamples)
                sumFps += sample;
            float averageFps = sumFps / FpsSamples.Length;

            //no, it's not no-alloc, Unity needs to fix their fucking GC instead of forcing us to work around it
            DisplayText.text = $"{averageFps:F1} fps";
        }

        private void HandleMessageReceived(QdmsMessage message)
        {
            if(message is QdmsFlagMessage flagMessage && flagMessage.Flag == "ConfigChanged")
            {
                SetStateFromConfig();
            }
        }

        private void SetStateFromConfig()
        {
            //check config, enable/disable based on config setting
            this.gameObject.SetActive(ConfigState.Instance.ShowFps);
        }
    }
}