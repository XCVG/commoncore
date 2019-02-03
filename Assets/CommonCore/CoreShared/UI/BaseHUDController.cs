using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{

    public class BaseHUDController : MonoBehaviour
    {
        public static BaseHUDController Current { get; protected set; }
    }
}