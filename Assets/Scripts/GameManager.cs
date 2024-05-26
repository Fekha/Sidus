using StartaneousAPI.ServerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
    public GameObject alertPanel;
    public GameObject customAlertPanel;
    public GameObject areYouSurePanel;
    public GameObject selectModulePanel;
    public GameObject helpPanel;
    public GameObject moduleMarket;
    public GameObject asteroidPanel;

    private List<GameObject> currentPathObjects = new List<GameObject>();
    private List<PathNode> SelectedPath;

    public Transform ActionBar;
    public Transform UnitModuleBar;
    public Transform ModuleGrid;
    public Transform SelectedModuleGrid;
    public Transform turnOrder;

    public Sprite lockActionBar;
    public Sprite lockModuleBar;
    public Sprite attachModuleBar;
    public Sprite emptyModuleBar;
    public Sprite unReadyCircle;
    public Sprite readyCircle;

    private PathNode SelectedNode;
    private Unit SelectedUnit;
    private List<Node> currentMovementRange = new List<Node>();
    private List<Module> currentModules = new List<Module>();
    private List<Module> currentModulesForSelection = new List<Module>();
    internal List<Station> Stations = new List<Station>();

    private bool tapped = false;
    private bool isMoving = false;
    private bool isEndingTurn = false;
    private Button upgradeButton;
    public Button createFleetButton;
    public Button generateModuleButton;
    public Image endTurnButton;
    private TextMeshProUGUI createFleetCost;
    private TextMeshProUGUI upgradeCost;
    private TextMeshProUGUI nameValue;
    private TextMeshProUGUI levelValue;
    private TextMeshProUGUI HPValue;
    private TextMeshProUGUI miningValue;
    private TextMeshProUGUI rangeValue;
    private TextMeshProUGUI PowerValueText;
    private TextMeshProUGUI SupportValueText;
    private TextMeshProUGUI ModuleEffectText;
    private TextMeshProUGUI turnValue;
    private TextMeshProUGUI phaseText;
    public GameObject turnTapText;
    private TextMeshProUGUI moduleInfoValue;
    public TextMeshProUGUI ScoreToWinText;
    public TextMeshProUGUI ColorText;
    public TextMeshProUGUI CreditText;
    private TextMeshProUGUI alertText;
    private TextMeshProUGUI customAlertText;
    private TextMeshProUGUI creditsText;
    private TextMeshProUGUI hexesOwnedText;
 
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
    private bool SubmittedTurn = false;
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
        StartCoroutine(GetTurnReadiness());
    }

    private IEnumerator GetTurnReadiness()
    {
        while (!Globals.IsCPUGame)
        {
            while (!isEndingTurn)
            {
                yield return StartCoroutine(sql.GetRoutine<GameTurn>($"Game/HasTakenTurn?gameGuid={Globals.GameMatch.GameGuid}&turnNumber={TurnNumber}", UpdateGameTurnStatus));
                yield return new WaitForSeconds(.1f);
            }
            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateGameTurnStatus(GameTurn turn)
    {
        if (turn != null) {
            for(int i = 0; i < turn.Players.Length; i++)
            {
                turnOrder.Find($"TurnOrder{i}").GetComponent<Image>().sprite = turn.Players[(TurnNumber - 1 + i) % turn.Players.Length] == null ? unReadyCircle : readyCircle;
            }
        }
    }

    private void FindUI()
    {
        highlightParent = GameObject.Find("Highlights").transform;
        nameValue = infoPanel.transform.Find("NameValue").GetComponent<TextMeshProUGUI>();
        levelValue = infoPanel.transform.Find("LevelValue").GetComponent<TextMeshProUGUI>();
        HPValue = infoPanel.transform.Find("HPValue").GetComponent<TextMeshProUGUI>();
        miningValue = infoPanel.transform.Find("MiningValue").GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("MovementValue").GetComponent<TextMeshProUGUI>();
        PowerValueText = infoPanel.transform.Find("PowerValue").GetComponent<TextMeshProUGUI>();
        SupportValueText = infoPanel.transform.Find("SupportValue").GetComponent<TextMeshProUGUI>();
        ModuleEffectText = infoPanel.transform.Find("DamageTakenValue").GetComponent<TextMeshProUGUI>();
        creditsText = infoPanel.transform.Find("Credits").GetComponent<TextMeshProUGUI>();
        hexesOwnedText = infoPanel.transform.Find("HexesOwned").GetComponent<TextMeshProUGUI>();
        turnValue = turnLabel.transform.Find("TurnValue").GetComponent<TextMeshProUGUI>();
        phaseText = turnLabel.transform.Find("PhaseText").GetComponent<TextMeshProUGUI>();
        moduleInfoValue = moduleInfoPanel.transform.Find("ModuleInfoText").GetComponent<TextMeshProUGUI>();
        createFleetCost = createFleetButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        upgradeButton = infoPanel.transform.Find("UpgradeButton").GetComponent<Button>();
        upgradeCost = upgradeButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        alertText = alertPanel.transform.Find("Background/AlertText").GetComponent<TextMeshProUGUI>();
        customAlertText = customAlertPanel.transform.Find("Background/AlertText").GetComponent<TextMeshProUGUI>();
    }
    public void SetUnitTextValues(Unit unit)
    {
        nameValue.text = unit.unitName;
        levelValue.text = unit.level.ToString();
        if (unit.teamId != MyStation.teamId && unit.moduleEffects.Contains(ModuleEffect.HiddenStats))
        {
            HPValue.text = "?/?";
            rangeValue.text = "?";
            PowerValueText.text = "?|?|?";
            ModuleEffectText.text = "?";
            miningValue.text = "?";
        }
        else
        {
            HPValue.text = unit.HP + "/" + unit.maxHP;
            rangeValue.text = unit.maxRange.ToString();
            PowerValueText.text = $"{unit.kineticPower}|{unit.thermalPower}|{unit.explosivePower}";
            ModuleEffectText.text = "None";
            if (unit.attachedModules.Count > 0)
            {
                ModuleEffectText.text = unit.attachedModules[0].effectText.Replace(" \n", ",");
                for (int i = 1; i < unit.attachedModules.Count; i++)
                {
                    ModuleEffectText.text += $"\n {unit.attachedModules[i].effectText.Replace(" \n", ",")}";
                }
            }
            miningValue.text = unit.mining.ToString();
        }
        var supportingFleets = GridManager.i.GetNeighbors(unit.currentPathNode).Select(x => x.structureOnPath).Where(x => x != null && x.teamId == unit.teamId);
        var kineticSupport = 0;
        var thermalSupport = 0;
        var explosiveSupport = 0;
        if (supportingFleets.Any())
        {
            kineticSupport = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.kineticPower * x.supportValue)));
            thermalSupport = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.thermalPower * x.supportValue)));
            explosiveSupport = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.explosivePower * x.supportValue)));
        }
        SupportValueText.text = $"{kineticSupport}|{thermalSupport}|{explosiveSupport}";
        var actionType = unit is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        upgradeButton.interactable = false;
        upgradeButton.gameObject.SetActive(false);
        createFleetButton.gameObject.SetActive(false);
        creditsText.text = $"Player Credits: {Stations[unit.stationId].credits}";
        hexesOwnedText.text = $"Hexes Owned: {Stations[unit.stationId].score}";
        if (unit.stationId == MyStation.stationId)
        {
            if (CanLevelUp(unit, actionType, true))
            {
                var cost = GetCostOfAction(actionType, unit, true);
                upgradeCost.text = GetCostText(cost);
                upgradeButton.interactable = currentCredits >= cost;
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
            if (unit is Station)
            {
                createFleetButton.gameObject.SetActive(true);
                creditsText.text = $"Player Credits: {currentCredits}";
            }
            ToggleMineralText(true);
        }
        ToggleHPText(true);
        SetModuleBar(unit);
        infoPanel.gameObject.SetActive(true);
        
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tapped = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);
            if (!isEndingTurn && hit.collider != null && !isMoving)
            {
                PathNode targetNode = hit.collider.GetComponent<PathNode>();
                if (targetNode != null)
                {
                    moduleMarket.SetActive(false); 
                    Unit targetUnit = null;
                    if (targetNode.structureOnPath != null)
                    {
                        targetUnit = targetNode.structureOnPath;
                    }
                    //Original click on fleet
                    if (SelectedUnit == null && targetUnit != null && targetUnit.stationId == MyStation.stationId)
                    {
                        SelectedUnit = targetUnit;
                        SetUnitTextValues(SelectedUnit);
                        if (HasQueuedMovement(SelectedUnit))
                        {
                            HighlightQueuedMovement(MyStation.actions.FirstOrDefault(x => (x.actionType == ActionType.MoveUnit || x.actionType == ActionType.MoveAndMine) && x.selectedUnit.unitGuid == SelectedUnit.unitGuid));
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
                                SelectedPath = GridManager.i.FindPath(SelectedUnit.currentPathNode, SelectedNode, SelectedUnit.teamId);
                                SelectedUnit.subtractMovement(SelectedPath.Last().gCost);
                                Debug.Log($"Path created for {SelectedUnit.unitName}");
                            }
                            else
                            {
                                var newPath = GridManager.i.FindPath(SelectedPath.Last(), SelectedNode, SelectedUnit.teamId);
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
                            if (SelectedUnit == null)
                            {
                                if (targetUnit != null)
                                {
                                    SetUnitTextValues(targetUnit);
                                    ViewUnitInformation(true);
                                }
                                else if (targetNode.isAsteroid)
                                {
                                    SetAsteroidTextValues(targetNode);
                                    ViewAsteroidInformation(true);
                                }
                                else
                                {
                                    DeselectMovement();
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

    private void SetAsteroidTextValues(PathNode targetNode)
    {
        asteroidPanel.transform.Find("CurrentValue").GetComponent<TextMeshProUGUI>().text = $"Current Value: {targetNode.currentCredits}";
        asteroidPanel.transform.Find("MaxValue").GetComponent<TextMeshProUGUI>().text = $"Max Value: {targetNode.maxCredits}";
        asteroidPanel.transform.Find("RegenValue").GetComponent<TextMeshProUGUI>().text = $"Regen Per Turn: {targetNode.creditsRegin}";
    }

    public void ShowQueuedAction(int i)
    {
        if(MyStation.actions[i].actionType == ActionType.MoveUnit || MyStation.actions[i].actionType == ActionType.MoveAndMine)
            HighlightQueuedMovement(MyStation.actions[i]);
    }
    private void HighlightQueuedMovement(Action movementAction)
    {
        SelectedUnit = movementAction.selectedUnit;
        SelectedUnit.subtractMovement(99);
        DrawPath(movementAction.selectedPath);
        Debug.Log($"{SelectedUnit.unitName} already has a pending movement action.");
        HighlightRangeOfMovement(movementAction.selectedPath.LastOrDefault(x => !x.isAsteroid), SelectedUnit, true);
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
    public void ViewModuleMarket(bool active)
    {
        moduleMarket.SetActive(active);
        if (active)
        {
            DeselectMovement();
            ClearAuctionObjects();
            for(int j = 0; j < AuctionModules.Count; j++)
            {
                int i = j;
                var module = AuctionModules[i];
                var hasQueued = MyStation.actions.Any(x => x.actionType == ActionType.BidOnModule && x.selectedModule?.moduleGuid == module.moduleGuid);
                if (!hasQueued) { module.currentBid = module.minBid; }
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
        ViewModuleMarket(false);
        QueueAction(ActionType.BidOnModule, AuctionModules[index]);
    }

    public void ViewHelpPanel(bool active)
    {
        helpPageNumber++;
        if (helpPageNumber > 3)
        {
            helpPageNumber = 0;
            helpPanel.SetActive(false);
            helpPanel.transform.Find("Page2").gameObject.SetActive(false);
            helpPanel.transform.Find("Page3").gameObject.SetActive(false);
        }
        else
        {
            helpPanel.transform.Find($"Page{helpPageNumber}").gameObject.SetActive(true);
        }
        if (active)
        {
            helpPanel.SetActive(active);
        }
    }

    public void ShowUnlockPanel(int unlock)
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
    public void ViewUnitInformation(bool active)
    {
        infoPanel.SetActive(active);
    } 
    public void ViewAsteroidInformation(bool active)
    {
        ToggleMineralText(active);
        asteroidPanel.SetActive(active);
    }
    private void UpdateAuctionModules(int i)
    {
        ClearAuctionObjects();
        AuctionModules.Clear();
        AuctionModules.AddRange(Globals.GameMatch.GameTurns[i].MarketModules.Select(x => new Module(x)));
    }
    #region Queue Actions
    public void ModifyModule(ActionType actionType, Guid? dettachGuid = null)
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
        else if (actionType == ActionType.SwapModule && MyStation.actions.Any(x => x.actionType == ActionType.SwapModule && x.generatedGuid == dettachGuid))
        {
            ShowCustomAlertPanel($"The action {ActionType.SwapModule} for this module has already been queued up");
        }
        else if (actionType == ActionType.AttachModule && (SelectedUnit.attachedModules.Count + MyStation.actions.Count(x => x.actionType == ActionType.AttachModule && x.selectedUnit.unitGuid == SelectedUnit.unitGuid)) >= GetUnitMaxAttachmentCount(SelectedUnit))
        {
            ShowCustomAlertPanel($"This action has already been queued up.");

        }
        else if (MyStation.stationId != SelectedUnit.stationId)
        {
            ShowCustomAlertPanel("This is not your unit!");
        } 
        else 
        {
            var selectedUnit = SelectedUnit;
            DeselectMovement();
            ClearSelectableModules();
            List<Guid?> assignedModules = MyStation.actions.Select(x => x.selectedModule?.moduleGuid).ToList();
            var availableModules = MyStation.modules.Where(x => !assignedModules.Contains(x.moduleGuid)).ToList();
            if (availableModules.Count > 0)
            {
                foreach (var module in availableModules)
                {
                    var moduleObject = Instantiate(modulePrefab, SelectedModuleGrid);
                    moduleObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => SetSelectedModule(module, selectedUnit, actionType, dettachGuid));
                    moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
                    moduleObject.transform.Find("Queued").gameObject.SetActive(false);
                    currentModulesForSelection.Add(moduleObject.GetComponent<Module>());
                }
                ViewModuleSelection(true);
            }
        }
    }
    private void SetSelectedModule(Module module, Unit unit, ActionType action, Guid? dettachGuid)
    {
        //if not already queued up
        if (MyStation.actions.Any(x => x.selectedModule?.moduleGuid == module.moduleGuid))
        {
            ShowCustomAlertPanel("This action is already queued up.");
        }
        else
        {
            ViewModuleSelection(false);
            QueueAction(action, module, unit, null, dettachGuid);
            ClearSelectableModules();
        }
    }
    public void CreateFleet()
    {
        if (!IsUnderMaxFleets(MyStation,true))
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
        if (!CanLevelUp(SelectedUnit, actionType, true))
        {
            ShowCustomAlertPanel("Unit is already max level, or has it's max level queued.");
        }
        else
        {
            QueueAction(actionType);
        }
    }

    public void CancelAction(int slot)
    {
        var action = MyStation.actions[slot];
        Debug.Log($"{MyStation.unitName} removed action {GetDescription(action.actionType)} from queue");
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
            if (costOfAction > currentCredits)
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
        ActionBar.Find($"Action{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
        ActionBar.Find($"Action{i}/Image").GetComponent<Button>().onClick.AddListener(() => ShowQueuedAction(i));
        ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(true);
    }
  
    private void UpdateCreateFleetCostText()
    {
        if (IsUnderMaxFleets(MyStation, true))
        {
            var cost = GetCostOfAction(ActionType.CreateFleet, MyStation, true);
            createFleetCost.text = GetCostText(cost);
            createFleetButton.interactable = (currentCredits >= cost);
        }
        else
        {
            createFleetCost.text = "(Station Upgrade Required)";
            createFleetButton.interactable = false;
        }
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
            return nextLevel < 4;
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
            return (station.fleets.Count + countingQueue) * (6 - (station.fleets.Count + countingQueue)); //5,8,9
        }
        else if (actionType == ActionType.UpgradeFleet)
        {
            return (structure.level + countingQueue) * (7 - (structure.level + countingQueue)); //6,10,12
        }
        else if (actionType == ActionType.UpgradeStation)
        {
            return (structure.level + countingQueue) * (9 - (structure.level + countingQueue)); //8,14,18
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
        Debug.Log($"Resetting UI");
        ToggleMineralText(false);
        ToggleHPText(false);
        UpdateCreateFleetCostText();
        UpdateCreditTotal();
        SetModuleGrid();
        ClearMovementPath();
        ClearMovementRange();
        SelectedNode = null;
        SelectedUnit = null;
        SelectedPath = null;
        ViewAsteroidInformation(false);
        ViewUnitInformation(false);
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
            if (GridManager.i.GetNeighbors(unitMoving.currentPathNode).Contains(node))
            {
                bool blockedMovement = false;
                unitMoving.subtractMovement(GridManager.i.GetGCost(node, unitMoving.teamId));
                //if ally, and theres room after, move through.
                if (node.structureOnPath != null && node.structureOnPath.teamId == unitMoving.teamId)
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
                            else if (path[j].structureOnPath != null && path[j].structureOnPath.teamId != MyStation.teamId)
                            {
                                break;
                            }
                            //If next spot after ally is ally, keep looking
                        }
                    }
                }
                if (unitMoving.movement < 0)
                {
                    blockedMovement = true;
                }
                var toRot = GetDirection(unitMoving, node);
                var unitImage = unitMoving.transform.Find("Unit"); 
                float elapsedTime = 0f;
                float totalTime = .6f;
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
                    if (!node.isAsteroid)
                    {
                        CheckDestroyAsteroidModules(unitMoving);
                    }
                    blockedMovement = node.isAsteroid;
                }
                //if enemy, attack, if you didn't destroy them, stay blocked and move back
                if (unitMoving.movement >= 0 && node.structureOnPath != null && node.structureOnPath.teamId != unitMoving.teamId)
                {
                    yield return StartCoroutine(FightEnemyUnit(unitMoving, node));
                    if (unitMoving == null || !AllUnits.Contains(unitMoving))
                        break;
                    blockedMovement = node.structureOnPath != null && AllUnits.Contains(node.structureOnPath);
                }
                if (blockedMovement)
                {
                    elapsedTime = 0f;
                    totalTime = .4f;
                    while (elapsedTime <= totalTime)
                    {
                        unitMoving.transform.position = Vector3.Lerp(node.transform.position, unitMoving.currentPathNode.transform.position, elapsedTime / totalTime);
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    unitMoving.hasMoved = true;
                    unitMoving.currentPathNode = node;
                    if (node.structureOnPath != null && node.structureOnPath.unitGuid != unitMoving.unitGuid && node.structureOnPath.teamId == unitMoving.teamId)
                    {
                        node.structureOnPath.RegenHP(1);
                    }
                }
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
            else
            {
                Debug.Log("Next Hex was not a neighbor");
            }
        }
        if (unitMoving != null && AllUnits.Contains(unitMoving))
        {
            unitMoving.transform.position = unitMoving.currentPathNode.transform.position;
            unitMoving.currentPathNode.structureOnPath = unitMoving;
        }
    }

    internal IEnumerator FightEnemyUnit(Unit unitMoving, PathNode node)
    {
        var unitOnPath = node.structureOnPath;
        Debug.Log($"{unitMoving.unitName} is attacking {unitOnPath.unitName}");
        CheckEnterCombatModules(unitMoving);
        CheckEnterCombatModules(unitOnPath);
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
            if (supportFleet.teamId == unitMoving.teamId)
            {
                s1sKinetic += Convert.ToInt32(Math.Floor(supportFleet.kineticPower * supportFleet.supportValue));
                s1sThermal += Convert.ToInt32(Math.Floor(supportFleet.thermalPower * supportFleet.supportValue));
                s1sExplosive += Convert.ToInt32(Math.Floor(supportFleet.explosivePower * supportFleet.supportValue));
            }
            else if (supportFleet.teamId == unitOnPath.teamId)
            {
                s2sKinetic += Convert.ToInt32(Math.Floor(supportFleet.kineticPower * supportFleet.supportValue));
                s2sThermal += Convert.ToInt32(Math.Floor(supportFleet.thermalPower * supportFleet.supportValue));
                s2sExplosive += Convert.ToInt32(Math.Floor(supportFleet.explosivePower * supportFleet.supportValue));
            }
        }
        var beforeText = turnValue.text;
        phaseText.gameObject.SetActive(true);
        for (int attackType = 0; attackType <= (int)AttackType.Explosive; attackType++)
        {
            if (unitMoving.HP > 0 && unitOnPath.HP > 0)
            {
                turnValue.text = DoCombat(unitMoving, unitOnPath, (AttackType)attackType, s1sKinetic, s1sThermal, s1sExplosive, s2sKinetic, s2sThermal, s2sExplosive);
                yield return StartCoroutine(WaitforSecondsOrTap(1));
            }
        }
        phaseText.gameObject.SetActive(false);
        turnValue.text = beforeText;
        unitOnPath.inCombatIcon.SetActive(false);
        unitMoving.selectIcon.SetActive(false);
        if (unitMoving.HP <= 0)
        {
            Debug.Log($"{unitOnPath.unitName} destroyed {unitMoving.unitName}");
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
        if (unitOnPath.HP <= 0)
        {
            Debug.Log($"{unitMoving.unitName} destroyed {unitOnPath.unitName}");
            if (unitOnPath is Station)
            {
                var station = (unitOnPath as Station);
                station.defeated = true;
                while (station.fleets.Count > 0) { AllUnits.Remove(station.fleets[0]); Destroy(station.fleets[0].gameObject); station.fleets.RemoveAt(0); }
            }
            else if (unitOnPath is Fleet)
            {
                if (unitOnPath.moduleEffects.Contains(ModuleEffect.SelfDestruct))
                {
                    Stations[unitMoving.stationId].fleets.Remove(unitMoving as Fleet);
                    AllUnits.Remove(unitMoving);
                    unitMoving.currentPathNode.structureOnPath = null;
                    Destroy(unitMoving.gameObject);
                }
                Stations[unitOnPath.stationId].fleets.Remove(unitOnPath as Fleet);
            }
            AllUnits.Remove(unitOnPath);
            node.structureOnPath = null;
            Destroy(unitOnPath.gameObject); //they are dead
        }
        else
        {
            Debug.Log($"{unitMoving.unitName} movement was blocked by {unitOnPath.unitName}");
        }
    }

    private void CheckEnterCombatModules(Unit unit)
    {
        if (unit.moduleEffects.Contains(ModuleEffect.CombatHeal3))
        {
            unit.RegenHP(3);
        }
        if (unit.moduleEffects.Contains(ModuleEffect.CombatKinetic1))
        {
            unit.kineticPower++;
        }  
        if (unit.moduleEffects.Contains(ModuleEffect.CombatThermal1))
        {
            unit.thermalPower++;
        }
        if (unit.moduleEffects.Contains(ModuleEffect.CombatExplosive1))
        {
            unit.explosivePower++;
        }
    }
    private void CheckDestroyAsteroidModules(Unit unit)
    {
        if (unit.moduleEffects.Contains(ModuleEffect.AsteroidHP2))
        {
            unit.maxHP += 2;
            unit.HP += 2;
        }
        if (unit.moduleEffects.Contains(ModuleEffect.AsteroidMining1))
        {
            unit.mining++;
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
        int s1Dmg = (s2.explosivePower+s2sExplosive) - (s1.explosivePower+s1sExplosive);
        int s2Dmg = (s1.explosivePower+s1sExplosive) - (s2.explosivePower+s2sExplosive);
        int s1Amr = s1.explosiveDamageModifier;
        int s2Amr = s2.explosiveDamageModifier;
        string support1Text = (s1sExplosive > 0 ? $"({s1.explosivePower} base +{s1sExplosive} from support)" : "");
        string support2Text = (s2sExplosive > 0 ? $"({s2.explosivePower} base +{s2sExplosive} from support)" : "");
        string power1Text = $"{s1.explosivePower + s1sExplosive}";
        string power2Text = $"{s2.explosivePower + s2sExplosive}";

        if (type == AttackType.Kinetic)
        {
            s1Dmg = (s2.kineticPower+s2sKinetic) - (s1.kineticPower+s1sKinetic);
            s2Dmg = (s1.kineticPower+s1sKinetic) - (s2.kineticPower+s2sKinetic);
            s1Amr = s1.kineticDamageModifier;
            s2Amr = s2.kineticDamageModifier;
            support1Text = (s1sKinetic > 0 ? $"({s1.kineticPower} base +{s1sKinetic} from support)" : "");
            support2Text = (s2sKinetic > 0 ? $"({s2.kineticPower} base +{s2sKinetic} from support)" : "");
            power1Text = $"{s1.kineticPower + s1sKinetic}";
            power2Text = $"{s2.kineticPower + s2sKinetic}";
        }
        else if (type == AttackType.Thermal)
        {
            s1Dmg = (s2.thermalPower+s2sThermal) - (s1.thermalPower+s1sThermal);
            s2Dmg = (s1.thermalPower+s1sThermal) - (s2.thermalPower+s2sThermal);
            s1Amr = s1.thermalDamageModifier;
            s2Amr = s2.thermalDamageModifier;
            support1Text = (s1sThermal > 0 ? $"({s1.thermalPower} base +{s1sThermal} from support)" : "");
            support2Text = (s2sThermal > 0 ? $"({s2.thermalPower} base +{s2sThermal} from support)" : "");
            power1Text = $"{s1.thermalPower + s1sThermal}";
            power2Text = $"{s2.thermalPower + s2sThermal}";
        }
        var returnText = "";
        if (s1Dmg > 0)
        {
            var damage = Mathf.Max(s1Dmg - s1Amr, 0);
            s1.TakeDamage(damage, s2.moduleEffects.Contains(ModuleEffect.ReduceMaxHp));
            returnText += $"<u><b>{s1.color} took {damage} damage</u></b> and has {s1.HP} HP left.";
            if (s1Amr != 0)
            {
                var modifierSymbol = s1Amr > 0 ? "-" : "+";
                returnText += $"\n({s1Dmg} base damage {modifierSymbol}{Mathf.Abs(s1Amr)} from modules)";
            }
        }
        else if (s2Dmg > 0)
        {
            var damage = Mathf.Max(s2Dmg - s2Amr, 0);
            s2.TakeDamage(damage, s1.moduleEffects.Contains(ModuleEffect.ReduceMaxHp));
            returnText += $"<u><b>{s2.color} took {damage} damage</u></b> and has {s2.HP} HP left.";
            if (s2Amr != 0)
            {
                var modifierSymbol = s2Amr > 0 ? "-" : "+";
                returnText += $"\n({s2Dmg} base damage {modifierSymbol}{Mathf.Abs(s2Amr)} from modules)";
            }
        }
        else
        {
            returnText += $"<u><b>Niether unit took damage.</u></b>";
        }
        returnText += $"\n\n{s1.color} had {power1Text} Power{support1Text}.\n{s2.color} had {power2Text} Power{support2Text}.\n";
        phaseText.text = "Phase:\n";
        if (type is AttackType.Kinetic)
        {
            phaseText.text += $"<b>Kinetic</b> Thermal Explosive";
        }
        else if (type is AttackType.Thermal)
        {
            phaseText.text += $"Kinetic <b>Thermal</b> Explosive";
        }
        else
        {
            phaseText.text += $"Kinetic Thermal <b>Explosive</b>";
        }
        return returnText;
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
    private IEnumerator GetTurnsFromServer()
    {
        isWaitingForTurns = true;
        int i = 0;
        do
        {
            if (i == 1)
            {
                var plural = Globals.GameMatch.MaxPlayers > 2 ? "s" : "";
                customAlertText.text = $"Your turn was submitted! \n\n If you change your mind, your last submitted turn will be used.";
                customAlertPanel.SetActive(true);
            }
            yield return StartCoroutine(sql.GetRoutine<GameTurn>($"Game/GetTurns?gameGuid={Globals.GameMatch.GameGuid}&turnNumber={TurnNumber}&quickSearch={i == 0}", CheckForTurns));
            i++;
        }
        while (!Globals.GameMatch.GameTurns.Any(x => x.TurnNumber == TurnNumber));
        customAlertPanel.SetActive(false);
        isEndingTurn = true;
        turnValue.text = $"Turn #{TurnNumber} Complete!";
        turnLabel.SetActive(true);
        yield return StartCoroutine(WaitforSecondsOrTap(1));
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
                while (MyStation.actions.Count < MyStation.maxActions)
                {
                    QueueAction(ActionType.GainCredit);
                }
                List<int> actionOrders = new List<int>();
                for (int i = 0; i <= Stations.Count * 5; i += Stations.Count)
                {
                    for (int j = 0; j < Stations.Count; j++)
                    {
                        int k = (TurnNumber-1 + j) % Stations.Count;
                        if (k == Globals.localStationIndex)
                            actionOrders.Add(i + j+1);
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
                StartCoroutine(DoEndTurn(gameTurn));
            }
            else
            {
                ShowAreYouSurePanel(true);
            }
        }
    }

    private IEnumerator DoEndTurn(GameTurn gameTurn)
    {
        customAlertText.text = "Submitting Turn.";
        customAlertPanel.SetActive(true);
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameTurn);
        yield return StartCoroutine(sql.PostRoutine<bool>($"Game/EndTurn", stringToPost, SubmitResponse));
        if (SubmittedTurn)
        {
            SubmittedTurn = false;
            //turnOrder.Find($"TurnOrder{(Stations.Count-1 - ((TurnNumber - 1 + MyStation.stationId) % Stations.Count) % (Stations.Count - 1))}").GetComponent<Image>().sprite = readyCircle;
            endTurnButton.color = Color.blue;
            if (!isWaitingForTurns)
            {
                yield return StartCoroutine(GetTurnsFromServer());
                var turnFromServer = Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == TurnNumber)?.Players;
                if (turnFromServer != null)
                {
                    ClearModules();
                    ToggleMineralText(true);
                    var serverActions = turnFromServer.SelectMany(x => x.Actions).OrderBy(x => x.ActionOrder);
                    foreach (var serverAction in serverActions)
                    {
                        yield return StartCoroutine(PerformAction(new Action(serverAction)));
                        yield return new WaitForSeconds(.1f);
                    }
                    //turnValue.text = $"Turn #{TurnNumber} over.";
                    foreach (var asteroid in GridManager.i.AllNodes)
                    {
                        asteroid.ReginCredits();
                    }
                    //yield return StartCoroutine(WaitforSecondsOrTap(1));
                    FinishTurns();
                }
            }
            else
            {
                var plural = Globals.GameMatch.MaxPlayers > 2 ? "s" : "";
                customAlertText.text = $"Your new turn was submitted, overriding your previous one.";
                customAlertPanel.SetActive(true);
            }
        }
        else
        {
            customAlertText.text = $"Your turn was NOT submitted successfully. \n\n Please try again.";
            customAlertPanel.SetActive(true);
        }
    }

    private void SubmitResponse(bool response)
    {
        SubmittedTurn = response;
    }

    private void FinishTurns()
    {
        endTurnButton.color = Color.black;
        winner = GridManager.i.CheckForWin();
        UpdateAuctionModules(TurnNumber);
        foreach (var unit in AllUnits)
        {
            if (!unit.hasMoved)
            {
                unit.RegenHP(1);
            }
            unit.hasMoved = false;
        }
        foreach (var station in Stations)
        {
            station.credits += 1+station.fleets.Sum(x=>x.globalCreditGain);
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
            ToggleHPText(true);
            turnValue.text = $"{Stations[winner].color} player won after {TurnNumber} turns";
            Debug.Log($"{Stations[winner].color} player won after {TurnNumber} turns");
            //Add EOG Stats
        }
        else
        {
            StartTurn();
        }
    }

    private IEnumerator PerformAction(Action action)
    {
        Debug.Log($"Perfoming {Stations[action.playerId].color}'s action {action.actionOrder}:\n{GetDescription(action.actionType)}"); 
        turnValue.text = $"{action.selectedUnit.unitName}:\n";
        if (action.selectedUnit != null && AllUnits.Contains(action.selectedUnit))
        {
            var currentUnit = action.selectedUnit;
            if (currentUnit != null)
            {
                var currentStation = Stations[currentUnit.stationId];
                var actionCost = GetCostOfAction(action.actionType, action.selectedUnit, false, action.selectedModule);
                if (currentStation.credits >= actionCost)
                {
                    if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine || action.actionType == ActionType.MineAsteroid)
                    {
                        isMoving = true;
                        if (currentUnit != null && action.selectedPath != null && action.selectedPath.Count > 0 && action.selectedPath.Count <= currentUnit.getMaxMovementRange())
                        {
                            turnTapText.SetActive(false);
                            turnValue.text += $"{GetDescription(action.actionType)}";
                            Debug.Log("Moving to position: " + currentUnit.currentPathNode.transform.position);
                            yield return StartCoroutine(MoveOnPath(currentUnit, action.selectedPath));
                        }
                        else
                        {
                            turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                            Debug.Log("Out of range or no longer exists.");
                        }
                        
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                        isMoving = false;
                        if(currentUnit != null && AllUnits.Contains(currentUnit))
                            currentUnit.resetMovementRange();
                    }
                    else if (action.actionType == ActionType.CreateFleet)
                    {
                        if (IsUnderMaxFleets(currentStation, false))
                        {
                            currentStation.credits -= actionCost;
                            turnValue.text += $"{GetDescription(action.actionType)}";
                            yield return StartCoroutine(GridManager.i.CreateFleet(currentStation, (Guid)action.generatedGuid, false));
                        }
                        else
                        {
                            turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                            Debug.Log($"{currentUnit.unitName} not eligible for {GetDescription(action.actionType)}");
                            yield return StartCoroutine(WaitforSecondsOrTap(1));
                        }
                    }
                    else if (action.actionType == ActionType.UpgradeFleet)
                    {
                        if (CanLevelUp(currentUnit, action.actionType, false))
                        {
                            currentStation.credits -= actionCost;
                            turnValue.text += $"{GetDescription(action.actionType)}";
                            LevelUpUnit(currentUnit);
                        }
                        else
                        {
                            turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                            Debug.Log($"{currentUnit.unitName} not eligible for {GetDescription(action.actionType)}");
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.UpgradeStation)
                    {
                        if (CanLevelUp(currentUnit, action.actionType, false))
                        {
                            currentStation.credits -= actionCost;
                            turnValue.text += $"{GetDescription(action.actionType)}";
                            LevelUpUnit(currentUnit);
                        }
                        else
                        {
                            turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.BidOnModule)
                    {
                        if (action.selectedModule is object)
                        {
                            currentStation.credits -= actionCost;
                            var wonModule = new Module(action.selectedModule.moduleId, action.selectedModule.moduleGuid);
                            turnValue.text += $"Won bid on {wonModule.effectText}.\nPaid {action.selectedModule.currentBid} credits.";
                            currentStation.modules.Add(wonModule);
                        }
                        else
                        {
                            turnValue.text += "Lost bid\nGained 1 credit.";
                            currentStation.credits++;
                        }
                        currentUnit.selectIcon.SetActive(true);
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else if (action.actionType == ActionType.AttachModule)
                    {
                        if (currentStation.modules.Count > 0 && currentUnit.attachedModules.Count <= currentUnit.maxAttachedModules)
                        {
                            Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModule?.moduleGuid);
                            if (selectedModule is object)
                            {
                                turnValue.text += $"{GetDescription(action.actionType)}.\n{selectedModule.effectText}.";
                                currentUnit.attachedModules.Add(selectedModule);
                                currentUnit.EditModule(selectedModule.moduleId);
                                currentStation.modules.Remove(currentStation.modules.FirstOrDefault(x => x.moduleGuid == selectedModule.moduleGuid));
                            }
                            else
                            {
                                turnValue.text += $"Could not perform {GetDescription(action.actionType)}, module not available";
                            }
                            currentUnit.selectIcon.SetActive(true);
                            yield return StartCoroutine(WaitforSecondsOrTap(1));
                            currentUnit.selectIcon.SetActive(false);
                        }
                        else
                        {
                            turnValue.text += $"Could not perform {GetDescription(action.actionType)}, max attached modules";
                        }
                    }
                    else if (action.actionType == ActionType.SwapModule)
                    {
                        if (currentUnit.attachedModules.Count > 0)
                        {
                            Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModule?.moduleGuid);
                            Module dettachedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.generatedGuid);
                            if (selectedModule is object && dettachedModule is object)
                            {
                                turnValue.text += $"{GetDescription(action.actionType)}.\n{selectedModule.effectText}.";
                                //dettach old
                                currentStation.modules.Add(dettachedModule);
                                currentUnit.EditModule(dettachedModule.moduleId, -1);
                                currentUnit.attachedModules.Remove(currentUnit.attachedModules.FirstOrDefault(x => x.moduleGuid == dettachedModule.moduleGuid));
                                //attach new
                                currentUnit.attachedModules.Add(selectedModule);
                                currentUnit.EditModule(selectedModule.moduleId);
                                currentStation.modules.Remove(currentStation.modules.FirstOrDefault(x => x.moduleGuid == selectedModule.moduleGuid));
                            }
                            else
                            {
                                turnValue.text += $"Could not perform {GetDescription(action.actionType)}, module not available";
                            }
                            currentUnit.selectIcon.SetActive(true);
                            yield return StartCoroutine(WaitforSecondsOrTap(1));
                            currentUnit.selectIcon.SetActive(false);
                        }
                        else
                        {
                            turnValue.text += $"Could not perform {GetDescription(action.actionType)}, module not attached";
                        }
                    }
                    else if (action.actionType == ActionType.GainCredit)
                    {
                        currentStation.credits -= actionCost;
                        turnValue.text += $"{GetDescription(action.actionType)}";
                        Debug.Log($"Gained 1 credit");
                        currentUnit.selectIcon.SetActive(true);
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                        currentUnit.selectIcon.SetActive(false);
                    }
                    else
                    {
                        turnValue.text += $"Unknown Error";
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                    }
                }
                else
                {
                    turnValue.text += $"Could not perform {GetDescription(action.actionType)}, could not afford";
                    Debug.Log($"Broke ass {currentUnit.color} bitch couldn't afford {GetDescription(action.actionType)}");
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                }
            }
        }
    }

    internal IEnumerator WaitforSecondsOrTap(float time)
    {
        float elapsedTime = 0f;
        float totalTime = time;
        turnTapText.SetActive(true);
        tapped = false;
        while (!tapped)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public string GetDescription(ActionType value)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            FieldInfo field = type.GetField(name);
            if (field != null)
            {
                DescriptionAttribute attr =
                       Attribute.GetCustomAttribute(field,
                         typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return null;
    }
    private IEnumerator PerformMine(Unit currentUnit, PathNode asteroidToMine)
    {
        Station currentStation = Stations[currentUnit.stationId];
        if (AllUnits.Contains(currentUnit))
        {
            var minedAmount = asteroidToMine.MineCredits(currentUnit.mining);
            currentStation.credits += minedAmount;
            currentUnit.selectIcon.SetActive(true);
            asteroidToMine.ShowMineIcon(true);
            var toRot = GetDirection(currentUnit, asteroidToMine);
            var unitImage = currentUnit.transform.Find("Unit");
            float elapsedTime = 0f;
            float totalTime = 1f;
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
        for (int i = 0; i < 4; i++)
        {
            var orderObj = turnOrder.Find($"TurnOrder{i}").GetComponent<Image>();
            orderObj.sprite = unReadyCircle;
            if (Stations.Count > i)
                orderObj.transform.Find($"Color").GetComponent<Image>().color = GridManager.i.playerColors[(TurnNumber - 1 + i) % Stations.Count];
            else
                orderObj.gameObject.SetActive(false);
        }
        currentCredits = MyStation.credits;
        infoToggle = false;
        ViewModuleMarket(false);
        ResetAfterSelection();
        isEndingTurn = false;
        Debug.Log($"New Turn {TurnNumber} Starting");
    }
#endregion
    private void LevelUpUnit(Unit unit)
    {
        if (CanLevelUp(unit, unit is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet, false))
        {
            unit.level++;
            unit.maxAttachedModules++;
            unit.maxHP += 3;
            unit.HP += 3;
            unit.kineticPower++;
            unit.explosivePower++;
            unit.thermalPower++;
            var spriteRenderer = unit.transform.Find("Unit").GetComponent<SpriteRenderer>();
            if (unit is Station)
            {
                (unit as Station).maxFleets++;
                if (unit.level == 2)
                {
                    spriteRenderer.sprite = GridManager.i.stationlvl2;
                }
                else if (unit.level == 3)
                {
                    spriteRenderer.sprite = GridManager.i.stationlvl3;
                }
                else if (unit.level == 4)
                {
                    spriteRenderer.sprite = GridManager.i.stationlvl4;
                }
            }
            else
            {
                if (unit.level == 2)
                {
                    spriteRenderer.sprite = GridManager.i.fleetlvl2;
                }
                else if (unit.level == 3)
                {
                    spriteRenderer.sprite = GridManager.i.fleetlvl3;
                }
                else if (unit.level == 4)
                {
                    spriteRenderer.sprite = GridManager.i.fleetlvl4;
                }
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
            ActionBar.Find($"Action{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
            if (i < MyStation.maxActions) {
                ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = emptyModuleBar;
                
            } else {
                ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = lockActionBar;
                int k = i-1;
                ActionBar.Find($"Action{i}/Image").GetComponent<Button>().onClick.AddListener(() => ShowUnlockPanel(k));
            }
            ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(false);
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
        for (int i = 0; i < 4; i++)
        {
            UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
            UnitModuleBar.Find($"Module{i}/Remove").GetComponent<Button>().onClick.RemoveAllListeners();
            var maxAttached = GetUnitMaxAttachmentCount(unit);
            if (i < maxAttached)
            {
                if (i < unit.attachedModules.Count)
                {
                   
                    UnitModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(unit.stationId == MyStation.stationId);
                    if (unit.teamId != MyStation.teamId && unit.moduleEffects.Contains(ModuleEffect.HiddenStats) && unit.attachedModules[i].moduleId != 49)
                    {
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = lockModuleBar;
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() => ShowCustomAlertPanel("This units modules are hidden."));
                    }
                    else
                    {
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = unit.attachedModules[i].icon;
                        var module = unit.attachedModules[i];
                        UnitModuleBar.Find($"Module{i}/Remove").GetComponent<Button>().onClick.AddListener(() => ModifyModule(ActionType.SwapModule, module.moduleGuid));
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() => SetModuleInfo(module.effectText));
                    }
                }
                else
                {
                    UnitModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(false);
                    if (unit.stationId == MyStation.stationId)
                    {
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = attachModuleBar;
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() => ModifyModule(ActionType.AttachModule));
                    }
                    else
                    {
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = emptyModuleBar;
                    }
                }
            }
            else
            {
                UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = lockModuleBar;
                UnitModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(false);
            }
        }
    }

    internal List<PathNode> GetPathFromCoords(List<Coords> selectedCoords)
    {
        return selectedCoords.Select(x => GridManager.i.grid[x.x, x.y]).ToList();
    }
}
