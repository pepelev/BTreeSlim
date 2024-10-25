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

            // this                         this
            //   | (index)    ->           /    \
            //  targetChild       targetChild  secondPart
            public void SplitChild(int index)
            {
                var targetChild = Contract.As<Node>(children.Items[index]);
                var split = targetChild.Split();

                items.InsertAt(index, split.MiddleItem);
                children.InsertAt(index + 1, split.NewRightNode);
            }

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

                    if (0 < comparison)
                    {
                        break;
                    }
                }

                var childIndex = i + 1;
                var node = Contract.As<Node>(children.Items[childIndex]);
                if (node.IsFull)
                {
                    SplitChild(childIndex);
                    if (items.Items[childIndex].Key.CompareTo(key) < 0)
                    {
                        childIndex++;
                    }
                }

                var result = Contract.As<Node>(children.Items[childIndex]).TryAdd(item);
                return result;
            }

            public override (T MiddleItem, Node NewRightNode) Split()
            {
                // todo check completely erased in Release
                Contract.Assert(items.Count == MaximumItems);

                var centralItem = items.Items[MinimumDegree - 1];
                var newRightNode = new NonLeaf();

                items.Items[MinimumDegree..].CopyTo(newRightNode.items.Buffer);
                items.Items[(MinimumDegree - 1)..].Clear();
                items.Count = MinimumDegree - 1;
                newRightNode.items.Count = MinimumDegree - 1;

                children.Items[MinimumDegree..].CopyTo(newRightNode.children.Buffer);
                children.Items[MinimumDegree..].Clear();
                children.Count = MinimumDegree;
                newRightNode.children.Count = MinimumDegree;

                return (centralItem, newRightNode);
            }

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