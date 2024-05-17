using StartaneousAPI.ServerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GameObject auctionPrefab;
    public GameObject moduleInfoPanel;
    public GameObject infoPanel;
    public GameObject fightPanel;
    public GameObject alertPanel;
    public GameObject customAlertPanel;
    public GameObject areYouSurePanel;
    public GameObject selectModulePanel;
    public GameObject helpPanel;
    public GameObject readyToSeeTurnsPanel;
    public GameObject moduleMarket;
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
    //public Button mineButton;
    public Button createFleetButton;
    public Button generateModuleButton;
    public Image endTurnButton;
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
    internal List<Module> AuctionModules = new List<Module>();
    internal List<GameObject> AuctionObjects = new List<GameObject>();
    internal int winner = -1;
    internal Station MyStation {get {return Stations[Globals.localStationIndex];}}
    public GameObject turnLabel;
    private SqlManager sql;
    private int TurnNumber = 0;
    private bool infoToggle = false;
    private int currentCredits;
    private int helpPageNumber = 0;
    private bool isWaitingForTurns = false;
    private bool gameWon = false;
    //Got game icons from https://game-icons.net/
    private void Awake()
    {
        i = this;
        sql = new SqlManager();
#if UNITY_EDITOR
        if(!Globals.HasBeenToLobby)
            SceneManager.LoadScene((int)Scene.Lobby);
#endif
    }
    void Start()
    { 
        StartCoroutine(WaitforGameToStart());
    }

    private IEnumerator WaitforGameToStart()
    {
        FindUI();
        while (!GridManager.i.DoneLoading)
        {
            yield return new WaitForSeconds(.1f);
        }
        ColorText.text = $"You are {MyStation.color}";
        ColorText.color = GridManager.i.playerColors[MyStation.stationId];
        StartTurn();
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
        kineticAttackValue.text = $"{unit.kineticAttack}~{kineticSupport}~{unit.kineticArmor}";
        thermalAttackValue.text = $"{unit.thermalAttack}~{thermalSupport}~{unit.thermalArmor}";
        explosiveAttackValue.text = $"{unit.explosiveAttack}~{explosiveSupport}~{unit.explosiveArmor}";
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
            if (unit is Fleet)
            {
                createFleetButton.gameObject.SetActive(false);
            }
            else
            {
                createFleetButton.gameObject.SetActive(true);
            }
        }
        else
        {
            upgradeButton.gameObject.SetActive(false);
            createFleetButton.gameObject.SetActive(false);
        }
        SetModuleBar(unit);
        infoPanel.gameObject.SetActive(true);
        ToggleMineralText(true);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isEndingTurn)
        {
            if (gameWon)
            {
                SceneManager.LoadScene((int)Scene.Lobby);
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);
            if (hit.collider != null && !isMoving)
            {
                PathNode targetNode = hit.collider.GetComponent<PathNode>();
                if (targetNode != null)
                {
                    Unit targetUnit = null;
                    if (targetNode.structureOnPath != null)
                    {
                        targetUnit = targetNode.structureOnPath;
                    }
                    //Original click on fleet
                    if (SelectedUnit == null && targetUnit != null && targetUnit.stationId == MyStation.stationId)
                    {
                        SelectedUnit = targetUnit;
                        SetTextValues(SelectedUnit);
                        if (HasQueuedMovement(SelectedUnit))
                        {
                            SelectedUnit.subtractMovement(99);
                            var movementAction = MyStation.actions.FirstOrDefault(x => (x.actionType == ActionType.MoveUnit || x.actionType == ActionType.MoveAndMine) && x.selectedUnit.unitGuid == targetUnit.unitGuid);
                            DrawPath(movementAction.selectedPath);
                            Debug.Log($"{SelectedUnit.unitName} already has a pending movement action.");
                            HighlightRangeOfMovement(movementAction.selectedPath.LastOrDefault(x => !x.isAsteroid), SelectedUnit, true);
                        }
                        else
                        {
                            Debug.Log($"{SelectedUnit.unitName} Selected.");
                            HighlightRangeOfMovement(SelectedUnit.currentPathNode, SelectedUnit);
                        }
                    }
                    else
                    {
                        //Mine after move
                        if (SelectedUnit != null && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode) && HasQueuedMovement(SelectedUnit) && targetNode.isAsteroid)
                        {
                            QueueAction(ActionType.MineAsteroid, null, null, new List<PathNode>() { targetNode });
                        }
                        //Double click confirm to movement
                        else if (SelectedUnit != null && SelectedNode != null && targetNode == SelectedNode && SelectedPath != null)
                        {
                            if (SelectedPath.Count == 1 && SelectedNode.isAsteroid)
                            {
                                SelectedUnit.resetMovementRange();
                                QueueAction(ActionType.MineAsteroid);
                            }
                            else if (!HasQueuedMovement(SelectedUnit))
                            {
                                if(SelectedPath.Last().isAsteroid)
                                    QueueAction(ActionType.MoveAndMine);
                                else
                                    QueueAction(ActionType.MoveUnit);
                            }
                        }
                        //Create movement
                        else if (SelectedUnit != null && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode) && !HasQueuedMovement(SelectedUnit))
                        {
                            SelectedNode = targetNode;
                            int oldPathCount = SelectedPath?.Count ?? 0;
                            if (SelectedPath == null || SelectedPath.Count == 0)
                            {
                                SelectedPath = GridManager.i.FindPath(SelectedUnit.currentPathNode, SelectedNode, SelectedUnit.stationId);
                                SelectedUnit.subtractMovement(SelectedPath.Last().gCost);
                                Debug.Log($"Path created for {SelectedUnit.unitName}");
                            }
                            else
                            {
                                var newPath = GridManager.i.FindPath(SelectedPath.Last(), SelectedNode, SelectedUnit.stationId);
                                SelectedUnit.subtractMovement(newPath.Last().gCost);
                                SelectedPath.AddRange(newPath);
                                Debug.Log($"Path edited for {SelectedUnit.unitName}");
                            }
                            DrawPath(SelectedPath);
                            HighlightRangeOfMovement(targetNode, SelectedUnit);
                        }
                        //Clicked on invalid tile
                        else
                        {
                            if (targetUnit != null)
                            {
                                if (SelectedUnit == null)
                                {
                                    SetTextValues(targetUnit);
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
                else
                {
                    DeselectMovement();
                }
            }
        }
    }

    private void DrawPath(List<PathNode> selectedPath)
    {
        ClearMovementPath();
        foreach (var node in selectedPath)
        {
            if (node == selectedPath.Last())
                currentPathObjects.Add(Instantiate(selectPrefab, node.transform.position, Quaternion.identity));
            else
                currentPathObjects.Add(Instantiate(pathPrefab, node.transform.position, Quaternion.identity));
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
    public void ViewModuleMarket(bool active)
    {
        moduleMarket.SetActive(active);
        if (active)
        {
            ClearAuctionObjects();
            for(int j = 0; j < AuctionModules.Count; j++)
            {
                int i = j;
                var module = AuctionModules[i];
                var hasQueued = MyStation.actions.Any(x => x.actionType == ActionType.BidOnModule && x.selectedModule?.moduleGuid == module.moduleGuid);
                if(!hasQueued)
                    module.currentBid = module.minBid;
                var moduleObject = Instantiate(auctionPrefab, moduleMarket.transform.Find("MarketBar"));
                AuctionObjects.Add(moduleObject);
                moduleObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => SetModuleInfo(module.effectText));
                var addButton = moduleObject.transform.Find("Add").GetComponent<Button>();
                addButton.onClick.AddListener(() => ModifyBid(true, i));
                addButton.interactable = !hasQueued && currentCredits > module.currentBid;
                var subtractButton = moduleObject.transform.Find("Subtract").GetComponent<Button>();
                subtractButton.interactable = !hasQueued && module.minBid < module.currentBid;
                subtractButton.onClick.AddListener(() => ModifyBid(false,i));
                var bidButton = moduleObject.transform.Find("Bid").GetComponent<Button>();
                bidButton.interactable = !hasQueued && currentCredits >= module.currentBid;
                bidButton.onClick.AddListener(() => BidOn(i));
                moduleObject.transform.Find("Bid/BidText").GetComponent<TextMeshProUGUI>().text = $"Bid {module.currentBid}";
                moduleObject.transform.Find("Queued").gameObject.SetActive(hasQueued);
                moduleObject.transform.Find("TurnTimer").GetComponent<TextMeshProUGUI>().text = module.turnsLeftOnMarket > 1 ? $"{module.turnsLeftOnMarket} Turns Left" : "Last Turn!!!";
                moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
            }
        }
    }

    private void ModifyBid(bool add, int index)
    {
        var module = AuctionModules[index];
        var moduleObj = AuctionObjects[index];
        if (add) {
            module.currentBid++;
        }
        else if (module.currentBid > module.minBid)
        {
            module.currentBid--;
        }
        moduleObj.transform.Find("Bid/BidText").GetComponent<TextMeshProUGUI>().text = $"Bid {module.currentBid}";
        moduleObj.transform.Find("Add").GetComponent<Button>().interactable = currentCredits > module.currentBid;
        moduleObj.transform.Find("Subtract").GetComponent<Button>().interactable = module.minBid < module.currentBid;
        moduleObj.transform.Find("Bid").GetComponent<Button>().interactable = currentCredits >= module.currentBid;
    }

    private void BidOn(int index)
    {
        QueueAction(ActionType.BidOnModule, AuctionModules[index]);
    }

    //public void UpdateMarket() {
    //    foreach (var module in AuctionModules)
    //    {
    //        module.minBid--;
    //        module.turnsLeftOnMarket--;
    //    }
    //    AuctionModules.RemoveAll(x => x.turnsLeftOnMarket < 1);
    //    while (AuctionModules.Count < 4)
    //    {
    //        AuctionModules.Add(new Module(UnityEngine.Random.Range(0, 23), Guid.NewGuid()));
    //    }
    //}
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
        infoPanel.SetActive(active);
    }
    private void UpdateAuctionModules(int i)
    {
        ClearAuctionObjects();
        AuctionModules.Clear();
        AuctionModules.AddRange(Globals.GameMatch.GameTurns[i].MarketModules.Select(x => new Module(x)));
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
            List<Guid?> assignedModules = MyStation.actions.Select(x => x.selectedModule?.moduleGuid).ToList();
            var availableModules = MyStation.modules.Where(x => !assignedModules.Contains(x.moduleGuid)).ToList();
            if (availableModules.Count > 0)
            {
                foreach (var module in availableModules)
                {
                    var moduleObject = Instantiate(modulePrefab, SelectedModuleGrid);
                    moduleObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => SetSelectedModule(module, selectedStructure));
                    moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
                    moduleObject.transform.Find("Queued").gameObject.SetActive(false);
                    currentModulesForSelection.Add(moduleObject.GetComponent<Module>());
                }
                ViewModuleSelection(true);
            }
        }
    }
    private void SetSelectedModule(Module module, Unit structure)
    {
        //if not already queued up
        if (!MyStation.actions.Any(x => x.actionType == ActionType.AttachModule && x.selectedModule?.moduleGuid == module.moduleGuid))
        {
            ViewModuleSelection(false);
            QueueAction(ActionType.AttachModule, module, structure);
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
        else if (MyStation.actions.Any(x => x.actionType == ActionType.DetachModule && x.selectedModule?.moduleGuid == SelectedUnit.attachedModules[i].moduleGuid))
        {
             ShowCustomAlertPanel($"The action {ActionType.DetachModule} for the module {SelectedUnit.attachedModules[i].moduleId} has already been queued up");
        }
        else
        {
            QueueAction(ActionType.DetachModule, SelectedUnit.attachedModules[i]);
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
            QueueAction(ActionType.CreateFleet,null,null,null,Guid.NewGuid());
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

    //public void MineAsteroid()
    //{
    //    QueueAction(ActionType.MineAsteroid);
    //}
    
    public void CancelAction(int slot)
    {
        var action = MyStation.actions[slot];
        Debug.Log($"{MyStation.unitName} removed action {action.actionType} from queue");
        if (action is object)
        {
            if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine)
            {
                action.selectedUnit.resetMovementRange();
            }
            currentCredits += action.costOfAction;
            ClearActionBar();
            MyStation.actions.RemoveAt(slot);
            for (int i = 0; i < MyStation.actions.Count; i++)
            {
                AddActionBarImage(MyStation.actions[i].actionType, i);
            }
        }
        ResetAfterSelection();
    }
    private void QueueAction(ActionType actionType, Module? selectedModule = null, Unit _structure = null, List<PathNode> _selectedPath = null, Guid? generatedGuid = null)
    {
        if (MyStation.actions.Count >= MyStation.maxActions)
        {
            ShowCustomAlertPanel($"No action slots available to queue action: {actionType}.");
        } else {
            var structure = _structure ?? SelectedUnit ?? MyStation;
            var selectedPath = _selectedPath ?? SelectedPath;
            var costOfAction = GetCostOfAction(actionType, structure, true, selectedModule);
            if (costOfAction + MyStation.actions.Sum(x => x.costOfAction) > MyStation.credits)
            {
                ShowCustomAlertPanel($"Can not afford to queue action: {actionType}.");
            }
            else
            {
                currentCredits -= costOfAction;
                Debug.Log($"{MyStation.unitName} queuing up action {actionType}");
                MyStation.actions.Add(new Action(actionType, structure, costOfAction, selectedModule, selectedPath, generatedGuid));
                AddActionBarImage(actionType, MyStation.actions.Count()-1);
                ResetAfterSelection();
            }
        }
    }
    #endregion
    private void ToggleMineralText(bool value)
    {
        foreach (var asteroid in GridManager.i.AllNodes)
        {
            asteroid.ShowMineralText(value);
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
        int image = (int)actionType;
        ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Actions/{image}");
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
    private int GetCostOfAction(ActionType actionType, Unit structure, bool countQueue, Module? auctionModule = null)
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
        else if (actionType == ActionType.BidOnModule)
        {
            return auctionModule?.currentBid ?? 0;
        }
        else if (actionType == ActionType.GainCredit)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    private bool HasQueuedMovement(Unit selectedUnit)
    {
        return MyStation.actions.Any(x => (x.actionType == ActionType.MoveUnit || x.actionType == ActionType.MoveAndMine) && x.selectedUnit.unitGuid == selectedUnit.unitGuid);
    }

    private void ResetAfterSelection()
    {
        ToggleMineralText(false);
        ToggleHPText(false);
        infoToggle = false;
        UpdateCreateFleetCostText();
        UpdateCreditTotal();
        SetModuleGrid();
        ClearMovementPath();
        ClearMovementRange();
        SelectedNode = null;
        SelectedUnit = null;
        SelectedPath = null;
        ViewStructureInformation(false);
        ViewModuleMarket(false);
    }

    private void UpdateCreditTotal()
    {
        CreditText.text = $"{currentCredits} Credits";
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
            if (unitMoving.range < 0 || node.isAsteroid)
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
                yield return StartCoroutine(PerformMine(unitMoving, node));
            }
            //if enemy, attack, if you didn't destroy them, stay blocked and move back
            if (unitMoving.range >= 0 && node.structureOnPath != null && node.structureOnPath.stationId != unitMoving.stationId)
            {
                yield return StartCoroutine(FightEnemyUnit(unitMoving, node));
                blockedMovement = node.structureOnPath != null && AllUnits.Contains(node.structureOnPath);
            }
            if (!blockedMovement)
                unitMoving.currentPathNode = node;
            if (Globals.GameMatch.GameSettings.Contains(GameSettingType.TakeoverCosts2.ToString())
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
                var station = (unitMoving as Station);
                station.defeated = true;
                while (station.fleets.Count > 0) { AllUnits.Remove(station.fleets[0]);  Destroy(station.fleets[0].gameObject); station.fleets.RemoveAt(0); }
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
                var station = (unitOnPath as Station);
                station.defeated = true;
                while (station.fleets.Count > 0) { AllUnits.Remove(station.fleets[0]); Destroy(station.fleets[0].gameObject); station.fleets.RemoveAt(0); }
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
        int x = node.coords.x - unit.currentPathNode.coords.x;
        int y = node.coords.y - unit.currentPathNode.coords.y;
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
            var modifierSymbol = s1Amr > 0 ? "-" : "+";
            return $"{type} Phase: {s1.unitName} lost {damage} HP after resistances were applied, {s1Dmg}({modifierSymbol}{Mathf.Abs(s1Amr)}) \n";
        }
        else if (s2Dmg > 0)
        {
            var damage = Mathf.Max(s2Dmg - s2Amr, 0);
            s2.hp -= damage;
            if (s2Amr == 0)
            {
                return $"{type} Phase: {s2.unitName} lost {damage} HP \n";
            }
            var modifierSymbol = s2Amr > 0 ? "-" : "+";
            return $"{type} Phase: {s2.unitName} lost {damage} HP after resistances were applied, {s2Dmg}({modifierSymbol}{Mathf.Abs(s2Amr)}) \n";
        }
        return $"{type} Phase: Stalemate.\n";
    }

    public void HighlightRangeOfMovement(PathNode currentNode, Unit unit, bool forMining = false)
    {
        ClearMovementRange();
        List<PathNode> nodesWithinRange = GridManager.i.GetNodesWithinRange(currentNode, unit, forMining);
        foreach (PathNode node in nodesWithinRange)
        {
            GameObject range = Instantiate(node.isAsteroid ? movementMinePrefab : movementRangePrefab, node.transform.position, Quaternion.identity);
            range.transform.SetParent(highlightParent);
            var rangeComponent = range.AddComponent<Node>();
            rangeComponent.Initialize(node);
            currentMovementRange.Add(rangeComponent);
        }
    }
   
    public void HideReadyToSeeTurnsPanel()
    {
        readyToSeeTurnsPanel.SetActive(false);
    }
    private IEnumerator GetTurnsFromServer()
    {
        isWaitingForTurns = true;
        while (!Globals.GameMatch.GameTurns.Any(x=>x.TurnNumber == TurnNumber))
        {
            yield return StartCoroutine(sql.GetRoutine<GameTurn>($"Game/GetTurns?gameGuid={Globals.GameMatch.GameGuid}&turnNumber={TurnNumber}", CheckForTurns));
        }
        customAlertPanel.SetActive(false);
        isEndingTurn = true;
        turnValue.text = $"Turn #{TurnNumber} complete! \n (Tap to watch) ";
        turnLabel.SetActive(true);
        readyToSeeTurnsPanel.SetActive(true);
        while (readyToSeeTurnsPanel.activeInHierarchy)
        {
            yield return new WaitForSeconds(.5f);
        }
        isWaitingForTurns = false;
    }

    private void CheckForTurns(GameTurn turns)
    {
        if (turns != null && !Globals.GameMatch.GameTurns.Any(x => x.TurnNumber == TurnNumber))
        {
            Globals.GameMatch.GameTurns.Add(turns);
        }
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
#region Complete Turn
    public void EndTurn(bool theyAreSure)
    {
        if (!isEndingTurn)
        {
            if (theyAreSure || MyStation.actions.Count == MyStation.maxActions)
            {
                ShowAreYouSurePanel(false);
                Debug.Log($"Turn Ending, Starting Simultanous Turns");
                while (MyStation.actions.Count < MyStation.maxActions)
                {
                    QueueAction(ActionType.GainCredit);
                }
                List<int> actionOrders = new List<int>();
                for (int i = 1; i <= Stations.Count * 5; i += Stations.Count)
                {
                    for (int j = 0; j < Stations.Count; j++)
                    {
                        if (j == Globals.localStationIndex)
                            actionOrders.Add(i + j);
                    }
                }
                GameTurn gameTurn = new GameTurn()
                {
                    GameGuid = Globals.GameMatch.GameGuid,
                    TurnNumber = TurnNumber,
                    MarketModules = Globals.GameMatch.GameTurns.LastOrDefault().MarketModules,
                    Players = new Player[Globals.GameMatch.MaxPlayers]
                };
                gameTurn.Players[Globals.localStationIndex] = new Player()
                {
                    Actions = MyStation.actions.Select((x, i) =>
                        new ServerAction()
                        {
                            PlayerId = Globals.localStationIndex,
                            ActionOrder = actionOrders[i],
                            ActionTypeId = (int)x.actionType,
                            GeneratedGuid = x.generatedGuid,
                            SelectedCoords = x.selectedPath?.Select(y => y.coords?.ToServerCoords()).ToList(),
                            SelectedModule = x.selectedModule?.ToServerModule(),
                            SelectedUnitGuid = x.selectedUnit?.unitGuid,
                        }).ToList(),
                    Credits = MyStation.credits,
                    Station = MyStation.ToServerUnit(),
                    Fleets = MyStation.fleets.Select(x => x.ToServerUnit()).ToList(),
                    ModulesGuids = MyStation.modules.Select(x => x.moduleGuid).ToList(),
                };
                var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameTurn);
                StartCoroutine(sql.PostRoutine<bool>($"Game/EndTurn", stringToPost));
                endTurnButton.color = Color.blue;
                var plural = Globals.GameMatch.MaxPlayers > 2 ? "s" : "";
                customAlertText.text = $"Your turn was submitted! \n\n Tap to continue strategizing while you wait for the other player{plural}. \n\n Your last submitted turn will be used.";
                customAlertPanel.SetActive(true);
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
        if (!isWaitingForTurns)
        {
            yield return StartCoroutine(GetTurnsFromServer());
            var turnFromServer = Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == TurnNumber)?.Players;
            if (turnFromServer != null)
            {
                ClearModules();
                ToggleMineralText(true);
                var serverActions = turnFromServer.SelectMany(x => x.Actions).OrderBy(x=>x.ActionOrder);
                //ToggleHPText(true);
                foreach(var serverAction in serverActions){
                    var action = new Action(serverAction);
                    turnValue.text = $"{Stations[serverAction.PlayerId].color} action {action.actionOrder}:\n{action.actionType}";
                    Debug.Log($"Perfoming {Stations[serverAction.PlayerId].color}'s action {action.actionOrder}:\n{action.actionType}");
                    yield return StartCoroutine(PerformAction(action));
                    yield return new WaitForSeconds(.1f);
                }
                FinishTurns();
            }
        }
    }

    private void FinishTurns()
    {
        endTurnButton.color = Color.black;
        winner = GridManager.i.CheckForWin();
        UpdateAuctionModules(TurnNumber);
        foreach (var station in Stations)
        {
            station.actions.Clear();
            for (int i = 1; i <= 3; i++)
            {
                if (station.score >= Convert.ToInt32(Math.Floor(GridManager.i.scoreToWin * (.25 * i))))
                {
                    station.maxActions = 2 + i;
                }
            }
        }
        if (winner != -1)
        {
            turnValue.text = $"{Stations[winner].color} player won after {TurnNumber + 1} turns";
            Debug.Log($"{Stations[winner].color} player won after {TurnNumber + 1} turns");
            gameWon = true;
        }
        else
        {
            foreach (var asteroid in GridManager.i.AllNodes)
            {
                asteroid.ReginCredits();
            }
            //foreach (var unit in AllUnits)
            //{
            //    unit.hasMinedThisTurn = false;
            //}
            StartTurn();
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
                var actionCost = GetCostOfAction(action.actionType, action.selectedUnit, false, action.selectedModule);
                if (currentStation.credits >= actionCost)
                {
                    currentStation.credits -= actionCost;
                    if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine)
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
                        if (IsUnderMaxFleets(currentStation, false))
                        {
                            yield return StartCoroutine(GridManager.i.CreateFleet(currentStation, (Guid)action.generatedGuid, false));
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
                    else if (action.actionType == ActionType.BidOnModule)
                    {
                        if (action.selectedModule is object)
                        {
                            currentStation.modules.Add(new Module(action.selectedModule.moduleId, action.selectedModule.moduleGuid));
                        }
                        else
                        {
                            Debug.Log($"{currentUnit.unitName} lost the bid");
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.AttachModule)
                    {
                        if (currentStation.modules.Count > 0 && currentUnit.attachedModules.Count < currentUnit.maxAttachedModules)
                        {
                            Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModule?.moduleGuid);
                            if (selectedModule is object)
                            {
                                currentUnit.attachedModules.Add(selectedModule);
                                currentUnit.EditModule(selectedModule.moduleId);
                                currentStation.modules.RemoveAt(currentStation.modules.Select(x => x.moduleGuid).ToList().IndexOf(selectedModule.moduleGuid));
                            }
                            else
                            {
                                Debug.Log($"Module not available");
                            }
                        }
                        else
                        {
                            Debug.Log($"No modules on station or max attached modules");
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.DetachModule)
                    {
                        if (currentUnit.attachedModules.Count > 0)
                        {
                            Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModule?.moduleGuid);
                            if (selectedModule is object)
                            {
                                currentStation.modules.Add(selectedModule);
                                currentUnit.EditModule(selectedModule.moduleId, -1);
                                currentUnit.attachedModules.Remove(currentUnit.attachedModules.FirstOrDefault(x => x.moduleGuid == selectedModule.moduleGuid));
                            }
                            else
                            {
                                Debug.Log($"Module not available");
                            }
                        }
                        else
                        {
                            Debug.Log($"No Modules attached");
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.MineAsteroid)
                    {
                        var targetNode = action.selectedPath.Last();
                        if (GridManager.i.GetNeighbors(currentUnit.currentPathNode).Contains(targetNode))
                        {
                            yield return StartCoroutine(PerformMine(currentUnit, targetNode));
                        }
                    }
                    else if (action.actionType == ActionType.GainCredit)
                    {
                        Debug.Log($"Gained 1 credit");
                        currentUnit.selectIcon.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        currentUnit.selectIcon.SetActive(false);
                    }
                }
                else
                {
                    Debug.Log($"Broke ass {currentUnit.color} bitch couldn't afford {action.actionType}");
                }
            }
        }
    }

    private IEnumerator PerformMine(Unit currentUnit, PathNode asteroidToMine)
    {
        Station currentStation = Stations[currentUnit.stationId];
        if (AllUnits.Contains(currentUnit))
        {
            currentStation.credits += asteroidToMine.MineCredits(currentUnit.mining);
            currentUnit.selectIcon.SetActive(true);
            asteroidToMine.ShowMineIcon(true);
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
            asteroidToMine.ShowMineIcon(false);
            currentUnit.selectIcon.SetActive(false);
        }
    }

    private void StartTurn()
    {
        UpdateAuctionModules(TurnNumber);
        TurnNumber++;
        turnLabel.SetActive(false);
        ClearActionBar();
        GridManager.i.GetScores();
        ScoreToWinText.text = $"Hexes to win: {MyStation.score}/{GridManager.i.scoreToWin}";
        TurnOrderText.text = $"Turn Order: ";
        for (int i = 0; i < Stations.Count; i++)
        {
            TurnOrderText.text += Stations[(TurnNumber+i) % Stations.Count].color;
            TurnOrderText.text += i == Stations.Count - 1 ? "." : ", ";
        }
        currentCredits = MyStation.credits;
        ResetAfterSelection();
        isEndingTurn = false;
        Debug.Log($"New Turn {TurnNumber} Starting");
    }
    #endregion
    private void LevelUpUnit(Unit structure)
    {
        if (CanLevelUp(structure, structure is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet, false))
        {
            structure.level++;
            structure.maxAttachedModules++;
            structure.maxHp += 4;
            structure.hp += 4;
            structure.kineticAttack++;
            structure.explosiveAttack++;
            structure.thermalAttack++;
            if (structure is Station)
            {
                (structure as Station).maxFleets++;
            }
        }
    }
    private void SetModuleGrid()
    {
        ClearModules();
        foreach (var module in MyStation.modules)
        {
            var moduleObject = Instantiate(modulePrefab, ModuleGrid);
            moduleObject.GetComponentInChildren<Button>().onClick.AddListener(() => SetModuleInfo(module.effectText));
            moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
            moduleObject.transform.Find("Queued").gameObject.SetActive(MyStation.actions.Any(x=>x.selectedModule?.moduleGuid == module.moduleGuid));
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
    private void ClearAuctionObjects()
    {
        while (AuctionObjects.Count > 0) { Destroy(AuctionObjects[0].gameObject); AuctionObjects.RemoveAt(0); }
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
