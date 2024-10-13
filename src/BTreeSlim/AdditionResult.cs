#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;

namespace BTreeSlim;

public readonly ref struct AdditionResult<T>
{
    private readonly ref T conflictingValue;

    public static AdditionResult<T> Success => new(ref Default<T>.Ref());

    public AdditionResult(ref T conflictingValue)
    {
        this.conflictingValue = ref conflictingValue;
    }

    // todo check !IsSuccess
    public ref T ConflictingValue => ref conflictingValue;
    public bool IsSuccess => Unsafe.AreSame(ref Default<T>.Ref(), ref conflictingValue);

    public void ThrowOnConflict()
    {
        if (!IsSuccess)
        {
            throw new InvalidOperationException();
        }
    }
}
#endif