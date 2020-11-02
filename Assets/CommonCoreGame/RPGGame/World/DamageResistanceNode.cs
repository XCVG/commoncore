using CommonCore.RpgGame.Rpg;
using System;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Represents a set of damage resistance values for a damage type
    /// </summary>
    [Serializable]
    public struct DamageResistanceNode
    {
        public DamageType DamageType;
        public float DamageResistance;
        public float DamageThreshold;
    }
}