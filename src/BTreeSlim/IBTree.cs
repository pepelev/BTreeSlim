#if NET8_0_OR_GREATER
namespace BTreeSlim;

public interface IBTree<TKey, T> : IReadOnlyCollection<T>
    where TKey : IComparable<TKey>
{
    public AdditionResult<T> TryAdd(T item);
    public ICursor<TKey, T> CreateCursor();
}
#endif