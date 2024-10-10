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

        /// <summary>The found path for the grid</summary>
        public List<Node>? Path;

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
            percent = Raymath.Vector2ClampValue(percent, 0, 1);
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
                    if (Path is not null)
                    {
                        if (Path.Contains(nodes[x, y]))
                        {
                            Raylib.DrawPlane(new Vector3(nodes[x, y].Position.X, 0.2f, nodes[x, y].Position.Y), new Vector2(nodeDiameter), Color.Blue);
                        }
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

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            // Initialize set with genesis node
            openSet.Add(startNode);

            // Algorithm search loop
            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost || openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Check if the path has been found
                if (currentNode.Position == targetNode.Position)
                {
                    RetracePath(startNode, targetNode);
                    return;
                }

                foreach (Node neighbour in Grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.Walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int costToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                    if (costToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = costToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.Parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour); 
                        }
                    }
                }
            }
        }

        /// <summary>Retraces a path according to the parents of the nodes.</summary>
        /// <param name="startNode">Starting node.</param>
        /// <param name="targetNode">Target node.</param>
        private void RetracePath(Node startNode, Node targetNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            Grid.Path = path;
        }

        /// <summary>Returns the distance between two nodes according to their costs.</summary>
        /// <param name="node1">Node A to check.</param>
        /// <param name="node2">Node B to check.</param>
        /// <returns>The calculated distance.</returns>
        private int GetDistance(Node node1, Node node2)
        {
            int distX = Math.Abs((int)node1.GridPosition.X - (int)node2.GridPosition.X);
            int distY = Math.Abs((int)node1.GridPosition.Y - (int)node2.GridPosition.Y);

            if (distX > distY) 
            {
                return 14 * distY + 10 * (distX - distY);
            }
            return 14 * distX + 10 * (distY - distX);
        }
    }
}   