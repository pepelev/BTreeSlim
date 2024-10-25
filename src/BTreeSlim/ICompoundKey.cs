namespace BTreeSlim;

internal interface ICompoundKey<in TPrefix, in TSelf> where TSelf : ICompoundKey<TPrefix, TSelf>
{
    static abstract int Compare(TPrefix prefix, TSelf key);
}