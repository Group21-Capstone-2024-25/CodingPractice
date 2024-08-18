using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IAI
{
    public float moveSpeed = 5f;
    private bool isMoving = false;
    private PlayerUnit playerUnit;
    private GridManager gridManager;
    private ObstacleManager obstacleManager;
 
    void Start()
    {
        playerUnit = FindObjectOfType<PlayerUnit>();
        gridManager = FindObjectOfType<GridManager>();
        obstacleManager = FindObjectOfType<ObstacleManager>();
        StartCoroutine(MoveTowardsPlayer());
    }

    void Update()
    {
        Move();
    }

    public void Move()
    {
        if (isMoving) return;

        Vector3 targetPosition = playerUnit.transform.position;
        List<Vector3> path = FindPath(transform.position, targetPosition);

        if (path != null && path.Count > 0)
        {
            path.RemoveAt(path.Count - 1); // Remove the last node to avoid overshooting
            StartCoroutine(MoveAlongPath(path));
        }
    }

    List<Vector3> FindPath(Vector3 start, Vector3 target)
    {
        Vector2Int startGridPos = new Vector2Int(Mathf.RoundToInt(start.x / 1.1f), Mathf.RoundToInt(start.z / 1.1f));
        Vector2Int targetGridPos = new Vector2Int(Mathf.RoundToInt(target.x / 1.1f), Mathf.RoundToInt(target.z / 1.1f));

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = new Node(startGridPos, null, 0, GetDistance(startGridPos, targetGridPos));
        Node targetNode = new Node(targetGridPos, null, 0, 0);

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < currentNode.FCost || (openList[i].FCost == currentNode.FCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.Position == targetNode.Position)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Vector2Int neighborPos in GetNeighbors(currentNode.Position))
            {
                if (IsObstacle(neighborPos) || closedList.Contains(new Node(neighborPos)))
                {
                    continue;
                }

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode.Position, neighborPos);
                Node neighborNode = new Node(neighborPos, currentNode, newCostToNeighbor, GetDistance(neighborPos, targetGridPos));

                if (newCostToNeighbor < neighborNode.gCost || !openList.Contains(neighborNode))
                {
                    neighborNode.gCost = newCostToNeighbor;
                    neighborNode.hCost = GetDistance(neighborPos, targetGridPos);
                    neighborNode.Parent = currentNode;

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        return null; // No path found
    }

    List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(position.x - 1, position.y),
            new Vector2Int(position.x + 1, position.y),
            new Vector2Int(position.x, position.y - 1),
            new Vector2Int(position.x, position.y + 1)
        };

        // Remove neighbors that are out of bounds
        neighbors.RemoveAll(neighbor => neighbor.x < 0 || neighbor.x >= 10 || neighbor.y < 0 || neighbor.y >= 10);

        return neighbors;
    }

    bool IsObstacle(Vector2Int position)
    {
        if (position.x < 0 || position.x >= 10 || position.y < 0 || position.y >= 10)
        {
            return true; // Out of bounds positions are considered obstacles
        }

        int index = position.y * 10 + position.x;
        return obstacleManager.obstacleData.obstacleGrid[index];
    }

    int GetDistance(Vector2Int posA, Vector2Int posB)
    {
        int dstX = Mathf.Abs(posA.x - posB.x);
        int dstY = Mathf.Abs(posA.y - posB.y);
        return dstX + dstY;
    }

    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(new Vector3(currentNode.Position.x * 1.1f, 1, currentNode.Position.y * 1.1f));
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    IEnumerator MoveAlongPath(List<Vector3> path)
    {
        isMoving = true;
        foreach (var target in path)
        {
            while ((target - transform.position).sqrMagnitude > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;

            // Check for overlap with playerUnit
            if (playerUnit != null && Vector3.Distance(transform.position, playerUnit.transform.position) < 0.5f)
            {
                // Move to an adjacent empty tile
                Vector2Int currentGridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x / 1.1f), Mathf.RoundToInt(transform.position.z / 1.1f));
                Vector2Int newGridPos = FindAdjacentEmptyTile(currentGridPos);

                if (newGridPos != currentGridPos)
                {
                    transform.position = new Vector3(newGridPos.x * 1.1f, 1, newGridPos.y * 1.1f);
                }
            }

        }
        isMoving = false;
    }

    Vector2Int FindAdjacentEmptyTile(Vector2Int currentPos)
    {
        List<Vector2Int> neighbors = GetNeighbors(currentPos);
        foreach (Vector2Int neighbor in neighbors)
        {
            if (!IsObstacle(neighbor))
            {
                return neighbor;
            }
        }
        return currentPos; // Return current position if no adjacent empty tile found
    }

    IEnumerator MoveTowardsPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Adjust the delay as needed
            Move();
        }
    }

    class Node
    {
        public Vector2Int Position;
        public Node Parent;
        public int gCost;
        public int hCost;

        public int FCost => gCost + hCost;

        public Node(Vector2Int position, Node parent = null, int gCost = 0, int hCost = 0)
        {
            Position = position;
            Parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Node other = (Node)obj;
            return Position == other.Position;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
