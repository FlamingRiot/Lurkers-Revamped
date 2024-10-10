namespace AStar
{
    /// <summary>Implements heap items conditions.</summary>
    /// <typeparam name="T">Type of the item.</typeparam>
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }

    /// <summary>Represents an instance of a <see cref="Heap{T}"/> object.</summary>
    /// <typeparam name="T">Type of the heap.</typeparam>
    public class Heap<T> where T : IHeapItem<T>
    {
        private T[] items;
        private int currentItemCount;

        /// <summary>Creates a <see cref="Heap{T}"/> object.</summary>
        /// <param name="maxSize">Maximum size allowed for the heap.</param>
        public Heap(int maxSize)
        {
            items = new T[maxSize];
        }

        // ------------------------------------------------------------------------
        // Public heap functions
        // ------------------------------------------------------------------------

        /// <summary>Adds an element to the heap at the right position.</summary>
        /// <param name="item">Item of type <see cref="T"/> to add to the heap.</param>
        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            SortUp(item);
            currentItemCount++;
        }

        /// <summary>Clears the items of the heap.</summary>
        public void Clear()
        {
            currentItemCount = 0;
        }

        /// <summary>Removes and returns the first item <see cref="T"/> of the heap.</summary>
        /// <returns>The first item <see cref="T"/> of the heap.</returns>
        public T RemoveFirst()
        {
            T firstItem = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }

        /// <summary>Updates a certain item <see cref="T"/> into the heap.</summary>
        /// <param name="item">Item <see cref="T"/> to update.</param>
        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        /// <summary>Returns the number of items <see cref="T"/> in the heap.</summary>
        public int Count { get { return currentItemCount; } }

        /// <summary>Checks if the heap contains a certain item <see cref="T"/>.</summary>
        /// <param name="item">Item <see cref="T"/>.</param>
        /// <returns><see langword="true"/> if the heap contains the item <see cref="T"/>. <see langword="false"/> otherwise.</returns>
        public bool Contains(T item)
        {
            if (item.HeapIndex < currentItemCount)
            {
                return Equals(items[item.HeapIndex], item);
            }
            else
            {
                return false;
            }
        }

        // ------------------------------------------------------------------------
        // Private heap functions
        // ------------------------------------------------------------------------

        /// <summary>Sorts an item to the top of the heap.</summary>
        /// <param name="item">Item to sort.</param>
        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>Sorts an item to the bottom of the heap.</summary>
        /// <param name="item">Item to sort</param>
        private void SortDown(T item)
        {
            while (true)
            {
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;
                int swapIndex = 0;

                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;
                    if (childIndexRight < currentItemCount)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;    
                        }
                    }
                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>Swaps two items in the heap.</summary>
        /// <param name="itemA"></param>
        /// <param name="itemB"></param>
        private void Swap(T itemA, T itemB)
        {
            // Swap items in heap
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            // Swap heap indexes
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }
    }
}