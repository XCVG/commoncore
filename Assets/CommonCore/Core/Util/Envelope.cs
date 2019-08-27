using System;
namespace CommonCore
{

    /// <summary>
    /// Min/Max/Gain/Decay datatype for whatever you want
    /// </summary>
    public struct RangeEnvelope
    {
        public float Min;
        public float Max;
        public float Gain;
        public float Decay;

        public RangeEnvelope(float min, float max, float gain, float decay)
        {
            Min = min;
            Max = max;
            Gain = gain;
            Decay = decay;
        }
    }
}