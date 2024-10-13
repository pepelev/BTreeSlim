namespace BTreeSlim;

internal struct BufferList<T, TBuffer> where TBuffer : IBuffer<T>
{
    public int Count { get; set; }
    private TBuffer buffer;

    public BufferList(TBuffer buffer)
    {
        this.buffer = buffer;
        Count = 0;
    }

    public Span<T> Buffer => buffer.Content;
    public Span<T> Items => buffer.Content[..Count];

    public void Clear()
    {
        Items.Clear();
        Count = 0;
    }

    public void InsertAt(int index, T item)
    {
        Buffer[index..Count].CopyTo(Buffer[(index + 1)..]);
        Buffer[index] = item;
        Count++;
    }

    public void InsertAt(int index, ref T item)
    {
        Buffer[index..Count].CopyTo(Buffer[(index + 1)..]);
        Buffer[index] = item;
        Count++;
    }

    public void RemoveAt(int index)
    {
        Buffer[(index + 1)..Count].CopyTo(Buffer[index..]);
        ShrinkTo(Count - 1);
    }

    public void Add(T value)
    {
        buffer.Content[Count++] = value;
    }

    public void Add(ref T value)
    {
        buffer.Content[Count++] = value;
    }

    public void Add(ReadOnlySpan<T> values)
    {
        values.CopyTo(buffer.Content[Count..]);
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