namespace BTreeSlim;

internal readonly record struct Key<T1, T2>(T1 Part1, T2 Part2) : ICompoundKey<T1, Key<T1, T2>>
    where T1 : IComparable<T1>
    where T2 : IComparable<T2>
{
    public static int Compare(T1 prefix, Key<T1, T2> key) => Comparer<T1>.Default.Compare(prefix, key.Part1);
}