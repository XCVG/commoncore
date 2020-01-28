using System;
namespace CommonCore
{
    //note that some of the terminology used is very stupid, because things were written in a hurry and now too much relies on it to change it

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

    /// <summary>
    /// Min/Max datatype for whatever you need
    /// </summary>
    public struct IntervalEnvelope
    {
        public float Min;
        public float Max;

        public IntervalEnvelope(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// Middle/spread datatype meant for randomized things
    /// </summary>
    public struct RandomEnvelope
    {
        public float Average;
        public float Spread;

        public RandomEnvelope(float average, float spread)
        {
            Average = average;
            Spread = spread;
        }
    }

    /// <summary>
    /// Intensity/Time/Violence datatype mostly for defining pulses
    /// </summary>
    public struct PulseEnvelope
    {
        public float Intensity;
        public float Time;
        public float Violence;

        public PulseEnvelope(float intensity, float time, float violence)
        {
            Intensity = intensity;
            Time = time;
            Violence = violence;
        }
    }
}