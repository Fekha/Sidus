using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.UI;
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

    private List<GameObject> currentPath = new List<GameObject>();
    private Transform characterParent;
    private Transform highlightParent;
    public Transform ActionBar;
    public Sprite movementIcon;


    private PathNode selectedNode;
    private Ship selectedShip;
    private Vector2 gridSize = new Vector2(8, 8);
    private List<Node> currentMovementRange = new List<Node>();
    internal List<Station> stations = new List<Station>();

    private Vector3 cellPrefabSize;
    internal PathNode[,] grid;
    private bool isMoving = false;
    private List<PathNode> path;
    internal int currentStationTurn = 0;
    void Start()
    {
        i = this;
        cellPrefabSize = cellPrefab.GetComponent<Renderer>().bounds.size;
        CreateGrid();

        characterParent = GameObject.Find("Characters").transform;
        highlightParent = GameObject.Find("Highlights").transform;

        CreateStation(enemyStationPrefab, enemyPrefab, (int)gridSize.y - 1);
        CreateStation(playerStationPrefab, playerPrefab, 0);
    }

    private void CreateStation(GameObject stationPrefab, GameObject shipPrefab, int spawnY)
    {
        var station = Instantiate(stationPrefab);
        station.transform.parent = characterParent;
        var stationNode = station.AddComponent<Station>();
        var spawnX = (int)Random.Range(1, gridSize.x - 1);
        stationNode.InitializeStation(spawnX, spawnY, 5);

        var ship = Instantiate(shipPrefab);
        ship.transform.parent = characterParent;
        var shipNode = ship.AddComponent<Ship>();
        spawnX = Random.Range(0, 2) == 0 ? spawnX - 1 : spawnX + 1;
        shipNode.InitializeShip(spawnX, spawnY, 3, 5, stationNode);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);

            if (hit.collider != null && !isMoving)
            {
                Ship targetShip = hit.collider.GetComponent<Ship>();
                if (targetShip != null && targetShip.stationId == currentStationTurn)
                {
                    selectedShip = targetShip;
                    HighlightRangeOfMovement(targetShip);
                }
                else
                {
                    PathNode targetNode = hit.collider.GetComponent<PathNode>();
                    if (targetNode != null && !targetNode.isTaken && currentMovementRange.Select(x => x.currentNode).Contains(targetNode))
                    {
                        if (targetNode == selectedNode)
                        {

                            ActionBar.Find("Slot1/Image").GetComponent<Image>().sprite = movementIcon;
                            ActionBar.Find("Slot1/Remove").gameObject.SetActive(true);
                            stations[currentStationTurn].actions.Add(new Action("Move",path,selectedShip));
                            ClearMovementPath();
                            selectedNode = null;
                        }
                        else
                        {
                            path = FindPath(selectedShip.currentNode, targetNode);
                            if (targetNode != selectedNode)
                            {
                                ClearMovementPath();
                            }
                            selectedNode = targetNode;
                            foreach (var node in path)
                            {
                                if (node == path.Last())
                                    currentPath.Add(Instantiate(selectPrefab, node.transform.position, Quaternion.identity));
                                else
                                    currentPath.Add(Instantiate(pathPrefab, node.transform.position, Quaternion.identity));
                            }
                        }
                    }
                }
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

    private IEnumerator MovePlayer(List<PathNode> path, Ship selectedShip)
    {
        isMoving = true;
        if (path.Count > 0 && path.Count <= selectedShip.movementRange)
        {
            Debug.Log("Moving to position: " + selectedShip.currentNode.transform.position);

            yield return StartCoroutine(MoveOnPath(selectedShip, path));
        }
        else
        {
            Debug.Log("Cannot move to position: " + path.Last().transform.position + ". Out of range.");
        }
        yield return new WaitForSeconds(.1f);
        isMoving = false;
    }

    private IEnumerator MoveOnPath(Ship character, List<PathNode> path)
    {
        int i = 0;
        character.currentNode.OnPathNode = null;
        ClearMovementRange();
        foreach (var node in path)
        {
            i++;
            if (i > character.movementRange)
            {
                break;
            }
            float elapsedTime = 0f;
            float totalTime = .25f;
            while (elapsedTime <= totalTime)
            {
                character.transform.position = Vector3.Lerp(character.currentNode.transform.position, node.transform.position, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            character.currentNode = node;
            yield return new WaitForSeconds(.25f);
        }
        character.movementRange -= path.Count();
        HighlightRangeOfMovement(character);
        character.transform.position = character.currentNode.transform.position;
        character.currentNode.OnPathNode = character;
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
                if (neighbor != targetNode && (neighbor.isTaken || closedSet.Contains(neighbor)))
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

    public void HighlightRangeOfMovement(Ship ship)
    {
        ClearMovementRange();
        List<PathNode> nodesWithinRange = GetNodesWithinRange(ship.currentNode, ship.movementRange);
        foreach (PathNode node in nodesWithinRange)
        {
            GameObject range = Instantiate(movementRangePrefab, node.transform.position, Quaternion.identity);
            range.transform.parent = highlightParent;
            var rangeComponent = range.AddComponent<Node>();
            rangeComponent.Initialize(node);
            GameManager.i.currentMovementRange.Add(rangeComponent);
        }
    }
    public void EndTurn()
    {
        currentStationTurn++;
        if (currentStationTurn >= stations.Count)
        {
            foreach (var station in stations)
            {
                foreach (var action in station.actions)
                {
                    if(action.ActionType == "Move")
                        StartCoroutine(MovePlayer(action.movement,action.selectedShip));
                }
                foreach (var ship in station.ships)
                {
                    ship.resetMovementRange();
                }
            }
            currentStationTurn = 0;
        }
        //StartCoroutine(AITurn());
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

    List<PathNode> GetNodesWithinRange(PathNode clickedNode, int range, bool ignoreTaken = false)
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

                if(currentNode != clickedNode)
                    nodesWithinRange.Add(currentNode);

                foreach (PathNode neighbor in GetNeighbors(currentNode))
                {
                    if (!visited.Contains(neighbor) && (ignoreTaken || !neighbor.isTaken))
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
            if (!currentNode.isTaken)
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
