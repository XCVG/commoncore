using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// very simple "tack on" script that makes an AudioSource ignore listener pause
    /// </summary>
    public class IgnoreListenerPauseTackon : MonoBehaviour
    {

        void Start()
        {
            GetComponent<AudioSource>().ignoreListenerPause = true;
        }

    }
}