using System;
using System.Collections;
using System.Collections.Generic;

namespace CommonCore.World
{

    public interface IPlayerFlagsSource
    {
        IEnumerable<string> Flags { get; }
        int Count { get; }
        bool Contains(string flag); //should be case-insensitive!
    }

    /// <summary>
    /// Player flags source backed by a HashSet. Great for times you prefer composition over inheritance, and just need a bag of flags.
    /// </summary>
    public class SetPlayerFlagsSource : IPlayerFlagsSource, ISet<string>, IReadOnlyCollection<string>
    {

        private HashSet<string> Flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public SetPlayerFlagsSource()
        {

        }

        //IPlayerFlagsSource implementation

        IEnumerable<string> IPlayerFlagsSource.Flags => new List<string>(Flags);

        //ISet<T> and parents implementation

        public int Count => Flags.Count;

        public bool IsReadOnly => ((ICollection<string>)Flags).IsReadOnly;

        public bool Add(string item) => Flags.Add(item);

        public void Clear() => Flags.Clear();

        public bool Contains(string item) => Flags.Contains(item);

        public void CopyTo(string[] array, int arrayIndex) => Flags.CopyTo(array, arrayIndex);

        public void ExceptWith(IEnumerable<string> other) => Flags.ExceptWith(other);

        public IEnumerator<string> GetEnumerator() => Flags.GetEnumerator();

        public void IntersectWith(IEnumerable<string> other) => Flags.IntersectWith(other);

        public bool IsProperSubsetOf(IEnumerable<string> other) => Flags.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<string> other) => Flags.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<string> other) => Flags.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<string> other) => Flags.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<string> other) => Flags.Overlaps(other);

        public bool Remove(string item) => Flags.Remove(item);

        public bool SetEquals(IEnumerable<string> other) => Flags.SetEquals(other);

        public void SymmetricExceptWith(IEnumerable<string> other) => Flags.SymmetricExceptWith(other);

        public void UnionWith(IEnumerable<string> other) => Flags.UnionWith(other);

        void ICollection<string>.Add(string item) => ((ICollection<string>)Flags).Add(item);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Flags).GetEnumerator();
    }
}
