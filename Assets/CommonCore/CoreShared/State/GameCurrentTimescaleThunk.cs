namespace CommonCore.State
{
    /// <summary>
    /// Thunk used by PauseLock in Core to get game timescale
    /// </summary>
    public class GameCurrentTimescaleThunk : IValueThunk<float>
    {
        public float Value
        {
            get
            {
                return GameState.Instance?.CurrentTimescale ?? 1.0f;
            }
            set
            {
                if (GameState.Exists)
                    GameState.Instance.CurrentTimescale = value;
            }
        }

    }
}