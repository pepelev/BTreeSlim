using Microsoft.Extensions.ObjectPool;

namespace BTreeSlim;

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer>
{
    // todo allocate in advance for Adding?
    public sealed class Pool
    {
        private BufferList<Node.Leaf, Buffers.S4<Node.Leaf>> leafs = new(new Buffers.S4<Node.Leaf>());
        private BufferList<Node.NonLeaf, Buffers.S32<Node.NonLeaf>> nonLeafs = new(new Buffers.S32<Node.NonLeaf>());

        private Pool(ObjectPool<Node.Leaf> leafs, ObjectPool<Node.NonLeaf> nonLeafs)
        {
            Leafs = leafs;
            NonLeafs = nonLeafs;
        }

        public static Pool Create(ObjectPoolProvider objectPoolProvider) =>
            Create(objectPoolProvider, objectPoolProvider);

        public static Pool Create(ObjectPoolProvider forLeafs, ObjectPoolProvider forNonLeafs) => new(
            forLeafs.Create(Node.Leaf.Policy.Singleton),
            forNonLeafs.Create(Node.NonLeaf.Policy.Singleton)
        );

        public static Pool CreateDefault(int factor) => new(
            new DefaultObjectPool<Node.Leaf>(Node.Leaf.Policy.Singleton, factor * 2 * MinimumDegree),
            new DefaultObjectPool<Node.NonLeaf>(Node.NonLeaf.Policy.Singleton, factor)
        );

        internal void Return(Node.Leaf leaf)
        {
            Leafs.Return(leaf);
        }

        internal void Return(Node.NonLeaf nonLeaf)
        {
            NonLeafs.Return(nonLeaf);
        }
        internal void DeferReturn(Node.Leaf leaf)
        {
            if (leafs.IsFull)
            {
                return;
            }

            leafs.Add(leaf);
        }

        internal void DeferReturn(Node.NonLeaf nonLeaf)
        {
            if (nonLeafs.IsFull)
            {
                return;
            }

            nonLeafs.Add(nonLeaf);
        }

        public void ActualReturnDeferred()
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

        internal Node.Leaf GetLeaf() => Leafs.Get();
        internal Node.NonLeaf GetNonLeaf() => NonLeafs.Get();

        private ObjectPool<Node.Leaf> Leafs { get; }
        private ObjectPool<Node.NonLeaf> NonLeafs { get; }
    }
}