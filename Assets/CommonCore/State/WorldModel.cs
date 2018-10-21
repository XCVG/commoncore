using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{

    //WorldModel mostly keeps track of time for now, but could also be used to store data on weather or other such things
    public class WorldModel
    {
        public WorldModel()
        {
            //defaults here
            WorldTimeScale = 60.0f;
            WorldTimeUseRollover = true;
        }

        public float RealTimeElapsed { get; set; }
        public float GameTimeElapsed { get; set; }
        public float WorldDaysElapsed { get; set; }
        public float WorldSecondsElapsed { get; set; }
        public float WorldTimeScale { get; set; }
        public bool WorldTimeUseRollover { get; set; }
    }
}