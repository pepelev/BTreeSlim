using System.Runtime.CompilerServices;

namespace BTreeSlim;

public static class Self
{
    public static ref T Ref<T>(ref Self<T> self)
    {
        ref readonly var readonlyRef = ref self.value;
        ref var value = ref Unsafe.AsRef(in readonlyRef);
        return ref value;
    }
}

public readonly struct Self<T> : IKeyed<T>, IEquatable<Self<T>>
{
    internal readonly T value;

    public Self(T value)
    {
        this.value = value;
    }

    public T Value => value;
    T IKeyed<T>.Key => Value;
    public override string? ToString() => Value?.ToString();
    public bool Equals(Self<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value);
    public override bool Equals(object? obj) => obj is Self<T> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value);
    public static implicit operator Self<T>(T value) => new (value);
    public static implicit operator T(Self<T> self) => self.Value;
    public static bool operator ==(Self<T> left, Self<T> right) => left.Equals(right);
    public static bool operator !=(Self<T> left, Self<T> right) => !(left == right);
}