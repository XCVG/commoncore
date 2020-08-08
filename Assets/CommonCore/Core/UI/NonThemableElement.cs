using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{

    /// <summary>
    /// Attach to a UI element to make it (and optionally its children) ignore themes
    /// </summary>
    public class NonThemableElement : MonoBehaviour
    {
        public bool IgnoreChildren = true;

        //theme engine will just hit this and ignore
    }
}