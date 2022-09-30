using CommonCore.RpgGame.Rpg;
using CommonCore.World;
using PseudoExtensibleEnum;
using System;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Represents a set of damage resistance values for a damage type
    /// </summary>
    [Serializable]
    public struct DamageResistanceNode
    {
        [PxEnumProperty(typeof(DefaultDamageTypes))]
        public int DamageType;
        public float DamageResistance;
        public float DamageThreshold;
    }
}