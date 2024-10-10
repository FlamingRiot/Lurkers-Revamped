using System.Numerics;
using Lurkers_revamped;
using Raylib_cs;

namespace AStar
{
    /// <summary>Represents a <see cref="Node"/> object of the A* algorithm.</summary>
    public class Node
    {
        /// <summary>Define if the node is obstructed.</summary>
        public bool Walkable;

        /// <summary>World position of the node.</summary>
        public Vector2 Position;

        /// <summary>Creates a <see cref="Node"/> object.</summary>
        /// <param name="_walkable">Define if the node if obstructed.</param>
        /// <param name="_position">World position of the node.</param>
        public Node(bool _walkable, Vector2 _position) 
        { 
            Walkable = _walkable;
            Position = _position;
        }
    }

    /// <summary>Represents a <see cref="Grid"/> object of the A* algorithm.</summary>
    public class Grid
    {
        /// <summary>World position of the grid.</summary>
        public Vector3 Center;

        /// <summary>World size of the grid.</summary>
        public Vector2 WorldSize;

        /// <summary>Number of nodes on each axis of the grid.</summary>
        public Vector2 GridSize;

        /// <summary>Radius of a single node.</summary>
        public float NodeRadius;

        private Node[,] nodes;
        private float nodeDiameter;

        /// <summary>Creates a <see cref="Grid"/> object.</summary>
        /// <param name="center">3D center of the grid.</param>
        /// <param name="size">2D size of the grid.</param>
        /// <param name="nodeRadius">Radius of a single node of the grid.</param>
        public Grid(Vector3 center, Vector2 size, float nodeRadius)
        {
            WorldSize = size;
            NodeRadius = nodeRadius;
            Center = center;

            nodeDiameter = NodeRadius * 2;
            GridSize = WorldSize / nodeDiameter;

            nodes = CreateGrid();
        }

        /// <summary>Creates a <see cref="Grid"/> object and instanciate array of <see cref="Node"/>.</summary>
        /// <returns>The array of <see cref="Node"/> clearly defined.</returns>
        private Node[,] CreateGrid()
        {
            // Init grid of nodes
            Node[,] _nodes = new Node[(int)GridSize.X, (int)GridSize.Y];

            // Define nodes
            Vector3 worldCorner = Center - Vector3.UnitX * WorldSize.X / 2 - Vector3.UnitZ * WorldSize.Y / 2;
            for (int x = 0; x < GridSize.X; x++) 
            {
                for (int y = 0; x < GridSize.Y; y++)
                {
                    Vector2 worldPoint = new Vector2(worldCorner.X + x * nodeDiameter + NodeRadius, worldCorner.Z + y * nodeDiameter + NodeRadius);
                    bool obstructed = Physics.CheckSphere(new Vector3(worldPoint.X, Center.Y, worldPoint.Y), NodeRadius);
                    _nodes[x, y] = new Node(!obstructed, worldPoint);
                }
            }

            return _nodes;
        }
    }
}