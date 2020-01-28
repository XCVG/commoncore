using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.Dialogue
{
    /// <summary>
    /// Attach to an object to define it as the target for the dialogue camera
    /// </summary>
    public class DialogueCameraTarget : MonoBehaviour
    {
        //overrides will go here, eventually
        public float DistanceOverride = -1;
    }
}