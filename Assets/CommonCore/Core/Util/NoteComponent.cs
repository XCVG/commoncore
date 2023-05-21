using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Util
{
    [AddComponentMenu("CommonCore/Note")]
    public sealed class NoteComponent : MonoBehaviour
    {
        [TextArea(10, 100)]
        public string Comment = "";
    }

}
