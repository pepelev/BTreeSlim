namespace BTreeSlim;

public readonly struct Self<T> : IKeyed<T>, IEquatable<Self<T>>
{
    public Self(T value)
    {
        Value = value;
    }

    public T Value { get; }
    T IKeyed<T>.Key => Value;
    public override string? ToString() => Value?.ToString();
    public bool Equals(Self<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value);
    public override bool Equals(object? obj) => obj is Self<T> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value);
    public static implicit operator Self<T>(T value) => new (value);
    public static implicit operator T(Self<T> self) => self.Value;
}