using StartaneousAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    //prefabs
    private Transform highlightParent;
    public GameObject selectPrefab;
    public GameObject pathPrefab;
    public GameObject movementRangePrefab;
    public GameObject modulePrefab;
    public GameObject moduleInfoPanel;

    private List<GameObject> currentPathObjects = new List<GameObject>();
    private List<PathNode> SelectedPath;

    public Transform ActionBar;
    public Transform StructureModuleBar;
    public Transform ModuleBar;

    public Sprite lockActionBar;
    public Sprite lockModuleBar;
    public Sprite attachModuleBar;

    private PathNode SelectedNode;
    private Structure SelectedStructure;
    private List<Node> currentMovementRange = new List<Node>();
    private List<Module> currentModules = new List<Module>();
    internal List<Station> stations = new List<Station>();

    private bool isMoving = false;
    private bool isEndingTurn = false;

    private Button upgradeButton;
    public Button createFleetButton;
    private TextMeshProUGUI createFleetCost;
    private TextMeshProUGUI upgradeCost;
    private TextMeshProUGUI nameValue;
    private TextMeshProUGUI levelValue;
    private TextMeshProUGUI hpValue;
    private TextMeshProUGUI rangeValue;
    private TextMeshProUGUI shieldValue;
    private TextMeshProUGUI electricValue;
    private TextMeshProUGUI thermalValue;
    private TextMeshProUGUI voidValue;
    private TextMeshProUGUI turnValue;
    private TextMeshProUGUI moduleInfoValue;
    public TextMeshProUGUI ScoreToWinText;
    public GameObject infoPanel;
    internal List<Structure> AllStructures = new List<Structure>();
    internal List<Module> AllModules = new List<Module>();
    internal int winner = -1;
    internal Station MyStation {get {return stations[Globals.myStationIndex];}}
    //temp, not for real game
    public GameObject turnLabel;
    private SqlManager sql;
    private int TurnNumber = 0;
    private Turn[]? TurnsFromServer;
    private int maxPlayers = 2;
    private void Awake()
    {
        i = this;
        sql = new SqlManager();
    }
    void Start()
    {
        FindUI();
        StartCoroutine(WaitforGameToStart());
    }

    private IEnumerator WaitforGameToStart()
    {
        while (!HasGameStarted())
        {
            yield return new WaitForSeconds(.1f);
        }
        ResetUI();
    }

    public bool HasGameStarted()
    {
        return stations.Count > 1 && Globals.GameId != Guid.Empty;
    }

    private void FindUI()
    {
        highlightParent = GameObject.Find("Highlights").transform;
        nameValue = infoPanel.transform.Find("NameValue").GetComponent<TextMeshProUGUI>();
        levelValue = infoPanel.transform.Find("LevelValue").GetComponent<TextMeshProUGUI>();
        hpValue = infoPanel.transform.Find("HPValue").GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("MovementValue").GetComponent<TextMeshProUGUI>();
        shieldValue = infoPanel.transform.Find("ShieldValue").GetComponent<TextMeshProUGUI>();
        electricValue = infoPanel.transform.Find("ElectricValue").GetComponent<TextMeshProUGUI>();
        thermalValue = infoPanel.transform.Find("ThermalValue").GetComponent<TextMeshProUGUI>();
        voidValue = infoPanel.transform.Find("VoidValue").GetComponent<TextMeshProUGUI>();
        turnValue = turnLabel.transform.Find("TurnValue").GetComponent<TextMeshProUGUI>();
        moduleInfoValue = moduleInfoPanel.transform.Find("ModuleInfoText").GetComponent<TextMeshProUGUI>();
        createFleetCost = createFleetButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        upgradeButton = infoPanel.transform.Find("UpgradeButton").GetComponent<Button>();
        upgradeCost = upgradeButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if(HasGameStarted()) {
            if (Input.GetMouseButtonDown(0) && !isEndingTurn)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);

                if (hit.collider != null && !isMoving)
                {
                    PathNode targetNode = hit.collider.GetComponent<PathNode>();
                    Structure targetStructure = null;
                    if (targetNode != null && targetNode.structureOnPath != null)
                    {
                        targetStructure = targetNode.structureOnPath;
                    }
                    //original click on ship
                    if (targetStructure != null && SelectedStructure == null && targetStructure.stationId == MyStation.stationId)
                    {
                        SetTextValues(targetStructure);
                        SelectedStructure = targetStructure;
                        if (MyStation.actions.Any(x => x.actionType == ActionType.MoveStructure && x.selectedStructure?.structureGuid == targetStructure.structureGuid))
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
                        if (targetNode != null && SelectedStructure != null && !targetNode.isObstacle && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode))
                        {
                            //double click confirm
                            if (targetNode == SelectedNode)
                            {
                                QueueAction(ActionType.MoveStructure);
                                ClearSelection();
                            }
                            //create movement
                            else
                            {
                                int oldPathCount = SelectedPath?.Count ?? 0;
                                var currentNode = SelectedStructure.currentPathNode;
                                if (SelectedPath == null || SelectedPath.Count == 0)
                                {
                                    SelectedPath = GridManager.i.FindPath(currentNode, targetNode);
                                    Debug.Log($"Path created for {SelectedStructure.structureName}");
                                }
                                else
                                {
                                    currentNode = SelectedPath.Last();
                                    SelectedPath.AddRange(GridManager.i.FindPath(currentNode, targetNode));
                                    Debug.Log($"Path edited for {SelectedStructure.structureName}");
                                }
                                SelectedStructure.subtractMovement(SelectedPath.Count - oldPathCount);
                                HighlightRangeOfMovement(targetNode, SelectedStructure.getMovementRange());
                                ClearMovementPath();
                                SelectedNode = targetNode;
                                foreach (var node in SelectedPath)
                                {
                                    if (node == SelectedPath.Last())
                                        currentPathObjects.Add(Instantiate(selectPrefab, node.transform.position, Quaternion.identity));
                                    else
                                        currentPathObjects.Add(Instantiate(pathPrefab, node.transform.position, Quaternion.identity));
                                }
                            }
                        }
                        //clicked on invalid tile
                        else
                        {
                            if (targetStructure != null)
                            {
                                if (SelectedStructure == null)
                                {
                                    SetTextValues(targetStructure);
                                    ViewStructureInformation(true);
                                }
                            }
                            else
                            {
                                if (SelectedStructure != null)
                                {
                                    Debug.Log($"Invalid tile selected, reseting path and selection.");
                                    SelectedStructure.resetMovementRange();
                                }
                                ClearSelection();
                            }
                        }
                    }
                }
            }
        }
    }
    public void ViewStructureInformation(bool active)
    {
        if(HasGameStarted())
            infoPanel.SetActive(active);
    }
    public void AttachModule()
    {
        if (SelectedStructure != null && MyStation.modules.Count > 0 && (SelectedStructure.attachedModules.Count+MyStation.actions.Where(x=>x.actionType == ActionType.AttachModule && x.selectedStructure.structureGuid == SelectedStructure.structureGuid).Count()) < SelectedStructure.maxAttachedModules && MyStation.stationId == SelectedStructure.stationId)
        {
            QueueAction(ActionType.AttachModule);
            ClearSelection();
        }
    }
    //only call for queying up
    private List<Guid>? GetAvailableModules(Station station, int modulesToGet)
    {
        if (modulesToGet > 0)
        {
            List<Guid> assignedModules = station.actions.SelectMany(x => x.selectedModulesIds).ToList();
            List<Guid> availableModules = station.modules.Where(x => !assignedModules.Contains(x.moduleGuid)).Select(x => x.moduleGuid).ToList();
            if (station.modules.Count - assignedModules.Count >= modulesToGet)
            {
                return availableModules.GetRange(0, modulesToGet);
            }
        }
        return null;
    }

    public void DetachModule(int i)
    {
        if (SelectedStructure != null && MyStation.stationId == SelectedStructure.stationId)
        {
            if (MyStation.actions.Any(x => x.actionType == ActionType.DetachModule && x.selectedModulesIds.Any(y => y == SelectedStructure.attachedModules[i].moduleGuid)))
            {
                Debug.Log($"The action {ActionType.DetachModule} for the module {SelectedStructure.attachedModules[i].type} has already been queued up");
            }
            else{
                QueueAction(ActionType.DetachModule, new List<Guid>() { SelectedStructure.attachedModules[i].moduleGuid });
                ClearSelection();
            }
        }
    }
    private void ClearSelection()
    {
        ClearMovementPath();
        ClearMovementRange();
        SelectedNode = null;
        SelectedStructure = null;
        SelectedPath = null;
        ViewStructureInformation(false);
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
        shieldValue.text = structure.shield.ToString();
        var actionType = structure is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        if (structure.stationId == MyStation.stationId)
        {
            upgradeButton.gameObject.SetActive(true);
            upgradeButton.interactable = CanQueueUpgrade(structure, actionType);
        }
        else
        {
            upgradeButton.gameObject.SetActive(false);
            upgradeButton.interactable = false;
        }
        upgradeCost.text = GetCostText(GetCostOfAction(actionType, structure, true));
        SetModuleBar(structure);
        infoPanel.gameObject.SetActive(true);
    }

    private string GetCostText(int cost)
    {
        return $"({cost} Modules)";
    }
    private void AddActionBarImage(ActionType actionType, int i)
    {
        ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Actions/{(int)actionType}");
        ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(true);
    }
    public void CreateFleet()
    {
        if (CanQueueBuildFleet(MyStation) && HasGameStarted())
        {
            if (MyStation.ships.Count + MyStation.actions.Count(x => x.actionType == ActionType.CreateFleet) < MyStation.maxShips)
            {
                QueueAction(ActionType.CreateFleet);
                
            }
            else
            {
                Debug.Log("Can not create new fleet, you will hit max fleets for your station level");
            }
        }
    }
    private void UpdateFleetCostText()
    {
        createFleetButton.interactable = CanQueueBuildFleet(MyStation);
        createFleetCost.text = GetCostText(GetCostOfAction(ActionType.CreateFleet, MyStation,true));
    }
    public void UpgradeStructure()
    {
        ActionType actionType = SelectedStructure is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        if (CanQueueUpgrade(SelectedStructure, actionType) && HasGameStarted()){
            QueueAction(actionType);
        }
        ClearSelection();
    }

    private bool CanQueueUpgrade(Structure structure, ActionType actionType)
    {
        return CanLevelUp(structure, actionType) && GetAvailableModules(MyStation, GetCostOfAction(actionType, structure,true)) != null;
    }
    private bool CanPerformUpgrade(Structure structure, ActionType actionType)
    {
        return CanLevelUp(structure, actionType) && stations[structure.stationId].modules.Count >= GetCostOfAction(actionType, structure,false);
    }
    private bool CanLevelUp(Structure structure, ActionType actionType)
    {
        var nextLevel = (structure.level + stations[structure.stationId].actions.Where(x => x.selectedStructure.structureGuid == structure.structureGuid && x.actionType == actionType).Count());
        if (actionType == ActionType.UpgradeFleet)
            return nextLevel < stations[structure.stationId].level * 2;
        else
            return nextLevel < 5;
    }

    private bool CanQueueBuildFleet(Station station)
    {
        return (station.ships.Count+ station.actions.Where(x => x.actionType == ActionType.CreateFleet).Count()) < station.maxShips && GetAvailableModules(station, GetCostOfAction(ActionType.CreateFleet, station,true)) != null;
    }
    private bool CanPerformBuildFleet(Station station)
    {
        return station.ships.Count < station.maxShips && station.modules.Count >= GetCostOfAction(ActionType.CreateFleet, station, false);
    }
    private int GetCostOfAction(ActionType actionType, Structure structure, bool queue)
    {
        var station = stations[structure.stationId];
        var countingQueue = queue ? station.actions.Where(x => x.actionType == actionType && x.selectedStructure.structureGuid == structure.structureGuid).Count() : 0;
        if (actionType == ActionType.CreateFleet)
        {
            return (station.ships.Count + countingQueue) * 3; //3,6,9
        }
        else if (actionType == ActionType.UpgradeFleet)
        {
            return ((structure.level + countingQueue) * 2) - 1; //1,3,5,7,9,11
        }
        else if (actionType == ActionType.UpgradeStation)
        {
            return (structure.level + countingQueue) * 4; //4,8,12
        }
        else if (actionType == ActionType.AttachModule)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void GenerateModule()
    {
        if (HasGameStarted())
        {
            QueueAction(ActionType.GenerateModule);
        }
    }

    private void QueueAction(ActionType actionType, List<Guid> selectedModules = null)
    {
        if (MyStation.actions.Count < MyStation.maxActions)
        {
            var costOfAction = GetCostOfAction(actionType, MyStation, true);
            if(selectedModules == null)
                selectedModules = GetAvailableModules(MyStation, costOfAction);
            if (costOfAction == 0 || selectedModules != null)
            {
                Debug.Log($"{MyStation.structureName} queuing up action {actionType}");
                AddActionBarImage(actionType, MyStation.actions.Count());
                MyStation.actions.Add(new Action(actionType, SelectedStructure ?? MyStation, selectedModules, SelectedPath));
                UpdateFleetCostText();
            }
            else
            {
                Debug.Log($"You can't afford to queue {actionType} due to pending actions or general lackthere of modules");
            }
        }
    }

    public void CancelAction(int slot)
    {
        var action = MyStation.actions[slot];
        Debug.Log($"{MyStation.structureName} removed action {action.actionType} from queue");
        if (action is object)
        {
            if (action.actionType == ActionType.MoveStructure)
            {
                action.selectedStructure.resetMovementRange();
            }
            ClearActionBar();
            MyStation.actions.RemoveAt(slot);
            for (int i = 0; i < MyStation.actions.Count; i++)
            {
                AddActionBarImage(MyStation.actions[i].actionType, i);
            }
        }
        UpdateFleetCostText();
    }

    private IEnumerator MoveStructure(Structure selectedStructure, List<PathNode> selectedPath)
    {
        isMoving = true;
        if (selectedStructure != null && selectedPath != null && selectedPath.Count > 0 && selectedPath.Count <= selectedStructure.getMaxMovementRange())
        {
            Debug.Log("Moving to position: " + selectedStructure.currentPathNode.transform.position);
            yield return StartCoroutine(MoveOnPath(selectedStructure, selectedPath));
        }
        else
        {
            Debug.Log("Cannot move to position: " + selectedPath?.Last()?.transform?.position + ". Out of range or no longer exists.");
        }
        yield return new WaitForSeconds(.1f);
        isMoving = false;
    }

    private IEnumerator MoveOnPath(Structure structure, List<PathNode> path)
    {
        int i = 0;
        structure.currentPathNode.structureOnPath = null;
        ClearMovementRange();
        foreach (var node in path)
        {
            i++;
            if (node.structureOnPath != null)
            {
                var structureOnPath = node.structureOnPath;
                //if not ally, attack instead of move
                if (structureOnPath.stationId != structure.stationId)
                {
                    Debug.Log($"{structure.structureName} is attacking {structureOnPath.structureName}");
                    if (structure.hp > 0 && structureOnPath.hp > 0)
                    {
                        if (structure.shield != AttackType.Electric)
                            structure.hp -= Math.Max(structureOnPath.electricAttack - structure.electricAttack, 0);
                        if (structureOnPath.shield != AttackType.Electric)
                            structureOnPath.hp -= Math.Max(structure.electricAttack - structureOnPath.electricAttack, 0);
                    }
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
                var endMovement = false;
                if (structure.hp <= 0)
                {
                    Debug.Log($"{structureOnPath.structureName} destroyed {structure.structureName}");
                    Destroy(structure.gameObject);
                    if (structureOnPath is Ship) {
                        if (structureOnPath.hp > 0)
                        {
                            LevelUpShip(structureOnPath as Ship);
                        }
                    }
                    if (structure is Station) {
                        (structure as Station).defeated = true;
                    }
                    endMovement = true; // you're dead
                }
                if (structureOnPath.hp > 0)
                {
                    //even if allied don't move through, don't feel like doing recursive checks right now
                    Debug.Log($"{structure.structureName} movement was blocked by {structureOnPath.structureName}");
                    endMovement = true; // they aren't dead
                }
                else
                {
                    Debug.Log($"{structure.structureName} destroyed {structureOnPath.structureName}");
                    Destroy(structureOnPath.gameObject); //they are dead
                    if (structure is Ship)
                        LevelUpShip(structure as Ship);
                    if (structureOnPath is Station)
                        (structureOnPath as Station).defeated = true;

                }
                if (endMovement)
                    break;
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
        structure.currentPathNode.structureOnPath = structure;
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
        if (!isEndingTurn && HasGameStarted())
        {
            isEndingTurn = true;
            Debug.Log($"Turn Ending, Starting Simultanous Turns");
            var actionToPost = MyStation.actions.Select(x => new ActionIds(x)).ToList();
            var turnToPost = new Turn(Globals.GameId, Globals.localStationId, TurnNumber,actionToPost);
            var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(turnToPost);
            StartCoroutine(sql.PostRoutine<bool>($"Game/EndTurn", stringToPost));
            StartCoroutine(TakeTurns()); 
        }
    }
    
    private IEnumerator TakeTurns()
    {
        turnValue.text = "Waiting for the other player...";
        turnLabel.SetActive(true);
        TurnsFromServer = null;
        while (TurnsFromServer == null)
        {
            yield return StartCoroutine(sql.GetRoutine<Turn[]?>($"Game/GetTurn?gameId={Globals.GameId}&turnNumber={TurnNumber}", CheckForTurns));
        }
        yield return StartCoroutine(AutomateTurns(TurnsFromServer));
        if (winner == -1)
        {
            ResetUI();
            turnLabel.SetActive(false);
            isEndingTurn = false;
            Debug.Log($"New Turn Starting");
        }
    }

    private void CheckForTurns(Turn[]? turns)
    {
        TurnsFromServer = turns;
    }

    private IEnumerator AutomateTurns(Turn[] turns)
    {
        ClearModules();
        for (int i = 0; i < 6; i++)
        {
            for(int j = 0; j < maxPlayers; j++)
            {
                if (i < turns[j].Actions.Count)
                {
                    if (turns[j].Actions[i] is object)
                    {
                        var action = new Action(turns[j].Actions[i]);
                        turnValue.text = $"{stations[j].color} action {i + 1}: {action.actionType}";
                        Debug.Log($"Perfoming {stations[j].color}'s action {i + 1}: {action.actionType}");
                        yield return StartCoroutine(PerformAction(action));
                    }
                    else
                    {
                        turnValue.text = $"{stations[j].color} action {i + 1}: No Action";
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
        TurnNumber++;
    }

    private IEnumerator PerformAction(Action action)
    {
        if (action.selectedStructure != null)
        {
            var currentStation = stations[action.selectedStructure.stationId];
            if (action.actionType == ActionType.MoveStructure)
            {
                yield return StartCoroutine(MoveStructure(action.selectedStructure, action.selectedPath));
                action.selectedStructure.resetMovementRange();
            }
            else if (action.actionType == ActionType.CreateFleet)
            {
                if (CanPerformBuildFleet(currentStation))
                {
                    ChargeModules(action);
                    yield return StartCoroutine(GridManager.i.CreateFleet(currentStation, action.generatedGuid));
                }
                else
                {
                    Debug.Log($"Broke ass {action.selectedStructure.color} bitch couldn't afford {ActionType.CreateFleet}");
                }
            }
            else if (action.actionType == ActionType.UpgradeFleet)
            {
                if (CanPerformUpgrade(action.selectedStructure, action.actionType))
                {
                    ChargeModules(action);
                    LevelUpShip(action.selectedStructure as Ship);
                }
                else
                {
                    Debug.Log($"Broke ass {action.selectedStructure.color} bitch couldn't afford {ActionType.UpgradeFleet}");
                }
            }
            else if (action.actionType == ActionType.UpgradeStation)
            {
                if (CanPerformUpgrade(action.selectedStructure, action.actionType))
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
                    Debug.Log($"Broke ass {action.selectedStructure.color} bitch couldn't afford {ActionType.UpgradeStation}");
                }
            }
            else if (action.actionType == ActionType.GenerateModule)
            {
                currentStation.modules.Add(new Module(action.generatedModuleId, action.generatedGuid));
            }
            else if (action.actionType == ActionType.AttachModule)
            {
                if (currentStation.modules.Count > 0 && action.selectedStructure.attachedModules.Count < action.selectedStructure.maxAttachedModules)
                {
                    Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModulesIds[0]);
                    if (selectedModule is object)
                    {
                        action.selectedStructure.attachedModules.Add(selectedModule);
                        action.selectedStructure.EditModule(selectedModule.type);
                        currentStation.modules.Remove(action.selectedStructure.attachedModules.FirstOrDefault(x => x.moduleGuid == selectedModule.moduleGuid));
                    }
                }
            }
            else if (action.actionType == ActionType.DetachModule)
            {
                if (action.selectedStructure.attachedModules.Count > 0 && action.selectedModulesIds != null && action.selectedModulesIds.Count > 0)
                {
                    Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModulesIds[0]);
                    if (selectedModule is object)
                    {
                        currentStation.modules.Add(selectedModule);
                        action.selectedStructure.EditModule(selectedModule.type, -1);
                        action.selectedStructure.attachedModules.Remove(action.selectedStructure.attachedModules.FirstOrDefault(x => x.moduleGuid == selectedModule.moduleGuid));
                    }
                }
            }
        }
    }

    private void LevelUpShip(Ship ship)
    {
        if (CanLevelUp(ship, ActionType.UpgradeFleet)){
            ship.level++;
            ship.maxAttachedModules += 1;
            ship.maxHp++;
            ship.hp++;
            ship.electricAttack++;
            ship.voidAttack++;
            ship.thermalAttack++;
        }
    }

    private void ChargeModules(Action action)
    {
        var station = stations[action.selectedStructure.stationId];
        //will need massive overhaul here and a better method name
        foreach (var selectedModule in action.selectedModulesIds)
        {
            station.modules.RemoveAt(station.modules.Select(x=>x.moduleGuid).ToList().IndexOf(selectedModule));
        }
    }

    private void ResetUI()
    {
        ClearActionBar();
        SetModuleBar();
        ClearSelection();
        GridManager.i.GetScores();
        ScoreToWinText.text = $"Tiles to win: {MyStation.score}/{GridManager.i.scoreToWin}";
        UpdateFleetCostText();
    }

    private void SetModuleBar()
    {
        ClearModules();
        foreach (var module in MyStation.modules)
        {
            var moduleObject = Instantiate(modulePrefab, ModuleBar);
            moduleObject.GetComponentInChildren<Button>().onClick.AddListener(() => SetModuleInfo(module));
            moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
            currentModules.Add(moduleObject.GetComponent<Module>());
        }
    }

    private void SetModuleInfo(Module module)
    {
        moduleInfoValue.text = module.effectText;
        ViewModuleInfo(true);
    }

    public void ViewModuleInfo(bool active)
    {
        moduleInfoPanel.SetActive(active);
    }

    private void ClearMovementRange()
    {
        while(currentMovementRange.Count > 0) { Destroy(currentMovementRange[0].gameObject); currentMovementRange.RemoveAt(0); }
    }

    private void ClearMovementPath()
    {
        while (currentPathObjects.Count > 0) { Destroy(currentPathObjects[0].gameObject); currentPathObjects.RemoveAt(0); }
    }
    private void ClearModules()
    {
        while (currentModules.Count > 0) { Destroy(currentModules[0].gameObject); currentModules.RemoveAt(0); }
    }

    private void ClearActionBar()
    {
        for (int i = 0; i < 5; i++)
        {
            ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = i < MyStation.maxActions ? null : lockActionBar;
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
                    StructureModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(structure.stationId == MyStation.stationId);
                    StructureModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
                    var module = structure.attachedModules[i];
                    StructureModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() =>
                        SetModuleInfo(module)
                    );
                }
                else
                {
                    StructureModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(false);
                    if (structure.stationId == MyStation.stationId)
                    {
                        StructureModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = attachModuleBar;
                        StructureModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
                        StructureModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() => AttachModule());
                    }
                    else
                    {
                        StructureModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = null;
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

    internal List<PathNode> GetPathFromCoords(List<Coords> selectedCoords)
    {
        return selectedCoords.Select(x => GridManager.i.grid[x.x, x.y]).ToList();
    }
}
