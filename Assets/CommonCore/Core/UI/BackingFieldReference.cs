using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{
    /// <summary>
    /// Component intended to be attached to a row or item in a view to reference a backing field or collection
    /// </summary>
    public class BackingFieldReference : MonoBehaviour
    {
        //use one or all of these, I don't care
        public int Index { get; set; } = -1;
        public object Value { get; set; } = null;
        public string Id { get; set; } = null;
    }
}