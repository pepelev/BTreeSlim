namespace BTreeSlim.Brief;

public readonly struct BTree8Set<T>(BTree<T, Self<T>, Buffers.S15<Self<T>>, Buffers.S16<object>> original)
    where T : IComparable<T>
{
    public BTree<T, Self<T>, Buffers.S15<Self<T>>, Buffers.S16<object>> Original => original;
    public BTree<T, Self<T>, Buffers.S15<Self<T>>, Buffers.S16<object>>.Cursor CreateCursor() => original.CreateCursor();

    public AdditionResult<T> TryAdd(T item)
    {
        var result = original.TryAdd(item);
        return new AdditionResult<T>(result.Outcome, ref Self.Ref(ref result.Ref));
    }

    public bool Contains(T item) => original.ContainsKey(item);

    public FindResult<T> Find(T item)
    {
        var result = original.Find(item);
        return result.IsFound
            ? new FindResult<T>(ref Self.Ref(ref result.Value))
            : FindResult<T>.NotFound;
    }

    public void Remove(T item) => original.Remove(item);
    public void Clear(bool returnNodesToPool = true) => original.Clear(returnNodesToPool);
    public int Count => original.Count;
    public BTree<T, Self<T>, Buffers.S15<Self<T>>, Buffers.S16<object>>.Enumerator GetEnumerator() => original.GetEnumerator();
}