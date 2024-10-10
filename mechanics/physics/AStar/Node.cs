using System.Numerics;

namespace AStar
{
    /// <summary>Represents a <see cref="Node"/> object of the A* algorithm.</summary>
    public class Node : IHeapItem<Node>
    {
        /// <summary>Define if the node is obstructed.</summary>
        public bool Walkable;

        /// <summary>World position of the node.</summary>
        public Vector2 Position;

        /// <summary>Position of the node in the grid.</summary>
        public Vector2 GridPosition;

        /// <summary>G Cost of the node.</summary>
        public int GCost;

        /// <summary>H Cost of the node.</summary>
        public int HCost;

        /// <summary>Parent <see cref="Node"/> of the current one.  </summary>
        public Node Parent;

        /// <summary>F Cost of the node.</summary>
        public int FCost { get { return GCost + HCost; } }

        /// <summary>Creates a <see cref="Node"/> object.</summary>
        /// <param name="_walkable">Define if the node if obstructed.</param>
        /// <param name="_position">World position of the node.</param>
        public Node(bool _walkable, Vector2 _position, Vector2 _gridPosition)
        {
            Walkable = _walkable;
            Position = _position;
            GridPosition = _gridPosition;
        }

        /// <summary>Heap index of the node</summary>
        public int HeapIndex { get; set; }

        public int CompareTo(Node? item)
        {
            if (item != null)
            {
                int compare = FCost.CompareTo(item.FCost);
                if (compare == 0)
                {
                    compare = HCost.CompareTo(item.HCost);
                }
                return -compare;

            }
            return 0;
        }
    }
}