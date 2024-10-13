#if NET8_0_OR_GREATER
namespace BTreeSlim;

internal static class Default<T>
{
    private static T Value = default!;

    public static ref T Ref() => ref Value;
}

#endif