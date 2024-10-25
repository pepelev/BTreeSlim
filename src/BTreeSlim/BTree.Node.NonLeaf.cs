#if NET8_0_OR_GREATER

using Microsoft.Extensions.ObjectPool;

namespace BTreeSlim;

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer>
{
    internal abstract partial class Node
    {
        public sealed class NonLeaf : Node
        {
            // todo count stored twice
            internal BufferList<T, TItemsBuffer> items;
            internal BufferList<object, TChildrenBuffer> children;

            public sealed class Policy : IPooledObjectPolicy<NonLeaf>
            {
                public static Policy Singleton { get; } = new();

                public NonLeaf Create() => new();

                public bool Return(NonLeaf obj)
                {
                    obj.items.Clear();
                    obj.children.Clear();
                    return true;
                }
            }

            public override Span<T> Items => items.Items;
            public override bool IsFull => children.IsFull;

            public override void EnsureValid(bool isRoot)
            {
                Contract.Assert(children.Count == items.Count + 1);
                if (!isRoot)
                {
                    Contract.Assert(MinimumItems <= items.Count);
                }

                // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                Contract.Assert(items.Items.ToArray().All(item => item is not null));
                Contract.Assert(children.Items.ToArray().All(item => item is not null));
                // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                
                Contract.Assert(
                    children.Items.ToArray().Select(value => value.GetType()).Distinct().Count() == 1
                );
            }
        }
    }
}

#endif