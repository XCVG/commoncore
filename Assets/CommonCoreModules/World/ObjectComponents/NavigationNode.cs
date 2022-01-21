using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Path node for navigating NPCs or something
    /// </summary>
    public class NavigationNode : MonoBehaviour
    {
        public bool StartNode = false;
        public bool EndNode = false;
        public float DistanceThreshold = 2f;
        public NavigationNode PreviousNode;
        public NavigationNode NextNode;

    }
}