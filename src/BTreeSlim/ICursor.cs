#if NET8_0_OR_GREATER
namespace BTreeSlim;

public interface ICursor<TKey, T> where TKey : IComparable<TKey>
{
    Range<TKey> Range { get; set; }
    Move MoveOnRestoreOnKeyDeletion { get; set; }
    CursorState State { get; }
    ref T Current { get; }
    void MoveToStart();
    void MoveToEnd();
    void Move(Move move);
    void MoveNext();
    void MovePrevious();

    IEnumerable<T> AsEnumerable(Move direction);
    void Remove(Move move);
}
#endif