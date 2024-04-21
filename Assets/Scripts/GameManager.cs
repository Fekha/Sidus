using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    //prefabs
    private Transform highlightParent;
    public GameObject selectPrefab;
    public GameObject pathPrefab;
    public GameObject movementRangePrefab;
    public GameObject modulePrefab;
    public GameObject modulePanel;

    private List<GameObject> currentPath = new List<GameObject>();

    public Transform ActionBar;
    public Transform StructureModuleBar;
    public Transform ModuleBar;

    public Sprite movementIcon;
    public Sprite moduleIcon;
    public Sprite createFleetIcon;
    public Sprite upgradeFleetIcon;
    public Sprite upgradeStationIcon;
    public Sprite changeShieldIcon;
    public Sprite attachModuleIcon;

    public Sprite lockActionBar;
    public Sprite lockModuleBar;
    public Sprite attachModuleBar;

    private PathNode selectedNode;
    private Structure selectedStructure;
    private List<Node> currentMovementRange = new List<Node>();
    private List<Module> currentModules = new List<Module>();
    internal List<Station> stations = new List<Station>();

    private bool isMoving = false;
    private bool isEndingTurn = false;
    internal int currentStationTurn = 0;

    private Button upgradeButton;
    public Button createFleetButton;
    private TextMeshProUGUI nameValue;
    private TextMeshProUGUI levelValue;
    private TextMeshProUGUI hpValue;
    private TextMeshProUGUI rangeValue;
    private TextMeshProUGUI shieldValue;
    private TextMeshProUGUI electricValue;
    private TextMeshProUGUI thermalValue;
    private TextMeshProUGUI voidValue;
    private TextMeshProUGUI turnValue;
    public GameObject infoPanel;

    internal int winner = -1;
    internal Station CurrentStation {get {return stations[currentStationTurn];}}
    //temp, not for real game
    public GameObject turnLabel;
    public TextMeshProUGUI playerText;

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
        levelValue = infoPanel.transform.Find("LevelValue").GetComponent<TextMeshProUGUI>();
        hpValue = infoPanel.transform.Find("HPValue").GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("MovementValue").GetComponent<TextMeshProUGUI>();
        shieldValue = infoPanel.transform.Find("ShieldValue").GetComponent<TextMeshProUGUI>();
        electricValue = infoPanel.transform.Find("ElectricValue").GetComponent<TextMeshProUGUI>();
        thermalValue = infoPanel.transform.Find("ThermalValue").GetComponent<TextMeshProUGUI>();
        voidValue = infoPanel.transform.Find("VoidValue").GetComponent<TextMeshProUGUI>();
        turnValue = turnLabel.transform.Find("TurnValue").GetComponent<TextMeshProUGUI>();
        upgradeButton = infoPanel.transform.Find("UpgradeButton").GetComponent<Button>();
    }

    void Update()
    {
        if(stations.Count > 1) {
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
                        selectedStructure = targetStructure;
                        if (CurrentStation.actions.Any(x => x.actionType == ActionType.MoveStructure && x.selectedStructure?.structureId == targetStructure.structureId))
                        {
                            Debug.Log($"{targetStructure.structureName} already has a pending movement action.");
                        }
                        else
                        {
                            Debug.Log($"{targetStructure.structureName} Selected.");
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
                                QueueAction(ActionType.MoveStructure);
                                ClearSelection();
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
                            if (targetNode?.nodeOnPath is Structure)
                            {
                                ViewStructureInformation(true);
                            }
                            else
                            {
                                if (selectedStructure != null)
                                {
                                    Debug.Log($"Invalid tile selected, reseting path and selection.");
                                    selectedStructure.resetMovementRange();
                                }
                                ClearSelection();
                            }
                            ViewModules(false);
                        }
                    }
                }
            }
        }
    }
    public void ViewModules(bool active)
    {
        modulePanel.SetActive(active);
    }
    public void ViewStructureInformation(bool active)
    {
        infoPanel.SetActive(active);
    }
    public void AttachModule()
    {
        if (CurrentStation.modules.Count > 0 && selectedStructure.attachedModules.Count < selectedStructure.maxAttachedModules && CurrentStation.stationId == selectedStructure.stationId)
        {
            QueueAction(ActionType.AttachModule);
            ViewStructureInformation(false);
        }
    }
    private void ClearSelection()
    {
        ClearMovementPath();
        ClearMovementRange();
        selectedNode = null;
        selectedStructure = null;
        ViewStructureInformation(false);
        ViewModules(false);
    }

    public void SetTextValues(Structure structure)
    {
        nameValue.text = structure.structureName;
        levelValue.text = structure.level.ToString();
        hpValue.text = structure.hp + "/" + structure.maxHp;
        rangeValue.text = structure.maxRange.ToString();
        electricValue.text = structure.electricAttack.ToString();
        thermalValue.text = structure.thermalAttack.ToString();
        voidValue.text = structure.voidAttack.ToString();
        if (structure.stationId == CurrentStation.stationId)
        {
            upgradeButton.gameObject.SetActive(true);
            upgradeButton.interactable = CanUpgrade(structure);
            shieldValue.text = structure.shield.ToString();
        }
        else
        {
            upgradeButton.gameObject.SetActive(false);
            upgradeButton.interactable = false;
            shieldValue.text = "?";
        }
        SetModuleBar(structure);
        infoPanel.gameObject.SetActive(true);
        ViewModules(false);
    }

    private void AddActionBarImage(ActionType actionType, int i)
    {
        var icon = moduleIcon;
        if (actionType == ActionType.MoveStructure) icon = movementIcon;
        else if (actionType == ActionType.CreateFleet) icon = createFleetIcon;
        else if (actionType == ActionType.UpgradeFleet) icon = upgradeFleetIcon;
        else if (actionType == ActionType.UpgradeStation) icon = upgradeStationIcon;
        else if (actionType == ActionType.AttachModule) icon = attachModuleIcon;
        ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = icon;
        ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(true);
    }
    public void CreateFleet()
    {
        if (CanBuildFleet(CurrentStation))
        {
            if (CurrentStation.ships.Count + CurrentStation.actions.Count(x => x.actionType == ActionType.CreateFleet) < CurrentStation.maxShips)
            {
                QueueAction(ActionType.CreateFleet);
                createFleetButton.interactable = false; //Only allow 1 fleet build per turn?
            }
            else
            {
                Debug.Log("Can not create new fleet, you will hit max fleets for your station level");
            }
        }
    }

    public void UpgradeStructure()
    {
        if (CanUpgrade(selectedStructure)){
            if (selectedStructure is Ship)
                QueueAction(ActionType.UpgradeFleet);
            if (selectedStructure is Station)
                QueueAction(ActionType.UpgradeStation);
        }
        ClearSelection();
    }

    private bool CanUpgrade(Structure structure)
    {
        if (structure is Ship)
            return structure.level < 5 && stations[structure.stationId].modules.Count >= GetCostOfAction(ActionType.UpgradeFleet, structure);
        else
            return structure.level < 5 && stations[structure.stationId].modules.Count >= GetCostOfAction(ActionType.UpgradeStation, structure);
    }
    private bool CanBuildFleet(Station station)
    {
        return station.ships.Count < station.maxShips && station.modules.Count >= GetCostOfAction(ActionType.CreateFleet, station);
    }

    private int GetCostOfAction(ActionType actionType, Structure structure)
    {
        if (structure != null)
        {
            if (actionType == ActionType.CreateFleet)
            {
                return stations[structure.stationId].ships.Count * 2; //2,4,6
            }
            else if (actionType == ActionType.UpgradeFleet)
            {
                return (structure.level * 2) - 1; //1,3,5
            }
            else if (actionType == ActionType.UpgradeStation)
            {
                return structure.level * 3; //3,6,9
            }   
        }
        return int.MaxValue;
    }

    public void GenerateModule()
    {
        QueueAction(ActionType.GenerateModule);
    }

    private void QueueAction(ActionType actionType, List<Module> cost = null)
    {
        if (CurrentStation.actions.Count < CurrentStation.maxActions)
        {
            Debug.Log($"{CurrentStation.structureName} queuing up action {actionType}");
            AddActionBarImage(actionType, CurrentStation.actions.Count());
            var selected = selectedStructure ?? CurrentStation;
            CurrentStation.actions.Add(new Action(actionType, selected, cost));
        }
    }

    public void CancelAction(int slot)
    {
        var action = CurrentStation.actions[slot];
        Debug.Log($"{CurrentStation.structureName} removed action ${action.actionType} from queue");
        if (action is object)
        {
            if (action.actionType == ActionType.MoveStructure)
            {
                action.selectedStructure.resetMovementRange();
            }
            ClearActionBar();
            CurrentStation.actions.RemoveAt(slot);
            for (int i = 0; i < CurrentStation.actions.Count; i++)
            {
                AddActionBarImage(CurrentStation.actions[i].actionType, i);
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
            Debug.Log("Cannot move to position: " + selectedStructure.path?.Last()?.transform?.position + ". Out of range or no longer exists.");
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
            StartCoroutine(TakeTurns()); 
        }
    }
    
    private IEnumerator TakeTurns()
    {
        if (currentStationTurn >= stations.Count)
        {
            playerText.text = "Automating Simultanous Turns";
            currentStationTurn = 0;
            ClearModules();
            for (int i = 0; i < 6; i++)
            {
                foreach (var station in stations)
                {
                    if (i < station.actions.Count)
                    {
                        var action = station.actions[i];
                        if (action is object)
                        {
                            turnValue.text = $"{station.color} action {i + 1}: {action.actionType}";
                            turnLabel.SetActive(true);
                            Debug.Log($"Perfoming {station.color}'s action {i + 1}: {action.actionType}");
                            yield return StartCoroutine(PerformAction(action));     
                        }
                        else
                        {
                            turnValue.text = $"{station.color} action {i + 1}: No Action";
                        }
                        yield return new WaitForSeconds(.5f);
                    }
                }
            }
            stations.ForEach(x => x.actions.Clear());
            winner = GridManager.i.CheckForWin();
            if (winner != -1)
            {
                turnValue.text = $"Player {winner} won";
                Debug.Log($"Player {winner} won");
            }
        }
        if (winner == -1)
        {
            ResetUI();        
            turnLabel.SetActive(false);
            isEndingTurn = false;
            Debug.Log($"New Turn Starting");
        }
    }
    
    private IEnumerator PerformAction(Action action)
    {
        var currentStation = stations[action.selectedStructure.stationId];
        if (action.actionType == ActionType.MoveStructure)
        {
            yield return StartCoroutine(MoveStructure(action.selectedStructure));
            action.selectedStructure.resetMovementRange();
        }
        else if (action.actionType == ActionType.CreateFleet)
        {
            if (CanBuildFleet(stations[action.selectedStructure.stationId]))
            {
                ChargeModules(action);
                yield return StartCoroutine(GridManager.i.CreateFleet(currentStation));
            }
            else
            {
                Debug.Log($"Broke ass ${action.selectedStructure.color} bitch couldn't afford {ActionType.CreateFleet}");
            }
        }
        else if (action.actionType == ActionType.UpgradeFleet)
        {
            if (CanUpgrade(action.selectedStructure))
            {
                ChargeModules(action);
                var ship = action.selectedStructure as Ship;
                ship.level++;
                ship.maxAttachedModules += 2;
                ship.maxHp++;
                ship.electricAttack++;
                ship.voidAttack++;
                ship.thermalAttack++;
            }
            else
            {
                Debug.Log($"Broke ass ${action.selectedStructure.color} bitch couldn't afford {ActionType.UpgradeFleet}");
            }
        }
        else if (action.actionType == ActionType.UpgradeStation)
        {
            if (CanUpgrade(action.selectedStructure))
            {
                ChargeModules(action);
                var station = action.selectedStructure as Station;
                station.level++;
                station.maxAttachedModules += 2;
                station.maxShips++;
                station.maxActions++;
            }
            else
            {
                Debug.Log($"Broke ass ${action.selectedStructure.color} bitch couldn't afford {ActionType.UpgradeStation}");
            }
        }
        else if (action.actionType == ActionType.GenerateModule)
        {
            currentStation.modules.Add(new Module(Random.Range(0,7)));
        } 
        else if (action.actionType == ActionType.AttachModule)
        {
            if (currentStation.modules.Count > 0)
            {
                var moduleToAttach = Random.Range(0, currentStation.modules.Count);
                action.selectedStructure.attachedModules.Add(currentStation.modules[moduleToAttach]);
                currentStation.modules.RemoveAt(moduleToAttach);
            }
        }
    }

    private void ChargeModules(Action action)
    {
        var cost = GetCostOfAction(action.actionType, action.selectedStructure);
        //will need massive overhaul here and a better method name
        for (int i = 0; i < cost; i++)
        {
            stations[action.selectedStructure.stationId].modules.RemoveAt(0);
        }
    }

    private void ResetUI()
    {
        ClearActionBar();
        ClearMovementPath();
        ClearMovementRange();
        SetModuleBar();
        ViewModules(false);
        ViewStructureInformation(false);
        selectedNode = null;
        playerText.text = $"{CurrentStation.color} player selecting actions";
        createFleetButton.interactable = CanBuildFleet(CurrentStation);
    }

    private void SetModuleBar()
    {
        ClearModules();
        foreach (var module in CurrentStation.modules)
        {
            var moduleObject = Instantiate(modulePrefab, ModuleBar);
            //moduleObject.GetComponentInChildren<Button>().onClick.AddListener(() => AttachModule());
            moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
            currentModules.Add(moduleObject.GetComponent<Module>());
        }
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
    private void ClearModules()
    {
        while (currentModules.Count > 0) { Destroy(currentModules[0].gameObject); currentModules.RemoveAt(0); }
    }

    private void ClearActionBar()
    {
        for (int i = 0; i < 5; i++)
        {
            ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = i < CurrentStation.maxActions ? null : lockActionBar;
            ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(false);
        }
    }
    
    private void SetModuleBar(Structure structure)
    {
        for (int i = 0; i < 6; i++)
        {
            if (i < structure.maxAttachedModules)
            {
                if (i < structure.attachedModules.Count)
                {
                    StructureModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = structure.attachedModules[i].icon;
                    StructureModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(true);
                }
                else
                {
                    StructureModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(false);
                    if (structure.stationId == CurrentStation.stationId)
                    {
                        StructureModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = attachModuleBar;
                    }
                    else
                    {
                        StructureModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = lockModuleBar;
                    }
                }
            }
            else
            {
                StructureModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = lockModuleBar;
                StructureModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(false);
            }

        }
    }
}
