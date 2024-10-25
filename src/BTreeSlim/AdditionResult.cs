#if NET8_0_OR_GREATER
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace BTreeSlim;

public enum AdditionOutcome : byte
{
    Success,
    AlreadyPresent
}

public readonly ref struct AdditionResult<T>
{
    private readonly ref T @ref;
    public AdditionOutcome Outcome { get; }

    [Pure]
    public static AdditionResult<T> Success(ref T value) => new(AdditionOutcome.Success, ref value);

    [Pure]
    public static AdditionResult<T> AlreadyPresent(ref T value) => new(AdditionOutcome.AlreadyPresent, ref value);

    internal AdditionResult(AdditionOutcome outcome, ref T @ref)
    {
        Outcome = outcome;
        this.@ref = ref @ref;
    }

    public ref T Ref => ref @ref;

    public ref T AddedValue
    {
        get
        {
            if (Outcome == AdditionOutcome.Success)
            {
                return ref @ref;
            }

            // todo text
            throw new InvalidOperationException();
        }
    }

    public ref T ConflictingValue
    {
        get
        {
            if (Outcome == AdditionOutcome.AlreadyPresent)
            {
                return ref @ref;
            }

            // todo text
            throw new InvalidOperationException();
        }
    }

    public void EnsureSuccess()
    {
        if (Outcome == AdditionOutcome.AlreadyPresent)
        {
            // todo
            throw new InvalidOperationException();
        }
    }
}

#endif