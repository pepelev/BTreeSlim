namespace BTreeSlim.Tests;

public sealed class SortedListCursor<TKey, T>
    where TKey : IComparable<TKey>
{
    private readonly SortedList<TKey, T> list;
    private int index;
    private Range<TKey> range = Range<TKey>.Universal;

    public SortedListCursor(SortedList<TKey, T> list)
    {
        this.list = list;
    }

    public Range<TKey> Range
    {
        get => range;
        set
        {
            if (State == CursorState.InsideTree)
            {
                if (!value.Contain(Current.Key))
                {
                    MoveToStart();
                }
            }

            range = value;
        }
    }

    public void MoveToStart()
    {
        index = -1;
    }

    public void MoveToEnd()
    {
        index = -2;
    }
    
    public void Move(Move move)
    {
        if (move == BTreeSlim.Move.Next)
        {
            MoveNext();
        }
        else if (move == BTreeSlim.Move.Previous)
        {
            MovePrevious();
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(move), move, "wrong");
        }
    }

    public void MoveNext()
    {
        if (index == -2)
        {
            return;
        }

        index += 1;
        for (; index < list.Values.Count; index++)
        {
            if (Range.Contain(list.Keys[index]))
            {
                return;
            }
        }

        index = -2;
    }

    public void MovePrevious()
    {
        if (index == -1)
        {
            return;
        }

        if (index == -2)
        {
            index = list.Count - 1;
        }

        for (; index >= 0; index--)
        {
            if (Range.Contain(list.Keys[index]))
            {
                return;
            }
        }

        index = -1;
    }

    public CursorState State => index switch
    {
        -1 => CursorState.BeforeTree,
        -2 => CursorState.AfterTree,
        _ => CursorState.InsideTree
    };

    public Pair<TKey, T> Current => new(list.Keys[index], list.Values[index]);
}