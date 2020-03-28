namespace CommonCore.World
{

    /// <summary>
    /// Interface for components that handle hitboxes
    /// </summary>
    /// <remarks>90% of the time you should just use HitboxComponent</remarks>
    public interface IHitboxComponent
    {
        BaseController ParentController { get; }
        int HitLocationOverride { get; }
        int HitMaterial { get; }
        float DamageMultiplier { get; }
        bool AllDamageIsPierce { get; }
    }
}
