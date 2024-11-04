using Raylib_cs;
using System.Numerics;

namespace Astar
{
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

        public Grid() 
        { 
            nodes = new Node[0, 0];
        }

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

        /// <summary>Maximum size of the grid</summary>
        public int MaxSize
        {
            get { return (int)GridSize.X * (int)GridSize.Y; }
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
                    _nodes[x, y] = new Node(!obstructed, worldPoint, new Vector2(x, y));
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
            percent = Raymath.Vector2Clamp(percent, Vector2.Zero, Vector2.One);
            int x = (int)Math.Round(MathF.Round(GridSize.X - 1) * percent.X);
            int y = (int)Math.Round(MathF.Round(GridSize.Y - 1) * percent.Y);
            return nodes[x, y];
        }

        /// <summary>Returns the surrounding neighbours of a specific <see cref="Node"/>.</summary>
        /// <param name="node">Node to use.</param>
        /// <returns>The list of neighbours of the node.</returns>
        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int checkX = (int)node.GridPosition.X + x;
                    int checkY = (int)node.GridPosition.Y + y;
                    if (checkX >= 0 && checkX < GridSize.X && checkY >= 0 && checkY < GridSize.Y)
                    {
                        neighbours.Add(nodes[checkX, checkY]);
                    }
                }
            }

            return neighbours;
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

        public void DrawPath(List<Node> path)
        {
            for (int i = 0; i < path.Count; i++) 
            {
                Raylib.DrawPlane(new Vector3(path[i].Position.X, 0.2f, path[i].Position.Y), new Vector2(nodeDiameter), Color.Blue);
            }
        }
    }
}