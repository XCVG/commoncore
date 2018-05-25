using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{

    public class WorldModel
    {
        public WorldModel()
        {
            //defaults here
        
        }

        //TODO world state like time of day and stuff (need to think about this some)
        public float RealTimeElapsed { get; set; }
        public float RpgDaysElapsed { get; set; }
        public float RpgSecondsElapsed { get; set; }
        public float RpgTimeScale { get; set; }
    }
}