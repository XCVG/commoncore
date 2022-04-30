namespace CommonCore.State
{
    /// <summary>
    /// Thunk used by Core to see if in game or not
    /// </summary>
    public class GameStateExistsThunk : IReadOnlyValueThunk<bool>
    {
        public bool Value
        {
            get
            {
                return GameState.Exists;
            }
        }

    }
}