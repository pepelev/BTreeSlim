#if NET8_0_OR_GREATER

using System.Collections;
using Microsoft.Extensions.ObjectPool;

namespace BTreeSlim;

public static class BTree
{
    public static BTree<T, Self<T>, Buffers.S15<Self<T>>, Buffers.S16<object>> Create<T>() where T : IComparable<T>
        => Create<T, Self<T>>();

    public static BTree<TKey, T, Buffers.S15<T>, Buffers.S16<object>> Create<TKey, T>()
        where TKey : IComparable<TKey>
        where T : IKeyed<TKey>
        => new(16 /* todo */ );
    public static BTree<TKey, Pair<TKey, T>, Buffers.S15<Pair<TKey, T>>, Buffers.S16<object>> CreateForPairs<TKey, T>()
        where TKey : IComparable<TKey>
        => new(16 /* todo */ );
}

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer> : IBTree<TKey, T>
    where TKey : IComparable<TKey>
    where T : IKeyed<TKey>
    where TItemsBuffer : IBuffer<T>
    where TChildrenBuffer : IBuffer<object>
{
    private long version;
    private readonly Pool pool;
    private Node? root;

    public Cursor CreateCursor() => new(this);

    public AdditionResult<T> TryAdd(T item)
    {
        if (root == null)
        {
            root = pool.GetLeaf();
        }

        if (root.IsFull)
        {
            var newRoot = pool.GetNonLeaf();
            newRoot.children.Buffer[0] = root;
            newRoot.children.Count = 1;
            newRoot.SplitChild(0);
            root = newRoot;
        }

        var result = root.TryAdd(item);
        if (result.IsSuccess)
        {
            version++;
        }

        root.EnsureValid(isRoot: true);
        return result;
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

    public BTree(int poolFactor)
    {
        pool = new Pool(
            new DefaultObjectPool<Node.Leaf>(Node.Leaf.Policy.Singleton, poolFactor * 2 * MinimumDegree),
            new DefaultObjectPool<Node.NonLeaf>(Node.NonLeaf.Policy.Singleton, poolFactor)
        );
    }

    private static int MinimumDegree => TChildrenBuffer.Capacity / 2;
    private static int MinimumItems => MinimumDegree - 1;
    private static int MaximumItems => 2 * MinimumDegree - 1;

    private static int MinimumChildren => MinimumDegree;
    private static int MaximumChildren => 2 * MinimumDegree;

    private sealed class Pool
    {
        private BufferList<Node.Leaf, Buffers.S4<Node.Leaf>> leafs = new(new Buffers.S4<Node.Leaf>());
        private BufferList<Node.NonLeaf, Buffers.S32<Node.NonLeaf>> nonLeafs = new(new Buffers.S32<Node.NonLeaf>());

        public Pool(ObjectPool<Node.Leaf> leafs, ObjectPool<Node.NonLeaf> nonLeafs)
        {
            Leafs = leafs;
            NonLeafs = nonLeafs;
        }

        // todo allocate in advance for Adding?
        public void DeferReturn(Node.Leaf leaf)
        {
            if (leafs.IsFull)
            {
                return;
            }

            leafs.Add(leaf);
        }

        public void DeferReturn(Node.NonLeaf nonLeaf)
        {
            if (nonLeafs.IsFull)
            {
                return;
            }

            nonLeafs.Add(nonLeaf);
        }

        public void ActualReturn()
        {
            try
            {
                foreach (var leaf in leafs.Items)
                {
                    Leafs.Return(leaf);
                }

                foreach (var nonLeaf in nonLeafs.Items)
                {
                    NonLeafs.Return(nonLeaf);
                }
            }
            finally
            {
                leafs.Clear();
                nonLeafs.Clear();
            }
        }

        public Node.Leaf GetLeaf() => Leafs.Get();
        public Node.NonLeaf GetNonLeaf() => NonLeafs.Get();

        private ObjectPool<Node.Leaf> Leafs { get; }
        private ObjectPool<Node.NonLeaf> NonLeafs { get; }
    }

    public int Count => throw new NotImplementedException();
    public Enumerator GetEnumerator() => CreateCursor().AsEnumerable(Move.Next).GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

#endif