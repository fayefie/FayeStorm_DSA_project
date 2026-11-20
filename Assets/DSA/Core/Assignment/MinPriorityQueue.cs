using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    // Assignment Phase 3. Implement as a binary min-heap.

    /// <summary>
    /// A minimum-priority queue implemented as a binary min-heap.
    ///
    /// Items with lower integer priority values should be returned before
    /// items with higher priority values.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    /// </summary>
    public sealed class MinPriorityQueue<T>
    {
        // The list stores the physical representation of the binary heap.
        // Your implementation is responsible for preserving the min-heap invariant.
        private readonly List<Entry> _heap = new List<Entry>();

        /// <summary>
        /// Number of items currently stored in the queue.
        /// </summary>
        public int Count => _heap.Count;

        /// <summary>
        /// Adds an item with the supplied priority.
        /// Lower priority values are removed before higher priority values.
        /// </summary>
        public void Enqueue(T item, int priority)
        {
            // TODO: insert the new entry and restore the min-heap invariant.
            _heap.Add(new Entry(item, priority));
            SiftUp(_heap.Count - 1);
        }

        /// <summary>
        /// Returns the minimum-priority item without removing it.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the queue is empty.
        /// </exception>
        public T Peek()
        {
            // TODO: return the current minimum without changing the queue.
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("Peek failed. Priority queue is empty");
            }

            return _heap[0].Item;
        }

        /// <summary>
        /// Removes and returns the minimum-priority item.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the queue is empty.
        /// </exception>
        public T Dequeue()
        {
            // TODO: remove the minimum item and restore the min-heap invariant.
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("Dequeue failed. Priority queue is empty");
            }

            T result = _heap[0].Item;

            int last = _heap.Count - 1;
            _heap[0] = _heap[last];
            _heap.RemoveAt(last);

            if (_heap.Count > 0)
            {
                SiftDown(0);
            }

            return result;
        }

        // restore min-heap invarient by moving _heap[index] toward the root while it is smaller than the parent
        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_heap[parent].Priority <= _heap[index].Priority)
                {
                    break;
                }

                Swap(parent, index);
                index = parent;
            }
        }

        // restore min-heap invarient by moving _heap[index] opposite the root while it is larger than either child
        private void SiftDown(int index)
        {
            int count = _heap.Count;
            while (true)
            {
                int left = 2 * index + 1;
                int right = 1 * index + 2;
                int smallest = index;

                if (left < count && _heap[left].Priority < _heap[smallest].Priority)
                {
                    smallest = left;
                }

                if (right < count && _heap[right].Priority < _heap[smallest].Priority)
                {
                    smallest = right;
                }

                if (smallest == index)
                {
                    break;
                }

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            Entry tmp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = tmp;
        }

        /// <summary>
        /// Stores one item together with the priority used by the heap.
        /// </summary>
        private readonly struct Entry
        {
            public Entry(T item, int priority)
            {
                Item = item;
                Priority = priority;
            }

            public T Item { get; }
            public int Priority { get; }
        }
    }
}
