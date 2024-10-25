namespace BTreeSlim.Brief;

public readonly struct BTree8<TKey, T>(BTree<TKey, T, Buffers.S15<T>, Buffers.S16<object>> original)
    where TKey : IComparable<TKey>
    where T : IKeyed<TKey>
{
    public BTree<TKey, T, Buffers.S15<T>, Buffers.S16<object>> Original => original;
    public BTree<TKey, T, Buffers.S15<T>, Buffers.S16<object>>.Cursor CreateCursor() => original.CreateCursor();
    public AdditionResult<T> TryAdd(T item) => original.TryAdd(item);
    public bool ContainsKey(TKey key) => original.ContainsKey(key);
    public FindResult<T> Find(TKey key) => original.Find(key);
    public void Remove(TKey key) => original.Remove(key);
    public void Clear(bool returnNodesToPool = true) => original.Clear(returnNodesToPool);
    public int Count => original.Count;
    public BTree<TKey, T, Buffers.S15<T>, Buffers.S16<object>>.Enumerator GetEnumerator() => original.GetEnumerator();
}