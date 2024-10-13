#if NET8_0_OR_GREATER

using System.Collections;

namespace BTreeSlim;

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer>
{
    public struct Enumerable : IEnumerable<T>
    {
        private readonly Move move;
        private Cursor cursor;

        internal Enumerable(Move move, ref Cursor cursor)
        {
            this.move = move;
            this.cursor = cursor;
        }

        public Enumerator GetEnumerator() => new(ref cursor, move);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

#endif