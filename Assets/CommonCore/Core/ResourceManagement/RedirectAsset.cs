using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ResourceManagement
{

    /// <summary>
    /// An asset representing a path redirect
    /// </summary>
    [CreateAssetMenu(fileName = "New Redirect Asset", menuName = "CCScriptableObjects/RedirectAsset")]
    public class RedirectAsset : ScriptableObject
    {
        /// <summary>
        /// The path to redirect to
        /// </summary>
        public string Path;
    }
}