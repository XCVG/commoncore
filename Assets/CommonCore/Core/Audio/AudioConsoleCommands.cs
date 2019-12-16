namespace CommonCore.Audio
{

    /// <summary>
    /// Console commands for audio system
    /// </summary>
    public static class AudioConsoleCommands
    {
        [Command(alias = "ClearSounds", className = "Audio", useClassName = true)]
        public static void ClearSounds()
        {
            AudioPlayer.Instance.ClearAllSounds();
        }

        [Command(alias = "ClearMusic", className = "Audio", useClassName = true)]
        public static void ClearMusic()
        {
            AudioPlayer.Instance.ClearAllMusic();
        }
    }
}