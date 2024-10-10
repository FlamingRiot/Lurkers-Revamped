using System.Numerics;

namespace AStar
{
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

            Heap<Node> openSet = new Heap<Node>(Grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

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