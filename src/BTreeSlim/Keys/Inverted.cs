using System.Collections;

namespace BTreeSlim.Keys;

internal readonly struct Inverted<T>(T value) : IComparable<Inverted<T>>
    where T : IComparable<T>
{
    public T Value => value;
    public int CompareTo(Inverted<T> other) => Comparer.Default.Compare(other.Value, value);
}