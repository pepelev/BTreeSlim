#if NET8_0_OR_GREATER

using Microsoft.Extensions.ObjectPool;

namespace BTreeSlim;

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer>
{
    internal abstract partial class Node
    {
        public sealed class Leaf : Node
        {
            internal BufferList<T, TItemsBuffer> items;

            public sealed class Policy : IPooledObjectPolicy<Leaf>
            {
                public static Policy Singleton { get; } = new();

                public Leaf Create() => new();

                public bool Return(Leaf obj)
                {
                    obj.items.Clear();
                    return true;
                }
            }

            public override Span<T> Items => items.Items;
            public override bool IsFull => items.IsFull;
            public bool IsEmpty => items.Count == 0;
            public bool ItemsEnoughToTakeAsNonRoot => MinimumItems < items.Count;

            public override void EnsureValid(bool isRoot)
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                Contract.Assert(items.Items.ToArray().All(item => item is not null));
                // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

                if (!isRoot)
                {
                    Contract.Assert(MinimumItems <= items.Count);
                }
            }
        }
    }
}

#endif