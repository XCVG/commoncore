using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    /*
     * Utility class for effects
     * Can specify destroy period and/or set as persistent
     */
    public class EffectScript : MonoBehaviour
    {
        public bool Persist;
        public bool AutoUnparent = true;
        public float DestroyAfter;

        void Awake()
        {
            if(Persist)
            {
                if (AutoUnparent)
                    transform.parent = null;

                DontDestroyOnLoad(this.gameObject);
            }
        }

        void Start()
        {
            if(DestroyAfter > 0)
            {
                Destroy(this.gameObject, DestroyAfter);
            }
        }


    }
}