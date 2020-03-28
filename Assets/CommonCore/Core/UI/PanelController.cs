using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{
    /// <summary>
    /// Base class for UI panel controllers
    /// </summary>
    public abstract class PanelController : MonoBehaviour
    {

        [SerializeField, Tooltip("If set, Signal* will be called automatically using Unity MonoBehaviour events")]
        protected bool HookupUnityEvents = true;

        private bool InitialPaintDone = false;

        private void Start()
        {
            if (HookupUnityEvents && !InitialPaintDone)
                SignalInitialPaint();
        }

        private void OnEnable()
        {
            if (HookupUnityEvents)
            {
                if (!InitialPaintDone)
                    SignalInitialPaint();
                SignalPaint();
            }
        }

        private void OnDisable()
        {
            if (HookupUnityEvents)
            {
                SignalUnpaint();
            }
        }

        private void OnDestroy()
        {
            if(HookupUnityEvents)
            {
                SignalFinalUnpaint();
            }
        }

        /// <summary>
        /// Called when the panel is initially created and shown for the first time
        /// </summary>
        public virtual void SignalInitialPaint()
        {
            InitialPaintDone = true;
        }

        /// <summary>
        /// Called when the panel needs to be repainted- on enable or when dependencies update
        /// </summary>
        public virtual void SignalPaint()
        {

        }

        /// <summary>
        /// Called just before the panel is hidden
        /// </summary>
        public virtual void SignalUnpaint()
        {

        }

        /// <summary>
        /// Called just before the panel is destroyed
        /// </summary>
        public virtual void SignalFinalUnpaint()
        {

        }
    }
}