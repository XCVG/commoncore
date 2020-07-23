namespace CommonCore.State
{

    //WorldModel mostly keeps track of time for now, but could also be used to store data on weather or other such things
    public class WorldTimeModel
    {
        public WorldTimeModel()
        {
            //defaults here
            WorldTimeScale = 60.0f;
            WorldTimeUseRollover = true;
        }

        public double RealTimeElapsed { get; set; }
        public double GameTimeElapsed { get; set; }
        public double WorldDaysElapsed { get; set; }
        public double WorldSecondsElapsed { get; set; }
        public double WorldTimeScale { get; set; }
        public bool WorldTimeUseRollover { get; set; }
    }
}