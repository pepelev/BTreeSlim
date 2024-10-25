namespace BTreeSlim.Keys;

public readonly struct OrdinalString(string value) :
    IComparable<OrdinalString>,
    IEquatable<OrdinalString>,
    ICompoundKey<string, OrdinalString> // todo надо?
{
    private static StringComparer Comparison => StringComparer.Ordinal;

    public string Value { get; } = value;

    public int CompareTo(OrdinalString other) => Comparison.Compare(Value, other.Value);
    public bool Equals(OrdinalString other) => Comparison.Equals(Value, other.Value);
    public static int Compare(string prefix, OrdinalString key)
    {
        var keySpan = key.Value.AsSpan();
        var commonLength = keySpan.CommonPrefixLength(prefix);
        if (commonLength == prefix.Length)
        {
            return 0;
        }

        if (commonLength == keySpan.Length)
        {
            return 1; //todo check
        }

        return prefix[commonLength].CompareTo(keySpan[commonLength]);
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is OrdinalString other && Equals(other);
    public override int GetHashCode() => Comparison.GetHashCode(Value);
    public static bool operator ==(OrdinalString left, OrdinalString right) => left.Equals(right);
    public static bool operator !=(OrdinalString left, OrdinalString right) => !left.Equals(right);
    public static implicit operator OrdinalString(string value) => new(value);
    public static implicit operator string(OrdinalString value) => value.Value;
}