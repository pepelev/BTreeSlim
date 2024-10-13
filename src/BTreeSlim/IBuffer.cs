namespace BTreeSlim;

public interface IBuffer<T>
{

#if NET7_0_OR_GREATER
    static abstract int Capacity { get; }
#endif

    Span<T> Content { get; }
}