using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager i;
    public GameObject nodePrefab;
    public GameObject enemyPrefab;
    public GameObject enemyStationPrefab;
    public GameObject obsticalPrefab;

    public GameObject playerPrefab;
    public GameObject playerStationPrefab;
    private Vector3 cellPrefabSize;
    private Transform characterParent;
    internal PathNode[,] grid;
    private Vector2 gridSize = new Vector2(8, 8);
    internal int scoreToWin = 0;
    private void Awake()
    {
        i = this;
    }
    void Start()
    {
        cellPrefabSize = nodePrefab.GetComponent<Renderer>().bounds.size;
        CreateGrid();
        characterParent = GameObject.Find("Characters").transform;
        CreateStation(enemyStationPrefab, enemyPrefab, "Red");
        CreateStation(playerStationPrefab, playerPrefab, "Green");
    }
    private void CreateStation(GameObject stationPrefab, GameObject shipPrefab, string team)
    {
        var spawnY = 0;
        if (team == "Red")
        {
            spawnY = (int)gridSize.y - 1;
        }
        var station = Instantiate(stationPrefab);
        station.transform.parent = characterParent;
        var stationNode = station.AddComponent<Station>();
        var spawnX = (int)Random.Range(1, gridSize.x - 1);
        stationNode.InitializeStation(spawnX, spawnY, team + " Station", 5, 1, Random.Range(0, 3), Random.Range(7, 10), Random.Range(7, 10), Random.Range(7, 10), 1);

        var ship = Instantiate(shipPrefab);
        ship.transform.parent = characterParent;
        var shipNode = ship.AddComponent<Ship>();
        spawnX = Random.Range(0, 2) == 0 ? spawnX - 1 : spawnX + 1;
        shipNode.InitializeShip(spawnX, spawnY, stationNode, team + " Ship", 3, 2, Random.Range(0, 3), Random.Range(1, 4), Random.Range(1, 4), Random.Range(1, 4), 1);
    }
    void CreateGrid()
    {
        var obstacleCount = 0;
        var nodeParent = GameObject.Find("Nodes").transform;
        grid = new PathNode[Mathf.RoundToInt(gridSize.x), Mathf.RoundToInt(gridSize.y)];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.up * gridSize.y / 2;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Calculate the world position based on the size of the cellPrefab
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * cellPrefabSize.x) + Vector3.up * (y * cellPrefabSize.y);
                var isObstacle = false;
                if (y != 0 && y != gridSize.y - 1 && x != 0 && x != gridSize.x - 1)
                    isObstacle = Random.Range(0, 6) == 0;
                if (isObstacle)
                    obstacleCount++;
                var cell = Instantiate(isObstacle ? obsticalPrefab : nodePrefab, worldPoint, Quaternion.identity);
                cell.transform.parent = nodeParent;
                grid[x, y] = cell.AddComponent<PathNode>();
                grid[x, y].InitializeNode(x, y, isObstacle);
            }
        }
        scoreToWin = (int)(((gridSize.x * gridSize.y) - obstacleCount) * .66);
    }

    internal List<PathNode> FindPath(PathNode startNode, PathNode targetNode)
    {
        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (PathNode neighbor in GetNeighbors(currentNode))
            {
                if (neighbor != targetNode && (neighbor.isObstacle || closedSet.Contains(neighbor)))
                    continue;

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return new List<PathNode>();
    }

    internal List<PathNode> GetNodesWithinRange(PathNode clickedNode, int range)
    {
        List<PathNode> nodesWithinRange = new List<PathNode>();

        Queue<PathNode> queue = new Queue<PathNode>();
        HashSet<PathNode> visited = new HashSet<PathNode>();

        queue.Enqueue(clickedNode);
        visited.Add(clickedNode);

        while (queue.Count > 0 && range >= 0)
        {
            int levelSize = queue.Count;

            for (int i = 0; i < levelSize; i++)
            {
                PathNode currentNode = queue.Dequeue();

                nodesWithinRange.Add(currentNode);

                foreach (PathNode neighbor in GetNeighbors(currentNode))
                {
                    if (!visited.Contains(neighbor) && !neighbor.isObstacle)
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            range--;
        }
        return nodesWithinRange;
    }

    List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;
        while (currentNode != startNode)
        {
            if (!currentNode.isObstacle)
                path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);

        return dstX + dstY;
    }

    List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();

        int[] xOffsets = { 0, 1, 0, -1 }; // Right, Up, Left, Down
        int[] yOffsets = { 1, 0, -1, 0 }; // Right, Up, Left, Down

        for (int i = 0; i < xOffsets.Length; i++)
        {
            int checkX = node.x + xOffsets[i];
            int checkY = node.y + yOffsets[i];

            if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    internal int CheckForWin()
    {
        Dictionary<int, int> scores = new Dictionary<int, int>();
        scores.Add(-1, 0);
        scores.Add(0, 0);
        scores.Add(1, 0);
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                scores[grid[i, j].ownedById]++;
            }
        }
        for (int i = -1; i <= 1; i++)
        {
            if (scores[i] >= scoreToWin)
            {
                return i;
            }
        }
        return -1;
    }
}