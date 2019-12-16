using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Interface for controllers that cause damage on hit (ie projectiles)
    /// </summary>
    interface IDamageOnHit
    {
        void HandleCollision(BaseController hitObjectController, int hitLocation, int hitMaterial, Vector3? hitCoords); //will be called by the hitbox component
    }
}