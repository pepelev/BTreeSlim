#if NET8_0_OR_GREATER

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BTreeSlim;

public static class Buffers
{
    public struct S0<T> : IBuffer<T>
    {
        public static int Capacity => 0;
        public Span<T> Content => Span<T>.Empty;
    }

    [InlineArray(4)]
    public struct S4<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 4;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(15)]
    public struct S15<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 15;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(16)]
    public struct S16<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 16;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(32)]
    public struct S32<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 32;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }
}

#endif