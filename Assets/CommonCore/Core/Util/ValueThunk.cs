namespace CommonCore
{
    //these are interfaces designed for things that allow grabbing values "upward" from Core into Assembly-CSharp or whatnot

    /// <summary>
    /// A get-only thunk
    /// </summary>
    public interface IReadOnlyValueThunk<T>
    {
        T Value { get; }
    }

    /// <summary>
    /// A get/set thunk
    /// </summary>
    public interface IValueThunk<T> : IReadOnlyValueThunk<T>
    {
        new T Value { get; set; }
    }
}