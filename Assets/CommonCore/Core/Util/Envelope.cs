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