using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{
    /// <summary>
    /// Base class for config subpanel controllers
    /// </summary>
    public abstract class ConfigSubpanelController : MonoBehaviour
    {
        /// <summary>
        /// Signals that there are pending changes (call FROM the subpanel controller)
        /// </summary>
        public Action<PendingChangesFlags> SignalPendingChanges { get; set; }

        /// <summary>
        /// Set to true to "lock" the UI and prevent value changes/pending changes from being signalled (ie when changing values)
        /// </summary>
        protected bool IgnoreValueChanges = false;

        /// <summary>
        /// Applies the values from the backing config to the UI
        /// </summary>
        public abstract void PaintValues();

        /// <summary>
        /// Applies the values from the UI to the backing config
        /// </summary>
        public abstract void UpdateValues();

        /// <summary>
        /// Call from control when value changes. Signals pending changes if IgnoreValueChanges = false
        /// </summary>
        public void HandleAnyChanged()
        {
            if (IgnoreValueChanges)
                return;

            SignalPendingChanges?.Invoke(PendingChangesFlags.None);
        }
    }
}