using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager i;
    public GameObject nodePrefab;
    public GameObject unitPrefab;
    public GameObject bombPrefab;
    public TextMeshProUGUI amountToWinText;
    private Vector3 cellPrefabSize;
    internal Transform characterParent;
    internal PathNode[,] grid;
    private Vector2 gridSize = new Vector2(Constants.GridSize, Constants.GridSize);
    internal int scoreToWin = 99;
    internal List<Color> playerColors;
    internal List<Color> uiColors;
    internal List<Color> tileColors;
    internal List<PathNode> AllNodes = new List<PathNode>();
    internal bool DoneLoading = false;
    internal Sprite[,] fleetSprites;
    public Sprite[] stationSprites;

    public GameObject fx_Explosion;
    public List<Sprite> nebulaSprite;
    public RuntimeAnimatorController nodeController;
    public AnimationClip nebulaRotationClip;
    public GameObject burgerList;
    private void Awake()
    {
        i = this;
        //Got colors from https://rgbcolorpicker.com/0-1
        //Blue, Red, Purple, Orange,
        playerColors = new List<Color>() { new Color(0.2117598f, 0.3275293f, 0.8113208f, 1), new Color(0.764151f, 0.01922386f, 0.01922386f, 1), new Color(0.5379764f, 0.01993586f, 0.7924528f, 1), new Color(1, 0.5f, 0, 1), };
        tileColors = new List<Color>() { new Color(0.529f, 0.769f, 1, 1), new Color(0.98f, 0.561f, 0.561f, 1), new Color(0.871f, 0.514f, 1f, 1), new Color(1, .714f, .42f, 1), };
        uiColors = new List<Color>() { new Color(0.2824255f, .3930371f, .5283019f, 1), new Color(0.3396226f, 0.09825558f, 0.1029435f, 1), new Color(0.1921593f, 0.1135635f, 0.3113208f, 1), new Color(0.545f, 0.2878966f, 0.106f, 1), new Color(0.3773585f, 0.3773585f, 0.3773585f, 1), };
        fleetSprites = new Sprite[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                string path = $"Sprites/Units/Fleets/fleet_{i}_{j}";
                fleetSprites[i, j] = Resources.Load<Sprite>(path);
            }
        }
    }
    void Start()
    {
        cellPrefabSize = nodePrefab.GetComponent<Renderer>().bounds.size;
        string jsonString = Globals.GameMatch.ModuleJson.Replace("\\\"", "\"").Trim('"');
        GameManager.i.AllModulesStats = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ModuleStats>>(jsonString);
        var currentGameTurn = Globals.GameMatch.GameTurns.Where(x => x.Players.Count() == Globals.GameMatch.MaxPlayers).Last();
        if (currentGameTurn.TurnNumber > 0)
        {
            GameManager.i.TurnNumber = currentGameTurn.TurnNumber;
            GameManager.i.AllModules = currentGameTurn.AllModules.Select(x => new Module(x)).ToList();
        }
        CreateGrid(currentGameTurn);
        characterParent = GameObject.Find("Nodes/Characters").transform;
        if (Globals.GameMatch.MaxPlayers == 1)
        {
            for (int i = 0; i < Constants.MaxPlayers; i++)
            {
                CreateStation(i, currentGameTurn);
            }
        }
        else
        {
            foreach(var station in currentGameTurn.Players.OrderBy(x=>x.PlayerColor))
            {
                CreateStation(station.PlayerColor, currentGameTurn);
            }
        }
        GameManager.i.Stations = GameManager.i.Stations.OrderBy(x => x.playerColor).ToList();
        scoreToWin = GetScoreToWin();
        amountToWinText.text = $"{scoreToWin}";
        DoneLoading = true;
    }

    void CreateGrid(GameTurn currentGameTurn)
    {
        var nodeParent = GameObject.Find("Nodes").transform;
        grid = new PathNode[Mathf.RoundToInt(gridSize.x), Mathf.RoundToInt(gridSize.y)];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.up * gridSize.y / 2;
        List<Coords> asteroids = new List<Coords>()
        {
            new Coords(1, 1), new Coords(0, 5), new Coords(3, 9), new Coords(5, 0),
            new Coords(4, 9), new Coords(2, 6), new Coords(2, 2), new Coords(3, 3),
            new Coords(8, 8), new Coords(6, 6), new Coords(7, 7),new Coords(0, 8),
            new Coords(7, 3), new Coords(6, 0), new Coords(4, 2), new Coords(9, 1),
            new Coords(9, 4), new Coords(5, 7),  new Coords(9, 0),new Coords(0, 9),
            new Coords(9, 9),new Coords(0, 0),
        };
        List<Coords> rifts = new List<Coords>()
        {
            new Coords(1, 6), new Coords(6, 8), new Coords(3, 1), new Coords(4, 1),
            new Coords(1, 5), new Coords(4, 4), new Coords(8, 4), new Coords(8, 3), 
            new Coords(5, 5), new Coords(5, 4),new Coords(4, 5), new Coords(5, 8),
        };
        int riftCount = 0;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * .955f * cellPrefabSize.x) + Vector3.up * (y * .75f * cellPrefabSize.y) + Vector3.right * (y % 2) * (-0.475f * cellPrefabSize.x);
                var cell = Instantiate(nodePrefab, worldPoint, Quaternion.identity);
                cell.transform.SetParent(nodeParent);
                grid[x, y] = cell.AddComponent<PathNode>();
                var coords = new Coords(x, y);
                if (currentGameTurn.TurnNumber > 0)
                {
                    grid[x, y].InitializeNode(currentGameTurn.AllNodes.FirstOrDefault(x => new Coords(x.X,x.Y).CoordsEquals(coords)));
                }
                else
                {
                    int maxCredits = 0;
                    int startCredits = 0;
                    int creditRegin = 0;
                    bool isRift = false;
                    if (asteroids.Any(x => x.CoordsEquals(coords)))
                    {
                        startCredits = 2+Globals.GameMatch.MaxPlayers;
                        maxCredits = 12+(Globals.GameMatch.MaxPlayers*2);
                        creditRegin = (Globals.GameMatch.MaxPlayers==4?2:1);
                    }
                    else if (rifts.Any(x => x.CoordsEquals(coords)))
                    {
                        isRift = true;
                    }
                    grid[x, y].InitializeNode(x, y, startCredits, maxCredits, creditRegin, isRift);
                }
                if (grid[x, y].isRift)
                {
                    cell.transform.Find("Nebula").GetComponent<SpriteRenderer>().sprite = nebulaSprite[riftCount % nebulaSprite.Count];
                    cell.transform.Find("Node").GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 1);
                    cell.transform.Find("Node").GetComponent<SpriteRenderer>().sortingOrder = -4;
                    cell.transform.Find("Node/Background").gameObject.SetActive(false);
                    riftCount++;
                    Animator animator = cell.AddComponent<Animator>();
                    animator.runtimeAnimatorController = nodeController;
                    AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                    overrideController["DefaultAnimation"] = nebulaRotationClip; // Replace "DefaultAnimation" with the actual animation state name if needed
                    animator.runtimeAnimatorController = overrideController;
                }
            }
        }
    }
    private void CreateStation(int stationColor, GameTurn currentGameTurn)
    {
        GameObject stationPrefab = unitPrefab;
        SpriteRenderer unitSprite = stationPrefab.transform.Find("Unit").GetComponent<SpriteRenderer>();
        unitSprite.sprite = stationSprites[0];
        unitSprite.color = playerColors[stationColor];
        //unitSprite.color = playerColors[stationId];
        GamePlayer serverPlayer = null;
        Guid stationGuid = Guid.NewGuid();
        Guid fleetGuid = Guid.NewGuid();
        Guid bombGuid = Guid.NewGuid();
        if (Globals.GameMatch.MaxPlayers != 1 || stationColor == 0)
        {
            serverPlayer = currentGameTurn.Players.FirstOrDefault(x=>x.PlayerColor == stationColor);
            stationGuid = (Guid)serverPlayer.PlayerGuid;
            fleetGuid = (Guid)serverPlayer.Units.FirstOrDefault(x => x.UnitType == (int)UnitType.Bomber).UnitGuid;
            bombGuid = (Guid)serverPlayer.Units.FirstOrDefault(x => x.UnitType == (int)UnitType.Bomb).UnitGuid;
        }
        var station = Instantiate(stationPrefab);
        station.transform.SetParent(characterParent);
        var stationNode = station.AddComponent<Station>();
        if (currentGameTurn.TurnNumber > 0)
        {
            stationNode.InitializeStation(serverPlayer);
        } 
        else 
        {
            int spawnX = 2;
            int spawnY = 3;
            Direction facing = Direction.Left;
            if (stationColor == 1)
            {
                spawnX = 7;
                spawnY = 6;
                facing = Direction.Right;
            }
            else if (stationColor == 2)
            {
                spawnX = 3;
                spawnY = 8;
                facing = Direction.BottomLeft;
            }
            else if (stationColor == 3)
            {
                spawnX = 6;
                spawnY = 1;
                facing = Direction.TopRight;
            }
            stationNode.InitializeStation(spawnX, spawnY, stationColor, 15, 1, 7, 6, 5, stationGuid, facing, fleetGuid,bombGuid, serverPlayer?.Credits ?? Constants.StartingCredits);
        }
    }

    public Unit Deploy(Unit unitNode, Guid fleetGuid, Coords coords, UnitType unitType, Guid? _bombGuid = null, bool initialSpawn = false)
    {
        Station station = GameManager.i.Stations.FirstOrDefault(x => x.playerGuid == unitNode.playerGuid);
        var hexesNearby = GameManager.i.GetNodesForDeploy(unitNode,false);
        var startIndex = hexesNearby.FindIndex(x => x.actualCoords.CoordsEquals(coords));
        if (startIndex != -1)
        {
            for (int i = 0; i < hexesNearby.Count; i++)
            {
                int j = (i + startIndex) % hexesNearby.Count;
                if (hexesNearby[j].isAsteroid)
                    continue;
                var unitOnPath = hexesNearby[j].unitOnPath;
                if (unitType == UnitType.Bomber)
                {
                    if (unitOnPath == null)
                    {
                        var fleet = Instantiate(unitPrefab);
                        fleet.transform.SetParent(characterParent);
                        var fleetNode = fleet.AddComponent<Bomber>();
                        fleetNode.InitializeFleet(hexesNearby[j].actualCoords.x, hexesNearby[j].actualCoords.y, station, (int)unitNode.playerColor, 10 + station.bonusHP, 2, 1 + station.bonusMining, station.bonusKinetic + station.kineticDeployPower, station.bonusThermal + station.thermalDeployPower, station.bonusExplosive + station.explosiveDeployPower, fleetGuid, _bombGuid);
                        return fleetNode;
                    }
                }
                else
                {
                    if (unitOnPath == null || (unitOnPath != null && unitOnPath.teamId != unitNode.teamId))
                    {
                        var bomb = Instantiate(bombPrefab);
                        bomb.transform.SetParent(characterParent);
                        var bombNode = bomb.AddComponent<Bomb>();
                        bombNode.InitializeBomb(hexesNearby[j].actualCoords.x, hexesNearby[j].actualCoords.y, station, (int)unitNode.playerColor, 0, 0, (initialSpawn ? 10 : unitNode.explosivePower), fleetGuid);
                        return bombNode;
                    }
                }
            }
        }
        return null;
    }

    internal List<PathNode> FindPath(PathNode startNode, PathNode targetNode, Unit unit, bool checkAsteroids = true)
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
    internal List<PathNode> GetNodesWithinRange(PathNode startNode, int movementLeft, int miningLeft)
    {
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
            List<PathNode> neighbors = GetNeighbors(currentNode, currentNode.minerals > miningLeft);
            foreach (PathNode neighbor in neighbors)
            {
                // Check if enemy owned tile and adjust cost
                int moveCost = currentNode.gCost + GetGCost(neighbor); 
                if (!visited.Contains(neighbor) && moveCost <= movementLeft)
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
            var gridNode = grid[WrapAround(node.actualCoords.x + node.offSet[i].x), WrapAround(node.actualCoords.y + node.offSet[i].y)];
            if (!gridNode.isRift)
                neighbors.Add(gridNode);
        }
        if (node.isAsteroid && blockedByAsteroid && node.parent != null && neighbors.Count > 0)
        {
            neighbors = GetNeighbors(node.parent, true).Where(x => !x.isAsteroid && neighbors.Any(y => y.actualCoords.CoordsEquals(x.actualCoords))).ToList();
            neighbors.Add(node.parent);
        }
        return neighbors;
    }
    internal int GetScoreToWin()
    {
        return (int)(gridSize.x * gridSize.y / GameManager.i.Stations.Select(x => x.teamId).Distinct().Count()) + Globals.Teams;
    }
    internal Guid CheckForWin()
    {
        GetScores();
        scoreToWin = GetScoreToWin();
        for (int i = 0; i < Globals.Teams; i++)
        {
            var teamStations = GameManager.i.Stations.Where(x => x.teamId == i).ToList();
            if (teamStations.Sum(x=>x.score) >= scoreToWin)
            {
                return teamStations.FirstOrDefault().playerGuid;
            }
        }
        return Guid.Empty;
    }

    internal void GetScores()
    {
        Dictionary<Guid, int> scores = new Dictionary<Guid, int>();
        scores.Add(new Guid(), 0);
        foreach (var station in GameManager.i.Stations)
        {
            scores.Add(station.playerGuid, 0);
        }
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                scores[grid[i, j].ownedByGuid]++;
            }
        }
        foreach (var station in GameManager.i.Stations)
        {
            station.score = scores[station.playerGuid];
        }
    }

    public void CloseBurger()
    {
        burgerList.SetActive(false);
    }
    public void ToggleBurger(){
        burgerList.SetActive(!burgerList.activeSelf);
    }
}