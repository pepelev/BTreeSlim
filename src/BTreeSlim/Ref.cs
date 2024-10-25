namespace BTreeSlim;

internal static class Ref
{
    public static void Swap<T>(ref T a, ref T b)
    {
        (a, b) = (b, a);
    }
}