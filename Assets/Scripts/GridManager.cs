using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager i;
    public GameObject nodePrefab;
    public GameObject obsticalPrefab;

    public GameObject playerPrefab;
    public GameObject playerStationPrefab;
    private Vector3 cellPrefabSize;
    private Transform characterParent;
    internal PathNode[,] grid;
    internal List<PathNode> asteroids = new List<PathNode>();
    private Vector2 gridSize = new Vector2(8, 8);
    internal int scoreToWin = 99;
    public List<Color> playerColors;
    public List<Color> tileColors;
   
    private void Awake()
    {
        i = this;
        //Got colors from https://rgbcolorpicker.com/0-1
        //Blue, Pink, Yellow, Red
        playerColors = new List<Color>() { new Color(0, 0.502f, 1, 1), new Color(1, 0, 0.894f, 1), new Color(1, 0.757f, 0, 1), new Color(1, 0, 0, 1), };
        tileColors = new List<Color>() { new Color(0, 0.769f, 1, 1), new Color(0.98f, 0.561f, 0.937f, 1), new Color(0.945f, 1, 0, 1), new Color(0.98f, 0.561f, 0.561f, 1),  };
    }
    void Start()
    {
        cellPrefabSize = nodePrefab.GetComponent<Renderer>().bounds.size;
        CreateGrid();
        GameManager.i.ScoreToWinText.text = $"Tiles to win: 2/{scoreToWin}";
        characterParent = GameObject.Find("Characters").transform;
        for (int i = 0; i < Globals.Players.Length; i++)
        {
            CreateStation(i);
        }
    }
    private void CreateStation(int team)
    {
        
        var stationOfClient = team == Globals.localStationIndex;
        string teamColor = "Blue";
        GameObject stationPrefab = playerStationPrefab;
        stationPrefab.transform.Find("Unit").GetComponent<SpriteRenderer>().color = playerColors[team];
        Guid stationGuid = Globals.Players[team].StationGuid;
        Guid fleetGuid = Globals.Players[team].FleetGuids[0];
        int spawnX = 1;
        int spawnY = 5;
        if (team == 1)
        {
            teamColor = "Pink";
            spawnX = 6;
            spawnY = 2;
        }
        else if (team == 2)
        {
            teamColor = "Yellow";
            spawnX = 2;
            spawnY = 1;
        }
        else if (team == 3)
        {
            teamColor = "Red";
            spawnX = 5;
            spawnY = 6;
        }
        var station = Instantiate(stationPrefab);
        station.transform.SetParent(characterParent);
        var stationNode = station.AddComponent<Station>();
        stationNode.InitializeStation(spawnX, spawnY, teamColor, 10, 2, 3, 4, 5, 1, stationGuid);
        StartCoroutine(CreateFleet(stationNode, fleetGuid, true));
    }

    public IEnumerator CreateFleet(Station stationNode, Guid fleetGuid, bool originalSpawn)
    {
        GameObject fleetPrefab = playerPrefab;
        fleetPrefab.transform.Find("Unit/Unit1").GetComponent<SpriteRenderer>().color = playerColors[stationNode.stationId];
        fleetPrefab.transform.Find("Unit/Unit2").GetComponent<SpriteRenderer>().color = playerColors[stationNode.stationId];
        fleetPrefab.transform.Find("Unit/Unit3").GetComponent<SpriteRenderer>().color = playerColors[stationNode.stationId];
        var hexesNearby = GetNeighbors(stationNode.currentPathNode);
        foreach(var hex in hexesNearby) {
            if (CanSpawnFleet(hex.x, hex.y))
            {
                var fleet = Instantiate(fleetPrefab);
                fleet.transform.SetParent(characterParent);
                var fleetNode = fleet.AddComponent<Fleet>();
                fleetNode.InitializeFleet(hex.x, hex.y, stationNode, stationNode.color, 6, 3, 2, 3, 4, 1, fleetGuid);
                if (!originalSpawn)
                {
                    fleetNode.selectIcon.SetActive(true);
                    yield return new WaitForSeconds(1f);
                    fleetNode.selectIcon.SetActive(false);
                }
                break;
            }
        }
    }
    bool CanSpawnFleet(int x, int y)
    {
        if (IsInGridBounds(x, y)) {
            if (grid[x, y].structureOnPath == null && !grid[x, y].isAsteroid)
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
    bool IsInGridBounds(int x, int y)
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
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * 1.08f  * cellPrefabSize.x) + Vector3.up * (y * .85f * cellPrefabSize.y) + Vector3.right * (y % 2) * (-0.53f * cellPrefabSize.x);
                bool isAsteroid = false;
                int maxCredits = 0;
                int startCredits = 0;
                int creditRegin = 0;
                if (y != 0 && y != gridSize.y - 1 && x != 0 && x != gridSize.x - 1)
                    if(!((x == 3 && (y == 3 || y == 4)) || (x == 4 && (y == 3 || y == 4))))
                        isAsteroid = (x+y+y)%3 == 0;
                if (isAsteroid)
                {
                    obstacleCount++;
                    startCredits = 10;
                    maxCredits = 20;
                    creditRegin = 3;
                    //var obstacle = Instantiate(obsticalPrefab, worldPoint, Quaternion.identity);
                }
                var cell = Instantiate(isAsteroid ? obsticalPrefab : nodePrefab, worldPoint, Quaternion.identity);
                cell.transform.SetParent(nodeParent);
                grid[x, y] = cell.AddComponent<PathNode>();
                grid[x, y].InitializeNode(x, y, isAsteroid, startCredits, maxCredits, creditRegin);
                if (isAsteroid)
                {
                    asteroids.Add(grid[x, y]);
                }
            }
        }
        //(int)((((gridSize.x * gridSize.y) - obstacleCount) / Globals.Players.Count()) * 1.5);
        scoreToWin = 42 - ((Globals.Players.Count()-1) * 7);
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
                if (neighbor != targetNode && (neighbor.isAsteroid || closedSet.Contains(neighbor)))
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
                if (!currentNode.isAsteroid)
                {
                    foreach (PathNode neighbor in GetNeighbors(currentNode))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
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
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        int deltaX = Mathf.Abs(nodeA.x - nodeB.x);
        int deltaY = Mathf.Abs(nodeA.y - nodeB.y);
        int deltaZ = Mathf.Abs((-nodeA.x - nodeA.y) - (-nodeB.x - nodeB.y));

        return Mathf.Max(deltaX, deltaY, deltaZ);
    }

    internal List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();
        for (int i = 0; i < 6; i++)
        {
            int checkX = node.x + node.offSet[i].x;
            int checkY = node.y + node.offSet[i].y;
            if (IsInGridBounds(checkX, checkY))
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    internal int CheckForWin()
    {
        GetScores();
        for (int i = 0; i < GameManager.i.Stations.Count; i++)
        {
            if (GameManager.i.Stations[i].score >= scoreToWin)
            {
                return i;
            }
        }
        if (GameManager.i.Stations.Where(x => x.defeated == false).Count() == 1)
        {
            return GameManager.i.Stations.FirstOrDefault(x => x.defeated == false).stationId;
        }
        return -1;
    }

    internal void GetScores()
    {
        Dictionary<int, int> scores = new Dictionary<int, int>();
        //-1 for unowned stations
        for (int i = -1; i < GameManager.i.Stations.Count; i++)
        {
            scores.Add(i, 0);
        }
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                scores[grid[i, j].ownedById]++;
            }
        }
        for (int i = 0; i < GameManager.i.Stations.Count; i++)
        {
            GameManager.i.Stations[i].score = scores[i];
        }
    }
}