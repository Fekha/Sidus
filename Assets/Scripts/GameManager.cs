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
    private Transform highlightParent;
    public GameObject selectPrefab;
    public GameObject pathPrefab;
    public GameObject movementRangePrefab;

    private List<GameObject> currentPath = new List<GameObject>();

    public Transform ActionBar;
    public Sprite movementIcon;
    public Sprite moduleIcon;

    private PathNode selectedNode;
    private Structure selectedStructure;
    private List<Node> currentMovementRange = new List<Node>();
    internal List<Station> stations = new List<Station>();

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
    public GameObject infoPanel;

    private void Awake()
    {
        i = this;
    }
    void Start()
    {
        FindUI();
        highlightParent = GameObject.Find("Highlights").transform;
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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);

            if (hit.collider != null && !isMoving)
            {
                PathNode targetNode = hit.collider.GetComponent<PathNode>();
                Structure targetStructure = null;
                if (targetNode != null && targetNode.nodeOnPath is Structure)
                {
                    targetStructure = targetNode.nodeOnPath as Structure;
                    SetTextValues(targetStructure);
                }
                //original click on ship
                if (targetStructure != null && selectedStructure == null && targetStructure.stationId == currentStationTurn)
                {
                    if (stations[currentStationTurn].actions.Any(x => x.selectedStructure.structureId == targetStructure.structureId))
                    {
                        Debug.Log($"{targetStructure.structureName} already has a pending action.");
                    }
                    else
                    {
                        Debug.Log($"{targetStructure.structureName} Selected.");
                        selectedStructure = targetStructure;
                        HighlightRangeOfMovement(targetStructure.currentPathNode, targetStructure.getMovementRange());
                    }
                }
                else
                {
                    //clicked on valid move
                    if (targetNode != null && selectedStructure != null && !targetNode.isObstacle && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode))
                    {
                        //double click confirm
                        if (targetNode == selectedNode)
                        {
                            QueueAction(ActionType.Movement);
                            selectedStructure.clearMovementRange();
                            ClearMovementPath();
                            ClearMovementRange();
                            selectedNode = null;
                            selectedStructure = null;
                            infoPanel.SetActive(false);
                        }
                        //create movement
                        else
                        {
                           
                            int oldPathCount = selectedStructure.path?.Count ?? 0;
                            var currentNode = selectedStructure.currentPathNode;
                            if (selectedStructure.path == null || selectedStructure.path.Count == 0)
                            {
                                selectedStructure.path = GridManager.i.FindPath(currentNode, targetNode);
                                Debug.Log($"Path created for {selectedStructure.structureName}");
                            }
                            else
                            {
                                currentNode = selectedStructure.path.Last();
                                selectedStructure.path.AddRange(GridManager.i.FindPath(currentNode, targetNode));
                                Debug.Log($"Path edited for {selectedStructure.structureName}");
                            }
                            selectedStructure.subtractMovement(selectedStructure.path.Count - oldPathCount);
                            HighlightRangeOfMovement(targetNode, selectedStructure.getMovementRange());
                            ClearMovementPath();
                            selectedNode = targetNode;
                            foreach (var node in selectedStructure.path)
                            {
                                if (node == selectedStructure.path.Last())
                                    currentPath.Add(Instantiate(selectPrefab, node.transform.position, Quaternion.identity));
                                else
                                    currentPath.Add(Instantiate(pathPrefab, node.transform.position, Quaternion.identity));
                            }
                        }
                    }
                    //clicked on invalid tile
                    else
                    {
                        if (selectedStructure != null)
                        {
                            Debug.Log($"Invalid tile selected, reseting path and selection.");
                            selectedStructure.resetMovementRange();
                        }
                        if (targetNode?.nodeOnPath is not Structure)
                        {
                            infoPanel.gameObject.SetActive(false);
                        }
                        ClearMovementPath();
                        ClearMovementRange();
                        selectedNode = null;
                        selectedStructure = null;
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

    public void GenerateModule()
    {
        QueueAction(ActionType.Module);
    }

    private void QueueAction(ActionType actionType)
    {
        Debug.Log($"{stations[currentStationTurn].structureName} queuing up action {actionType}");
        AddActionBarImage(actionType, stations[currentStationTurn].actions.Count());
        Structure selShip = selectedStructure;
        if (actionType == ActionType.Module)
            selShip = null;
        stations[currentStationTurn].actions.Add(new Action(actionType, selShip));
    }

    public void CancelAction(int slot)
    {
        var action = stations[currentStationTurn].actions[slot];
        Debug.Log($"{stations[currentStationTurn].structureName} removed action ${action.actionType} from queue");
        if (action is object)
        {
            if (action.actionType == ActionType.Movement)
            {
                action.selectedStructure.resetMovementRange();
            }
            ClearActionBar();
            stations[currentStationTurn].actions.RemoveAt(slot);
            for (int i = 0; i < stations[currentStationTurn].actions.Count; i++)
            {
                AddActionBarImage(stations[currentStationTurn].actions[i].actionType, i);
            }
        }
    }

    private IEnumerator MoveStructure(Structure selectedStructure)
    {
        isMoving = true;
        if (selectedStructure != null && selectedStructure.path != null && selectedStructure.path.Count > 0 && selectedStructure.path.Count <= selectedStructure.getMaxMovementRange())
        {
            Debug.Log("Moving to position: " + selectedStructure.currentPathNode.transform.position);
            yield return StartCoroutine(MoveOnPath(selectedStructure, selectedStructure.path));
        }
        else
        {
            Debug.Log("Cannot move to position: " + selectedStructure.path.Last().transform.position + ". Out of range or no longer exists.");
        }
        yield return new WaitForSeconds(.1f);
        isMoving = false;
    }

    private IEnumerator MoveOnPath(Structure structure, List<PathNode> path)
    {
        int i = 0;
        structure.currentPathNode.nodeOnPath = null;
        ClearMovementRange();
        foreach (var node in path)
        {
            i++;
            if (node.nodeOnPath is Structure)
            {
                var structureOnPath = node.nodeOnPath as Structure;
                //if not ally, attack instead of move
                if (structureOnPath.stationId != structure.stationId)
                {
                    Debug.Log($"{structure.structureName} is attacking {structureOnPath.structureName}");
                    if (structure.shield != AttackType.Electric)
                        structure.hp -= Math.Max(structureOnPath.electricAttack - structure.electricAttack, 0);
                    if(structureOnPath.shield != AttackType.Electric)
                        structureOnPath.hp -= Math.Max(structure.electricAttack - structureOnPath.electricAttack, 0);
                    if (structure.hp > 0 && structureOnPath.hp > 0)
                    {
                        if (structure.shield != AttackType.Thermal)
                            structure.hp -= Math.Max(structureOnPath.thermalAttack - structure.thermalAttack, 0);
                        if(structureOnPath.shield != AttackType.Thermal)
                            structureOnPath.hp -= Math.Max(structure.thermalAttack - structureOnPath.thermalAttack, 0);
                    }
                    if (structure.hp > 0 && structureOnPath.hp > 0)
                    {
                        if(structure.shield != AttackType.Void)
                            structure.hp -= Math.Max(structureOnPath.voidAttack - structure.voidAttack, 0);
                        if(structureOnPath.shield != AttackType.Void)
                            structureOnPath.hp -= Math.Max(structure.voidAttack - structureOnPath.voidAttack, 0);
                    }
                }
                if (structureOnPath.hp > 0)
                {
                    //even if allied don't move through, don't feel like doing recursive checks right now
                    Debug.Log($"{structure.structureName} movement was blocked by {structureOnPath.structureName}");
                    break;
                }
                else
                {
                    Debug.Log($"{structure.structureName} destroyed {structureOnPath.structureName}");
                    Destroy(structureOnPath.gameObject);
                }
                if (structure.hp <= 0)
                {
                    Debug.Log($"{structureOnPath.structureName} destroyed {structure.structureName}");
                    Destroy(structure.gameObject);
                    break;
                }
            }
            float elapsedTime = 0f;
            float totalTime = .25f;
            while (elapsedTime <= totalTime)
            {
                structure.transform.position = Vector3.Lerp(structure.currentPathNode.transform.position, node.transform.position, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            structure.currentPathNode = node;
            if (path.Count == i || node.ownedById == -1)
            {
                structure.SetNodeColor();
            }
            yield return new WaitForSeconds(.25f);
        }
        //character.movementRange -= path.Count();
        //HighlightRangeOfMovement(ship.currentPathNode, ship.getMovementRange());
        structure.transform.position = structure.currentPathNode.transform.position;
        structure.currentPathNode.nodeOnPath = structure;
    }
    
    public void HighlightRangeOfMovement(PathNode currentNode, int shipRange)
    {
        ClearMovementRange();
        List<PathNode> nodesWithinRange = GridManager.i.GetNodesWithinRange(currentNode, shipRange);
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
            var winner = GridManager.i.CheckForWin();
            if (winner != -1)
            {
                Debug.Log($"Player {winner} won");
            }
        }
        isEndingTurn = false;
        Debug.Log($"New Turn Starting");
    }
    
    private IEnumerator PerformAction(Action action)
    {
        if (action.actionType == ActionType.Movement)
        {
            yield return StartCoroutine(MoveStructure(action.selectedStructure));
            action.selectedStructure.resetMovementRange();
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
}
