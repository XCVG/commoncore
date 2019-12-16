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
        /// Applies the values from the backing config to the UI
        /// </summary>
        public abstract void PaintValues();

        /// <summary>
        /// Applies the values from the UI to the backing config
        /// </summary>
        public abstract void UpdateValues();

    }
}