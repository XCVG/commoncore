namespace CommonCore.Audio
{
    /// <summary>
    /// Slots to play music into. Higher slots override lower slots
    /// </summary>
    public enum MusicSlot
    {
        /// <summary>
        /// Base slot, for ambiance or background music
        /// </summary>
        Ambient,
        /// <summary>
        /// Slot for "event" music (intended for dynamic music schemes)
        /// </summary>
        Event,
        /// <summary>
        /// Slot for music provided by a UserMusicComponent (for implementing in-game radio etc)
        /// </summary>
        /// <remarks>
        /// DO NOT PLAY INTO THIS SLOT other than from a UserMusicComponent or Weird Shit will happen
        /// </remarks>
        User,
        /// <summary>
        /// Slot for "cinematic" music (cutscenes etc)
        /// </summary>
        Cinematic,
        /// <summary>
        /// Slot for generic override of any other music
        /// </summary>
        Override
    }
}