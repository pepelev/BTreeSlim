namespace BTreeSlim;

public static class Border
{
    public static Border<T> Inclusive<T>(T bound) where T : IComparable<T> => new(BorderKind.Inclusive, bound);
    public static Border<T> Exclusive<T>(T bound) where T : IComparable<T> => new(BorderKind.Exclusive, bound);
}

public enum BorderSide : sbyte
{
    Left = -1,
    Right = 1
}

public readonly struct Border<T> where T : IComparable<T>
{
    public BorderKind Kind { get; }
    public T Value { get; }

    internal Border(BorderKind kind, T value)
    {
        Kind = kind;
        Value = value;
    }

    public static Border<T> Infinite => new(BorderKind.Infinite, default!);

    public bool IsInfinite => Kind == BorderKind.Infinite;

    public bool IsInclusive(out T border)
    {
        border = Value;
        return Kind == BorderKind.Inclusive;
    }

    public bool IsExclusive(out T border)
    {
        border = Value;
        return Kind == BorderKind.Exclusive;
    }

    public Sided AsLeft() => new(this, BorderSide.Left);
    public Sided AsRight() => new(this, BorderSide.Right);

    // todo maybe range type
    // todo allow empty range?
    internal static bool IsValidRange(Border<T> left, Border<T> right)
    {
        if (left.Kind == BorderKind.Infinite || right.Kind == BorderKind.Infinite)
        {
            return true;
        }

        return left.Kind == BorderKind.Inclusive && right.Kind == BorderKind.Inclusive
            ? Comparer<T>.Default.Compare(left.Value, right.Value) <= 0
            : Comparer<T>.Default.Compare(left.Value, right.Value) < 0;
    }

    public readonly struct Sided
    {
        private readonly Border<T> border;

        internal Sided(Border<T> border, BorderSide side)
        {
            this.border = border;
            Side = side;
        }

        public BorderSide Side { get; }

        public bool Contain(T value)
        {
            if (border.Kind == BorderKind.Infinite)
            {
                return true;
            }

            var result = Math.Sign(Comparer<T>.Default.Compare(border.Value, value));
            return result == (int)Side ||
                   result == 0 && border.Kind == BorderKind.Inclusive;
        }

        public bool IsInfinite => border.IsInfinite;
        public bool IsInclusive(out T value) => border.IsInclusive(out value); 
        public bool IsExclusive(out T value) => border.IsExclusive(out value);

        public override string ToString() => (side: Side, kind: border.Kind) switch
        {
            (BorderSide.Left, BorderKind.Infinite) => "(-inf",
            (BorderSide.Left, BorderKind.Inclusive) => $"[{border.Value}",
            (BorderSide.Left, BorderKind.Exclusive) => $"({border.Value}",

            (BorderSide.Right, BorderKind.Infinite) => "+inf)",
            (BorderSide.Right, BorderKind.Inclusive) => $"{border.Value}]",
            (BorderSide.Right, BorderKind.Exclusive) => $"{border.Value})",
            _ => $"malformed {nameof(Border<T>)}.{nameof(Sided)}"
        };
    }
}