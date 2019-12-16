namespace CommonCore.World
{
    /// <summary>
    /// Interface representing something that can take damage
    /// </summary>
    /// <remarks>
    /// Implement this in a BaseController derivative.
    /// </remarks>
    public interface ITakeDamage
    {
        void TakeDamage(ActorHitInfo data);
    }
}