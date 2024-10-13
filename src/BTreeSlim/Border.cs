namespace BTreeSlim;

public static class Border
{
    public static Border<T> Inclusive<T>(T bound) where T : IComparable<T> => new(BorderKind.Inclusive, bound);
    public static Border<T> Exclusive<T>(T bound) where T : IComparable<T> => new(BorderKind.Exclusive, bound);
}

public readonly struct Border<T> where T : IComparable<T>
{
    private const sbyte LeftSide = -1;
    private const sbyte RightSide = 1;

    private readonly BorderKind kind;
    private readonly T value;

    internal Border(BorderKind kind, T value)
    {
        this.kind = kind;
        this.value = value;
    }

    public static Border<T> Infinite => new(BorderKind.Infinite, default!);

    public bool IsInfinite => kind == BorderKind.Infinite;

    public bool IsInclusive(out T border)
    {
        border = value;
        return kind == BorderKind.Inclusive;
    }

    public bool IsExclusive(out T border)
    {
        border = value;
        return kind == BorderKind.Exclusive;
    }

    public Sided AsLeft() => new(this, LeftSide);
    public Sided AsRight() => new(this, RightSide);

    // todo maybe range type
    // todo allow empty range?
    internal static bool IsValidRange(Border<T> left, Border<T> right)
    {
        if (left.kind == BorderKind.Infinite || right.kind == BorderKind.Infinite)
        {
            return true;
        }

        return left.kind == BorderKind.Inclusive && right.kind == BorderKind.Inclusive
            ? Comparer<T>.Default.Compare(left.value, right.value) <= 0
            : Comparer<T>.Default.Compare(left.value, right.value) < 0;
    }

    public readonly struct Sided
    {
        private readonly Border<T> border;
        private readonly sbyte side;

        internal Sided(Border<T> border, sbyte side)
        {
            this.border = border;
            this.side = side;
        }

        public bool Contain(T value)
        {
            if (border.kind == BorderKind.Infinite)
            {
                return true;
            }

            var result = Math.Sign(Comparer<T>.Default.Compare(border.value, value));
            return result == side ||
                   result == 0 && border.kind == BorderKind.Inclusive;
        }

        public bool IsInfinite => border.IsInfinite;
        public bool IsInclusive(out T value) => border.IsInclusive(out value); 
        public bool IsExclusive(out T value) => border.IsInclusive(out value);

        public override string ToString() => (side, border.kind) switch
        {
            (LeftSide, BorderKind.Infinite) => "(-inf",
            (LeftSide, BorderKind.Inclusive) => $"[{border.value}",
            (LeftSide, BorderKind.Exclusive) => $"({border.value}",

            (RightSide, BorderKind.Infinite) => "+inf)",
            (RightSide, BorderKind.Inclusive) => $"{border.value}]",
            (RightSide, BorderKind.Exclusive) => $"{border.value})",
            _ => $"malformed {nameof(Border<T>)}.{nameof(Sided)}"
        };
    }
}