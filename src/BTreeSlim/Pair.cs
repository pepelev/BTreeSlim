namespace BTreeSlim;

public readonly struct Pair<TKey, TValue> : IKeyed<TKey>, IEquatable<Pair<TKey, TValue>>
{
    public Pair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public TKey Key { get; }
    public TValue Value { get; }

    public override string ToString() => $"({Key}, {Value})";

    public void Deconstruct(out TKey key, out TValue value)
    {
        key = Key;
        value = Value;
    }

    public bool Equals(Pair<TKey, TValue> other) =>
        EqualityComparer<TKey>.Default.Equals(Key, other.Key) &&
        EqualityComparer<TValue>.Default.Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj is Pair<TKey, TValue> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Key, Value);
}