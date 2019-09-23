using CommonCore.Messaging;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Player view types, pretty self explanatory
    /// </summary>
    public enum PlayerViewType
    {
        PreferFirst, PreferThird, ForceFirst, ForceThird, ExplicitOther
    }

    /// <summary>
    /// Message signaling a request to shake the player view. Must be received by the player controller or somesuch.
    /// </summary>
    public class ViewShakeMessage : QdmsMessage
    {
        public readonly float Intensity;
        public readonly Vector3 Direction;
        public readonly float Time;
        public readonly int Priority;
        public readonly bool OverrideCurrent;

        public ViewShakeMessage(float intensity, Vector3 direction, float time, int priority, bool overrideCurrent)
        {
            Intensity = intensity;
            Direction = direction;
            Time = time;
            Priority = priority;
            OverrideCurrent = overrideCurrent;
        }
    }

    /// <summary>
    /// Message signaling a request to stop shaking the player view. Must be received by the player controller or somesuch.
    /// </summary>
    public class ViewShakeCancelMessage : QdmsMessage
    {

    }

}