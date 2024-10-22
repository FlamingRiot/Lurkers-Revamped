using System.Numerics;

namespace Astar
{
    /// <summary>Represents an instance of the A* algorithm.</summary>
    public static class AStar
    {
        /// <summary>Grid of the algorithm instance.</summary>
        public static Grid Grid = new Grid();

        // Sets of opened and closed nodes
        private static Heap<Node> openSet = new Heap<Node>(Grid.MaxSize);
        private static HashSet<Node> closedSet = new HashSet<Node>();

        /// <summary>Loads the grid for the A* algorithm.</summary>
        /// <param name="grid">The grid to use.</param>
        public static void Load(Grid grid)
        {
            Grid = grid;
            openSet = new Heap<Node>(Grid.MaxSize);
            closedSet = new HashSet<Node>();
        }

        /// <summary>Finds the shortest path from a <see cref="Node"/> to another.</summary>
        /// <param name="startPos">Starting position.</param>
        /// <param name="targetPos">Target position.</param>
        public static void FindPath(Vector3 startPos, Vector3 targetPos, List<Node> path)
        {
            Node startNode = Grid.GetWorldToNode(startPos);
            Node targetNode = Grid.GetWorldToNode(targetPos);

            openSet.Clear();
            closedSet.Clear();

            // Initialize set with genesis node
            openSet.Add(startNode);

            // Algorithm search loop
            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // Check if the path has been found
                if (currentNode.Position == targetNode.Position)
                {
                    RetracePath(startNode, targetNode, path);
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
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }

        /// <summary>Retraces a path according to the parents of the nodes.</summary>
        /// <param name="startNode">Starting node.</param>
        /// <param name="targetNode">Target node.</param>
        private static void RetracePath(Node startNode, Node targetNode, List<Node> path)
        {
            path.Clear();
            Node currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
        }

        /// <summary>Returns the distance between two nodes according to their costs.</summary>
        /// <param name="node1">Node A to check.</param>
        /// <param name="node2">Node B to check.</param>
        /// <returns>The calculated distance.</returns>
        private static int GetDistance(Node node1, Node node2)
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