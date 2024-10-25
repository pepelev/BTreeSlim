#if NET8_0_OR_GREATER

using System.Collections;

namespace BTreeSlim;

public static class BTree
{
    public static BTree<T, Self<T>, Buffers.S15<Self<T>>, Buffers.S16<object>> Create<T>() where T : IComparable<T>
        => Create<T, Self<T>>();

    public static BTree<TKey, T, Buffers.S15<T>, Buffers.S16<object>> Create<TKey, T>()
        where TKey : IComparable<TKey>
        where T : IKeyed<TKey>
        => new(BTree<TKey, T, Buffers.S15<T>, Buffers.S16<object>>.Pool.CreateDefault(factor: 16));

    public static BTree<TKey, Pair<TKey, T>, Buffers.S15<Pair<TKey, T>>, Buffers.S16<object>> CreateForPairs<TKey, T>()
        where TKey : IComparable<TKey>
        => new(BTree<TKey, Pair<TKey, T>, Buffers.S15<Pair<TKey, T>>, Buffers.S16<object>>.Pool.CreateDefault(factor: 16));
}

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer> : IBTree<TKey, T>
    where TKey : IComparable<TKey>
    where T : IKeyed<TKey>
    where TItemsBuffer : IBuffer<T>
    where TChildrenBuffer : IBuffer<object>
{
    private long version;
    private readonly Pool pool;
    internal Node? root;

    public Cursor CreateCursor() => new(this);

    public AdditionResult<T> TryAdd(T item)
    {
        if (root == null)
        {
            root = pool.GetLeaf();
        }
        else if (root.IsFull)
        {
            var newRoot = pool.GetNonLeaf();
            newRoot.children.Add(root);
            SplitChild(newRoot, 0, root);
            root = newRoot;
        }

        var result = TryAdd(root, item.Key, item);
        version++;
        root.EnsureValid(isRoot: true);
        return result;
    }

    private void SplitChild(Node.NonLeaf parent, int childIndex, Node child)
    {
#if DEBUG
        Contract.Assert(!parent.IsFull);
        Contract.Assert(child.IsFull);
        Contract.Assert(ReferenceEquals(parent.children.Items[childIndex], child));
#endif

        var (middle, newRight) = Split(child);

        parent.items.InsertAt(childIndex, middle);
        parent.children.InsertAt(childIndex + 1, newRight);
    }

    // todo нормальное название
    private (T MiddleItem, Node NewRightNode) Split(Node node)
    {
#if DEBUG
        Contract.Assert(node.IsFull);
#endif

        if (node is Node.Leaf leaf)
        {
            // items: TItemsBuffer.Capacity -> MinimumItems(this) + 1(MiddleItem) + MinimumItems(NewRightNode)

            var central = leaf.items.Items[MinimumItems];
            var newRightNode = pool.GetLeaf();

            newRightNode.items.Add(leaf.items.Items[(MinimumItems + 1)..]);
            leaf.items.ShrinkTo(MinimumItems);
            return (central, newRightNode);
        }
        else
        {
            // items:    TItemsBuffer.Capacity -> MinimumItems(this) + 1(MiddleItem) + MinimumItems(NewRightNode)
            // children: TChildrenBuffer.Capacity -> MinimumChildren(this) + MinimumChildren(NewRightNode)
            var nonLeaf = Contract.As<Node.NonLeaf>(node);
            var centralItem = nonLeaf.items.Items[MinimumItems];
            var newRightNode = pool.GetNonLeaf();

            newRightNode.items.Add(nonLeaf.items.Items[MinimumChildren..]);
            nonLeaf.items.ShrinkTo(MinimumChildren);

            newRightNode.children.Add(nonLeaf.children.Items[MinimumChildren..]);
            nonLeaf.children.ShrinkTo(MinimumChildren);

            return (centralItem, newRightNode);
        }
    }

    private AdditionResult<T> TryAdd(Node node, TKey key, T item)
    {
        while (true)
        {
#if DEBUG
            Contract.Assert(!node.IsFull);
#endif

            if (node is Node.Leaf leaf)
            {
                var i = leaf.items.Count - 1;
                for (; i >= 0; i--)
                {
                    ref var candidateItem = ref leaf.items.Items[i];
                    var comparison = Comparer<TKey>.Default.Compare(key, candidateItem.Key);
                    if (comparison == 0)
                    {
                        return AdditionResult<T>.AlreadyPresent(ref candidateItem);
                    }

                    if (comparison > 0)
                    {
                        break;
                    }
                }

                var itemIndex = i + 1;

                ref var addedItem = ref leaf.items.InsertAtAndGetRef(itemIndex, ref item);
                return AdditionResult<T>.Success(ref addedItem);
            }
            else
            {
                var nonLeaf = Contract.As<Node.NonLeaf>(node);
                var i = nonLeaf.items.Count - 1;
                for (; i >= 0; i--)
                {
                    ref var candidateItem = ref nonLeaf.items.Items[i];
                    var comparison = Comparer<TKey>.Default.Compare(key, candidateItem.Key);
                    if (comparison == 0)
                    {
                        return AdditionResult<T>.AlreadyPresent(ref candidateItem);
                    }

                    if (comparison > 0)
                    {
                        break;
                    }
                }

                var childIndex = i + 1;
                var child = Contract.As<Node>(nonLeaf.children.Items[childIndex]);
                if (!child.IsFull)
                {
                    node = child;
                }
                else
                {
                    SplitChild(nonLeaf, childIndex, child);
                    ref var middleOfSplit = ref nonLeaf.items.Items[childIndex];
                    var comparison = Comparer<TKey>.Default.Compare(key, middleOfSplit.Key);
                    if (comparison == 0)
                    {
                        return AdditionResult<T>.AlreadyPresent(ref middleOfSplit);
                    }

                    if (comparison > 0)
                    {
                        childIndex++;
                    }

                    node = Contract.As<Node>(nonLeaf.children.Items[childIndex]);
                }
            }
        }
    }

    public bool ContainsKey(TKey key) => Find(key).IsFound;

    public FindResult<T> Find(TKey key)
    {
        if (root == null)
        {
            return FindResult<T>.NotFound;
        }

        var node = root;
        m1: while (true)
        {
            if (node is Node.Leaf leaf)
            {
                var items = leaf.items.Items;
                for (var i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (Comparer<TKey>.Default.Compare(item.Key, key) == 0)
                    {
                        return new FindResult<T>(ref items[i]);
                    }
                }

                return FindResult<T>.NotFound;
            }
            else
            {
                var nonLeaf = Contract.As<Node.NonLeaf>(node);
                var items = nonLeaf.Items;
                for (var i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (Comparer<TKey>.Default.Compare(key, item.Key) < 0)
                    {
                        node = Contract.As<Node>(nonLeaf.children.Items[i]);
                        goto m1; // todo
                    }
                }

                node = Contract.As<Node>(nonLeaf.children.Items[^1]);
            }
        }
    }

    public void Remove(TKey key)
    {
        throw new NotImplementedException();
    }

    public void Clear(bool returnNodesToPool = true)
    {
        (var previousRoot, root) = (root, null);
        // todo Count = 0;
        version++;

        if (returnNodesToPool && previousRoot != null)
        {
            Return(previousRoot);

            void Return(Node node)
            {
                if (node is Node.Leaf leaf)
                {
                    pool.Return(leaf);
                    return;
                }

                var nonLeaf = Contract.As<Node.NonLeaf>(node);
                foreach (var child in nonLeaf.children.Buffer)
                {
                    Return(Contract.As<Node>(child));
                }

                pool.Return(nonLeaf);
            }
        }
    }

    ICursor<TKey, T> IBTree<TKey, T>.CreateCursor() => CreateCursor();

    static BTree()
    {
        if (TChildrenBuffer.Capacity != TItemsBuffer.Capacity + 1)
        {
            throw new Exception($"{nameof(TChildrenBuffer)} and {nameof(TItemsBuffer)} capacities are incompatible");
        }

        if (TChildrenBuffer.Capacity % 2 != 0)
        {
            throw new Exception($"{nameof(TChildrenBuffer)}.{nameof(TChildrenBuffer.Capacity)} must be even");
        }
    }

    public BTree(Pool pool)
    {
        this.pool = pool;
    }

    private static int MinimumDegree => TChildrenBuffer.Capacity / 2;
    private static int MinimumItems => MinimumDegree - 1;
    private static int MaximumItems => 2 * MinimumDegree - 1;

    private static int MinimumChildren => MinimumDegree;
    private static int MaximumChildren => 2 * MinimumDegree;

    public int Count => throw new NotImplementedException();
    public Enumerator GetEnumerator() => CreateCursor().AsEnumerable(Move.Next).GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

#endif