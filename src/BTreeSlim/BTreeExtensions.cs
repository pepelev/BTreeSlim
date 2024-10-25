#if NET8_0_OR_GREATER
namespace BTreeSlim;

internal static class BTreeExtensions
{
    public static FindResult<T> FindFirstOnRight<TPrefix, TKey, T, TItemsBuffer, TChildrenBuffer>(
        this BTree<TKey, T, TItemsBuffer, TChildrenBuffer> tree,
        Border<TPrefix> border
    )
        where TPrefix : IComparable<TPrefix>
        where TKey : ICompoundKey<TPrefix, TKey>, IComparable<TKey>
        where T : IKeyed<TKey>
        where TItemsBuffer : IBuffer<T>
        where TChildrenBuffer : IBuffer<object>
    {
        var rootNode = tree.root;
        if (rootNode == null)
        {
            return FindResult<T>.NotFound;
        }

        if (border.Kind == BorderKind.Infinite)
        {
            var node = rootNode;
            while (node is BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node.NonLeaf nonLeaf)
            {
                var nextNode = nonLeaf.children.Items[0];
                node = Contract.As<BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node>(nextNode);
            }

            var leaf = Contract.As<BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node.Leaf>(node);
            return new FindResult<T>(ref leaf.Items[0]);
        }

        var comparisonBorder = border.Kind == BorderKind.Inclusive
            ? 0
            : 1;
        var prefix = border.Value;
        return Find(rootNode);

        FindResult<T> Find(BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node node)
        {
            if (node is BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node.NonLeaf nonLeaf)
            {
                var items = nonLeaf.Items;
                var childrenItems = nonLeaf.children.Items;
                for (var i = 0; i < items.Length; i++) // todo bin-search
                {
                    var item = items[i];
                    var comparison = TKey.Compare(prefix, item.Key);
                    if (comparison < 0 || comparison == 0 && border.Kind == BorderKind.Inclusive)
                    {
                        var leftChild = Contract.As<BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node>(childrenItems[i]);
                        var result = Find(leftChild);
                        if (result.IsFound)
                        {
                            return result;
                        }

                        return new FindResult<T>(ref items[i]);
                    }

                    if (comparison == 0 && border.Kind == BorderKind.Exclusive)
                    {
                        continue;
                    }

                    return FindResult<T>.NotFound;
                }

                var mostRightChild = Contract.As<BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node>(childrenItems[^1]);
                return Find(mostRightChild);
            }
            else
            {
                var leaf = Contract.As<BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node.Leaf>(node);
                var items = leaf.Items;
                for (var i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (TKey.Compare(prefix, item.Key) >= comparisonBorder) //todo bin-search
                    {
                        return new FindResult<T>(ref items[i]);
                    }
                }

                return FindResult<T>.NotFound;
            }
        }
    }

    public static FindResult<T> FindLastOnLeft<TPrefix, TKey, T, TItemsBuffer, TChildrenBuffer>(
        this BTree<TKey, T, TItemsBuffer, TChildrenBuffer> tree,
        Border<TPrefix> border
    )
        where TPrefix : IComparable<TPrefix>
        where TKey : ICompoundKey<TPrefix, TKey>, IComparable<TKey>
        where T : IKeyed<TKey>
        where TItemsBuffer : IBuffer<T>
        where TChildrenBuffer : IBuffer<object>
    {
        var node = tree.root;
        if (node == null)
        {
            return FindResult<T>.NotFound;
        }

        if (border.IsInfinite)
        {
            while (node is BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node.NonLeaf nonLeaf)
            {
                var nextNode = nonLeaf.children.Items[^1];
                node = Contract.As<BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node>(nextNode);
            }

            var leaf = Contract.As<BTree<TKey, T, TItemsBuffer, TChildrenBuffer>.Node.Leaf>(node);
            return new FindResult<T>(ref leaf.Items[^1]);
        }

        throw new NotImplementedException();
    }
}
#endif