using System.Collections.Generic;

namespace CommonCore.World
{
    /// <summary>
    /// Interface representing a component on an object that has extra data to save/restore
    /// </summary>
    public interface IHaveRestorableExtraData
    {
        void CommitExtraData(IDictionary<string, object> data);
        void RestoreExtraData(IDictionary<string, object> data);

    }
}