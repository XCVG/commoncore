using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Interface representing something that can be pushed (attach to entity controllers)
    /// </summary>
    public interface IAmPushable
    {
        void Push(Vector3 impulse);
    }
}


