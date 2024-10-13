#if NET8_0_OR_GREATER

using Microsoft.Extensions.ObjectPool;

namespace BTreeSlim;

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer>
{
    private abstract partial class Node
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

            public override AdditionResult<T> TryAdd(T item)
            {
                // todo check completely erased in Release
                Contract.Assert(!IsFull);

                var key = item.Key;
                var i = items.Count - 1;
                for (; i >= 0; i--)
                {
                    var comparison = key.CompareTo(items.Items[i].Key);
                    if (comparison == 0)
                    {
                        return new AdditionResult<T>(ref items.Items[i]);
                    }

                    if (comparison > 0)
                    {
                        break;
                    }
                }

                var itemIndex = i + 1;

                items.InsertAt(itemIndex, item);
                return AdditionResult<T>.Success;
            }

            public override (T MiddleItem, Node NewRightNode) Split()
            {
                // todo check completely erased in Release
                Contract.Assert(IsFull);

                var central = items.Items[MinimumDegree - 1];
                // todo pool
                var newRightNode = new Leaf();

                newRightNode.items.Add(items.Items[MinimumDegree..]);
                items.ShrinkTo(MinimumDegree - 1);
                return (central, newRightNode);
            }

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