using StartaneousAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;

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
    public GameObject infoPanel;
    public GameObject fightPanel;
    public GameObject alertPanel;
    public GameObject selectModuelPanel;

    private List<GameObject> currentPathObjects = new List<GameObject>();
    private List<PathNode> SelectedPath;

    public Transform ActionBar;
    public Transform StructureModuleBar;
    public Transform ModuleGrid;
    public Transform SelectedModuleGrid;

    public Sprite lockActionBar;
    public Sprite lockModuleBar;
    public Sprite attachModuleBar;

    private PathNode SelectedNode;
    private Structure SelectedStructure;
    private List<Node> currentMovementRange = new List<Node>();
    private List<Module> currentModules = new List<Module>();
    private List<Module> currentModulesForSelection = new List<Module>();
    internal List<Station> Stations = new List<Station>();

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
    private TextMeshProUGUI fightText;
    public TextMeshProUGUI ScoreToWinText;
    public TextMeshProUGUI TurnOrderText;
    public TextMeshProUGUI alertText;
 
    internal List<Structure> AllStructures = new List<Structure>();
    internal List<Module> AllModules = new List<Module>();
    internal int winner = -1;
    internal Station MyStation {get {return Stations[Globals.myStationIndex];}}
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
    public void ViewFightPanel(bool active)
    {
        fightPanel.SetActive(active);
    }
    
    public void ShowAlertPanel(int unlock)
    {
        alertPanel.SetActive(true);
        alertText.text = $"This action slot will unlock once you own {Convert.ToInt32(Math.Floor(GridManager.i.scoreToWin * (.25 * unlock)))} tiles.";
    }
    public void HideAlertPanel()
    {
        alertPanel.SetActive(false);
    }
    public bool HasGameStarted()
    {
        return Stations.Count > 1 && Globals.GameId != Guid.Empty;
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
        fightText = fightPanel.transform.Find("Panel/FightText").GetComponent<TextMeshProUGUI>();
        alertText = alertPanel.transform.Find("AlertText").GetComponent<TextMeshProUGUI>();
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
                    //original click on fleet
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
                        if (targetNode != null && SelectedStructure != null && !targetNode.isObstacle && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode) && !MyStation.actions.Any(x => x.actionType == ActionType.MoveStructure && x.selectedStructure?.structureGuid == SelectedStructure.structureGuid))
                        {
                            //double click confirm
                            if (targetNode == SelectedNode)
                            {
                                QueueAction(ActionType.MoveStructure);
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
            ClearSelectableModules();
            List<Guid> assignedModules = MyStation.actions.SelectMany(x => x.selectedModulesIds).ToList();
            var availableModules = MyStation.modules.Where(x => !assignedModules.Contains(x.moduleGuid)).ToList();
            foreach (var module in availableModules)
            {
                var selectedStructure = SelectedStructure;
                var moduleObject = Instantiate(modulePrefab, SelectedModuleGrid);
                moduleObject.GetComponentInChildren<Button>().onClick.AddListener(() => SetSelectedModule(module.moduleGuid, selectedStructure));
                moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
                moduleObject.transform.Find("Queued").gameObject.SetActive(false);
                currentModulesForSelection.Add(moduleObject.GetComponent<Module>());
            }
            ViewModuleSelection(true);
        }
    }
    //only call for queying up
    private List<Guid>? GetAvailableModules(Station station, int modulesToGet)
    {
        if (modulesToGet > 0)
        {
            List<Guid> assignedModules = station.actions.SelectMany(x => x.selectedModulesIds).ToList();
            List<Guid> availableModules = station.modules.Where(x => !assignedModules.Contains(x.moduleGuid)).Select(x => x.moduleGuid).ToList();
            if (availableModules.Count >= modulesToGet)
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
        upgradeButton.interactable = false;
        if (structure.stationId == MyStation.stationId)
        {
            if (CanLevelUp(structure, actionType, true))
            {
                upgradeCost.text = GetCostText(GetCostOfAction(actionType, structure, true));
                upgradeButton.interactable = CanQueueUpgrade(structure, actionType);
            }
            else
            {
                if (structure is Fleet)
                {
                    upgradeCost.text = "(Station Upgrade Required)";
                }
                else
                {
                    upgradeCost.text = "(Max Station Level)";
                }
            }
            upgradeButton.gameObject.SetActive(true);
        }
        else
        {
            upgradeButton.gameObject.SetActive(false);
        }
        SetModuleBar(structure);
        infoPanel.gameObject.SetActive(true);
    }

    private string GetCostText(int cost)
    {
        string plural = cost == 1 ? "" : "s";
        return $"(Costs {cost} Module{plural})";
    }
    private void AddActionBarImage(ActionType actionType, int i)
    {
        ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Actions/{(int)actionType}");
        ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(true);
        ActionBar.Find($"Action{i}/UnlockInfo").gameObject.SetActive(false);
    }
    public void CreateFleet()
    {
        if (CanQueueBuildFleet(MyStation) && HasGameStarted())
        {
            if (MyStation.fleets.Count + MyStation.actions.Count(x => x.actionType == ActionType.CreateFleet) < MyStation.maxFleets)
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
        if (CanBuildAdditonalFleet(MyStation))
        {
            createFleetCost.text = GetCostText(GetCostOfAction(ActionType.CreateFleet, MyStation, true));
        }
        else
        {
            createFleetCost.text = "(Station Upgrade Required)";
        }
    }
    public void UpgradeStructure()
    {
        ActionType actionType = SelectedStructure is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        if (CanQueueUpgrade(SelectedStructure, actionType) && HasGameStarted()){
            QueueAction(actionType);
        }
    }
    private bool CanQueueUpgrade(Structure structure, ActionType actionType)
    {
        return CanLevelUp(structure, actionType, true) && GetAvailableModules(MyStation, GetCostOfAction(actionType, structure, true)) != null;
    }
    private bool CanPerformUpgrade(Structure structure, ActionType actionType)
    {
        return CanLevelUp(structure, actionType, false) && Stations[structure.stationId].modules.Count >= GetCostOfAction(actionType, structure,false);
    }
    private bool CanLevelUp(Structure structure, ActionType actionType, bool countQueue)
    {
        var countingQueue = countQueue ? Stations[structure.stationId].actions.Where(x => x.selectedStructure.structureGuid == structure.structureGuid && x.actionType == actionType).Count() : 0;
        var nextLevel = (structure.level + countingQueue);
        if (actionType == ActionType.UpgradeFleet)
            return nextLevel < Stations[structure.stationId].level;
        else
            return nextLevel < 7;
    }

    private bool CanQueueBuildFleet(Station station)
    {
        return CanBuildAdditonalFleet(station) && GetAvailableModules(station, GetCostOfAction(ActionType.CreateFleet, station,true)) != null;
    }
    private bool CanBuildAdditonalFleet(Station station)
    {
        return station.fleets.Count + station.actions.Where(x => x.actionType == ActionType.CreateFleet).Count() < station.maxFleets;
    }
    private bool CanPerformBuildFleet(Station station)
    {
        return station.fleets.Count < station.maxFleets && station.modules.Count >= GetCostOfAction(ActionType.CreateFleet, station, false);
    }
    private int GetCostOfAction(ActionType actionType, Structure structure, bool countQueue)
    {
        var station = Stations[structure.stationId];
        var countingQueue = countQueue ? station.actions.Where(x => x.actionType == actionType && x.selectedStructure.structureGuid == structure.structureGuid).Count() : 0;
        if (actionType == ActionType.CreateFleet)
        {
            return (station.fleets.Count + countingQueue) * 2; //2,4,6,8,10
        }
        else if (actionType == ActionType.UpgradeFleet)
        {
            return ((structure.level + countingQueue) * 2) - 1; //1,3,5,7,9
        }
        else if (actionType == ActionType.UpgradeStation)
        {
            return ((structure.level + countingQueue) * 2) + 1; //3,5,7,9,11
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

    private void QueueAction(ActionType actionType, List<Guid> selectedModules = null, Structure _structure = null)
    {
        if (MyStation.actions.Count < MyStation.maxActions)
        {
            var structure = _structure ?? SelectedStructure ?? MyStation;
            var costOfAction = GetCostOfAction(actionType, structure, true);
            if(selectedModules == null)
                selectedModules = GetAvailableModules(MyStation, costOfAction);
            if (costOfAction == 0 || selectedModules != null)
            {
                Debug.Log($"{MyStation.structureName} queuing up action {actionType}");
                AddActionBarImage(actionType, MyStation.actions.Count());
                MyStation.actions.Add(new Action(actionType, structure, selectedModules, SelectedPath));
                UpdateFleetCostText();
                SetModuleGrid();
                ClearSelection();
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
        SetModuleGrid();
        ClearSelection();
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
            bool blockedMovement = false;
            if (node.structureOnPath != null)
            {
                var structureOnPath = node.structureOnPath;
                var beforeStats = $"Pre-fight stats:\n{structure.structureName}: HP {structure.hp}, Electric {structure.electricAttack}, Thermal {structure.thermalAttack}, Void {structure.voidAttack}.\n{structureOnPath.structureName}: HP {structureOnPath.hp}, Electric {structureOnPath.electricAttack}, Thermal {structureOnPath.thermalAttack}, Void {structureOnPath.voidAttack}.";
                var duringFightText = "";
                //if not ally, attack instead of move
                if (structureOnPath.stationId != structure.stationId)
                {
                    Debug.Log($"{structure.structureName} is attacking {structureOnPath.structureName}");
                    for (int attackType = 0; attackType <= (int)AttackType.Void; attackType++)
                    {
                        if (structure.hp > 0 && structureOnPath.hp > 0)
                            duringFightText += Fight(structure, structureOnPath, (AttackType)attackType);
                    }
                    fightText.text = $"{beforeStats}\n\n{duringFightText}\nPost-fight stats:\n{structure.structureName}: HP {structure.hp}, Electric {structure.electricAttack}, Thermal {structure.thermalAttack}, Void {structure.voidAttack}.\n{structureOnPath.structureName}: HP {structureOnPath.hp}, Electric {structureOnPath.electricAttack}, Thermal {structureOnPath.thermalAttack}, Void {structureOnPath.voidAttack}.";
                    ViewFightPanel(true);
                    while (fightPanel.activeInHierarchy)
                    {
                        yield return new WaitForSeconds(.5f);
                    }
                }
                if (structure.hp <= 0)
                {
                    Debug.Log($"{structureOnPath.structureName} destroyed {structure.structureName}");
                    if (structureOnPath is Fleet) {
                        if (structureOnPath.hp > 0)
                        {
                            LevelUpStructure(structureOnPath as Fleet);
                        }
                    }
                    if (structure is Station)
                    {
                        (structure as Station).defeated = true;
                    }
                    else if (structure is Fleet)
                    {
                        Stations[structure.stationId].fleets.Remove(structure as Fleet);
                    }
                    Destroy(structure.gameObject);
                    blockedMovement = true; // you're dead
                }
                if (structureOnPath.hp > 0)
                {
                    //even if allied don't move through, don't feel like doing recursive checks right now
                    Debug.Log($"{structure.structureName} movement was blocked by {structureOnPath.structureName}");
                    blockedMovement = true; // they aren't dead
                }
                else
                {
                    Debug.Log($"{structure.structureName} destroyed {structureOnPath.structureName}");
                    Destroy(structureOnPath.gameObject); //they are dead
                    if (structure is Fleet)
                        LevelUpStructure(structure as Fleet);
                    if (structureOnPath is Station)
                    {
                        (structureOnPath as Station).defeated = true;
                    }
                    else if (structureOnPath is Fleet)
                    {
                        Stations[structureOnPath.stationId].fleets.Remove(structureOnPath as Fleet);
                    }
                }
            }
            if (!blockedMovement)
            {
                float elapsedTime = 0f;
                float totalTime = .25f;
                while (elapsedTime <= totalTime)
                {
                    structure.transform.position = Vector3.Lerp(structure.currentPathNode.transform.position, node.transform.position, elapsedTime / totalTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                structure.currentPathNode = node;
            }
            if (path.Count == i || node.ownedById == -1 || blockedMovement)
            {
                structure.SetNodeColor();
            }
            yield return new WaitForSeconds(.25f);
        }
        //character.movementRange -= path.Count();
        //HighlightRangeOfMovement(fleet.currentPathNode, fleet.getMovementRange());
        structure.transform.position = structure.currentPathNode.transform.position;
        structure.currentPathNode.structureOnPath = structure;
    }

    private string Fight(Structure s1, Structure s2, AttackType type)
    {
        int s1Dmg = s2.voidAttack - s1.voidAttack;
        int s2Dmg = s1.voidAttack - s2.voidAttack;
        if (type == AttackType.Electric)
        {
            s1Dmg = s2.electricAttack - s1.electricAttack;
            s2Dmg = s1.electricAttack - s2.electricAttack;
        }
        else if (type == AttackType.Thermal)
        {
            s1Dmg = s2.thermalAttack - s1.thermalAttack;
            s2Dmg = s1.thermalAttack - s2.thermalAttack;
        }

        if (s1Dmg > 0)
        {
            if (s1.shield != type)
            {
                s1.hp -= s1Dmg;
                if (type == AttackType.Electric)
                    s2.electricAttack--;
                else if (type == AttackType.Thermal)
                    s2.electricAttack--;
                else
                    s2.voidAttack--;
                return $"{type.ToString()} Phase: {s1.structureName} lost {s1Dmg} HP, {s2.structureName} expends 1 {type} attack.\n";
            }
            else
            {
                return $"{type.ToString()} Phase: {s1.structureName} shielded {s1Dmg} damage.\n";
            }
        }
        else if (s2Dmg > 0)
        {
            if (s2.shield != type)
            {
                s2.hp -= s2Dmg;
                if (type == AttackType.Electric)
                    s1.electricAttack--;
                else if (type == AttackType.Thermal)
                    s1.electricAttack--;
                else
                    s1.voidAttack--;
                return $"{type.ToString()} Phase: {s2.structureName} lost {s2Dmg} HP, {s1.structureName} expends 1 {type} attack.\n";
            }
            else
            {
                return $"{type.ToString()} Phase: {s2.structureName} shielded {s2Dmg} damage.\n";
            }
        }
        return $"{type.ToString()} Phase: Stalemate.\n";
    }

    public void HighlightRangeOfMovement(PathNode currentNode, int fleetRange)
    {
        ClearMovementRange();
        List<PathNode> nodesWithinRange = GridManager.i.GetNodesWithinRange(currentNode, fleetRange);
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
            var turnToPost = new Turn(Globals.GameId, Globals.localStationGuid, TurnNumber,actionToPost);
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
        int c = TurnNumber % maxPlayers;
        for (int i = 0; i < 6; i++)
        {
            for(int j = 0; j < maxPlayers; j++)
            {
                c = (c+j) % maxPlayers;
                if (i < turns[c].Actions.Count)
                {
                    if (turns[c].Actions[i] is object)
                    {
                        var action = new Action(turns[c].Actions[i]);
                        turnValue.text = $"{Stations[c].color} action {i + 1}: {action.actionType}";
                        Debug.Log($"Perfoming {Stations[c].color}'s action {i + 1}: {action.actionType}");
                        yield return StartCoroutine(PerformAction(action));
                    }
                    else
                    {
                        turnValue.text = $"{Stations[c].color} action {i + 1}: No Action";
                    }
                    yield return new WaitForSeconds(1f);
                }
            }
        }
        winner = GridManager.i.CheckForWin();
        foreach (var station in Stations)
        {
            station.actions.Clear();
            for (int i = 1; i <= 3; i++) {
                if (station.score >= Convert.ToInt32(Math.Floor(GridManager.i.scoreToWin * (.25 * i))))
                {
                    station.maxActions = 2 + i;
                }
            }
        }
        if (winner != -1)
        {
            turnValue.text = $"Player {Stations[winner].color} won";
            Debug.Log($"Player {Stations[winner].color} won");
        }
        TurnNumber++;
    }

    private IEnumerator PerformAction(Action action)
    {
        if (action.selectedStructure != null)
        {
            var currentStructure = action.selectedStructure;
            var currentStation = Stations[currentStructure.stationId];

            if (action.actionType == ActionType.MoveStructure)
            {
                yield return StartCoroutine(MoveStructure(currentStructure, action.selectedPath));
                currentStructure.resetMovementRange();
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
                    Debug.Log($"Broke ass {currentStructure.color} bitch couldn't afford {ActionType.CreateFleet}");
                }
            }
            else if (action.actionType == ActionType.UpgradeFleet)
            {
                if (CanPerformUpgrade(currentStructure, action.actionType))
                {
                    ChargeModules(action);
                    LevelUpStructure(currentStructure as Fleet);
                }
                else
                {
                    Debug.Log($"Broke ass {currentStructure.color} bitch couldn't afford {ActionType.UpgradeFleet}");
                }
            }
            else if (action.actionType == ActionType.UpgradeStation)
            {
                if (CanPerformUpgrade(currentStructure, action.actionType))
                {
                    ChargeModules(action);
                    LevelUpStructure(currentStructure);
                }
                else
                {
                    Debug.Log($"Broke ass {currentStructure.color} bitch couldn't afford {ActionType.UpgradeStation}");
                }
            }
            else if (action.actionType == ActionType.GenerateModule)
            {
                currentStation.modules.Add(new Module(action.generatedModuleId, action.generatedGuid));
            }
            else if (action.actionType == ActionType.AttachModule)
            {
                if (currentStation.modules.Count > 0 && currentStructure.attachedModules.Count < currentStructure.maxAttachedModules)
                {
                    Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModulesIds[0]);
                    if (selectedModule is object)
                    {
                        currentStructure.attachedModules.Add(selectedModule);
                        currentStructure.EditModule(selectedModule.type);
                        ChargeModules(action);
                    }
                }
            }
            else if (action.actionType == ActionType.DetachModule)
            {
                if (currentStructure.attachedModules.Count > 0 && action.selectedModulesIds != null && action.selectedModulesIds.Count > 0)
                {
                    Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModulesIds[0]);
                    if (selectedModule is object)
                    {
                        currentStation.modules.Add(selectedModule);
                        currentStructure.EditModule(selectedModule.type, -1);
                        currentStructure.attachedModules.Remove(currentStructure.attachedModules.FirstOrDefault(x => x.moduleGuid == selectedModule.moduleGuid));
                    }
                }
            }
        }
    }

    private void LevelUpStructure(Structure structure)
    {
        if (CanLevelUp(structure, structure is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet, false)){
            structure.level++;
            structure.maxAttachedModules++;
            structure.maxHp++;
            structure.hp++;
            structure.electricAttack++;
            structure.voidAttack++;
            structure.thermalAttack++;
            if (structure is Station)
            {
                (structure as Station).maxFleets++;
            }
        }
    }

    private void ChargeModules(Action action)
    {
        var station = Stations[action.selectedStructure.stationId];
        //will need massive overhaul here and a better method name
        foreach (var selectedModule in action.selectedModulesIds)
        {
            station.modules.RemoveAt(station.modules.Select(x=>x.moduleGuid).ToList().IndexOf(selectedModule));
        }
    }

    private void ResetUI()
    {
        ClearActionBar();
        SetModuleGrid();
        ClearSelection();
        GridManager.i.GetScores();
        ScoreToWinText.text = $"Tiles to win: {MyStation.score}/{GridManager.i.scoreToWin}";
        var firstText = MyStation.stationId == TurnNumber % maxPlayers ? "You" : "They";
        TurnOrderText.text = $"{firstText} have the first action";
        UpdateFleetCostText();
    }

    private void SetModuleGrid()
    {
        ClearModules();
        foreach (var module in MyStation.modules)
        {
            var moduleObject = Instantiate(modulePrefab, ModuleGrid);
            moduleObject.GetComponentInChildren<Button>().onClick.AddListener(() => SetModuleInfo(module.effectText));
            moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
            moduleObject.transform.Find("Queued").gameObject.SetActive(MyStation.actions.Any(x=>x.selectedModulesIds.Contains(module.moduleGuid)));
            currentModules.Add(moduleObject.GetComponent<Module>());
        }
    }

    private void SetModuleInfo(string effectText)
    {
        moduleInfoValue.text = effectText;
        ViewModuleInfo(true);
    } 
    private void SetSelectedModule(Guid moduleGuid, Structure structure)
    {
        //if not already queued up
        if (!MyStation.actions.Any(x => x.actionType == ActionType.GenerateModule && x.selectedModulesIds.Contains(moduleGuid)))
        {
            ViewModuleSelection(false);
            QueueAction(ActionType.AttachModule, new List<Guid>() { moduleGuid }, structure);
            ClearSelectableModules();
        }
    }

    public void ViewModuleInfo(bool active)
    {
        moduleInfoPanel.SetActive(active);
    }
    public void ViewModuleSelection(bool active)
    {
        selectModuelPanel.SetActive(active);
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
    private void ClearSelectableModules()
    {
        while (currentModulesForSelection.Count > 0) { Destroy(currentModulesForSelection[0].gameObject); currentModulesForSelection.RemoveAt(0); }
    }

    private void ClearActionBar()
    {
        for (int i = 0; i < 5; i++)
        {
            ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = i < MyStation.maxActions ? null : lockActionBar;
            ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(false);
            ActionBar.Find($"Action{i}/UnlockInfo").gameObject.SetActive(i >= MyStation.maxActions);
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
                    var moduleText = structure.attachedModules[i].effectText;
                    StructureModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() =>
                        SetModuleInfo(moduleText)
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
