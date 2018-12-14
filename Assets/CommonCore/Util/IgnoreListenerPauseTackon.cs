using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//very simple "tack on" script that makes an AudioSource ignore listener pause
public class IgnoreListenerPauseTackon : MonoBehaviour
{
    
	void Start ()
    {
        GetComponent<AudioSource>().ignoreListenerPause = true;
	}

}
