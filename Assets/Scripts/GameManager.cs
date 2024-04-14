using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    //prefabs
    public GameObject cellPrefab;
    public GameObject enemyPrefab;
    public GameObject enemyStationPrefab;
    public GameObject obsticalPrefab;
    public GameObject movementRangePrefab;
    public GameObject selectPrefab;
    public GameObject pathPrefab;
    public GameObject playerPrefab;
    public GameObject playerStationPrefab;
    public GameObject infoPanel;

    private List<GameObject> currentPath = new List<GameObject>();
    private Transform characterParent;
    private Transform highlightParent;
    public Transform ActionBar;
    public Sprite movementIcon;
    public Sprite moduleIcon;

    private PathNode selectedNode;
    private Ship selectedShip;
    private Vector2 gridSize = new Vector2(8, 8);
    private List<Node> currentMovementRange = new List<Node>();
    internal List<Station> stations = new List<Station>();

    private Vector3 cellPrefabSize;
    internal PathNode[,] grid;
    private bool isMoving = false;
    private bool isEndingTurn = false;
    
    internal int currentStationTurn = 0;

    private TextMeshProUGUI nameValue;
    private TextMeshProUGUI hpValue;
    private TextMeshProUGUI rangeValue;
    private TextMeshProUGUI shieldValue;
    private TextMeshProUGUI electricValue;
    private TextMeshProUGUI thermalValue;
    private TextMeshProUGUI voidValue;
    void Start()
    {
        i = this;
        cellPrefabSize = cellPrefab.GetComponent<Renderer>().bounds.size;
        CreateGrid();
        FindUI();
        characterParent = GameObject.Find("Characters").transform;
        highlightParent = GameObject.Find("Highlights").transform;
        CreateStation(enemyStationPrefab, enemyPrefab, "Red");
        CreateStation(playerStationPrefab, playerPrefab, "Green");
    }

    private void FindUI()
    {
        nameValue = infoPanel.transform.Find("NameValue").GetComponent<TextMeshProUGUI>();
        hpValue = infoPanel.transform.Find("HPValue").GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("MovementValue").GetComponent<TextMeshProUGUI>();
        shieldValue = infoPanel.transform.Find("ShieldValue").GetComponent<TextMeshProUGUI>();
        electricValue = infoPanel.transform.Find("ElectricValue").GetComponent<TextMeshProUGUI>();
        thermalValue = infoPanel.transform.Find("ThermalValue").GetComponent<TextMeshProUGUI>();
        voidValue = infoPanel.transform.Find("VoidValue").GetComponent<TextMeshProUGUI>();
    }

    private void CreateStation(GameObject stationPrefab, GameObject shipPrefab, string team)
    {
        var spawnY = 0;
        if (team == "Red") {
            spawnY = (int)gridSize.y - 1;
        }
        var station = Instantiate(stationPrefab);
        station.transform.parent = characterParent;
        var stationNode = station.AddComponent<Station>();
        var spawnX = (int)Random.Range(1, gridSize.x - 1);
        stationNode.InitializeStation(spawnX, spawnY, team + " Station", 5, 0, Random.Range(0, 3), Random.Range(7, 10), Random.Range(7, 10), Random.Range(7, 10),1);

        var ship = Instantiate(shipPrefab);
        ship.transform.parent = characterParent;
        var shipNode = ship.AddComponent<Ship>();
        spawnX = Random.Range(0, 2) == 0 ? spawnX - 1 : spawnX + 1;
        shipNode.InitializeShip(spawnX, spawnY, stationNode, team + " Ship", 3, 3, Random.Range(0, 3), Random.Range(1, 4), Random.Range(1, 4), Random.Range(1, 4), 1);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);

            if (hit.collider != null && !isMoving)
            {
                PathNode targetNode = hit.collider.GetComponent<PathNode>();
                Ship targetShip = null;
                if (targetNode != null && targetNode.nodeOnPath is Structure)
                {
                    SetTextValues(targetNode.nodeOnPath as Structure);
                    if (targetNode.nodeOnPath is Ship)
                        targetShip = targetNode.nodeOnPath as Ship;
                }
                //original click on ship
                if (targetShip != null && selectedShip == null && targetShip.stationId == currentStationTurn)
                {
                    if (stations[currentStationTurn].actions.Any(x => x.selectedShip.stationId == targetShip.stationId))
                    {
                        Debug.Log($"{targetShip.structureName} already has a pending action.");
                    }
                    else
                    {
                        Debug.Log($"{targetShip.structureName} Selected.");
                        selectedShip = targetShip;
                        HighlightRangeOfMovement(targetShip.currentPathNode, targetShip.getMovementRange());
                    }
                   
                }
                else
                {
                    //clicked on valid move
                    if (targetNode != null && selectedShip != null && !targetNode.isObstacle && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode))
                    {
                        //double click confirm
                        if (targetNode == selectedNode)
                        {
                            QueueAction(ActionType.Movement);
                            selectedShip.clearMovementRange();
                            ClearMovementPath();
                            ClearMovementRange();
                            selectedNode = null;
                            selectedShip = null;
                            infoPanel.SetActive(false);
                        }
                        //create movement
                        else
                        {
                           
                            int oldPathCount = selectedShip.path?.Count ?? 0;
                            var currentNode = selectedShip.currentPathNode;
                            if (selectedShip.path == null || selectedShip.path.Count == 0)
                            {
                                selectedShip.path = FindPath(currentNode, targetNode);
                                Debug.Log($"Path created for {selectedShip.structureName}");
                            }
                            else
                            {
                                currentNode = selectedShip.path.Last();
                                selectedShip.path.AddRange(FindPath(currentNode, targetNode));
                                Debug.Log($"Path edited for {selectedShip.structureName}");
                            }
                            selectedShip.subtractMovement(selectedShip.path.Count - oldPathCount);
                            HighlightRangeOfMovement(targetNode, selectedShip.getMovementRange());
                            ClearMovementPath();
                            selectedNode = targetNode;
                            foreach (var node in selectedShip.path)
                            {
                                if (node == selectedShip.path.Last())
                                    currentPath.Add(Instantiate(selectPrefab, node.transform.position, Quaternion.identity));
                                else
                                    currentPath.Add(Instantiate(pathPrefab, node.transform.position, Quaternion.identity));
                            }
                        }
                    }
                    //clicked on invalid tile
                    else
                    {
                        if (selectedShip != null)
                        {
                            Debug.Log($"Invalid tile selected, reseting path and selection.");
                            selectedShip.resetMovementRange();
                        }
                        if (targetNode?.nodeOnPath is not Structure)
                        {
                            infoPanel.gameObject.SetActive(false);
                        }
                        ClearMovementPath();
                        ClearMovementRange();
                        selectedNode = null;
                        selectedShip = null;
                    }
                }
            }
        }
    }
    public void SetTextValues(Structure structure)
    {
        infoPanel.gameObject.SetActive(true);
        nameValue.text = structure.structureName;
        hpValue.text = structure.hp + "/" + structure.maxHp;
        rangeValue.text = structure.maxRange.ToString();
        shieldValue.text = structure.stationId == stations[currentStationTurn].stationId ? structure.shield.ToString() : "?";
        electricValue.text = structure.electricAttack.ToString();
        thermalValue.text = structure.thermalAttack.ToString();
        voidValue.text = structure.voidAttack.ToString();
    }
    private void AddActionBarImage(ActionType actionType, int i)
    {
        var icon = moduleIcon;
        if (actionType == ActionType.Movement)
            icon = movementIcon;
        ActionBar.Find($"Slot{i}/Image").GetComponent<Image>().sprite = icon;
        ActionBar.Find($"Slot{i}/Remove").gameObject.SetActive(true);
    }

    public void generateModule()
    {
        QueueAction(ActionType.Module);
    }

    private void QueueAction(ActionType actionType)
    {
        Debug.Log($"{stations[currentStationTurn].structureName} queuing up action {actionType}");
        AddActionBarImage(actionType, stations[currentStationTurn].actions.Count());
        Ship selShip = selectedShip;
        if (actionType == ActionType.Module)
            selShip = null;
        stations[currentStationTurn].actions.Add(new Action(actionType, selShip));
    }

   
    public void cancelAction(int slot)
    {
        var action = stations[currentStationTurn].actions[slot];
        Debug.Log($"{stations[currentStationTurn].structureName} removed action ${action.actionType} from queue");
        if (action is object)
        {
            if (action.actionType == ActionType.Movement)
            {
                action.selectedShip.resetMovementRange();
            }
            ClearActionBar();
            stations[currentStationTurn].actions.RemoveAt(slot);
            for (int i = 0; i < stations[currentStationTurn].actions.Count; i++)
            {
                AddActionBarImage(stations[currentStationTurn].actions[i].actionType, i);
            }
        }
    }
    void CreateGrid()
    {
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
                if (y != 0 && y != gridSize.y-1 && x != 0 && x != gridSize.x - 1)
                    isObstacle = Random.Range(0, 6) == 0; 

                var cell = Instantiate(isObstacle ? obsticalPrefab : cellPrefab, worldPoint, Quaternion.identity);
                cell.transform.parent = nodeParent;
                grid[x, y] = cell.AddComponent<PathNode>();
                grid[x, y].InitializeNode(x, y, isObstacle);
            }
        }
    }

    private IEnumerator MoveShip(Ship selectedShip)
    {
        isMoving = true;
        if (selectedShip != null && selectedShip.path != null && selectedShip.path.Count > 0 && selectedShip.path.Count <= selectedShip.getMaxMovementRange())
        {
            Debug.Log("Moving to position: " + selectedShip.currentPathNode.transform.position);
            yield return StartCoroutine(MoveOnPath(selectedShip, selectedShip.path));
        }
        else
        {
            Debug.Log("Cannot move to position: " + selectedShip.path.Last().transform.position + ". Out of range or no longer exists.");
        }
        yield return new WaitForSeconds(.1f);
        isMoving = false;
    }

    private IEnumerator MoveOnPath(Ship ship, List<PathNode> path)
    {
        int i = 0;
        ship.currentPathNode.nodeOnPath = null;
        ClearMovementRange();
        foreach (var node in path)
        {
            i++;
            if (node.nodeOnPath is Structure)
            {
                var structureOnPath = node.nodeOnPath as Structure;
                //if not ally, attack instead of move
                if (structureOnPath.stationId != ship.stationId)
                {
                    Debug.Log($"{ship.structureName} is attacking {structureOnPath.structureName}");
                    if (ship.shield != AttackType.Electric && structureOnPath.shield != AttackType.Electric)
                    {
                        ship.hp -= Math.Max(structureOnPath.electricAttack - ship.electricAttack, 0);
                        structureOnPath.hp -= Math.Max(ship.electricAttack - structureOnPath.electricAttack, 0);
                    }
                    if (ship.hp > 0 && structureOnPath.hp > 0 && ship.shield != AttackType.Thermal && structureOnPath.shield != AttackType.Thermal)
                    {
                        ship.hp -= Math.Max(structureOnPath.thermalAttack - ship.thermalAttack, 0);
                        structureOnPath.hp -= Math.Max(ship.thermalAttack - structureOnPath.thermalAttack, 0);
                    }
                    if (ship.hp > 0 && structureOnPath.hp > 0 && ship.shield != AttackType.Void && structureOnPath.shield != AttackType.Void)
                    {
                        ship.hp -= Math.Max(structureOnPath.voidAttack - ship.voidAttack, 0);
                        structureOnPath.hp -= Math.Max(ship.voidAttack - structureOnPath.voidAttack, 0);
                    }
                }
                if (structureOnPath.hp > 0)
                {
                    //even if allied don't move through, don't feel like doing recursive checks right now
                    Debug.Log($"{ship.structureName} movement was blocked by {structureOnPath.structureName}");
                    break;
                }
                else
                {
                    Debug.Log($"{ship.structureName} destroyed {structureOnPath.structureName}");
                    Destroy(structureOnPath.gameObject);
                    break;
                }
                if (ship.hp <= 0)
                {
                    Debug.Log($"{structureOnPath.structureName} destroyed {ship.structureName}");
                    Destroy(ship.gameObject);
                    break;
                }
            }
            float elapsedTime = 0f;
            float totalTime = .25f;
            while (elapsedTime <= totalTime)
            {
                ship.transform.position = Vector3.Lerp(ship.currentPathNode.transform.position, node.transform.position, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ship.currentPathNode = node;
            ship.SetNodeColor();
            yield return new WaitForSeconds(.25f);
        }
        //character.movementRange -= path.Count();
        //HighlightRangeOfMovement(ship.currentPathNode, ship.getMovementRange());
        ship.transform.position = ship.currentPathNode.transform.position;
        ship.currentPathNode.nodeOnPath = ship;
    }

    List<PathNode> FindPath(PathNode startNode, PathNode targetNode)
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

    public void HighlightRangeOfMovement(PathNode currentNode, int shipRange)
    {
        ClearMovementRange();
        List<PathNode> nodesWithinRange = GetNodesWithinRange(currentNode, shipRange);
        foreach (PathNode node in nodesWithinRange)
        {
            GameObject range = Instantiate(movementRangePrefab, node.transform.position, Quaternion.identity);
            range.transform.parent = highlightParent;
            var rangeComponent = range.AddComponent<Node>();
            rangeComponent.Initialize(node);
            currentMovementRange.Add(rangeComponent);
        }
    }
    public void EndTurn()
    {
        if (!isEndingTurn)
        {
            isEndingTurn = true;
            Debug.Log($"Turn Ending, Starting Simultanous Turns");
            currentStationTurn++;
            ResetUI();
            StartCoroutine(TakeTurns());
        }
    }

    private IEnumerator TakeTurns()
    {
        if (currentStationTurn >= stations.Count)
        {
            for (int i = 0; i < 6; i++)
            {
                foreach (var station in stations)
                {
                    if (i < station.actions.Count)
                    {
                        var action = station.actions[i];
                        if (action is object)
                        {
                            Debug.Log($"Perfoming {station.structureName}'s action {i + 1}: {action.actionType}");
                            yield return StartCoroutine(PerformAction(action));
                        }
                    }
                }
            }
            stations.ForEach(x => x.actions.Clear());
            currentStationTurn = 0;
        }
        isEndingTurn = false;
        Debug.Log($"New Turn Starting");

    }

    private IEnumerator PerformAction(Action action)
    {
        if (action.actionType == ActionType.Movement)
        {
            yield return StartCoroutine(MoveShip(action.selectedShip));
            action.selectedShip.resetMovementRange();
        }
    }

    private void ResetUI()
    {
        ClearActionBar();
        ClearMovementPath();
        ClearMovementRange();
        infoPanel.SetActive(false);
        selectedNode = null;
    }

    //private IEnumerator AITurn()
    //{
    //    enemy.SetMovementRange();
    //    List<Node> enemyPath = FindPath(enemy.currentNode, selectedShip.currentNode);
    //    yield return new WaitForSeconds(.5f);
    //    yield return StartCoroutine(MoveOnPath(enemy, enemyPath));
    //    //AI Ends Turn
    //    selectedShip.SetMovementRange();
    //}

    private void ClearMovementRange()
    {
        while(currentMovementRange.Count > 0) { Destroy(currentMovementRange[0].gameObject); currentMovementRange.RemoveAt(0); }
    }

    private void ClearMovementPath()
    {
        while (currentPath.Count > 0) { Destroy(currentPath[0].gameObject); currentPath.RemoveAt(0); }
    }

    private void ClearActionBar()
    {
        foreach (var station in stations)
        {
            int i = 0;
            foreach (var action in station.actions)
            {
                ActionBar.Find($"Slot{i}/Image").GetComponent<Image>().sprite = null;
                ActionBar.Find($"Slot{i}/Remove").gameObject.SetActive(false);
                i++;
            }
        }
    }

        List<PathNode> GetNodesWithinRange(PathNode clickedNode, int range)
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
}
