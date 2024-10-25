namespace BTreeSlim.Brief;

public readonly struct BTree8Map<TKey, TValue>(BTree<TKey, Pair<TKey, TValue>, Buffers.S15<Pair<TKey, TValue>>, Buffers.S16<object>> original)
    where TKey : IComparable<TKey>
{
    public BTree<TKey, Pair<TKey, TValue>, Buffers.S15<Pair<TKey, TValue>>, Buffers.S16<object>> Original => original;
    public BTree<TKey, Pair<TKey, TValue>, Buffers.S15<Pair<TKey, TValue>>, Buffers.S16<object>>.Cursor CreateCursor() => original.CreateCursor();
    public AdditionResult<Pair<TKey, TValue>> TryAdd(Pair<TKey, TValue> item) => original.TryAdd(item);
    public bool ContainsKey(TKey key) => original.ContainsKey(key);
    public FindResult<Pair<TKey, TValue>> Find(TKey key) => original.Find(key);
    public void Remove(TKey key) => original.Remove(key);
    public void Clear(bool returnNodesToPool = true) => original.Clear(returnNodesToPool);
    public int Count => original.Count;
    public BTree<TKey, Pair<TKey, TValue>, Buffers.S15<Pair<TKey, TValue>>, Buffers.S16<object>>.Enumerator GetEnumerator() => original.GetEnumerator();
}