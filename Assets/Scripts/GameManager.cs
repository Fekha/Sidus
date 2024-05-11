using StartaneousAPI.Models;
using StarTaneousAPI.Models;
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
    public GameObject movementMinePrefab;
    public GameObject modulePrefab;
    public GameObject moduleInfoPanel;
    public GameObject infoPanel;
    public GameObject fightPanel;
    public GameObject alertPanel;
    public GameObject customAlertPanel;
    public GameObject areYouSurePanel;
    public GameObject selectModulePanel;
    public GameObject helpPanel;
    public Sprite UISprite;

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
    private Unit SelectedUnit;
    private List<Node> currentMovementRange = new List<Node>();
    private List<Module> currentModules = new List<Module>();
    private List<Module> currentModulesForSelection = new List<Module>();
    internal List<Station> Stations = new List<Station>();

    private bool isMoving = false;
    private bool isEndingTurn = false;
    private Button upgradeButton;
    public Button mineButton;
    public Button createFleetButton;
    public Button generateModuleButton;
    private TextMeshProUGUI createFleetCost;
    private TextMeshProUGUI mineRestrictedText;
    private TextMeshProUGUI generateModuleCost;
    private TextMeshProUGUI upgradeCost;
    private TextMeshProUGUI nameValue;
    private TextMeshProUGUI levelValue;
    private TextMeshProUGUI hpValue;
    private TextMeshProUGUI miningValue;
    private TextMeshProUGUI rangeValue;
    private TextMeshProUGUI kineticAttackValue;
    private TextMeshProUGUI thermalAttackValue;
    private TextMeshProUGUI explosiveAttackValue;
    private TextMeshProUGUI turnValue;
    private TextMeshProUGUI moduleInfoValue;
    private TextMeshProUGUI fightText;
    public TextMeshProUGUI ScoreToWinText;
    public TextMeshProUGUI TurnOrderText;
    public TextMeshProUGUI ColorText;
    public TextMeshProUGUI CreditText;
    private TextMeshProUGUI alertText;
    private TextMeshProUGUI customAlertText;
 
    internal List<Unit> AllUnits = new List<Unit>();
    internal List<Module> AllModules = new List<Module>();
    internal int winner = -1;
    internal Station MyStation {get {return Stations[Globals.localStationIndex];}}
    public GameObject turnLabel;
    private SqlManager sql;
    private int TurnNumber = 0;
    private Turn[] TurnsFromServer;
    private bool infoToggle = false;
    private int fakeCredits;
    private int helpPageNumber = 0;
    //Got game icons from https://game-icons.net/
    private void Awake()
    {
        i = this;
        sql = new SqlManager();
#if UNITY_EDITOR
        if (Globals.Players == null) {
            Globals.Online = false;
            Globals.GameId = Guid.NewGuid();
            Globals.localStationGuid = Guid.NewGuid();
            Globals.localStationIndex = 0;
            var player = new Player();
            player.StationGuid = Globals.localStationGuid;
            player.FleetGuids = new List<Guid>() { Guid.NewGuid() };
            Globals.Players = new Player[1] { player };
        }
#endif
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
        ColorText.text = $"You are {MyStation.color}";
        ColorText.color = GridManager.i.playerColors[MyStation.stationId];
        ResetUI();
    }
   
    public bool HasGameStarted()
    {
        return Stations.Count > 0 && Globals.GameId != Guid.Empty;
    }

    private void FindUI()
    {
        highlightParent = GameObject.Find("Highlights").transform;
        nameValue = infoPanel.transform.Find("NameValue").GetComponent<TextMeshProUGUI>();
        levelValue = infoPanel.transform.Find("LevelValue").GetComponent<TextMeshProUGUI>();
        hpValue = infoPanel.transform.Find("HPValue").GetComponent<TextMeshProUGUI>();
        miningValue = infoPanel.transform.Find("MiningValue").GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("MovementValue").GetComponent<TextMeshProUGUI>();
        kineticAttackValue = infoPanel.transform.Find("KineticValue").GetComponent<TextMeshProUGUI>();
        thermalAttackValue = infoPanel.transform.Find("ThermalValue").GetComponent<TextMeshProUGUI>();
        explosiveAttackValue = infoPanel.transform.Find("ExplosiveValue").GetComponent<TextMeshProUGUI>();
        turnValue = turnLabel.transform.Find("TurnValue").GetComponent<TextMeshProUGUI>();
        moduleInfoValue = moduleInfoPanel.transform.Find("ModuleInfoText").GetComponent<TextMeshProUGUI>();
        createFleetCost = createFleetButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        generateModuleCost = generateModuleButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        upgradeButton = infoPanel.transform.Find("UpgradeButton").GetComponent<Button>();
        upgradeCost = upgradeButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        mineRestrictedText = mineButton.transform.Find("Restriction").GetComponent<TextMeshProUGUI>();
        fightText = fightPanel.transform.Find("Panel/FightText").GetComponent<TextMeshProUGUI>();
        alertText = alertPanel.transform.Find("Background/AlertText").GetComponent<TextMeshProUGUI>();
        customAlertText = customAlertPanel.transform.Find("Background/AlertText").GetComponent<TextMeshProUGUI>();
    }
    public void SetTextValues(Unit unit)
    {
        nameValue.text = unit.unitName;
        levelValue.text = unit.level.ToString();
        hpValue.text = unit.hp + "/" + unit.maxHp;
        rangeValue.text = unit.maxRange.ToString();
        var supportingFleets = GridManager.i.GetNeighbors(unit.currentPathNode).Select(x => x.structureOnPath).Where(x => x != null && x.stationId == unit.stationId);
        var kineticSupport = 0;
        var thermalSupport = 0;
        var explosiveSupport = 0;
        if (supportingFleets.Any())
        {
            kineticSupport = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.kineticAttack * .5)));
            thermalSupport = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.thermalAttack * .5)));
            explosiveSupport = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.explosiveAttack * .5)));
        }
        kineticAttackValue.text = $"{unit.kineticAttack}|{kineticSupport}|{unit.kineticArmor}";
        thermalAttackValue.text = $"{unit.thermalAttack}|{thermalSupport}|{unit.thermalArmor}";
        explosiveAttackValue.text = $"{unit.explosiveAttack}|{explosiveSupport}|{unit.explosiveArmor}";
        miningValue.text = unit.mining.ToString();
        var actionType = unit is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        upgradeButton.interactable = false;
        if (unit.stationId == MyStation.stationId)
        {
            if (CanLevelUp(unit, actionType, true))
            {
                upgradeCost.text = GetCostText(GetCostOfAction(actionType, unit, true));
                upgradeButton.interactable = CanQueueUpgrade(unit, actionType);
            }
            else
            {
                if (unit is Fleet)
                {
                    upgradeCost.text = "(Station Upgrade Required)";
                }
                else
                {
                    upgradeCost.text = "(Max Station Level)";
                }
            }
            upgradeButton.gameObject.SetActive(true);
            mineButton.gameObject.SetActive(true);
        }
        else
        {
            upgradeButton.gameObject.SetActive(false);
            mineButton.gameObject.SetActive(false);
        }
        SetModuleBar(unit);
        infoPanel.gameObject.SetActive(true);
        ToggleMineralText(true);
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
                    Unit targetStructure = null;
                    if (targetNode != null && targetNode.structureOnPath != null)
                    {
                        targetStructure = targetNode.structureOnPath;
                    }
                    //original click on fleet
                    if (targetStructure != null && SelectedUnit == null && targetStructure.stationId == MyStation.stationId)
                    {
                        SetTextValues(targetStructure);
                        SelectedUnit = targetStructure;
                        if (HasQueuedAction(ActionType.MoveStructure, targetStructure))
                        {
                            Debug.Log($"{targetStructure.unitName} already has a pending movement action.");
                        }
                        else
                        {
                            Debug.Log($"{targetStructure.unitName} Selected.");
                            HighlightRangeOfMovement(targetStructure.currentPathNode, targetStructure);
                        }
                    }
                    else
                    {
                        //double click confirm to movement
                        if (SelectedNode != null && targetNode == SelectedNode && SelectedUnit != null && SelectedPath != null)
                        {
                            if (SelectedPath.Count == 1 && SelectedNode.isAsteroid)
                            {
                                SelectedUnit.resetMovementRange();
                                QueueAction(ActionType.MineTargetedAsteroid);
                            }
                            else
                            {
                                QueueAction(ActionType.MoveStructure);
                            }
                        }
                        //create movement
                        else if (targetNode != null && SelectedUnit != null && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode) && !HasQueuedAction(ActionType.MoveStructure, SelectedUnit))
                        {
                            int oldPathCount = SelectedPath?.Count ?? 0;
                            if (SelectedPath == null || SelectedPath.Count == 0)
                            {
                                SelectedPath = GridManager.i.FindPath(SelectedUnit.currentPathNode, targetNode, SelectedUnit.stationId);
                                SelectedUnit.subtractMovement(SelectedPath.Last().gCost);
                                Debug.Log($"Path created for {SelectedUnit.unitName}");
                            }
                            else
                            {
                                var newPath = GridManager.i.FindPath(SelectedPath.Last(), targetNode, SelectedUnit.stationId);
                                SelectedUnit.subtractMovement(newPath.Last().gCost);
                                SelectedPath.AddRange(newPath);
                                Debug.Log($"Path edited for {SelectedUnit.unitName}");
                            }
                            HighlightRangeOfMovement(targetNode, SelectedUnit);
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
                        //clicked on invalid tile
                        else
                        {
                            if (targetStructure != null)
                            {
                                if (SelectedUnit == null)
                                {
                                    SetTextValues(targetStructure);
                                    ViewStructureInformation(true);
                                }
                            }
                            else
                            {
                                DeselectMovement();
                            }
                        }
                    }
                }
            }
        }
    }

    private void DeselectMovement()
    {
        if (SelectedUnit != null)
        {
            Debug.Log($"Invalid tile selected, reseting path and selection.");
            SelectedUnit.resetMovementRange();
        }
        ResetAfterSelection();
    }
    public void ViewFightPanel(bool active)
    {
        fightPanel.SetActive(active);
    }  
    public void ViewHelpPanel(bool active)
    {
        helpPageNumber++;
        if (active) {
            helpPanel.transform.Find("Page2").gameObject.SetActive(false);
            helpPanel.SetActive(active);
        } else if (helpPageNumber == 2) {
            helpPanel.transform.Find("Page2").gameObject.SetActive(true);
        } else {
            helpPageNumber = 0;
            helpPanel.SetActive(false);
            helpPanel.transform.Find("Page2").gameObject.SetActive(false);

        }
    }

    public void ShowAlertPanel(int unlock)
    {
        alertText.text = $"This action slot will unlock once you own {Convert.ToInt32(Math.Floor(GridManager.i.scoreToWin * (.25 * unlock)))} hexes.";
        alertPanel.SetActive(true);
    }
    public void ShowCustomAlertPanel(string message)
    {
        customAlertText.text = message;
        customAlertPanel.SetActive(true);
        Debug.Log(message);
    }
    public void HideAlertPanel()
    {
        alertPanel.SetActive(false);
        customAlertPanel.SetActive(false);
    }
    public void ShowAreYouSurePanel(bool value)
    {
        areYouSurePanel.SetActive(value);
    }
    public void ViewStructureInformation(bool active)
    {
        if(HasGameStarted())
            infoPanel.SetActive(active);
    }
    #region Queue Actions
    public void AttachModule()
    {
        if (SelectedUnit == null)
        {
            ShowCustomAlertPanel("No unit selected.");
        }
        else if (MyStation.actions.Count >= MyStation.maxActions)
        {
            ShowCustomAlertPanel("No action slots available to queue this action.");
        }
        else if (MyStation.modules.Count <= 0)
        {
            ShowCustomAlertPanel("No modules available to attach.");
        }
        else if ((SelectedUnit.attachedModules.Count + MyStation.actions.Count(x => x.actionType == ActionType.AttachModule && x.selectedUnit.unitGuid == SelectedUnit.unitGuid)) >= GetUnitMaxAttachmentCount(SelectedUnit))
        {
            ShowCustomAlertPanel("You have already queued up this action.");
        }
        else if (MyStation.stationId != SelectedUnit.stationId)
        {
            ShowCustomAlertPanel("This is not your unit!");
        } 
        else 
        {
            var selectedStructure = SelectedUnit;
            DeselectMovement();
            ClearSelectableModules();
            List<Guid> assignedModules = MyStation.actions.SelectMany(x => x.selectedModulesIds).ToList();
            var availableModules = MyStation.modules.Where(x => !assignedModules.Contains(x.moduleGuid)).ToList();
            if (availableModules.Count > 0)
            {
                foreach (var module in availableModules)
                {
                    var moduleObject = Instantiate(modulePrefab, SelectedModuleGrid);
                    moduleObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => SetSelectedModule(module.moduleGuid, selectedStructure));
                    moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
                    moduleObject.transform.Find("Queued").gameObject.SetActive(false);
                    currentModulesForSelection.Add(moduleObject.GetComponent<Module>());
                }
                ViewModuleSelection(true);
            }
        }
    }
    private void SetSelectedModule(Guid moduleGuid, Unit structure)
    {
        //if not already queued up
        if (!MyStation.actions.Any(x => x.actionType == ActionType.AttachModule && x.selectedModulesIds.Contains(moduleGuid)))
        {
            ViewModuleSelection(false);
            QueueAction(ActionType.AttachModule, new List<Guid>() { moduleGuid }, structure);
            ClearSelectableModules();
        }
    }
    public void DetachModule(int i)
    {
        if (SelectedUnit == null)
        {
            ShowCustomAlertPanel("No unit selected.");
        }
        else if (MyStation.stationId != SelectedUnit.stationId)
        {
            ShowCustomAlertPanel("This is not your unit!");
        }
        else if (MyStation.actions.Any(x => x.actionType == ActionType.DetachModule && x.selectedModulesIds.Any(y => y == SelectedUnit.attachedModules[i].moduleGuid)))
        {
             ShowCustomAlertPanel($"The action {ActionType.DetachModule} for the module {SelectedUnit.attachedModules[i].type} has already been queued up");
        }
        else
        {
            QueueAction(ActionType.DetachModule, new List<Guid>() { SelectedUnit.attachedModules[i].moduleGuid });
        }
    }
    public void CreateFleet()
    {
        if (MyStation.credits < GetCostOfAction(ActionType.CreateFleet, MyStation, true))
        {
            ShowCustomAlertPanel("Can not afford to queue this action");
        }
        else if (!IsUnderMaxFleets(MyStation,true))
        {
            ShowCustomAlertPanel("Can not create new fleet, you will hit max fleets for your station level");
        }
        else
        {
            QueueAction(ActionType.CreateFleet);
        }
    }

    public void UpgradeStructure()
    {
        ActionType actionType = SelectedUnit is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        if (!CanQueueUpgrade(SelectedUnit, actionType))
        {
            ShowCustomAlertPanel("Can not afford to queue this action");
        }
        else
        {
            QueueAction(actionType);
        }
    }
    public void GenerateModule()
    {
        if (MyStation.credits < GetCostOfAction(ActionType.GenerateModule, MyStation, false))
        {
            ShowCustomAlertPanel("Can not afford to queue this action");
        }
        else
        {
            QueueAction(ActionType.GenerateModule);
        }
    }

    public void MineAsteroid()
    {
        QueueAction(ActionType.MineAsteroid);
    }
    
    public void CancelAction(int slot)
    {
        var action = MyStation.actions[slot];
        Debug.Log($"{MyStation.unitName} removed action {action.actionType} from queue");
        if (action is object)
        {
            if (action.actionType == ActionType.MoveStructure)
            {
                action.selectedUnit.resetMovementRange();
            }
            fakeCredits += action.costOfAction;
            ClearActionBar();
            MyStation.actions.RemoveAt(slot);
            for (int i = 0; i < MyStation.actions.Count; i++)
            {
                AddActionBarImage(MyStation.actions[i].actionType, i);
            }
        }
        ResetAfterSelection();
    }
    private void QueueAction(ActionType actionType, List<Guid> selectedModules = null, Unit _structure = null)
    {
        if (MyStation.actions.Count >= MyStation.maxActions)
        {
            ShowCustomAlertPanel("No action slots available to queue this action.");
        } else {
            var structure = _structure ?? SelectedUnit ?? MyStation;
            var costOfAction = GetCostOfAction(actionType, structure, true);
            if (costOfAction + MyStation.actions.Sum(x => x.costOfAction) > MyStation.credits)
            {
                ShowCustomAlertPanel("Can not afford to queue this action");
            }
            else
            {
                fakeCredits -= costOfAction;
                Debug.Log($"{MyStation.unitName} queuing up action {actionType}");
                AddActionBarImage(actionType, MyStation.actions.Count());
                MyStation.actions.Add(new Action(actionType, structure, costOfAction, selectedModules, SelectedPath));
                ResetAfterSelection();
            }
        }
    }
    #endregion
    private void ToggleMineralText(bool value)
    {
        foreach (var asteroid in GridManager.i.asteroids)
        {
            asteroid.mineralText.gameObject.SetActive(value);
        }
    }
    private void ToggleHPText(bool value)
    {
        foreach (var station in AllUnits)
        {
            station.ShowHPText(value);
        }
    }

    private string GetCostText(int cost)
    {
        string plural = cost == 1 ? "" : "s";
        return $"(Costs {cost} Credit{plural})";
    }
    private void AddActionBarImage(ActionType actionType, int i)
    {
        ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Actions/{(int)actionType}");
        ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(true);
        ActionBar.Find($"Action{i}/UnlockInfo").gameObject.SetActive(false);
    }
  
    private void UpdateCreateFleetCostText()
    {
        if (IsUnderMaxFleets(MyStation, true))
        {
            var cost = GetCostOfAction(ActionType.CreateFleet, MyStation, true);
            createFleetCost.text = GetCostText(cost);
            createFleetButton.interactable = (MyStation.credits >= cost);
        }
        else
        {
            createFleetCost.text = "(Station Upgrade Required)";
            createFleetButton.interactable = false;
        }
    }
    private void UpdateGenerateModuleCostText()
    {
        var actionCost = GetCostOfAction(ActionType.GenerateModule, MyStation, false);
        generateModuleCost.text = GetCostText(actionCost);
        generateModuleButton.interactable = MyStation.credits >= actionCost;
    }
    
    private bool CanQueueUpgrade(Unit structure, ActionType actionType)
    {
        return CanLevelUp(structure, actionType, true) && MyStation.credits >= GetCostOfAction(actionType, structure, true);
    }

    private bool CanLevelUp(Unit structure, ActionType actionType, bool countQueue)
    {
        var nextLevel = structure.level;
        var maxLevel = Stations[structure.stationId].level;
        if (countQueue) {
            nextLevel += Stations[structure.stationId].actions.Count(x => x.selectedUnit.unitGuid == structure.unitGuid && x.actionType == actionType);
            maxLevel += Stations[structure.stationId].actions.Count(x => x.actionType == ActionType.UpgradeStation);
        } 
        if (actionType == ActionType.UpgradeFleet)
            return nextLevel < maxLevel;
        else
            return nextLevel < 6;
    }

    private bool IsUnderMaxFleets(Station station, bool countQueue)
    {
        var currentFleets = station.fleets.Count;
        var maxFleets = station.maxFleets;
        if (countQueue)
        {
            currentFleets += station.actions.Count(x => x.actionType == ActionType.CreateFleet);
            maxFleets += station.actions.Count(x => x.actionType == ActionType.UpgradeStation);
        }
        return currentFleets < maxFleets;
    }
    private int GetCostOfAction(ActionType actionType, Unit structure, bool countQueue)
    {
        var station = Stations[structure.stationId];
        var countingQueue = countQueue ? station.actions.Where(x => x.actionType == actionType && x.selectedUnit.unitGuid == structure.unitGuid).Count() : 0;
        if (actionType == ActionType.CreateFleet)
        {
            if (station.fleets.Count <= 0)
                return 0;
            return (5 + ((station.fleets.Count-1 + countingQueue) * 3)); //5,8,11,14,17
        }
        else if (actionType == ActionType.UpgradeFleet)
        {
            return (3 + ((structure.level-1 + countingQueue) * 2)); //3,5,7,9,11
        }
        else if (actionType == ActionType.UpgradeStation)
        {
            return (7 + ((structure.level-1 + countingQueue) * 5)); //7,12,19,24,29
        }
        else if (actionType == ActionType.GenerateModule)
        {
            return 3;
        }
        else
        {
            return 0;
        }
    }

    private bool HasQueuedAction(ActionType actionType, Unit selectedUnit)
    {
        return MyStation.actions.Any(x => x.actionType == actionType && x.selectedUnit.unitGuid == selectedUnit.unitGuid);
    }

    private void ResetAfterSelection()
    {
        ToggleMineralText(false);
        ToggleHPText(false);
        infoToggle = false;
        UpdateGenerateModuleCostText();
        UpdateCreateFleetCostText();
        UpdateCreditTotal();
        SetModuleGrid();
        ClearMovementPath();
        ClearMovementRange();
        SelectedNode = null;
        SelectedUnit = null;
        SelectedPath = null;
        ViewStructureInformation(false);
    }

    private void UpdateCreditTotal()
    {
        CreditText.text = $"{fakeCredits} Credits";
    }

    private IEnumerator MoveOnPath(Unit unitMoving, List<PathNode> path)
    {
        int i = 0;
        unitMoving.currentPathNode.structureOnPath = null;
        ClearMovementRange();
        unitMoving.resetMovementRange();
        foreach (var node in path)
        {
            i++;
            bool blockedMovement = false;
            unitMoving.subtractMovement(GridManager.i.GetGCost(node, unitMoving.stationId));
            //if ally, and theres room after, move through.
            if (node.structureOnPath != null && node.structureOnPath.stationId == unitMoving.stationId)
            {
                blockedMovement = true;
                //Example: you move 3, first spot is ally, second is enemy, third is open : you should not move
                if (path.Count != i)
                {
                    for (int j = i; j < path.Count; j++)
                    {
                        //If next spot after ally is empty move
                        if (path[j].structureOnPath == null && !path[j].isAsteroid)
                        {
                            blockedMovement = false;
                            break;
                        }
                        //if next spot after ally is enemy, stop
                        else if (path[j].structureOnPath != null && path[j].structureOnPath.stationId != MyStation.stationId)
                        {
                            break;
                        }
                        //If next spot after ally is ally, keep looking
                    }
                }
            }
            if (node.isAsteroid || unitMoving.range < 0)
            {
                blockedMovement = true;
            }
            float elapsedTime = 0f;
            float totalTime = .5f;
            var toRot = GetDirection(unitMoving, node);
            var unitImage = unitMoving.transform.Find("Unit");
            while (elapsedTime <= totalTime)
            {
                unitImage.rotation = Quaternion.Lerp(unitImage.rotation, toRot, elapsedTime / totalTime);
                if (!blockedMovement)
                    unitMoving.transform.position = Vector3.Lerp(unitMoving.currentPathNode.transform.position, node.transform.position, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            unitImage.rotation = toRot;
            if (node.isAsteroid)
            {
                yield return StartCoroutine(PerformSingleMine(unitMoving, node));
            }
            //if enemy, attack, if you didn't destroy them, stay blocked and move back
            if (unitMoving.range >= 0 && node.structureOnPath != null && node.structureOnPath.stationId != unitMoving.stationId)
            {
                yield return StartCoroutine(FightEnemyUnit(unitMoving, node));
                blockedMovement = node.structureOnPath != null && AllUnits.Contains(node.structureOnPath);
            }
            if (!blockedMovement)
                unitMoving.currentPathNode = node;
            if (Globals.GameSettings.Contains(GameSettingType.TakeoverCosts2.ToString())
                || unitMoving.currentPathNode.ownedById == -1
                || ((i == path.Count || blockedMovement) && GridManager.i.GetNeighbors(unitMoving.currentPathNode).Any(x => x.ownedById == unitMoving.stationId)))
            {
                unitMoving.SetNodeColor();
            }
            if (blockedMovement)
                break;
            yield return new WaitForSeconds(.25f);
        }
        unitMoving.transform.position = unitMoving.currentPathNode.transform.position;
        unitMoving.currentPathNode.structureOnPath = unitMoving;
    }

    internal IEnumerator FightEnemyUnit(Unit unitMoving, PathNode node)
    {
        var unitOnPath = node.structureOnPath;
        Debug.Log($"{unitMoving.unitName} is attacking {unitOnPath.unitName}");
        unitMoving.selectIcon.SetActive(true);
        unitOnPath.inCombatIcon.SetActive(true);
        var supportingFleets = GridManager.i.GetNeighbors(node).Select(x => x.structureOnPath).Where(x => x != null && x.unitGuid != unitOnPath.unitGuid);
        int s1sKinetic = 0;
        int s2sKinetic = 0;
        int s1sThermal = 0;
        int s2sThermal = 0;
        int s1sExplosive = 0;
        int s2sExplosive = 0;
        foreach (var supportFleet in supportingFleets)
        {
            if (supportFleet.stationId == unitMoving.stationId)
            {
                s1sKinetic += Convert.ToInt32(Math.Floor(supportFleet.kineticAttack * .5));
                s1sThermal += Convert.ToInt32(Math.Floor(supportFleet.thermalAttack * .5));
                s1sExplosive += Convert.ToInt32(Math.Floor(supportFleet.explosiveAttack * .5));
            }
            else
            {
                s2sKinetic += Convert.ToInt32(Math.Floor(supportFleet.kineticAttack * .5));
                s2sThermal += Convert.ToInt32(Math.Floor(supportFleet.thermalAttack * .5));
                s2sExplosive += Convert.ToInt32(Math.Floor(supportFleet.explosiveAttack * .5));
            }
        }
        string s1sKineticText = s1sKinetic > 0 ? $"(+{s1sKinetic})" : "";
        string s1sThermalText = s1sThermal > 0 ? $"(+{s1sThermal})" : "";
        string s1sExplosiveText = s1sExplosive > 0 ? $"(+{s1sExplosive})" : "";
        string s2sKineticText = s2sKinetic > 0 ? $"(+{s2sKinetic})" : "";
        string s2sThermalText = s2sThermal > 0 ? $"(+{s2sThermal})" : "";
        string s2sExplosiveText = s2sExplosive > 0 ? $"(+{s2sExplosive})" : "";
        var preCombatHp1 = unitMoving.hp;
        var preCombatHp2 = unitOnPath.hp;
        var resistanceText1 = (unitMoving.kineticArmor > 0 || unitMoving.thermalArmor > 0 || unitMoving.explosiveArmor > 0) ? $"\nResistance: Kinetic {unitMoving.kineticArmor}, Thermal {unitMoving.thermalArmor}, Explosive {unitMoving.explosiveArmor}." : "";
        var resistanceText2 = (unitOnPath.kineticArmor > 0 || unitOnPath.thermalArmor > 0 || unitOnPath.explosiveArmor > 0) ? $"\nResistance: Kinetic {unitOnPath.kineticArmor}, Thermal {unitOnPath.thermalArmor}, Explosive {unitOnPath.explosiveArmor}." : "";
        string beforeStats = $"Pre-fight stats: \n{unitMoving.unitName}: {preCombatHp1} HP." +
            $"\nPower(+Support): Kinetic {unitMoving.kineticAttack}{s1sKineticText}, Thermal {unitMoving.thermalAttack}{s1sThermalText}, Explosive {unitMoving.explosiveAttack}{s1sExplosiveText}." +
            resistanceText1 +
            $"\n{unitOnPath.unitName}: {preCombatHp2} HP." +
            $"\nPower(+Support): Kinetic {unitOnPath.kineticAttack}{s2sKineticText}, Thermal {unitOnPath.thermalAttack}{s2sThermalText}, Explosive {unitOnPath.explosiveAttack}{s2sExplosiveText}." +
            resistanceText2;

        string duringFightText = "";
        for (int attackType = 0; attackType <= (int)AttackType.Explosive; attackType++)
        {
            if (unitMoving.hp > 0 && unitOnPath.hp > 0)
                duringFightText += DoCombat(unitMoving, unitOnPath, (AttackType)attackType, s1sKinetic, s1sThermal, s1sExplosive, s2sKinetic, s2sThermal, s2sExplosive);
        }
        var postFightHpDiff1 = preCombatHp1 - unitMoving.hp;
        var postFightHpDiff2 = preCombatHp2 - unitOnPath.hp;
        var postFightHpDiff1Text = postFightHpDiff1 > 0 ? $"(-{postFightHpDiff1})" : "";
        var postFightHpDiff2Text = postFightHpDiff2 > 0 ? $"(-{postFightHpDiff2})" : "";
        fightText.text = $"{beforeStats}\n\n{duringFightText}\nPost-fight stats: \n{unitMoving.unitName}: HP {unitMoving.hp}{postFightHpDiff1Text}\n{unitOnPath.unitName}: HP {unitOnPath.hp}{postFightHpDiff2Text}";
        ViewFightPanel(true);
        while (fightPanel.activeInHierarchy)
        {
            yield return new WaitForSeconds(.5f);
        }
        unitOnPath.inCombatIcon.SetActive(false);
        unitMoving.selectIcon.SetActive(false);
        if (unitMoving.hp <= 0)
        {
            Debug.Log($"{unitOnPath.unitName} destroyed {unitMoving.unitName}");
            //if (unitOnPath is Fleet)
            //{
            //    if (unitOnPath.hp > 0)
            //    {
            //        LevelUpUnit(unitOnPath as Fleet);
            //    }
            //}
            if (unitMoving is Station)
            {
                (unitMoving as Station).defeated = true;
            }
            else if (unitMoving is Fleet)
            {
                Stations[unitMoving.stationId].fleets.Remove(unitMoving as Fleet);
            }
            AllUnits.Remove(unitMoving);
            unitMoving.currentPathNode.structureOnPath = null;
            Destroy(unitMoving.gameObject);
        }
        if (unitOnPath.hp > 0)
        {
            //even if allied don't move through, don't feel like doing recursive checks right now
            Debug.Log($"{unitMoving.unitName} movement was blocked by {unitOnPath.unitName}");
        }
        else
        {
            Debug.Log($"{unitMoving.unitName} destroyed {unitOnPath.unitName}");
            //if (unitMoving is Fleet)
            //    LevelUpUnit(unitMoving as Fleet);
            if (unitOnPath is Station)
            {
                (unitOnPath as Station).defeated = true;
            }
            else if (unitOnPath is Fleet)
            {
                Stations[unitOnPath.stationId].fleets.Remove(unitOnPath as Fleet);
            }
            AllUnits.Remove(unitOnPath);
            node.structureOnPath = null;
            Destroy(unitOnPath.gameObject); //they are dead
        }
    }

    private Quaternion GetDirection(Unit unit, PathNode node)
    {
        int x = node.x - unit.currentPathNode.x;
        int y = node.y - unit.currentPathNode.y;
        var offSetCoords = unit.currentPathNode.offSet.FirstOrDefault(c=>c.x == x && c.y == y);
        unit.facing = (Direction)Array.IndexOf(unit.currentPathNode.offSet, offSetCoords);
        return Quaternion.Euler(unit.transform.rotation.x, unit.transform.rotation.y, 270 - (unit.facing == Direction.TopRight ? -60 : (int)unit.facing * 60));
    }

    private string DoCombat(Unit s1, Unit s2, AttackType type, int s1sKinetic, int s1sThermal, int s1sExplosive, int s2sKinetic, int s2sThermal, int s2sExplosive)
    {
        int s1Dmg = (s2.explosiveAttack+s2sExplosive) - (s1.explosiveAttack+s1sExplosive);
        int s2Dmg = (s1.explosiveAttack+s1sExplosive) - (s2.explosiveAttack+s2sExplosive);
        int s1Amr = s1.explosiveArmor;
        int s2Amr = s2.explosiveArmor;
        if (type == AttackType.Kinetic)
        {
            s1Dmg = (s2.kineticAttack+s2sKinetic) - (s1.kineticAttack+s1sKinetic);
            s2Dmg = (s1.kineticAttack+s1sKinetic) - (s2.kineticAttack+s2sKinetic);
            s1Amr = s1.kineticArmor;
            s2Amr = s2.kineticArmor;
        }
        else if (type == AttackType.Thermal)
        {
            s1Dmg = (s2.thermalAttack+s2sThermal) - (s1.thermalAttack+s1sThermal);
            s2Dmg = (s1.thermalAttack+s1sThermal) - (s2.thermalAttack+s2sThermal);
            s1Amr = s1.thermalArmor;
            s2Amr = s2.thermalArmor;
        }
        if (s1Dmg > 0)
        {
            var damage = Mathf.Max(s1Dmg - s1Amr, 0);
            s1.hp -= damage;
            if (s1Amr == 0)
            {
                return $"{type} Phase: {s1.unitName} lost {damage} HP \n";
            }
            var modifierSymbol = s1Amr > 0 ? "+" : "";
            return $"{type} Phase: {s1.unitName} lost {damage} HP after resistances were applied, {s1Dmg}({modifierSymbol}{s1Amr}) \n";
        }
        else if (s2Dmg > 0)
        {
            var damage = Mathf.Max(s2Dmg - s2Amr, 0);
            s2.hp -= damage;
            if (s2Amr == 0)
            {
                return $"{type} Phase: {s2.unitName} lost {damage} HP \n";
            }
            return $"{type} Phase: {s2.unitName} lost, base damage is {s2Dmg}, after modules applied they lost {damage} HP \n";
        }
        return $"{type} Phase: Stalemate.\n";
    }

    public void HighlightRangeOfMovement(PathNode currentNode, Unit unit)
    {
        ClearMovementRange();
        List<PathNode> nodesWithinRange = GridManager.i.GetNodesWithinRange(currentNode, unit);
        foreach (PathNode node in nodesWithinRange)
        {
            GameObject range = Instantiate(node.isAsteroid ? movementMinePrefab : movementRangePrefab, node.transform.position, Quaternion.identity);
            range.transform.SetParent(highlightParent);
            var rangeComponent = range.AddComponent<Node>();
            rangeComponent.Initialize(node);
            currentMovementRange.Add(rangeComponent);
        }
    }
    
    public void EndTurn(bool theyAreSure)
    {
        if (!isEndingTurn && HasGameStarted())
        {
            if (theyAreSure || MyStation.actions.Count == MyStation.maxActions)
            {
                ShowAreYouSurePanel(false);
                isEndingTurn = true;
                Debug.Log($"Turn Ending, Starting Simultanous Turns");
                var actionToPost = MyStation.actions.Select(x => new ActionIds(x)).ToList();
                var turnToPost = new Turn(Globals.GameId, Globals.localStationGuid, TurnNumber, actionToPost);
                var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(turnToPost);
                StartCoroutine(sql.PostRoutine<bool>($"Game/EndTurn", stringToPost));
                StartCoroutine(TakeTurns());
            }
            else
            {
                ShowAreYouSurePanel(true);
            }
        }
    }
    
    private IEnumerator TakeTurns()
    {
        turnValue.text = "Waiting for the other player...";
        turnLabel.SetActive(true);
        TurnsFromServer = null;
        while (TurnsFromServer == null)
        {
            yield return StartCoroutine(sql.GetRoutine<Turn[]>($"Game/GetTurn?gameId={Globals.GameId}&turnNumber={TurnNumber}", CheckForTurns));
        }
        yield return StartCoroutine(AutomateTurns(TurnsFromServer));
        if (winner != -1)
        {
            turnValue.text = $"Player {Stations[winner].color} won after {TurnNumber} turns";
            Debug.Log($"Player {Stations[winner].color} won after {TurnNumber} turns");
        } else {
            foreach (var asteroid in GridManager.i.asteroids)
            {
                asteroid.ReginCredits();
            }
            //foreach (var unit in AllUnits)
            //{
            //    unit.hasMinedThisTurn = false;
            //}
            TurnNumber++; 
            ResetUI();
            turnLabel.SetActive(false);
            isEndingTurn = false;
            Debug.Log($"New Turn {TurnNumber} Starting");
        }
    }

    private void CheckForTurns(Turn[] turns)
    {
        TurnsFromServer = turns;
    }
    public void ToggleAll()
    {
        if (!isEndingTurn)
        {
            infoToggle = !infoToggle;
            ToggleMineralText(infoToggle);
            ToggleHPText(infoToggle);
        }
    }
    private IEnumerator AutomateTurns(Turn[] turns)
    {
        ClearModules();
        ToggleMineralText(true);
        //ToggleHPText(true);
        for (int i = 0; i < 6; i++)
        {
            int c = TurnNumber % Stations.Count;
            for (int j = 0; j < Stations.Count; j++)
            {
                int k = (c+j) % Stations.Count;
                if (i < turns[k].Actions.Count)
                {
                    if (turns[k].Actions[i] is object)
                    {
                        var action = new Action(turns[k].Actions[i]);
                        turnValue.text = $"{Stations[k].color} action {i + 1}: {action.actionType}";
                        Debug.Log($"Perfoming {Stations[k].color}'s action {i + 1}: {action.actionType}");
                        yield return StartCoroutine(PerformAction(action));
                    }
                    else
                    {
                        turnValue.text = $"{Stations[k].color} action {i + 1}: No Action";
                    }
                    yield return new WaitForSeconds(.1f);
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
    }

    private IEnumerator PerformAction(Action action)
    {
        if (action.selectedUnit != null && AllUnits.Contains(action.selectedUnit))
        {
            var currentUnit = action.selectedUnit;
            if (currentUnit != null)
            {
                var currentStation = Stations[currentUnit.stationId];
                var actionCost = GetCostOfAction(action.actionType, action.selectedUnit, false);
                if (currentStation.credits >= actionCost)
                {
                    if (action.actionType == ActionType.MoveStructure)
                    {
                        isMoving = true;
                        if (currentUnit != null && action.selectedPath != null && action.selectedPath.Count > 0 && action.selectedPath.Count <= currentUnit.getMaxMovementRange())
                        {
                            Debug.Log("Moving to position: " + currentUnit.currentPathNode.transform.position);
                            yield return StartCoroutine(MoveOnPath(currentUnit, action.selectedPath));                               
                        }
                        else
                        {
                            Debug.Log("Out of range or no longer exists.");
                        }
                        yield return new WaitForSeconds(.1f);
                        isMoving = false;
                        currentUnit.resetMovementRange();      
                    }
                    else if (action.actionType == ActionType.CreateFleet)
                    {
                        if (IsUnderMaxFleets(currentStation,false))
                        {
                            currentStation.credits -= actionCost;
                            yield return StartCoroutine(GridManager.i.CreateFleet(currentStation, action.generatedGuid, false));
                        }
                        else
                        {
                            Debug.Log($"{currentUnit.unitName} not eligible for {action.actionType}");
                        }
                    }
                    else if (action.actionType == ActionType.UpgradeFleet)
                    {
                        if (CanLevelUp(currentUnit, action.actionType, false))
                        {
                            currentStation.credits -= actionCost;
                            LevelUpUnit(currentUnit as Fleet);
                        }
                        else
                        {
                            Debug.Log($"{currentUnit.unitName} not eligible for {action.actionType}");
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.UpgradeStation)
                    {
                        if (CanLevelUp(currentUnit, action.actionType, false))
                        {
                            currentStation.credits -= actionCost;
                            LevelUpUnit(currentUnit);
                        }
                        else
                        {
                            Debug.Log($"{currentUnit.unitName} not eligible for {action.actionType}");
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.GenerateModule)
                    {
                        currentStation.credits -= actionCost;
                        currentStation.modules.Add(new Module(action.generatedModuleId, action.generatedGuid));
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.AttachModule)
                    {
                        if (currentStation.modules.Count > 0 && currentUnit.attachedModules.Count < currentUnit.maxAttachedModules)
                        {
                            Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModulesIds[0]);
                            if (selectedModule is object)
                            {
                                currentUnit.attachedModules.Add(selectedModule);
                                currentUnit.EditModule(selectedModule.type);
                                currentStation.modules.RemoveAt(currentStation.modules.Select(x => x.moduleGuid).ToList().IndexOf(selectedModule.moduleGuid));
                            }
                            else
                            {
                                Debug.Log($"Module not available");
                            }
                        }
                        else
                        {
                            Debug.Log($"Module not available");
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.DetachModule)
                    {
                        if (currentUnit.attachedModules.Count > 0 && action.selectedModulesIds != null && action.selectedModulesIds.Count > 0)
                        {
                            Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModulesIds[0]);
                            if (selectedModule is object)
                            {
                                currentStation.modules.Add(selectedModule);
                                currentUnit.EditModule(selectedModule.type, -1);
                                currentUnit.attachedModules.Remove(currentUnit.attachedModules.FirstOrDefault(x => x.moduleGuid == selectedModule.moduleGuid));
                            }
                            else
                            {
                                Debug.Log($"Module not available");
                            }
                        }
                        else
                        {
                            Debug.Log($"Module not available");
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.MineAsteroid)
                    {
                        yield return StartCoroutine(PerformAoeMine(currentUnit));
                    }
                    else if (action.actionType == ActionType.MineTargetedAsteroid)
                    {
                        yield return StartCoroutine(PerformSingleMine(currentUnit, action.selectedPath.Last()));
                    }
                }
                else
                {
                    Debug.Log($"Broke ass {currentUnit.color} bitch couldn't afford {action.actionType}");
                }
            }
        }
    }

    private IEnumerator PerformAoeMine(Unit currentUnit)
    {
        Station currentStation = Stations[currentUnit.stationId];
        if (AllUnits.Contains(currentUnit))
        {
            var asteroidsToMine = GridManager.i.GetNeighbors(currentUnit.currentPathNode).Where(x => x.isAsteroid && x.currentCredits > 0).OrderBy(x=>x.currentCredits);
            if (asteroidsToMine.Count() > 0)
            {
                var minedAmount = 0;
                foreach (var asteroid in asteroidsToMine)
                {
                    if (minedAmount < currentUnit.mining)
                    {
                        minedAmount += asteroid.MineCredits(currentUnit.mining - minedAmount);
                        currentUnit.selectIcon.SetActive(true);
                        asteroid.transform.Find("Mine").gameObject.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        asteroid.transform.Find("Mine").gameObject.SetActive(false);
                        currentUnit.selectIcon.SetActive(false);
                    }
                }
                //if (minedAmount > 0)
                //{
                //    currentUnit.hasMinedThisTurn = true;
                //}
                currentStation.credits += minedAmount;
            }
        }
    }

    private IEnumerator PerformSingleMine(Unit currentUnit, PathNode asteroidToMine)
    {
        Station currentStation = Stations[currentUnit.stationId];
        if (AllUnits.Contains(currentUnit))
        {
            currentStation.credits += asteroidToMine.MineCredits(currentUnit.mining);
            currentUnit.selectIcon.SetActive(true);
            asteroidToMine.transform.Find("Mine").gameObject.SetActive(true);
            float elapsedTime = 0f;
            float totalTime = 1f;
            var toRot = GetDirection(currentUnit, asteroidToMine);
            var unitImage = currentUnit.transform.Find("Unit");
            while (elapsedTime <= totalTime)
            {
                unitImage.rotation = Quaternion.Lerp(unitImage.rotation, toRot, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            unitImage.rotation = toRot;
            asteroidToMine.transform.Find("Mine").gameObject.SetActive(false);
            currentUnit.selectIcon.SetActive(false);
        }
    }

    private void LevelUpUnit(Unit structure)
    {
        if (CanLevelUp(structure, structure is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet, false)){
            structure.level++;
            structure.maxAttachedModules++;
            structure.maxHp+=4;
            structure.hp+=4;
            structure.kineticAttack++;
            structure.explosiveAttack++;
            structure.thermalAttack++;
            if (structure is Station)
            {
                (structure as Station).maxFleets++;
            }
        }
    }

    private void ResetUI()
    {
        ClearActionBar();
        GridManager.i.GetScores();
        ScoreToWinText.text = $"Hexes to win: {MyStation.score}/{GridManager.i.scoreToWin}";
        TurnOrderText.text = $"Turn Order: ";
        for (int i = 0; i < Stations.Count; i++)
        {
            TurnOrderText.text += Stations[(TurnNumber+i) % Stations.Count].color;
            TurnOrderText.text += i == Stations.Count - 1 ? "." : ", ";
        }
        fakeCredits = MyStation.credits;
        ResetAfterSelection();
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
    
    public void ViewModuleInfo(bool active)
    {
        moduleInfoPanel.SetActive(active);
    }
    public void ViewModuleSelection(bool active)
    {
        selectModulePanel.SetActive(active);
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
    internal int GetUnitMaxAttachmentCount(Unit unit)
    {
        var maxAttached = unit.maxAttachedModules;
        if (unit is Fleet)
        {
            maxAttached += Stations[unit.stationId].actions.Count(x => x.actionType == ActionType.UpgradeFleet);
        }
        else
        {
            maxAttached += Stations[unit.stationId].actions.Count(x => x.actionType == ActionType.UpgradeStation);
        }
        return maxAttached;
    }
    private void SetModuleBar(Unit unit)
    {
        for (int i = 0; i < 6; i++)
        {
            StructureModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
            var maxAttached = GetUnitMaxAttachmentCount(unit);
            if (i < maxAttached)
            {
                if (i < unit.attachedModules.Count)
                {
                    StructureModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = unit.attachedModules[i].icon;
                    StructureModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(unit.stationId == MyStation.stationId);
                    var moduleText = unit.attachedModules[i].effectText;
                    StructureModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() => SetModuleInfo(moduleText));
                }
                else
                {
                    StructureModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(false);
                    if (unit.stationId == MyStation.stationId)
                    {
                        StructureModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = attachModuleBar;
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
