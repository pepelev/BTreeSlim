using System.Runtime.CompilerServices;

#if NET8_0_OR_GREATER
namespace BTreeSlim;

internal static class Contract
{
    public static void Assert(bool condition, [CallerArgumentExpression(nameof(condition))] string assert = "")
    {
        if (!condition)
        {
            throw new Exception(assert.Trim());
        }
    }

    public static T As<T>(object obj) where T : class
    {
        Assert(obj is T);
        return Unsafe.As<T>(obj);
    }
}

#endif