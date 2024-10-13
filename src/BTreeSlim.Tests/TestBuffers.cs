#if NET8_0_OR_GREATER

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BTreeSlim.Tests;

public static class TestBuffers
{
    [InlineArray(1)]
    public struct S1<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 1;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(2)]
    public struct S2<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 2;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }
}

#endif