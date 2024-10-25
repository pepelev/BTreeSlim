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

    [InlineArray(31)]
    public struct S31<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 31;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(32)]
    public struct S32<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 32;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(63)]
    public struct S63<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 63;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(64)]
    public struct S64<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 64;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(127)]
    public struct S127<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 127;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(128)]
    public struct S128<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 128;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(255)]
    public struct S255<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 255;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(256)]
    public struct S256<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 256;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(511)]
    public struct S511<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 511;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }

    [InlineArray(512)]
    public struct S512<T> : IBuffer<T>
    {
        private T value0;

        public static int Capacity => 512;
        public Span<T> Content => MemoryMarshal.CreateSpan(ref value0, Capacity);
    }
}

#endif