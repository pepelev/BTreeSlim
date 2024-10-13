#if NET8_0_OR_GREATER

using System.Collections;

namespace BTreeSlim;

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer>
{
    public struct Enumerator : IEnumerator<T>
    {
        private Cursor cursor;
        private readonly Move move;

        internal Enumerator(ref Cursor cursor, Move move = Move.Next)
        {
            this.cursor = cursor;
            this.move = move;
        }

        public bool MoveNext()
        {
            cursor.Move(move);
            return cursor.State == CursorState.InsideTree;
        }

        public void Reset()
        {
            if (move == Move.Next)
            {
                cursor.MoveToStart();
            }
            else
            {
                cursor.MoveToEnd();
            }
        }

        public T Current => cursor.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}

#endif