using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : MonoBehaviour
{
    public float moveSpeed = 5f;
    private bool isMoving = false;
    private GridManager gridManager;
    private ObstacleManager obstacleManager;
    private EnemyAI[] enemyUnits;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        obstacleManager = FindObjectOfType<ObstacleManager>();
        enemyUnits = FindObjectsOfType<EnemyAI>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                TileInfo tileInfo = hit.collider.GetComponent<TileInfo>();
                if (tileInfo != null)
                {
                    Vector3 targetPosition = new Vector3(tileInfo.x * 1.1f, 1, tileInfo.z * 1.1f);
                    bool canMove = CanMoveToPosition(targetPosition);
                    if (canMove)
                    {
                        List<Vector3> path = FindPath(transform.position, targetPosition);
                        if (path != null)
                        {
                            StartCoroutine(MoveAlongPath(path));
                        }
                    }
                    else
                    {
                        Debug.Log("Cannot move to tile occupied by enemy unit.");
                        // Handle feedback or action when player tries to move to an occupied tile
                    }

                }
            }
        }
    }

    bool CanMoveToPosition(Vector3 targetPosition)
    {
        // Check if any enemy unit is occupying the target position
        foreach (EnemyAI enemyUnit in enemyUnits)
        {
            if (enemyUnit.transform.position == targetPosition)
            {
                return false; // Cannot move to this position because it's occupied by an enemy unit
            }
        }
        return true; // Can move to this position
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
        }
        isMoving = false;
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
