using System.Collections;
using System.Collections.Generic;
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
        GameManager.i.ScoreToWinText.text = $"Tiles to win: 2/{scoreToWin}";
        characterParent = GameObject.Find("Characters").transform;
        CreateStation(enemyStationPrefab, 0);
        CreateStation(playerStationPrefab, 1);
    }
    private void CreateStation(GameObject stationPrefab,int team)
    {
        int spawnY;
        string teamColor;
        if (team == 0)
        {
            spawnY = (int)gridSize.y - 1;
            teamColor = "Red";
        }
        else
        {
            spawnY = 0;
            teamColor = "Green";
        }
        var station = Instantiate(stationPrefab);
        station.transform.parent = characterParent;
        var stationNode = station.AddComponent<Station>();
        var spawnX = 5;// (int)Random.Range(1, gridSize.x - 1);
        stationNode.InitializeStation(spawnX, spawnY, teamColor, 5, 1, 1, 7, 7, 7, 1);
        StartCoroutine(CreateFleet(stationNode));
    }

    public IEnumerator CreateFleet(Station stationNode)
    {
        GameObject shipPrefab;
        if (stationNode.color == "Red")
        {
            shipPrefab = enemyPrefab;
        }
        else
        {
            shipPrefab = playerPrefab;
        }
        var spawnX = stationNode.x;
        var spawnY = stationNode.y;
        if (CanSpawnShip(spawnX, spawnY -1) || CanSpawnShip(spawnX, spawnY +1) || CanSpawnShip(spawnX - 1, spawnY) || CanSpawnShip(spawnX +1, spawnY))
        {
            do {
                spawnX = stationNode.x;
                spawnY = stationNode.y;
                if (Random.Range(0, 2) == 0)
                {
                    spawnX += Random.Range(0, 2) == 0 ? -1 : 1;
                }
                else
                {
                    spawnY += Random.Range(0, 2) == 0 ? -1 : 1;
                }
            } while (!CanSpawnShip(spawnX, spawnY));
            var ship = Instantiate(shipPrefab);
            ship.transform.parent = characterParent;
            var shipNode = ship.AddComponent<Ship>();
            shipNode.InitializeShip(spawnX, spawnY, stationNode, stationNode.color, 3, 2, 2, 3, 3, 3, 1);
        }
        yield return new WaitForSeconds(.1f);
    }
    bool CanSpawnShip(int x, int y)
    {
        if (IsInTheBox(x, y)) {

            if (grid[x, y].nodeOnPath == null && !grid[x, y].isObstacle)
            {
                Debug.Log($"Can Spawn at {x},{y}");
                return true;
            }
            else
            {
                Debug.Log($"{x},{y} is blocked");
                return false;
            }
        }
        Debug.Log($"{x},{y} is out of bounds");
        return false;
        
    }
    bool IsInTheBox(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
        {
            return true;
        }
        return false;
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
                    isObstacle = (x+y)%4 == 0;
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
        GetScores();
        for (int i = 0; i < 2; i++)
        {
            if (GameManager.i.stations[i].score >= scoreToWin)
            {
                return i;
            }
        }
        return -1;
    }

    internal void GetScores()
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
        GameManager.i.stations[0].score = scores[0];
        GameManager.i.stations[1].score = scores[1];
    }
}