using System.Diagnostics;

#if NET8_0_OR_GREATER

namespace BTreeSlim;

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer>
{
    internal abstract partial class Node
    {
        public abstract Span<T> Items { get; }
        public abstract bool IsFull { get; }
        public abstract AdditionResult<T> TryAdd(T item);
        public abstract (T MiddleItem, Node NewRightNode) Split();

        [Conditional("DEBUG")]
        public abstract void EnsureValid(bool isRoot);
    }
}

#endif