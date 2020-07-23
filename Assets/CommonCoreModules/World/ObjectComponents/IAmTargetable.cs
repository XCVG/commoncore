namespace CommonCore.World
{
    /// <summary>
    /// Interface representing something that can be targeted
    /// </summary>
    /// <remarks>
    /// Implement this in a BaseController derivative.
    /// </remarks>
    public interface IAmTargetable
    {
        /// <summary>
        /// Is this target valid? (ie not dead, disabled, etc)
        /// </summary>
        bool ValidTarget { get; }

        /// <summary>
        /// Value for biasing detection (1 is normal, 0 is undetectable)
        /// </summary>
        float Detectability { get; }

        /// <summary>
        /// What is the faction of this target? (if you are using factions)
        /// </summary>
        string Faction { get; }

        
    }
}