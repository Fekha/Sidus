using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager i;
    public GameObject nodePrefab;
    public GameObject unitPrefab;
    private Vector3 cellPrefabSize;
    private Transform characterParent;
    internal PathNode[,] grid;
    private Vector2 gridSize = new Vector2(Constants.GridSize, Constants.GridSize);
    internal int scoreToWin = 99;
    public List<Color> playerColors;
    public List<Color> tileColors;
    internal List<PathNode> AllNodes = new List<PathNode>();
    internal bool DoneLoading = false;
    public Sprite stationlvl1;
    public Sprite stationlvl2;
    public Sprite stationlvl3;
    public Sprite stationlvl4;
    public Sprite fleetlvl1;
    public Sprite fleetlvl2;
    public Sprite fleetlvl3;
    public Sprite fleetlvl4;
    public List<Sprite> nebulaSprite;
    public RuntimeAnimatorController nodeController;
    public AnimationClip nebulaRotationClip;



    private void Awake()
    {
        i = this;
        //Got colors from https://rgbcolorpicker.com/0-1
        //Blue, Red, Orange, Purple,
        playerColors = new List<Color>() { new Color(0, 0.502f, 1, 1), new Color(1, 0, 0, 1), new Color(.776f, 0, 1, 1), new Color(1, 0.5f, 0, 1), };
        tileColors = new List<Color>() { new Color(0.529f, 0.769f, 1, 1), new Color(0.98f, 0.561f, 0.561f, 1), new Color(0.871f, 0.514f, 1f, 1), new Color(1, .714f, .42f, 1), };
    }
    void Start()
    {
        cellPrefabSize = nodePrefab.GetComponent<Renderer>().bounds.size;
        CreateGrid();
        characterParent = GameObject.Find("Characters").transform;
        var stationsCount = Globals.IsCPUGame ? Constants.MaxPlayers : Globals.GameMatch.GameTurns[0].Players.Length;
        for (int i = 0; i < stationsCount; i++)
        {
            CreateStation(i);
        }
        scoreToWin = GetScoreToWin();
        DoneLoading = true;
    }
    private void CreateStation(int stationId)
    {
        string playerColor = "Blue";
        GameObject stationPrefab = unitPrefab;
        SpriteRenderer unitSprite = stationPrefab.transform.Find("Unit").GetComponent<SpriteRenderer>();
        unitSprite.sprite = stationlvl1;
        unitSprite.color = playerColors[stationId];
        Guid stationGuid = Guid.NewGuid();
        Guid fleetGuid = Guid.NewGuid();
        if (!Globals.IsCPUGame || stationId == 0)
        {
            stationGuid = (Guid)Globals.GameMatch.GameTurns[0].Players[stationId].Station.UnitGuid;
            fleetGuid = (Guid)Globals.GameMatch.GameTurns[0].Players[stationId].Fleets[0].UnitGuid;
        }
        int spawnX = 1;
        int spawnY = 6;
        Direction facing = Direction.BottomLeft;
        if (stationId == 1)
        {
            playerColor = "Red";
            spawnX = 6;
            spawnY = 1;
            facing = Direction.TopRight;
        }
        else if (stationId == 2)
        {
            playerColor = "Purple";
            spawnX = 2;
            spawnY = 1;
            facing = Direction.Right;
        }
        else if (stationId == 3)
        {
            playerColor = "Orange";
            spawnX = 5;
            spawnY = 6;
            facing = Direction.Left;
        }
        var station = Instantiate(stationPrefab);
        station.transform.SetParent(characterParent);
        var stationNode = station.AddComponent<Station>();
        stationNode.InitializeStation(spawnX, spawnY, playerColor, 12, 1, 5, 6, 7, stationGuid, facing);
        StartCoroutine(CreateFleet(stationNode, fleetGuid, true));
    }

    public IEnumerator CreateFleet(Station stationNode, Guid fleetGuid, bool originalSpawn)
    {
        GameObject fleetPrefab = unitPrefab;
        SpriteRenderer unitSprite = fleetPrefab.transform.Find("Unit").GetComponent<SpriteRenderer>();
        unitSprite.sprite = fleetlvl1;
        unitSprite.color = playerColors[stationNode.stationId];
        var hexesNearby = GetNeighbors(stationNode.currentPathNode);
        if(!originalSpawn)
            hexesNearby = hexesNearby.Where(x => x.ownedById == stationNode.stationId).ToList();
        var startIndex = -1;
        int k = 0;
        while (startIndex == -1)
        {
            Coords coords = stationNode.currentPathNode.coords.AddCoords(stationNode.currentPathNode.offSet[((int)stationNode.facing+k)%6]);
            startIndex = hexesNearby.FindIndex(x => x.coords.CoordsEquals(coords));
            k++;
        }
        bool didSpawn = false;
        for (int i = 0; i < hexesNearby.Count; i++) {
            int j = (i + startIndex)%hexesNearby.Count;
            if (CanSpawnFleet(hexesNearby[j].coords.x, hexesNearby[j].coords.y))
            {
                var fleet = Instantiate(fleetPrefab);
                fleet.transform.SetParent(characterParent);
                var fleetNode = fleet.AddComponent<Fleet>();
                fleetNode.InitializeFleet(hexesNearby[j].coords.x, hexesNearby[j].coords.y, stationNode, stationNode.color, 6, 2, stationNode.bonusKinetic+3, stationNode.bonusThermal+4, stationNode.bonusExplosive+5, fleetGuid);
                if (!originalSpawn)
                {
                    StartCoroutine(GameManager.i.FloatingTextAnimation($"New Fleet", fleet.transform, fleetNode)); //floater7
                    fleetNode.selectIcon.SetActive(true);
                    yield return StartCoroutine(GameManager.i.WaitforSecondsOrTap(1));
                    fleetNode.selectIcon.SetActive(false);
                }
                didSpawn = true;
                break;
            }
        }
        if (!didSpawn)
        {
            GameManager.i.turnValue.text += "\nNo valid location to spawn fleet.";
            yield return StartCoroutine(GameManager.i.WaitforSecondsOrTap(1));
        }
    }
    bool CanSpawnFleet(int x, int y)
    {
        if (grid[x, y].unitOnPath == null && !grid[x, y].isAsteroid)
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

    void CreateGrid()
    {
        var nodeParent = GameObject.Find("Nodes").transform;
        grid = new PathNode[Mathf.RoundToInt(gridSize.x), Mathf.RoundToInt(gridSize.y)];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.up * gridSize.y / 2;
        List<Coords> asteroids = new List<Coords>() { new Coords(1,4),new Coords(2,5),new Coords(3,6),new Coords(1,1),new Coords(2,2),new Coords(3,3),new Coords(4,4),new Coords(5,5),new Coords(6,6),new Coords(4,1),new Coords(5,2),new Coords(6,3), };
        List<Coords> rifts = new List<Coords>() { new Coords(7,0),new Coords(0,7),new Coords(3,4),new Coords(4,3),new Coords(0,3),new Coords(7,4), };
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                var coords = new Coords(x, y);
                // Calculate the world position based on the size of the cellPrefab
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * .955f * cellPrefabSize.x) + Vector3.up * (y * .75f * cellPrefabSize.y) + Vector3.right * (y % 2) * (-0.475f * cellPrefabSize.x);
                var cell = Instantiate(nodePrefab, worldPoint, Quaternion.identity);
                cell.transform.SetParent(nodeParent);
                int maxCredits = 0;
                int startCredits = 0;
                int creditRegin = 0;
                bool isRift = false;
                var rift = rifts.FirstOrDefault(x => x.CoordsEquals(coords));
                if (asteroids.Any(x=>x.CoordsEquals(coords)))
                {
                    startCredits = 4;
                    maxCredits = 12;
                    creditRegin = 1;
                } else if (rift != null) {
                    isRift = true;
                    cell.transform.Find("Node").GetComponent<SpriteRenderer>().sprite = nebulaSprite[rifts.IndexOf(rift)%nebulaSprite.Count];
                    cell.transform.Find("Node").GetComponent<SpriteRenderer>().sortingOrder = -3;
                    cell.transform.Find("Node/Background").gameObject.SetActive(false);
                    Animator animator = cell.AddComponent<Animator>();
                    animator.runtimeAnimatorController = nodeController;
                    AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                    overrideController["DefaultAnimation"] = nebulaRotationClip; // Replace "DefaultAnimation" with the actual animation state name if needed
                    animator.runtimeAnimatorController = overrideController;
                }
                grid[x, y] = cell.AddComponent<PathNode>();
                grid[x, y].InitializeNode(x, y, startCredits, maxCredits, creditRegin, isRift);
            }
        }
    }

    internal List<PathNode> FindPath(PathNode startNode, PathNode targetNode, Unit unit, bool checkAsteroids = true)
    {
        startNode.parent = null;
        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        startNode.gCost = 0;
        openSet.Add(startNode);
        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].gCost < currentNode.gCost)
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
            List<PathNode> neighbors = GetNeighbors(currentNode, checkAsteroids ? currentNode.minerals > unit.miningLeft : checkAsteroids);
            foreach (PathNode neighbor in neighbors)
            {
                if (!openSet.Contains(neighbor) && !closedSet.Contains(neighbor))
                {
                    neighbor.gCost = currentNode.gCost + GetGCost(neighbor);
                    neighbor.parent = currentNode;
                    openSet.Add(neighbor);
                }
            }
        }
        return new List<PathNode>();
    }
    public int GetGCost(PathNode node)
    {
        return 1;
    }
    internal List<PathNode> GetNodesWithinRange(PathNode startNode, Unit unit)
    {
        var range = unit.getMovementRange();
        List<PathNode> nodesWithinRange = new List<PathNode>();
        Queue<PathNode> queue = new Queue<PathNode>();
        HashSet<PathNode> visited = new HashSet<PathNode>();
        queue.Enqueue(startNode);
        startNode.gCost = 0; // Set initial cost to 0 for clicked node
        visited.Add(startNode);
        while (queue.Count > 0)
        {
            PathNode currentNode = queue.Dequeue();
            if (currentNode != startNode)
                nodesWithinRange.Add(currentNode);
            List<PathNode> neighbors = GetNeighbors(currentNode, currentNode.minerals > unit.miningLeft);
            foreach (PathNode neighbor in neighbors)
            {
                // Check if enemy owned tile and adjust cost
                int moveCost = currentNode.gCost + GetGCost(neighbor); 
                if (!visited.Contains(neighbor) && moveCost <= range)
                {
                    neighbor.gCost = moveCost;
                    neighbor.parent = currentNode;
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
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
    internal int WrapAround(int coord)
    {
        return ((coord % Constants.GridSize) + Constants.GridSize) % Constants.GridSize;
    }
    internal List<PathNode> GetNeighbors(PathNode node, bool blockedByAsteroid = true)
    {
        List<PathNode> neighbors = new List<PathNode>();
        for (int i = 0; i < 6; i++)
        {
            var gridNode = grid[WrapAround(node.coords.x + node.offSet[i].x), WrapAround(node.coords.y + node.offSet[i].y)];
            if (!gridNode.isRift)
                neighbors.Add(gridNode);
        }
        if (node.isAsteroid && blockedByAsteroid && node.parent != null)
        {
            neighbors = GetNeighbors(node.parent, false).Where(x => !x.isAsteroid && neighbors.Any(y => y.coords.CoordsEquals(x.coords))).ToList();
            neighbors.Add(node.parent);
        }
        return neighbors;
    }
    internal int GetScoreToWin()
    {
        return (int)(gridSize.x * gridSize.y / GameManager.i.Stations.Where(x=>!x.defeated).Select(x=>x.teamId).Distinct().Count())+Globals.Teams;
    }
    internal int CheckForWin()
    {
        GetScores();
        scoreToWin = GetScoreToWin();
        for (int i = 0; i < Globals.Teams; i++)
        {
            if (GameManager.i.Stations.Where(x=>x.teamId == i).Sum(x=>x.score) >= scoreToWin)
            {
                return i;
            }
        }
        if (GameManager.i.Stations.Where(x => !x.defeated).Select(x => x.teamId).Distinct().Count() == 1)
        {
            return GameManager.i.Stations.FirstOrDefault(x => !x.defeated).stationId;
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
            var teamMembers = GameManager.i.Stations.Where(x => x.teamId == GameManager.i.Stations[i].teamId).Select(x => x.stationId);
            foreach (var team in teamMembers)
            {
                GameManager.i.Stations[i].score = scores[team];
            }
        }
    }
}