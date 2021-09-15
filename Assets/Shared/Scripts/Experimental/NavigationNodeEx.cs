using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Experimental
{

    /// <summary>
    /// Path node for navigating NPCs or something
    /// </summary>
    public class NavigationNodeEx : MonoBehaviour
    {
        public bool StartNode = false;
        public bool EndNode = false;
        public float DistanceThreshold = 2f;
        public NavigationNodeEx PreviousNode;
        public NavigationNodeEx NextNode;

    }
}