using CommonCore.World;

namespace CommonCore
{

    /// <summary>
    /// CommonCore game and game module parameters; specific to the game type
    /// </summary>
    public static partial class GameParams
    {

        public static bool UseCustomLeveling { get; private set; } = true;
        public static bool UseDerivedSkills { get; private set; } = true;
        public static PlayerViewType DefaultPlayerView { get; private set; } = PlayerViewType.PreferFirst;
        public static bool UseRandomDamage { get; private set; } = true;
        public static bool AutoQuestNotifications { get; private set; } = true;

    }
}