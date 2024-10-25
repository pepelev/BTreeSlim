#if NET8_0_OR_GREATER

namespace BTreeSlim;

public sealed partial class BTree<TKey, T, TItemsBuffer, TChildrenBuffer>
{
    public struct Cursor : ICursor<TKey, T>
    {
        private readonly BTree<TKey, T, TItemsBuffer, TChildrenBuffer> tree;
        private long version;
        private TKey? currentKey = default;
        private Move lastMove = BTreeSlim.Move.Previous;
        private Move moveOnRestoreOnKeyDeletion = BTreeSlim.Move.Next;
        private int index = -1;
        private Range<TKey> range = Range<TKey>.Universal;

        private StackSlim<(int ChildIndex, Node Node), Buffers.S32<(int ChildIndex, Node Node)>> stack = new(
            new Buffers.S32<(int ChildIndex, Node Node)>()
        );


        // ┌┬────┬┬────┬┬────┬┬────┬┐
        // ││ 10 ││ 11 ││ 12 ││ 13 ││
        // └┴────┴┴────┴┴────┴┴────┴┘
        // 

        public Cursor(BTree<TKey, T, TItemsBuffer, TChildrenBuffer> tree)
        {
            this.tree = tree;
            version = tree.version;
        }

        internal void EnsureValid()
        {
            var p = !typeof(T).IsValueType && stack.IsEmpty == (currentKey == null);
        }

        public Range<TKey> Range
        {
            readonly get => range;
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

        public Move MoveOnRestoreOnKeyDeletion
        {
            readonly get => moveOnRestoreOnKeyDeletion;
            set
            {
                EnsureValid(value);
                moveOnRestoreOnKeyDeletion = value;
            }
        }

        public void MoveToStart()
        {
            stack.Clear();
            lastMove = BTreeSlim.Move.Previous;
            currentKey = default;
        }

        public void MoveToEnd()
        {
            stack.Clear();
            lastMove = BTreeSlim.Move.Next;
            currentKey = default;
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
            MoveNextInternal();
            lastMove = BTreeSlim.Move.Next;
            var right = Range.Right;
            if (!right.IsInfinite && State == CursorState.InsideTree)
            {
                var key = Current.Key;
                if (!right.Contain(key))
                {
                    MoveToEnd();
                }
            }

            if (State == CursorState.InsideTree)
            {
                currentKey = Current.Key;
            }
        }

        private void MoveNextInternal()
        {
            switch (State)
            {
                case CursorState.BeforeTree:
                    FindMostLeftByRange();
                    return;
                case CursorState.InsideTree:
                    var peak = stack.Peek();
                    if (peak.Node is Node.Leaf leaf)
                    {
                        index++;
                        if (index < leaf.items.Count)
                        {
                            return;
                        }

                        while (true)
                        {
                            var childIndex = stack.Peek().ChildIndex;
                            stack.Pop();

                            if (stack.IsEmpty)
                            {
                                return;
                            }

                            var upperNode = Contract.As<Node.NonLeaf>(stack.Peek().Node);
                            if (childIndex < upperNode.children.Count - 1)
                            {
                                index = childIndex;
                                return;
                            }
                        }
                    }

                {
                    var nonLeaf = Contract.As<Node.NonLeaf>(peak.Node);
                    var childIndex = index + 1;
                    var child = Contract.As<Node>(nonLeaf.children.Items[childIndex]);
                    PushMostLeft(childIndex, child);
                }
                    return;
            }
        }

        public void MovePrevious()
        {
            MovePreviousInternal();
            lastMove = BTreeSlim.Move.Next;
            var left = Range.Left;
            if (!left.IsInfinite && State == CursorState.InsideTree)
            {
                var key = Current.Key;
                if (!left.Contain(key))
                {
                    MoveToStart();
                }
            }

            if (State == CursorState.InsideTree)
            {
                currentKey = Current.Key;
            }
        }

        private void MovePreviousInternal()
        {
            switch (State)
            {
                case CursorState.AfterTree:
                    FindMostRightByRange();
                    return;
                case CursorState.InsideTree:
                    var peak = stack.Peek();
                    if (peak.Node is Node.Leaf)
                    {
                        index--;
                        if (0 <= index)
                        {
                            return;
                        }

                        while (true)
                        {
                            var childIndex = stack.Peek().ChildIndex;
                            stack.Pop();

                            if (stack.IsEmpty)
                            {
                                return;
                            }

                            if (0 < childIndex)
                            {
                                index = childIndex;
                                return;
                            }
                        }
                    }

                {
                    var nonLeaf = Contract.As<Node.NonLeaf>(peak.Node);
                    var childIndex = index;
                    var child = Contract.As<Node>(nonLeaf.children.Items[childIndex]);
                    PushMostRight(childIndex, child);
                }
                    return;
            }
        }

        // todo move upper
        private void FindMostLeftByRange()
        {
            var left = range.Left;
            if (left.IsInfinite)
            {
                PushMostLeft(childIndex: -1, tree.root);
                return;
            }

            var root = tree.root;
            if (root == null)
            {
                return;
            }

            var inclusive = left.IsInclusive(out var border);
            if (!inclusive)
            {
                _ = left.IsExclusive(out border);
            }

            _ = Lookup(ref this, childIndex: -1, node: root);

            bool Lookup(ref Cursor @this, int childIndex, Node node)
            {
                @this.stack.Push((childIndex, node));

                if (node is Node.NonLeaf nonLeaf)
                {
                    var items = nonLeaf.items.Items;
                    var children = nonLeaf.children.Items;
                    // todo more performant search
                    for (var i = 0; i < items.Length; i++)
                    {
                        var item = items[i];
                        var comparison = border.CompareTo(item.Key);
                        if (comparison < 0)
                        {
                            var leftChild = Contract.As<Node>(children[i]);
                            if (Lookup(ref @this, childIndex: i, leftChild))
                            {
                                return true;
                            }

                            @this.index = i;
                            return true;
                        }

                        if (comparison == 0)
                        {
                            if (inclusive)
                            {
                                @this.index = i;
                                return true;
                            }

                            var rightChild = Contract.As<Node>(children[i + 1]);
                            @this.PushMostLeft(childIndex: i + 1, rightChild);
                            return true;
                        }
                    }

                    var lastChildIndex = children.Length - 1;
                    var lastChild = Contract.As<Node>(children[lastChildIndex]);
                    var found = Lookup(ref @this, childIndex: lastChildIndex, lastChild);
                    if (!found)
                    {
                        @this.stack.Pop();
                    }

                    return found;
                }
                else
                {
                    var leaf = Contract.As<Node.Leaf>(node);
                    var items = leaf.items.Items;
                    // todo more performant search
                    for (var i = 0; i < items.Length; i++)
                    {
                        var key = items[i].Key;
                        if (left.Contain(key))
                        {
                            @this.index = i;
                            return true;
                        }
                    }

                    @this.stack.Pop();
                    return false;
                }
            }
        }

        private void FindMostRightByRange()
        {
            var right = range.Right;
            if (right.IsInfinite)
            {
                PushMostRight(childIndex: -1, tree.root);
                return;
            }

            var root = tree.root;
            if (root == null)
            {
                return;
            }

            var inclusive = right.IsInclusive(out var border);
            if (!inclusive)
            {
                _ = right.IsExclusive(out border);
            }

            _ = Lookup(ref this, childIndex: -1, node: root);

            bool Lookup(ref Cursor @this, int childIndex, Node node)
            {
                @this.stack.Push((childIndex, node));

                if (node is Node.NonLeaf nonLeaf)
                {
                    var items = nonLeaf.items.Items;
                    var children = nonLeaf.children.Items;
                    // todo more performant search
                    for (var i = items.Length - 1; i >= 0; i--)
                    {
                        var item = items[i];
                        var comparison = item.Key.CompareTo(border);
                        if (comparison < 0)
                        {
                            var rightChildIndex = i + 1;
                            var rightChild = Contract.As<Node>(children[rightChildIndex]);
                            if (Lookup(ref @this, childIndex: rightChildIndex, rightChild))
                            {
                                return true;
                            }

                            @this.index = i;
                            return true;
                        }

                        if (comparison == 0)
                        {
                            if (inclusive)
                            {
                                @this.index = i;
                                return true;
                            }

                            var leftChild = Contract.As<Node>(children[i]);
                            @this.PushMostRight(childIndex: i, leftChild);
                            return true;
                        }
                    }

                    var firstChildIndex = 0;
                    var firstChild = Contract.As<Node>(children[firstChildIndex]);
                    var found = Lookup(ref @this, childIndex: firstChildIndex, firstChild);
                    if (!found)
                    {
                        @this.stack.Pop();
                    }

                    return found;
                }
                else
                {
                    var leaf = Contract.As<Node.Leaf>(node);
                    var items = leaf.items.Items;
                    // todo more performant search
                    for (var i = 0; i < items.Length; i++)
                    {
                        var key = items[i].Key;
                        if (right.Contain(key))
                        {
                            @this.index = i;
                            return true;
                        }
                    }

                    @this.stack.Pop();
                    return false;
                }
            }
        }

        private void PushMostLeft(int childIndex, Node? node)
        {
            if (node == null)
            {
                return;
            }

            while (true)
            {
                stack.Push((childIndex, node));
                if (node is Node.NonLeaf nonLeaf)
                {
                    node = Contract.As<Node>(nonLeaf.children.Items[0]);
                    childIndex = 0;
                }
                else
                {
                    index = 0;
                    return;
                }
            }
        }

        private void PushMostRight(int childIndex, Node? node)
        {
            if (node == null)
            {
                return;
            }

            while (true)
            {
                stack.Push((childIndex, node));
                if (node is Node.NonLeaf nonLeaf)
                {
                    var children = nonLeaf.children;
                    childIndex = children.Count - 1;
                    node = Contract.As<Node>(children.Items[childIndex]);
                }
                else
                {
                    var leaf = Contract.As<Node.Leaf>(node);
                    index = leaf.items.Count - 1;
                    return;
                }
            }
        }

        public CursorState State
        {
            get
            {
                RestoreIfOutdated();

                return (stack.IsEmpty, lastMove) switch
                {
                    (true, BTreeSlim.Move.Previous) => CursorState.BeforeTree,
                    (true, BTreeSlim.Move.Next) => CursorState.AfterTree,
                    _ => CursorState.InsideTree
                };
            }
        }

        private void RestoreIfOutdated()
        {
            if (!stack.IsEmpty && version != tree.version)
            {
                Restore();
            }
        }

        private void Restore()
        {
            var originalRange = Range;
            if (moveOnRestoreOnKeyDeletion == BTreeSlim.Move.Next)
            {
                MoveToStart();
                Range = BTreeSlim.Range.Create(Border.Inclusive(currentKey!), Border<TKey>.Infinite);
                MoveNext();
            }
            else
            {
                MoveToEnd();
                Range = BTreeSlim.Range.Create(Border<TKey>.Infinite, Border.Inclusive(currentKey!));
                MovePrevious();
            }

            Range = originalRange;
            version = tree.version;
            if (State != CursorState.InsideTree)
            {
                currentKey = default;
            }
        }

        public ref T Current
        {
            get
            {
                RestoreIfOutdated();

                if (State == CursorState.InsideTree)
                {
                    ref var entry = ref stack.Peek();
                    if (entry.Node is Node.Leaf leaf)
                    {
                        return ref leaf.items.Items[index];
                    }

                    var nonLeaf = Contract.As<Node.NonLeaf>(entry.Node);
                    return ref nonLeaf.items.Items[index];
                }

                throw new InvalidOperationException($"{nameof(Cursor)} is in wrong state: {State}");
            }
        }

        public Enumerable AsEnumerable(Move move)
        {
            EnsureValid(move);
            return new Enumerable(move, ref this);
        }

        IEnumerable<T> ICursor<TKey, T>.AsEnumerable(Move move) => AsEnumerable(move);

        private static void EnsureValid(Move move)
        {
            if (move != BTreeSlim.Move.Next && move != BTreeSlim.Move.Previous)
            {
                throw new ArgumentOutOfRangeException(nameof(move), move, "wrong");
            }
        }

        // todo can out of Range
        public void Remove(Move move)
        {
            EnsureValid(move);
            RestoreIfOutdated();

            if (State != CursorState.InsideTree)
            {
                throw new InvalidOperationException("Cursor not point to any value");
            }

            RemoveInternal(move);
            lastMove = move;
            version = ++tree.version;

            tree.pool.ActualReturnDeferred();
            tree.root?.EnsureValid(isRoot: true);
        }

        private void RemoveInternal(Move move)
        {
            var node = stack.Peek().Node;
            if (node is Node.Leaf leaf)
            {
                MakeCurrentNodeHasReserveItem();

                leaf.items.RemoveAt(index);

                if (IsRoot(leaf) && leaf.items.Count == 0)
                {
                    tree.root = null;
                    stack.Clear();
                    tree.pool.DeferReturn(leaf);
                    return;
                }

                // todo index can go out of 0, MaximumItems
                if (move == BTreeSlim.Move.Previous)
                {
                    if (index == 0)
                    {
                        stack.Clear();
                        return;
                    }

                    index--;
                }
            }
            else
            {
                var nonLeaf = Contract.As<Node.NonLeaf>(node);
                var children = nonLeaf.children.Items;
                var leftChild =  Contract.As<Node>(children[index]);

                // if left child or right child has ReserveItem
                // then swap current an closest element from its most closest value
                // and remove from there
                if (MinimumItems < leftChild.Items.Length)
                {
                    ref var itemToDelete = ref nonLeaf.items.Items[index];
                    PushMostRight(childIndex: index, leftChild);
                    ref var donor = ref Current;
                    Ref.Swap(ref itemToDelete, ref donor);

                    // todo is Range considered?
                    RemoveInternal(BTreeSlim.Move.Previous);
                    if (move == BTreeSlim.Move.Next)
                    {
                        // todo replace with no checks version of MoveNext 
                        MoveNext();
                    }

                    return;
                }

                var rightChild = Contract.As<Node>(children[index + 1]);
                if (MinimumItems < rightChild.Items.Length)
                {
                    ref var itemToDelete = ref nonLeaf.items.Items[index];
                    PushMostLeft(childIndex: index + 1, rightChild);
                    ref var donor = ref Current;
                    Ref.Swap(ref itemToDelete, ref donor);

                    // todo is Range considered?
                    // todo maybe store stack and index and use it if 
                    RemoveInternal(BTreeSlim.Move.Next);
                    if (move == BTreeSlim.Move.Previous)
                    {
                        // todo replace with no checks version of MovePrevious
                        MovePrevious();
                    }

                    return;
                }

                MakeCurrentNodeHasReserveItem();
                if (rightChild is Node.Leaf)
                {
                    RemoveCurrentAndMergeLeafChildrenOfCurrentItem(move);
                }
                else
                {
                    MergeNonLeafChildrenOfCurrentItem();
                    RemoveInternal(move);
                }
            }
        }

        private void RemoveCurrentAndMergeLeafChildrenOfCurrentItem(Move move)
        {
            var currentNode = Contract.As<Node.NonLeaf>(stack.Peek().Node);
            var leftChild = Contract.As<Node.Leaf>(currentNode.children.Items[index]);
            var rightChild = Contract.As<Node.Leaf>(currentNode.children.Items[index + 1]);

            Contract.Assert(leftChild.Items.Length == MinimumItems);
            Contract.Assert(rightChild.Items.Length == MinimumItems);

            leftChild.items.Add(rightChild.Items);
            currentNode.items.RemoveAt(index);
            currentNode.children.RemoveAt(index + 1);
            stack.Push((ChildIndex: index, rightChild));
            if (move == BTreeSlim.Move.Next)
            {
                index = MinimumItems;
            }
            else
            {
                index = MinimumItems - 1;
            }

            tree.pool.DeferReturn(rightChild);
        }

        private void MergeNonLeafChildrenOfCurrentItem()
        {
            var currentNode = Contract.As<Node.NonLeaf>(stack.Peek().Node);

            Contract.Assert(IsRoot(currentNode) || MinimumItems < currentNode.items.Count);

            var leftChild = Contract.As<Node.NonLeaf>(currentNode.children.Items[index]);
            var rightChild = Contract.As<Node.NonLeaf>(currentNode.children.Items[index + 1]);

            Contract.Assert(leftChild.items.Count == MinimumItems);
            Contract.Assert(rightChild.items.Count == MinimumItems);
            leftChild.items.Add(currentNode.Items[index]);
            leftChild.items.Add(rightChild.items.Items);
            leftChild.children.Add(rightChild.children.Items);

            currentNode.items.RemoveAt(index);
            currentNode.children.RemoveAt(index + 1);

            if (IsRoot(currentNode) && currentNode.items.Count == 0)
            {
                stack.Pop();
                stack.Push((ChildIndex: -1, leftChild));

                // todo bug?
                index = MinimumItems + 1;

                tree.pool.DeferReturn(currentNode);
            }
            else
            {
                stack.Push((ChildIndex: index, leftChild));

                // todo bug?
                index = MinimumItems + 1;
            }

            tree.pool.DeferReturn(rightChild);
        }


        private void MakeCurrentNodeHasReserveItem()
        {
            var (childIndex, node) = stack.Peek();
            if (IsRoot(node))
            {
                // all root items are reserve
                return;
            }

            if (MinimumItems < node.Items.Length)
            {
                return;
            }

            if (node is Node.Leaf leaf)
            {
                var parent = Contract.As<Node.NonLeaf>(stack[1].Node);
                if (childIndex + 1 < parent.children.Count)
                {
                    if (TryRotateLeft(parent, itemIndex: childIndex))
                    {
                        return;
                    }
                }

                if (0 < childIndex)
                {
                    if (TryRotateRight(parent, itemIndex: childIndex - 1))
                    {
                        index++;
                        return;
                    }
                }

                stack.Pop();
                // todo index = childIndex;
                MakeCurrentNodeHasReserveItem();

                MergeNonLeafChildrenOfCurrentItem();
                
                
                // todo stack.Push();

                
            }
            else
            {
                var parent = Contract.As<Node.NonLeaf>(stack[1].Node);
                if (childIndex + 1 < parent.children.Count)
                {
                    if (TryRotateLeft(parent, itemIndex: childIndex))
                    {
                        return;
                    }
                }

                if (0 < childIndex)
                {
                    if (TryRotateRight(parent, itemIndex: childIndex - 1))
                    {
                        index++;
                        return;
                    }
                }

                stack.Pop();
                childIndex = MakeCurrentNonLeafHasReserveItem(childIndex);
                stack.Push((childIndex, node));
            }
        }

        private void MakeCurrentLeafHasReserveItem()
        {
            var (childIndex, node) = stack.Peek();
            if (IsRoot(node))
            {
                // all root items are reserve
                return;
            }

            var parent = Contract.As<Node.NonLeaf>(stack[1].Node);
            if (childIndex + 1 < parent.children.Count)
            {
                if (TryRotateLeft(parent, itemIndex: childIndex))
                {
                    return;
                }
            }

            if (0 < childIndex)
            {
                if (TryRotateRight(parent, itemIndex: childIndex - 1))
                {
                    return;
                }
            }

            
        }

        private int MakeCurrentNonLeafHasReserveItem(int interestedChildIndex)
        {
            var (childIndex, node) = stack.Peek();
            if (IsRoot(node))
            {
                // all root items are reserve
                return interestedChildIndex;
            }

            // todo interestedChildIndex is really not changed?
            var parent = Contract.As<Node.NonLeaf>(stack[1].Node);
            if (childIndex + 1 < parent.children.Count)
            {
                if (TryRotateLeft(parent, itemIndex: childIndex))
                {
                    return interestedChildIndex;
                }
            }

            if (0 < childIndex)
            {
                if (TryRotateRight(parent, itemIndex: childIndex - 1))
                {
                    return interestedChildIndex;
                }
            }

            stack.Pop();
            childIndex = MakeCurrentNonLeafHasReserveItem(childIndex);
            var oldIndex = index;
            if (childIndex == 0)
            {
                index = 0;
            }
            else
            {
                index = childIndex - 1;
                interestedChildIndex += MinimumItems + 1;
            }

            MergeNonLeafChildrenOfCurrentItem();
            index = oldIndex;
            stack.Push((childIndex, node));
            return interestedChildIndex;
        }

        private bool TryRotateLeft(Node.NonLeaf parent, int itemIndex)
        {
            var children = parent.children.Items;
            var leftChild = children[itemIndex];
            var rightChild = children[itemIndex + 1];

            ref var item = ref parent.Items[itemIndex];
            if (leftChild is Node.Leaf)
            {
                var left = Contract.As<Node.Leaf>(leftChild);
                var right = Contract.As<Node.Leaf>(rightChild);

                if (right.items.Count == MinimumItems)
                {
                    return false;
                }

                Contract.Assert(left.items.Count < MaximumItems);

                left.items.Add(ref item);
                item = right.Items[0];
                right.items.RemoveAt(0);
                return true;
            }
            else
            {
                var left = Contract.As<Node.NonLeaf>(leftChild);
                var right = Contract.As<Node.NonLeaf>(rightChild);

                if (right.items.Count == MinimumItems)
                {
                    return false;
                }

                Contract.Assert(left.items.Count < MaximumItems);

                left.items.Add(ref item);
                item = right.Items[0];
                right.items.RemoveAt(0);

                left.children.Add(right.children.Items[0]);
                right.children.RemoveAt(0);
                return true;
            }
        }

        private bool TryRotateRight(Node.NonLeaf parent, int itemIndex)
        {
            var children = parent.children.Items;
            var leftChild = children[itemIndex];
            var rightChild = children[itemIndex + 1];

            ref var item = ref parent.Items[itemIndex];
            if (leftChild is Node.Leaf)
            {
                var left = Contract.As<Node.Leaf>(leftChild);
                var right = Contract.As<Node.Leaf>(rightChild);

                if (right.items.Count == MinimumItems)
                {
                    return false;
                }

                Contract.Assert(left.items.Count < MaximumItems);

                right.items.InsertAt(0, ref item);
                item = left.Items[^1];
                left.items.RemoveFromTail();
                return true;
            }
            else
            {
                var left = Contract.As<Node.NonLeaf>(leftChild);
                var right = Contract.As<Node.NonLeaf>(rightChild);

                if (right.items.Count == MinimumItems)
                {
                    return false;
                }

                Contract.Assert(left.items.Count < MaximumItems);

                right.items.InsertAt(0, ref item);
                item = left.Items[^1];
                left.items.RemoveFromTail();

                right.children.Add(left.children.Items[^1]);
                left.children.RemoveFromTail();
                return true;
            }
        }

        private bool IsRoot(Node node) => ReferenceEquals(node, tree.root);
    }
}

#endif