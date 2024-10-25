namespace BTreeSlim;

internal struct BufferList<T, TBuffer>(TBuffer buffer)
    where TBuffer : IBuffer<T>
{
    public int Count { get; private set; } = 0;

    public Span<T> Buffer => buffer.Content;
    public Span<T> Items => buffer.Content[..Count];

    public void Clear()
    {
        Items.Clear();
        Count = 0;
    }

    public void InsertAt(int index, T item)
    {
        var span = Buffer;
        span[index..Count].CopyTo(span[(index + 1)..]);
        span[index] = item;
        Count++;
    }

    public void InsertAt(int index, scoped ref T item)
    {
        var span = Buffer;
        span[index..Count].CopyTo(span[(index + 1)..]);
        span[index] = item;
        Count++;
    }

    public ref T InsertAtAndGetRef(int index, T item)
    {
        var span = Buffer;
        span[index..Count].CopyTo(span[(index + 1)..]);
        span[index] = item;
        Count++;
        return ref span[index];
    }

    public ref T InsertAtAndGetRef(int index, scoped ref T item)
    {
        var span = Buffer;
        span[index..Count].CopyTo(span[(index + 1)..]);
        span[index] = item;
        Count++;
        return ref span[index];
    }

    public void RemoveAt(int index)
    {
        var span = Buffer;
        span[(index + 1)..Count].CopyTo(span[index..]);
        ShrinkTo(Count - 1);
    }

    public void Add(T value)
    {
        Buffer[Count++] = value;
    }

    public void Add(ref T value)
    {
        Buffer[Count++] = value;
    }

    public void Add(ReadOnlySpan<T> values)
    {
        values.CopyTo(Buffer[Count..]);
        Count += values.Length;
    }

    public void RemoveFromTail()
    {
        ShrinkTo(Count - 1);
    }

    public void ShrinkTo(int size)
    {
        if (Count <= size)
        {
            return;
        }

        Buffer[size..Count].Clear();
        Count = size;
    }

#if NET7_0_OR_GREATER
    public bool IsFull => Count == TBuffer.Capacity;
#endif
}