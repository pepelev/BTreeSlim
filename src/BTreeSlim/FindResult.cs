#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;

namespace BTreeSlim;

public readonly ref struct FindResult<T>
{
    private readonly ref T found;

    public static FindResult<T> NotFound => new(ref Default<T>.Ref());

    public FindResult(ref T found)
    {
        this.found = ref found;
    }

    public ref T Value
    {
        get
        {
            if (!IsFound)
            {
                throw new InvalidOperationException("NotFound");
            }

            return ref found;
        }
    }

    public bool IsFound => !Unsafe.AreSame(ref Default<T>.Ref(), ref found);
}
#endif