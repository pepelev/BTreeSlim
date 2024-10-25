using System.Globalization;

namespace BTreeSlim.Keys;

public readonly struct InvariantCultureString(string value) :
    IComparable<InvariantCultureString>,
    IEquatable<InvariantCultureString>,
    ICompoundKey<string, InvariantCultureString>
{
    private static CompareInfo CompareInfo => CultureInfo.InvariantCulture.CompareInfo;
    private static StringComparer Comparison => StringComparer.InvariantCulture;

    public string Value { get; } = value;

    public int CompareTo(InvariantCultureString other) => Comparison.Compare(Value, other.Value);
    public bool Equals(InvariantCultureString other) => Comparison.Equals(Value, other.Value);
    public static int Compare(string prefix, InvariantCultureString key)
    {
        var prefixSpan = prefix.AsSpan();
        var keySpan = key.Value.AsSpan();
        if (CompareInfo.IsPrefix(keySpan, prefixSpan))
        {
            return 0;
        }

        return Comparison.Compare(prefix, key.Value);
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is InvariantCultureString other && Equals(other);
    public override int GetHashCode() => Comparison.GetHashCode(Value);
    public static bool operator ==(InvariantCultureString left, InvariantCultureString right) => left.Equals(right);
    public static bool operator !=(InvariantCultureString left, InvariantCultureString right) => !left.Equals(right);
    public static implicit operator InvariantCultureString(string value) => new(value);
    public static implicit operator string(InvariantCultureString value) => value.Value;
}