using System.Numerics;
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
        public Grid(Vector3 center, Vector2 size, float nodeRadius, List<BoundingBox> obstacles)
        {
            WorldSize = size;
            NodeRadius = nodeRadius;
            Center = center;

            nodeDiameter = NodeRadius * 2;
            GridSize = WorldSize / nodeDiameter;

            nodes = CreateGrid(obstacles);
        }

        /// <summary>Creates a <see cref="Grid"/> object and instanciate array of <see cref="Node"/>.</summary>
        /// <returns>The array of <see cref="Node"/> clearly defined.</returns>
        private Node[,] CreateGrid(List<BoundingBox> obstacles)
        {
            // Init grid of nodes
            Node[,] _nodes = new Node[(int)GridSize.X, (int)GridSize.Y];

            // Define nodes
            Vector3 worldCorner = Center - Vector3.UnitX * WorldSize.X / 2 - Vector3.UnitZ * WorldSize.Y / 2;
            for (int x = 0; x < GridSize.X; x++) 
            {
                for (int y = 0; y < GridSize.Y; y++)
                {
                    Vector2 worldPoint = new Vector2(worldCorner.X + x * nodeDiameter + NodeRadius, worldCorner.Z + y * nodeDiameter + NodeRadius);
                    bool obstructed = CheckObstaclesSphere(new Vector3(worldPoint.X, Center.Y, worldPoint.Y), NodeRadius, obstacles);
                    _nodes[x, y] = new Node(!obstructed, worldPoint);
                }
            }

            return _nodes;
        }

        /// <summary>Checks if a sphere collides with any box in the obstacles.</summary>
        /// <param name="position">Position of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        /// <returns><see langword="true"/> if a collision has been detected. <see langword="false"/> otherwise.</returns>
        private bool CheckObstaclesSphere(Vector3 position, float radius, List<BoundingBox> nodeObstacles)
        {
            bool obstructed = false;
            for (int i = 0; i < nodeObstacles.Count; i++)
            {
                obstructed = Raylib.CheckCollisionBoxSphere(nodeObstacles[i], position, radius);
                if (obstructed) return obstructed;
            }
            return obstructed;
        }

        /// <summary>Returns the <see cref="Node"/> a position is standing on.</summary>
        /// <param name="position">The world position to use.</param>
        /// <returns>The node the position is standing on.</returns>
        public Node GetWorldToNode(Vector3 position)
        {
            Vector2 percent = new Vector2((position.X + WorldSize.X / 2) / WorldSize.X, (position.Z + WorldSize.Y / 2) / WorldSize.Y);
            percent = Raymath.Vector2ClampValue(percent, 0, 1);
            int x = (int)Math.Round(MathF.Round(GridSize.X - 1) * percent.X);
            int y = (int)Math.Round(MathF.Round(GridSize.Y - 1) * percent.Y);
            return nodes[x, y];
        }

        /// <summary>Draws the node map of the algorithm.</summary>
        public void DrawNodeMap()
        {
            for (int x = 0; x < GridSize.X; x++)
            {
                for (int y = 0; y < GridSize.Y; y++)
                {
                    if (nodes[x, y].Walkable)
                    {
                        Raylib.DrawPlane(new Vector3(nodes[x, y].Position.X, 0.1f, nodes[x, y].Position.Y), new Vector2(nodeDiameter), Color.White);
                    }
                    else
                    {
                        Raylib.DrawPlane(new Vector3(nodes[x, y].Position.X, 0.1f, nodes[x, y].Position.Y), new Vector2(nodeDiameter), Color.Red);
                    }
                }
            }
        }
    }

    /// <summary>Represents an instance of the A* algorithm.</summary>
    public class AStar
    {
        /// <summary>Grid of the algorithm instance.</summary>
        public Grid Grid;

        /// <summary>Creates a <see cref="AStar"/> object.</summary>
        /// <param name="grid">Grid of the algorithm.</param>
        public AStar(Grid grid)
        {
            Grid = grid;
        }

        /// <summary>Finds the shortest path from a <see cref="Node"/> to another.</summary>
        /// <param name="startPos">Starting position.</param>
        /// <param name="targetPos">Target position.</param>
        public void FindPath(Vector3 startPos, Vector3 targetPos)
        {
            Node startNode = Grid.GetWorldToNode(startPos);
            Node targetNode = Grid.GetWorldToNode(targetPos);
        }
    }
}