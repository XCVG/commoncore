namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Hit materials for hitbox handling
    /// </summary>
    public enum HitMaterial
    {
        //defaults; try to sync these with the ones in World
        Unspecified = 0, Generic = 1, Metal = 2, Wood = 3, Stone = 4, Dirt = 5, Flesh = 6,

        //game-specific ones I guess
        DragonSkin = 256
    }
}