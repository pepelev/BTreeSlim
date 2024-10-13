#if NET8_0_OR_GREATER

namespace BTreeSlim;

internal static class StackSlim
{
    public static StackSlim<T, Buffers.S32<T>> Create32<T>() => new(new Buffers.S32<T>());
}

internal struct StackSlim<T, TBuffer> where TBuffer : IBuffer<T>
{
    public int Count { get; private set; } = 0;
    private TBuffer buffer;

    public StackSlim(TBuffer buffer)
    {
        this.buffer = buffer;
    }

    public ref T this[int index]
    {
        get
        {
            var targetIndex = Count - 1 - index;
            return ref buffer.Content[targetIndex];
        }
    }

    public void Push(T item)
    {
        buffer.Content[Count++] = item;
    }

    public ref T Peek() => ref buffer.Content[Count - 1];

    public void Pop()
    {
        buffer.Content[--Count] = default!;
    }

    public bool IsEmpty => Count == 0;

    public void Clear()
    {
        buffer.Content[..Count].Clear();
        Count = 0;
    }
}

#endif