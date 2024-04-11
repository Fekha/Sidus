using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    public GameObject cellPrefab;
    public GameObject enemyPrefab;
    public GameObject enemyStationPrefab;
    public GameObject obsticalPrefab;
    public GameObject movementRangePrefab;
    public GameObject cardRangePrefab;
    public GameObject selectPrefab;
    public GameObject cardTargetPrefab;
    public GameObject pathPrefab;
    private List<GameObject> currentPath = new List<GameObject>();
    internal HighlightNode spellTarget;
    public GameObject playerPrefab;
    public GameObject playerStationPrefab;
    private Transform highlightParent;
    private Node selectedNode;
    private List<Character> characters = new List<Character>();
    private Character player;
    private Character enemy;
    private Vector2 gridSize = new Vector2(8, 8);
    private List<HighlightNode> currentMovementRange = new List<HighlightNode>();
    internal List<HighlightNode> currentCardRange = new List<HighlightNode>();
    private Vector3 cellPrefabSize;
    private Node[,] grid;
    private bool isMoving = false;
    internal CardManager dragging;
    List<Node> path;
    public GameObject CardPrefab;
    void Start()
    {
        i = this;
        cellPrefabSize = cellPrefab.GetComponent<Renderer>().bounds.size;
        CreateGrid();

        var characterParent = GameObject.Find("Characters").transform;
        highlightParent = GameObject.Find("Highlights").transform;

        var enemyStation = Instantiate(enemyStationPrefab);
        enemyStation.transform.parent = characterParent;
        var enemyStationNode = enemyStation.AddComponent<Character>();
        var stationNodeX = (int)Random.Range(1, gridSize.x-1);
        enemyStationNode.Initialize(stationNodeX, (int)gridSize.y - 1, 0, 5);

        var enemyShip = Instantiate(enemyPrefab);
        enemyShip.transform.parent = characterParent;
        var enemy = enemyShip.AddComponent<Character>();
        var left = Random.Range(0, 2) == 0;
        enemy.Initialize(left ? stationNodeX-1 : stationNodeX+1, (int)gridSize.y-1, 3, 5);

        var playerStation = Instantiate(playerStationPrefab);
        playerStation.transform.parent = characterParent;
        var playerStationNode = playerStation.AddComponent<Character>();
        stationNodeX = (int)Random.Range(1, gridSize.x-1);
        playerStationNode.Initialize(stationNodeX, 0, 0, 5);

        var playerShip = Instantiate(playerPrefab);
        playerShip.transform.parent = characterParent;
        player = playerShip.AddComponent<Character>();
        left = Random.Range(0, 2) == 0;
        player.Initialize(left ? stationNodeX - 1 : stationNodeX + 1, 0, 4, 10);

        //CreateCard(5,2);
        //CreateCard(4,3);
        //CreateCard(3,4);
        //CreateCard(2,5);

        player.StartTurn();
    }

    private void CreateCard(int range, int damage)
    {
        var cardObj = Instantiate(CardPrefab);
        var card = cardObj.AddComponent<CardManager>();
        card.Initialize(range, damage);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);

            if (hit.collider != null && !isMoving)
            {
                Character targetCharacter = hit.collider.GetComponent<Character>();
                if (targetCharacter != null)
                {
                    HighlightRangeOfMovement(targetCharacter);
                }
                else
                {
                    Node targetNode = hit.collider.GetComponent<Node>();
                    if (targetNode != null && !targetNode.isTaken && currentMovementRange.Select(x => x.currentNode).Contains(targetNode))
                    {
                        if (targetNode == selectedNode)
                        {
                            ClearMovementPath();
                            selectedNode = null;
                            StartCoroutine(MovePlayer(path));
                        }
                        else
                        {
                            path = FindPath(player.currentNode, targetNode);
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

        if (Input.GetMouseButton(0))
        {
            if (dragging)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);

                if (hit.collider != null)
                {
                    Node targetNode = hit.collider.GetComponent<Node>();
                    if (targetNode != null)
                    {
                        if (spellTarget == null)
                        {
                            var spell = Instantiate(cardTargetPrefab, targetNode.transform.position, Quaternion.identity);
                            spellTarget = spell.AddComponent<HighlightNode>();
                            spellTarget.Initialize(targetNode);
                        }
                        else if (spellTarget.currentNode != targetNode)
                        {
                            ClearSpellTarget();
                        }
                    }
                }
                else
                {
                    ClearSpellTarget();
                }
            }
        }
    }

    void CreateGrid()
    {
        var nodeParent = GameObject.Find("Nodes").transform;
        grid = new Node[Mathf.RoundToInt(gridSize.x), Mathf.RoundToInt(gridSize.y)];
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
                grid[x, y] = cell.AddComponent<Node>();
                grid[x, y].Initialize(isObstacle, x, y);
            }
        }
    }

    private IEnumerator MovePlayer(List<Node> path)
    {
        isMoving = true;
        if (path.Count > 0 && path.Count <= player.movementRange)
        {
            Debug.Log("Moving to position: " + player.currentNode.transform.position);

            yield return StartCoroutine(MoveOnPath(player, path));
        }
        else
        {
            Debug.Log("Cannot move to position: " + path.Last().transform.position + ". Out of range.");
        }
        yield return new WaitForSeconds(.1f);
        isMoving = false;
    }

    private IEnumerator MoveOnPath(Character character, List<Node> path)
    {
        int i = 0;
        character.currentNode.character = null;
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
        character.currentNode.character = character;
    }

    List<Node> FindPath(Node startNode, Node targetNode)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
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

            foreach (Node neighbor in GetNeighbors(currentNode))
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
        return new List<Node>();
    }

    List<Node> GetLineOfSight(Node characterNode, Node targetNode)
    {
        List<Node> lineOfSight = new List<Node>();

        int x0 = characterNode.gridX;
        int y0 = characterNode.gridY;
        int x1 = targetNode.gridX;
        int y1 = targetNode.gridY;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            lineOfSight.Add(grid[x0, y0]); // Assuming 'grid' is your 2D array of nodes

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return lineOfSight;
    }
    public void HighlightCardRange()
    {
        ClearMovementRange();
        ClearCardRange();

        List<Node> nodesWithinRange = GetNodesWithinRange(player.currentNode, dragging.range, true);

        foreach (Node node in nodesWithinRange)
        {
            List<Node> lineOfSight = GetLineOfSight(player.currentNode, node);
            if (!lineOfSight.Any(x => x.isObstacle))
            {
                // The ray reached the target position without hitting an obstacle
                GameObject highlight = Instantiate(cardRangePrefab, node.transform.position, Quaternion.identity);
                var highlightNode = highlight.AddComponent<HighlightNode>();
                highlightNode.Initialize(node);
                currentCardRange.Add(highlightNode);
            }
        }
    }
  
    public void HighlightRangeOfMovement(Character character)
    {
        ClearMovementRange();
        List<Node> nodesWithinRange = GetNodesWithinRange(character.currentNode, character.movementRange);
        foreach (Node node in nodesWithinRange)
        {
            GameObject range = Instantiate(movementRangePrefab, node.transform.position, Quaternion.identity);
            range.transform.parent = highlightParent;
            var rangeComponent = range.AddComponent<HighlightNode>();
            rangeComponent.Initialize(node);
            GameManager.i.currentMovementRange.Add(rangeComponent);
        }
    }
    public void EndTurn()
    {
        //StartCoroutine(AITurn());
    }

    private IEnumerator AITurn()
    {
        enemy.StartTurn();
        List<Node> enemyPath = FindPath(enemy.currentNode, player.currentNode);
        yield return new WaitForSeconds(.5f);
        yield return StartCoroutine(MoveOnPath(enemy, enemyPath));
        //AI Ends Turn
        player.StartTurn();
    }

    private void ClearMovementRange()
    {
        while(currentMovementRange.Count > 0) { Destroy(currentMovementRange[0].gameObject); currentMovementRange.RemoveAt(0); }
    }

    private void ClearMovementPath()
    {
        while (currentPath.Count > 0) { Destroy(currentPath[0].gameObject); currentPath.RemoveAt(0); }
    }
    
    internal void ClearSpellTarget()
    {
        if (spellTarget != null)
        {
            Destroy(spellTarget.gameObject);
            spellTarget = null;
        }
    }

    public void ClearCardRange()
    {
        foreach (HighlightNode highlight in currentCardRange)
        {
            Destroy(highlight.gameObject);
        }
        currentCardRange.Clear();
    }

    List<Node> GetNodesWithinRange(Node clickedNode, int range, bool ignoreTaken = false)
    {
        List<Node> nodesWithinRange = new List<Node>();

        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        queue.Enqueue(clickedNode);
        visited.Add(clickedNode);

        while (queue.Count > 0 && range >= 0)
        {
            int levelSize = queue.Count;

            for (int i = 0; i < levelSize; i++)
            {
                Node currentNode = queue.Dequeue();

                if(currentNode != clickedNode)
                    nodesWithinRange.Add(currentNode);

                foreach (Node neighbor in GetNeighbors(currentNode))
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

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            if (!currentNode.isTaken)
                path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return dstX + dstY;
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        int[] xOffsets = { 0, 1, 0, -1 }; // Right, Up, Left, Down
        int[] yOffsets = { 1, 0, -1, 0 }; // Right, Up, Left, Down

        for (int i = 0; i < xOffsets.Length; i++)
        {
            int checkX = node.gridX + xOffsets[i];
            int checkY = node.gridY + yOffsets[i];

            if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    internal IEnumerator CastSpell()
    {
        var character = spellTarget.currentNode.character;
        if (character != null)
        {
            character.TakeDamage(dragging.damage);
        }
        FinishCastingSpell();
        yield return new WaitForEndOfFrame();
    }

    internal void FinishCastingSpell()
    {
        dragging = null;
        ClearCardRange();
        ClearSpellTarget();
        HighlightRangeOfMovement(player);
    }

    public class Node : MonoBehaviour
    {
        public bool isObstacle;
        public Character character;
        public int gridX;
        public int gridY;
        public int gCost;
        public int hCost;
        public bool isTaken { get { return isObstacle || character != null; } }
        public int fCost { get { return gCost + hCost; } }
        public Node parent;
        public void Initialize(bool obstacle, int x, int y)
        {
            isObstacle = obstacle;
            gridX = x;
            gridY = y;
        }
    }

    public class Character : MonoBehaviour
    {
        public Node currentNode { get { return GameManager.i.grid[x, y]; } set 
            {
                x = value.gridX;
                y = value.gridY;
                GameManager.i.grid[x, y] = value;
            } 
        }
        public int maxMovementRange;
        public int movementRange;
        private int x;
        private int y;
        private int maxHp;
        private int hp;
        private TMP_Text hpBar;
        public void Initialize(int _x, int _y, int _movementRange, int _hp)
        {
            x = _x;
            y = _y;
            //hpBar = transform.Find("HPBar").GetComponent<TMP_Text>();
            maxHp = _hp;
            hp = _hp;
            SetHPText();
            maxMovementRange = _movementRange;
            movementRange = _movementRange;
            currentNode.character = this;
            transform.position = currentNode.transform.position;
            GameManager.i.characters.Add(this);
        }
        public void SetHPText()
        {
            //hpBar.text = hp + "/" + maxHp; 
        }
        public void TakeDamage(int damage)
        {
            hp -= damage;
            SetHPText();
        }
        internal void StartTurn()
        {
            movementRange = maxMovementRange;
          
        }
    }

    public class HighlightNode : MonoBehaviour
    {
        public Node currentNode { get { return GameManager.i.grid[x, y]; } }
        private int x;
        private int y;
        internal void Initialize(Node node)
        {
            x = node.gridX; 
            y = node.gridY;
        }
    }
}
