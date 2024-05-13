using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class GridManager : MonoBehaviour
{
    public static GridManager i;
    public GameObject nodePrefab;
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
    internal List<PathNode> AllNodes = new List<PathNode>();

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
        Direction facing = Direction.BottomLeft;
        if (team == 1)
        {
            teamColor = "Pink";
            spawnX = 6;
            spawnY = 2;
            facing = Direction.TopRight;
        }
        else if (team == 2)
        {
            teamColor = "Yellow";
            spawnX = 2;
            spawnY = 1;
            facing = Direction.BottomRight;
        }
        else if (team == 3)
        {
            teamColor = "Red";
            spawnX = 5;
            spawnY = 6;
            facing = Direction.TopLeft;
        }
        var station = Instantiate(stationPrefab);
        station.transform.SetParent(characterParent);
        var stationNode = station.AddComponent<Station>();
        stationNode.InitializeStation(spawnX, spawnY, teamColor, 10, 2, 3, 4, 5, 1, stationGuid, facing);
        StartCoroutine(CreateFleet(stationNode, fleetGuid, true));
    }

    public IEnumerator CreateFleet(Station stationNode, Guid fleetGuid, bool originalSpawn, Coords coords = null)
    {
        GameObject fleetPrefab = playerPrefab;
        fleetPrefab.transform.Find("Unit/Unit1").GetComponent<SpriteRenderer>().color = playerColors[stationNode.stationId];
        fleetPrefab.transform.Find("Unit/Unit2").GetComponent<SpriteRenderer>().color = playerColors[stationNode.stationId];
        fleetPrefab.transform.Find("Unit/Unit3").GetComponent<SpriteRenderer>().color = playerColors[stationNode.stationId];
        var hexesNearby = GetNeighbors(stationNode.currentPathNode);
        var startIndex = 0;
        if (coords == null)
        {
            //Get spawn from station direction
            coords = stationNode.currentPathNode.coords.Add(stationNode.currentPathNode.offSet[(int)stationNode.facing]);
        }
        startIndex = hexesNearby.FindIndex(x => x.coords.Equals(coords));
        for (int i = 0; i < hexesNearby.Count; i++) {
            int j = (i + startIndex)%hexesNearby.Count;
            if (CanSpawnFleet(hexesNearby[j].coords.x, hexesNearby[j].coords.y))
            {
                var fleet = Instantiate(fleetPrefab);
                fleet.transform.SetParent(characterParent);
                var fleetNode = fleet.AddComponent<Fleet>();
                fleetNode.InitializeFleet(hexesNearby[j].coords.x, hexesNearby[j].coords.y, stationNode, stationNode.color, 6, 3, 2, 3, 4, 1, fleetGuid);
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
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * .98f * cellPrefabSize.x) + Vector3.up * (y * .75f * cellPrefabSize.y) + Vector3.right * (y % 2) * (-0.5f * cellPrefabSize.x);
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
                    startCredits = 8;
                    maxCredits = 15;
                    creditRegin = 2;
                }
                var cell = Instantiate(nodePrefab, worldPoint, Quaternion.identity);
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

    internal List<PathNode> FindPath(PathNode startNode, PathNode targetNode, int stationId)
    {
        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        startNode.gCost = 0;
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

                int newCostToNeighbor = currentNode.gCost + GetGCost(neighbor,stationId);
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
    public int GetGCost(PathNode node, int stationId )
    {
        if (Globals.GameSettings.Contains(GameSettingType.TakeoverCosts2.ToString()))
        {
            //Costs 1 if nuetral, owned by you, or has a fleet on it. Costs 2 if owned by enemy without fleet on it.
            return (node.ownedById == -1 || node.ownedById == stationId) ? 1 : 2; // || node.structureOnPath != null
        }
        return 1;
    }
    internal List<PathNode> GetNodesWithinRange(PathNode clickedNode, Unit unit, bool forMining)
    {
        var range = unit.getMovementRange();
        List<PathNode> nodesWithinRange = new List<PathNode>();
        Queue<PathNode> queue = new Queue<PathNode>();
        HashSet<PathNode> visited = new HashSet<PathNode>();
        queue.Enqueue(clickedNode);
        clickedNode.gCost = 0; // Set initial cost to 0 for clicked node
        visited.Add(clickedNode);

        while (queue.Count > 0)
        {
            PathNode currentNode = queue.Dequeue();
            if (currentNode != clickedNode)
                nodesWithinRange.Add(currentNode);
            if (!currentNode.isAsteroid)
            {
                foreach (PathNode neighbor in GetNeighbors(currentNode))
                {
                    // Check if enemy owned tile and adjust cost
                    int moveCost = currentNode.gCost + GetGCost(neighbor, unit.stationId); 
                    if (!visited.Contains(neighbor) && (moveCost <= range || (forMining && neighbor.isAsteroid)))
                    {
                        neighbor.gCost = moveCost;
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
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
        int deltaX = Mathf.Abs(nodeA.coords.x - nodeB.coords.x);
        int deltaY = Mathf.Abs(nodeA.coords.y - nodeB.coords.y);
        int deltaZ = Mathf.Abs((-nodeA.coords.x - (nodeA.coords.y - (nodeA.coords.y & 1))) - (-nodeB.coords.x - (nodeB.coords.y - (nodeB.coords.y & 1))));

        return Mathf.Max(deltaX, deltaY, deltaZ);
    }


    internal List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();
        for (int i = 0; i < 6; i++)
        {
            int checkX = node.coords.x + node.offSet[i].x;
            int checkY = node.coords.y + node.offSet[i].y;
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