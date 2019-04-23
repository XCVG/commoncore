namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Interface representing something that can take damage
    /// </summary>
    /// <remarks>
    /// Attach this to a BaseController derivative.
    /// </remarks>
    public interface ITakeDamage
    {
        void TakeDamage(ActorHitInfo data);
    }
}