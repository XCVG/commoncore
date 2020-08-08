using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{

    /// <summary>
    /// Options for a themable UI element
    /// </summary>
    public class ThemableElement : MonoBehaviour
    {
        public bool OverrideClass = false;
        public ElementClass ClassOverride = ElementClass.Unknown;

        public ThemableElementColor ElementColor = ThemableElementColor.Auto;


        public ThemableElementChildOption ThemeChildren = ThemableElementChildOption.Auto;

        [Serializable]
        public enum ThemableElementColor
        {
            Auto, Normal, Contrasting
        }

        [Serializable]
        public enum ThemableElementChildOption
        {
            Auto, Ignore, Apply
        }
    }


}