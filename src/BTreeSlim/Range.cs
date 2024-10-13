namespace BTreeSlim;

public static class Range
{
    public static Range<T> Point<T>(T point) where T : IComparable<T> =>
        new(Border.Inclusive(point), Border.Inclusive(point));

    public static Range<T> Create<T>(Border<T> left, Border<T> right) where T : IComparable<T> => new(left, right);
}

public readonly struct Range<T> where T : IComparable<T>
{
    private readonly Border<T> left;
    private readonly Border<T> right;
    public static Range<T> Universal => new(Border<T>.Infinite, Border<T>.Infinite);

    public Range(Border<T> left, Border<T> right)
    {
        if (!Border<T>.IsValidRange(left, right))
        {
            throw new ArgumentException("Provided borders is not valid range");
        }

        this.left = left;
        this.right = right;
    }

    public Border<T>.Sided Left => left.AsLeft();
    public Border<T>.Sided Right => right.AsRight();
    public bool Contain(T value) => Left.Contain(value) && Right.Contain(value);
    public override string ToString() => $"{Left.ToString()}; {Right.ToString()}";
}