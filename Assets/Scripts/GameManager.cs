using Newtonsoft.Json.Linq;
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
    private Transform highlightParent;
    public GameObject turnArchivePrefab;
    public GameObject floatingTextPrefab;
    public GameObject selectPrefab;
    public GameObject pathPrefab;
    public GameObject movementRangePrefab;
    public GameObject movementMinePrefab;
    public GameObject modulePrefab;
    public GameObject auctionPrefab;
    public GameObject techPrefab;
    public GameObject moduleInfoPanel;
    public GameObject actionPanel;
    public GameObject infoPanel;
    public GameObject alertPanel;
    public GameObject customAlertPanel;
    public GameObject areYouSurePanel;
    public GameObject selectModulePanel;
    public GameObject helpPanel;
    public GameObject moduleMarket;
    public GameObject technologyPanel;
    public GameObject asteroidPanel;
    public GameObject turnArchivePanel;
    public GameObject loadingPanel;
    public GameObject exitPanel;
    public GameObject forfietPanel;

    private List<GameObject> currentPathObjects = new List<GameObject>();
    private List<PathNode> SelectedPath;

    public Transform ActionBar;
    public Transform UnitModuleBar;
    public Transform ModuleGrid;
    public Transform SelectedModuleGrid;
    public Transform turnOrder;
    public Transform TurnArchiveList;

    public Sprite lockActionBar;
    public Sprite lockModuleBar;
    public Sprite attachModuleBar;
    public Sprite emptyModuleBar;
    public Sprite unReadyCircle;
    public Sprite readyCircle;

    private PathNode SelectedNode;
    private Unit SelectedUnit;
    private List<Node> currentMovementRange = new List<Node>();
    private List<GameObject> currentModules = new List<GameObject>();
    private List<GameObject> currentModulesForSelection = new List<GameObject>();
    internal List<Station> Stations = new List<Station>();

    private bool tapped = false;
    private bool isMoving = false;
    private bool isEndingTurn = false;
    private Button upgradeButton;
    public Button createFleetButton;
    public Button repairFleetButton;
    public Button generateModuleButton;
    public Button previousTurnButton;
    public Image endTurnButton;
    public Sprite endTurnButtonNotPressed;
    public Sprite endTurnButtonPressed;

    private TextMeshProUGUI createFleetCost;
    private TextMeshProUGUI upgradeCost;
    private TextMeshProUGUI nameValue;
    private TextMeshProUGUI levelValue;
    private TextMeshProUGUI HPValue;
    private TextMeshProUGUI miningValue;
    private TextMeshProUGUI rangeValue;
    private TextMeshProUGUI KineticValueText;
    private TextMeshProUGUI ThermalValueText;
    private TextMeshProUGUI ExplosiveValueText;
    private TextMeshProUGUI ModuleEffectText;
    internal TextMeshProUGUI turnValue;
    private TextMeshProUGUI phaseText;
    private TextMeshProUGUI moduleInfoValue;
    private TextMeshProUGUI alertText;
    private TextMeshProUGUI customAlertText;
    private TextMeshProUGUI creditsText;
    private TextMeshProUGUI hexesOwnedText;
    public TextMeshProUGUI ColorText;

    public GameObject turnTapText;
    private Image moduleInfoIcon;

    internal List<ActionType> TechActions = new List<ActionType>();
    internal List<Unit> AllUnits = new List<Unit>();
    internal List<Module> AllModules = new List<Module>();
    internal List<Module> AuctionModules = new List<Module>();
    internal List<GameObject> AuctionObjects = new List<GameObject>();
    internal List<GameObject> TechnologyObjects = new List<GameObject>();
    internal List<GameObject> TurnArchiveObjects = new List<GameObject>();
    internal int Winner = -1;
    internal Station MyStation {get {return Stations[Globals.localStationIndex];}}
    public GameObject turnLabel;
    private SqlManager sql;
    internal int TurnNumber = 0;
    private bool infoToggle = false;
    private int helpPageNumber = 0;
    private bool isWaitingForTurns = false;
    private bool SubmittedTurn = false;
    private List<Action> lastSubmittedTurn = new List<Action>();
    List<Tuple<string,string>> turnArchive = new List<Tuple<string, string>>();
    //Got game icons from https://game-icons.net/
    private void Awake()
    {
        i = this;
        sql = new SqlManager();
#if UNITY_EDITOR
        if(!Globals.HasBeenToLobby)
            SceneManager.LoadScene((int)Scene.Lobby);
#endif
        loadingPanel.SetActive(true);
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
        ColorText.text = $"You're {MyStation.color}";
        ColorText.color = GridManager.i.playerColors[MyStation.stationId];
        var currentGameTurn = Globals.GameMatch.GameTurns.Where(x => x.Players.All(y => y != null)).Last();
        for (int i = Constants.MinTech; i <= Constants.MaxTech; i++)
        {
            TechActions.Add((ActionType)i);
        }
        yield return new WaitForSeconds(.25f);
        loadingPanel.SetActive(false);
        if (Globals.GameMatch.GameTurns.Count() > 1 && TurnNumber > 0) {
            previousTurnButton.interactable = true;
            yield return StartCoroutine(DoEndTurn());
            if (Globals.GameMatch.GameTurns.Count() > TurnNumber)
            {
                if (Globals.GameMatch.GameTurns[TurnNumber]?.Players[Globals.localStationIndex]?.Actions != null)
                {
                    foreach (var serverAction in Globals.GameMatch.GameTurns[TurnNumber]?.Players[Globals.localStationIndex]?.Actions)
                    {
                        QueueAction(new Action(serverAction), true);
                    }
                    yield return StartCoroutine(CheckEndTurn());
                }
            }
        } else {
            StartTurn();
        }
        StartCoroutine(GetIsTurnReady());
    }

    private IEnumerator GetIsTurnReady()
    {
        while (!Globals.IsCPUGame)
        {
            while (!isEndingTurn)
            {
                yield return StartCoroutine(sql.GetRoutine<GameTurn>($"Game/HasTakenTurn?gameGuid={Globals.GameMatch.GameGuid}&turnNumber={TurnNumber}", UpdateGameTurnStatus));
                yield return new WaitForSeconds(.5f);
            }
            yield return new WaitForSeconds(1f);
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
        KineticValueText = infoPanel.transform.Find("KineticValue").GetComponent<TextMeshProUGUI>();
        ThermalValueText = infoPanel.transform.Find("ThermalValue").GetComponent<TextMeshProUGUI>();
        ExplosiveValueText = infoPanel.transform.Find("ExplosiveValue").GetComponent<TextMeshProUGUI>();
        ModuleEffectText = infoPanel.transform.Find("DamageTakenValue").GetComponent<TextMeshProUGUI>();
        creditsText = actionPanel.transform.Find("Credits").GetComponent<TextMeshProUGUI>();
        hexesOwnedText = actionPanel.transform.Find("HexesOwned").GetComponent<TextMeshProUGUI>();
        turnValue = turnLabel.transform.Find("TurnValue").GetComponent<TextMeshProUGUI>();
        phaseText = turnLabel.transform.Find("PhaseText").GetComponent<TextMeshProUGUI>();
        moduleInfoValue = moduleInfoPanel.transform.Find("ModuleInfoText").GetComponent<TextMeshProUGUI>();
        moduleInfoIcon = moduleInfoPanel.transform.Find("Image").GetComponent<Image>();
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

            KineticValueText.text = "?";
            ThermalValueText.text = "?";
            ExplosiveValueText.text = "?";

            ModuleEffectText.text = "?";
            miningValue.text = "?";
        }
        else
        {
            HPValue.text = Mathf.Min(unit.maxHP, unit.HP) + "/" + unit.maxHP;
            rangeValue.text = unit.maxMovement.ToString();
            ModuleEffectText.text = "No Modules Installed";
            if (unit.attachedModules.Count > 0)
            {
                ModuleEffectText.text = unit.attachedModules[0].effectText.Replace(" \n", ",");
                for (int i = 1; i < unit.attachedModules.Count; i++)
                {
                    ModuleEffectText.text += $"\n {unit.attachedModules[i].effectText.Replace(" \n", ",")}";
                }
            }
            miningValue.text = unit.maxMining.ToString();
        }
        var supportingFleets = GridManager.i.GetNeighbors(unit.currentPathNode).Select(x => x.unitOnPath).Where(x => x != null && x.teamId == unit.teamId);
        var kineticPower = $"{unit.kineticPower}";
        var thermalPower = $"{unit.thermalPower}";
        var explosivePower = $"{unit.explosivePower}";
        if (supportingFleets.Any())
        {
            int kineticSupportValue = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.kineticPower * x.supportValue)));
            int thermalSupportValue = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.thermalPower * x.supportValue)));
            int explosiveSupportValue = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.explosivePower * x.supportValue)));
            kineticPower += kineticSupportValue > 0 ? $" (+{kineticSupportValue})" : "";
            thermalPower += thermalSupportValue > 0 ? $" (+{thermalSupportValue})" : "";
            explosivePower += explosiveSupportValue > 0 ? $" (+{explosiveSupportValue})" : "";
        }
        KineticValueText.text = $"{kineticPower}"; 
        ThermalValueText.text = $"{thermalPower}";
        ExplosiveValueText.text = $"{explosivePower}";
        var actionType = unit is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        upgradeButton.interactable = true;
        upgradeButton.gameObject.SetActive(false);
        createFleetButton.gameObject.SetActive(false);
        repairFleetButton.gameObject.SetActive(false);
        UpdateCreditsAndHexesText(Stations[unit.stationId]);
        if (unit.stationId == MyStation.stationId)
        {
            if (CanLevelUp(unit, actionType, true))
            {
                var cost = GetCostOfAction(actionType, unit, true);
                upgradeCost.text = GetCostText(cost);
                upgradeButton.interactable = MyStation.credits >= cost;
            }
            else
            {
                upgradeCost.text = "(Technology Required)";
            }
            upgradeButton.gameObject.SetActive(true);
            if (unit is Station)
            {
                createFleetButton.gameObject.SetActive(true);
            }
            else
            {
                repairFleetButton.gameObject.SetActive(true);
            }
            ToggleMineralText(true);
        }
        ToggleHPText(true);
        SetModuleBar(unit);
        infoPanel.gameObject.SetActive(true);
    }
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            tapped = true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);
            if (!isEndingTurn && hit.collider != null && !isMoving)
            {
                PathNode targetNode = hit.collider.GetComponent<PathNode>();
                if (targetNode != null)
                {
                    moduleMarket.SetActive(false); 
                    technologyPanel.SetActive(false); 
                    Unit targetUnit = null;
                    //Check if you clicked on unit
                    if (targetNode.unitOnPath != null)
                    {
                        targetUnit = targetNode.unitOnPath;
                    }
                    //Original click on unit
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
                    //Double click confirm to movement early
                    else if (SelectedUnit != null && SelectedNode != null && targetNode == SelectedNode && SelectedPath != null && !HasQueuedMovement(SelectedUnit))
                    {
                        QueueAction(new Action(ActionType.MoveUnit, SelectedUnit, null, 0, SelectedPath));
                    }
                    //Create Movement
                    else if (SelectedUnit != null && targetNode != null && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode) && !HasQueuedMovement(SelectedUnit))
                    {
                        SelectedNode = targetNode;
                        if (SelectedPath == null || SelectedPath.Count == 0)
                        {
                            SelectedUnit.currentPathNode.parent = null;
                            SelectedPath = GridManager.i.FindPath(SelectedUnit.currentPathNode, SelectedNode, SelectedUnit);
                            Debug.Log($"Path created for {SelectedUnit.unitName}");
                        }
                        else
                        {
                            SelectedPath.AddRange(GridManager.i.FindPath(SelectedPath.Last(), SelectedNode, SelectedUnit));
                            Debug.Log($"Path edited for {SelectedUnit.unitName}");
                        }
                        SelectedUnit.AddMinedPath(SelectedPath);
                        SelectedUnit.subtractMovement(SelectedPath.Last().gCost);
                        DrawPath(SelectedPath);
                        HighlightRangeOfMovement(SelectedNode, SelectedUnit);
                    }
                    //Clicked on invalid tile, clear
                    else
                    {
                        DeselectUnitIcons();
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
                            else if (targetNode.isRift)
                            {
                                ShowCustomAlertPanel("This toxic cloud of gas is impassable terrain.");
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
                else
                {
                    DeselectMovement();
                }
            }
        }
    }

    private void SetAsteroidTextValues(PathNode targetNode)
    {
        asteroidPanel.transform.Find("CurrentValue").GetComponent<TextMeshProUGUI>().text = $"Current Value: {targetNode.minerals}";
        asteroidPanel.transform.Find("MaxValue").GetComponent<TextMeshProUGUI>().text = $"Max Value: {targetNode.maxCredits}";
        asteroidPanel.transform.Find("RegenValue").GetComponent<TextMeshProUGUI>().text = $"Regen Per Turn: {targetNode.creditRegin}";
    }
    //ViewQueuedAction
    //ClickOnAction
    public void ShowQueuedAction(int i)
    {
        DeselectUnitIcons();
        var action = MyStation.actions[i];
        action.selectedUnit.selectIcon.SetActive(true);
        if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine)
        {
            HighlightQueuedMovement(action);
        }
        else if (action.actionType == ActionType.UpgradeFleet || action.actionType == ActionType.UpgradeStation)
        {
            var extraBonus = action.selectedUnit.level % 2 == 0 ? "+1 Movement" : "+2 Mining Power";
            var message = $"{action.selectedUnit.unitName} will gain:\n+3 Max HP\n+1 Kinetic, Thermal, and Explosive Power\n{extraBonus}";
            //if (action.actionType == ActionType.UpgradeStation)
            //    message += "\n+1 Credit per turn.";
            ShowCustomAlertPanel(message);
        }
        else if (action.actionType == ActionType.BidOnModule || action.actionType == ActionType.SwapModule || action.actionType == ActionType.AttachModule)
        {
            SetModuleInfo(action.selectedModule);
        }
        else
        {
            SetUnitTextValues(action.selectedUnit);
            ViewUnitInformation(true);
        }
    }
    private void HighlightQueuedMovement(Action movementAction)
    {
        SelectedUnit = movementAction.selectedUnit;
        SelectedUnit.subtractMovement(99);
        SetUnitTextValues(SelectedUnit);
        ViewUnitInformation(true);
        DrawPath(movementAction.selectedPath);
        Debug.Log($"{SelectedUnit.unitName} already has a pending movement action.");
        HighlightRangeOfMovement(movementAction.selectedPath.LastOrDefault(), SelectedUnit);
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
            SelectedUnit.ClearMinedPath(SelectedPath);
            SelectedUnit.selectIcon.SetActive(false);
        }
        ResetAfterSelection();
    }
    public void ViewTechnology(bool active)
    {
        technologyPanel.SetActive(active);
        if (active)
        {
            DeselectMovement();
            ClearTechnologyObjects();
            for (int j = 0; j < MyStation.technology.Count; j++)
            {
                int i = j;
                var tech = MyStation.technology[i];
                var canQueue = true;
                switch ((ResearchType)i)
                {
                    case ResearchType.ResearchFleetLvl:
                        canQueue = tech.level < MyStation.technology[(int)ResearchType.ResearchStationLvl].level;
                        break;
                    case ResearchType.ResearchMaxFleets:
                        canQueue = tech.level < MyStation.technology[(int)ResearchType.ResearchFleetLvl].level;
                        break;
                    case ResearchType.ResearchHP:
                        canQueue = tech.level < MyStation.technology[(int)ResearchType.ResearchFleetLvl].level;
                        break;
                    case ResearchType.ResearchMining:
                        canQueue = tech.level < MyStation.technology[(int)ResearchType.ResearchMaxFleets].level;
                        break;
                    //case ResearchType.ResearchKinetic:
                    //    canQueue = tech.level < MyStation.technology[(int)ResearchType.ResearchFleetLvl].level;
                    //    break;
                    //case ResearchType.ResearchThermal:
                    //    canQueue = tech.level < MyStation.technology[(int)ResearchType.ResearchFleetLvl].level;
                    //    break;
                    //case ResearchType.ResearchExplosive:
                    //    canQueue = tech.level < MyStation.technology[(int)ResearchType.ResearchFleetLvl].level;
                    //    break;
                    default: break;
                }
                var techObject = Instantiate(techPrefab, technologyPanel.transform.Find("TechBar"));
                TechnologyObjects.Add(techObject);
                var infoText = (canQueue ? "" : tech.requirementText) + tech.effectText +"."+ tech.currentEffectText;
                techObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => ShowCustomAlertPanel(infoText));
                techObject.transform.Find("AmountNeeded").GetComponent<TextMeshProUGUI>().text = $"{tech.currentAmount}/{tech.neededAmount}";
                var researchButton = techObject.transform.Find("Research").GetComponent<Button>();
                researchButton.interactable = canQueue;
                researchButton.onClick.AddListener(() => Research(i));
                techObject.transform.Find("Queued").gameObject.SetActive(!canQueue);
                techObject.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Actions/{i+Constants.MinTech}");
            }
        }
    }

    private void Research(int i)
    {
        QueueAction(new Action((ActionType)i+Constants.MinTech));
        technologyPanel.SetActive(false);
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
                moduleObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => SetModuleInfo(module));
                var addButton = moduleObject.transform.Find("Add").GetComponent<Button>();
                addButton.onClick.AddListener(() => ModifyBid(true, i));
                addButton.interactable = !hasQueued && MyStation.credits > module.currentBid;
                var subtractButton = moduleObject.transform.Find("Subtract").GetComponent<Button>();
                subtractButton.interactable = !hasQueued && module.minBid < module.currentBid;
                subtractButton.onClick.AddListener(() => ModifyBid(false,i));
                var bidButton = moduleObject.transform.Find("Bid").GetComponent<Button>();
                bidButton.interactable = !hasQueued && MyStation.credits >= module.currentBid;
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
        moduleObj.transform.Find("Add").GetComponent<Button>().interactable = MyStation.credits > module.currentBid;
        moduleObj.transform.Find("Subtract").GetComponent<Button>().interactable = module.minBid < module.currentBid;
        moduleObj.transform.Find("Bid").GetComponent<Button>().interactable = MyStation.credits >= module.currentBid;
    }

    private void BidOn(int index)
    {
        ViewModuleMarket(false);
        QueueAction(new Action(ActionType.BidOnModule, null, AuctionModules[index]));
    }

    public void ViewHelpPanel(bool active)
    {
        helpPageNumber++;
        if (helpPageNumber > 4)
        {
            helpPageNumber = 0;
            helpPanel.SetActive(false);
            helpPanel.transform.Find("Page1").gameObject.SetActive(true);
            helpPanel.transform.Find("Page2").gameObject.SetActive(false);
            helpPanel.transform.Find("Page3").gameObject.SetActive(false);
            helpPanel.transform.Find("Page4").gameObject.SetActive(false);
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
        else if (MyStation.stationId != SelectedUnit.stationId)
        {
            ShowCustomAlertPanel("This is not your unit!");
        } 
        else 
        {
            var selectedUnit = SelectedUnit;
            DeselectMovement();
            ClearSelectableModules();
            List<Module> availableModules = new List<Module>(MyStation.modules);
            if(selectedUnit.unitGuid != MyStation.unitGuid)
                availableModules.AddRange(MyStation.attachedModules);
            availableModules.AddRange(MyStation.fleets.Where(x=>x.unitGuid != selectedUnit.unitGuid).SelectMany(x=>x.attachedModules).ToList());
            List<Guid?> assignedModules = MyStation.actions.Select(x => x.selectedModule?.moduleGuid).ToList();
            availableModules = availableModules.Where(x => !assignedModules.Contains(x.moduleGuid)).ToList();
            if (availableModules.Count > 0)
            {
                foreach (var module in availableModules)
                {
                    var moduleObject = Instantiate(modulePrefab, SelectedModuleGrid);
                    moduleObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => SetSelectedModule(module, selectedUnit, actionType, dettachGuid));
                    moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
                    moduleObject.transform.Find("Queued").gameObject.SetActive(false);
                    currentModulesForSelection.Add(moduleObject);
                }
                ViewModuleSelection(true);
            }
            else
            {
                ShowCustomAlertPanel("No modules available to attach.");
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
            QueueAction(new Action(action, unit, module, 0, null, dettachGuid));
            ClearSelectableModules();
        }
    }
    public void CreateFleet()
    {
        if (IsUnderMaxFleets(MyStation, true))
            QueueAction(new Action(ActionType.CreateFleet, null, null, 0, null, Guid.NewGuid()));
        else
            ViewTechnology(true);
    }

    public void UpgradeStructure()
    {
        var actionType = SelectedUnit is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        if (CanLevelUp(SelectedUnit, actionType, true)){
            QueueAction(new Action(actionType, SelectedUnit));
        } else {
            ViewTechnology(true);
        }
    }

    public void CancelAction(int slot)
    {
        CancelAction(slot, false);
    }
    internal void CancelAction(int slot, bool isRequeuing)
    {
        var action = MyStation.actions[slot];
        Debug.Log($"{MyStation.unitName} removed action {GetDescription(action.actionType)} from queue");
        if (action != null)
        {
            PerformUpdates(action, Constants.Remove, true);           
            ClearActionBar();
            MyStation.actions.RemoveAt(slot);
            for (int i = 0; i < MyStation.actions.Count; i++)
            {
                AddActionBarImage(MyStation.actions[i].actionType, i);
            }
            if (!isRequeuing)
                ReQueueActions();
        }
        ResetAfterSelection();
    }
    public void ReQueueActions()
    {
        List<Action> actions = new List<Action>(MyStation.actions);
        for (int i = MyStation.actions.Count() - 1; i >= 0; i--)
        {
            CancelAction(i, true);
        }
        foreach (var action in actions)
        {
            QueueAction(action, true);
        }
    }
    private void QueueAction(Action action, bool requeing = false)
    {
        if (action.actionType == ActionType.AttachModule && action.selectedUnit.attachedModules.Count >= action.selectedUnit.maxAttachedModules)
        {
            if (!requeing) ShowCustomAlertPanel("Unit already has maximum attached modules.");
        }
        else if ((action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine) && action.selectedPath.Count > action.selectedUnit.getMaxMovementRange())
        {
            if (!requeing) ShowCustomAlertPanel("The selected hex is out of range.");
        }
        else if ((action.actionType == ActionType.UpgradeFleet || action.actionType == ActionType.UpgradeStation) && !CanLevelUp(action.selectedUnit, action.actionType, true))
        {
            if (!requeing) ShowCustomAlertPanel("Unit is already max level, or has it's max level queued.");
        }
        else if (action.actionType == ActionType.CreateFleet && !IsUnderMaxFleets(MyStation, true))
        {
            if (!requeing) ShowCustomAlertPanel("Can not create new fleet, you will hit max fleets for your station level");
        }
        else if (MyStation.actions.Count >= MyStation.maxActions)
        {
            if(!requeing) ShowCustomAlertPanel($"No action slots available to queue action: {GetDescription(action.actionType)}.");
        } 
        else 
        {
            action.selectedUnit = action.selectedUnit ?? SelectedUnit ?? MyStation;
            action.selectedPath = SelectedPath ?? action.selectedPath;
            if (action.actionType == ActionType.MoveUnit && (action.selectedPath.Any(x => x.isAsteroid) || action.selectedUnit._minedPath.Count > 0))
            {
                action.actionType = ActionType.MoveAndMine;
            }
            if (action.actionType == ActionType.MoveAndMine) {
                var currentNode = action.selectedUnit.currentPathNode;
                foreach (var nextNode in action.selectedPath) {
                    nextNode.parent = currentNode;
                    if (!GridManager.i.GetNeighbors(currentNode, currentNode.minerals > action.selectedUnit.miningLeft).Contains(nextNode))
                    {
                        Debug.Log("Movement no longer valid");
                        return;
                    }
                    currentNode = nextNode;
                }
            }
            var costOfAction = GetCostOfAction(action.actionType, action.selectedUnit, true, action.selectedModule);
            if (costOfAction > MyStation.credits)
            {
                if(!requeing)
                    ShowCustomAlertPanel($"Can not afford to queue action: {GetDescription(action.actionType)}.");
            }
            else
            {
                Debug.Log($"{MyStation.unitName} queuing up action {GetDescription(action.actionType)}");
                action.costOfAction = costOfAction;
                MyStation.actions.Add(action);
                AddActionBarImage(action.actionType, MyStation.actions.Count()-1);
                PerformUpdates(action, Constants.Create, true);
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
        foreach (var unit in AllUnits)
        {
            unit.ShowHPText(value);
        }
    }
    private void DeselectUnitIcons()
    {
        foreach (var unit in AllUnits)
        {
            unit.selectIcon.SetActive(false);
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
        createFleetButton.interactable = true;
        if (IsUnderMaxFleets(MyStation, true))
        {
            var cost = GetCostOfAction(ActionType.CreateFleet, MyStation, true);
            createFleetCost.text = GetCostText(cost);
            createFleetButton.interactable = (MyStation.credits >= cost);
        }
        else
        {
            createFleetCost.text = "(Technology Required)";
        }
    }

    private bool CanLevelUp(Unit structure, ActionType actionType, bool countQueue)
    {
        var station = Stations[structure.stationId];
        var currentLevel = structure.level;
        var maxFleetLvl = station.technology[(int)ResearchType.ResearchFleetLvl].level;
        var maxStationLvl = station.technology[(int)ResearchType.ResearchStationLvl].level;
        if (countQueue) {
            currentLevel += Stations[structure.stationId].actions.Count(x => x.selectedUnit.unitGuid == structure.unitGuid && x.actionType == actionType);
            maxFleetLvl = station.technology[(int)ResearchType.ResearchFleetLvl].level;
            maxStationLvl = station.technology[(int)ResearchType.ResearchStationLvl].level;
        } 
        if (actionType == ActionType.UpgradeFleet)
            return currentLevel <= maxFleetLvl;
        else
            return currentLevel <= maxStationLvl;
    }

    private bool IsUnderMaxFleets(Station station, bool countQueue)
    {
        var currentFleets = station.fleets.Count;
        var maxNumFleets = station.technology[(int)ResearchType.ResearchMaxFleets].level + 1;
        if (countQueue)
        {
            currentFleets += station.actions.Count(x => x.actionType == ActionType.CreateFleet);
            maxNumFleets = station.technology[(int)ResearchType.ResearchMaxFleets].level + 1;
        }
        return currentFleets < maxNumFleets;
    }
    private int GetCostOfAction(ActionType actionType, Unit structure, bool countQueue, Module auctionModule = null)
    {
        var station = Stations[structure.stationId];
        var countingQueue = countQueue ? station.actions.Where(x => x.actionType == actionType && x.selectedUnit.unitGuid == structure.unitGuid).Count() : 0;
        if (actionType == ActionType.CreateFleet)
        {
            if (station.fleets.Count + countingQueue <= 0)
                return 0;
            return (station.fleets.Count + countingQueue) * (7 - (station.fleets.Count + countingQueue)); //6,10,12
        }
        else if (actionType == ActionType.UpgradeFleet)
        {
            return (structure.level + countingQueue) * (9 - (structure.level + countingQueue)); //8,14,18
        }
        else if (actionType == ActionType.UpgradeStation)
        {
            return ((structure.level + 1 + countingQueue) * 5); //10,15,20
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
        DeselectUnitIcons();
        UpdateCreateFleetCostText();
        UpdateCreditsAndHexesText(MyStation);
        SetModuleGrid();
        ClearMovementPath();
        ClearMovementRange();
        SelectedNode = null;
        SelectedUnit = null;
        SelectedPath = null;
        ViewAsteroidInformation(false);
        ViewUnitInformation(false);
    }

    private void UpdateCreditsAndHexesText(Station station)
    {
        creditsText.text = $"Player Credits: {station.credits}";
        hexesOwnedText.text = $"Hexes Owned: {station.score}/{GridManager.i.scoreToWin}";
    }

    private IEnumerator MoveOnPath(Unit unitMoving, List<PathNode> path)
    {
        int i = 0;
        unitMoving.currentPathNode.unitOnPath = null;
        ClearMovementRange();
        unitMoving.resetMovementRange();
        var beforeText = turnValue.text;
        bool didFightLastMove = false;
        PathNode currentNode = unitMoving.currentPathNode;
        tapped = false;
        foreach (var nextNode in path)
        {
            i++;
            didFightLastMove = false;
            unitMoving.subtractMovement(GridManager.i.GetGCost(nextNode));
            //Check that no one hacked their client to send back bad results, currently not full working and stopping real moves, todo come back to later
            //if (GridManager.i.GetNeighbors(currentNode, currentNode.minerals > unitMoving.miningLeft).Contains(nextNode) && unitMoving.movementLeft >= 0)
            //{
                turnValue.text = beforeText;
                unitMoving.hasMoved = true; 
                bool blockedMovement = false;
                bool isValidHex = true;
                //Move and turn toward node
                yield return StartCoroutine(MoveUnit(unitMoving, currentNode, nextNode, true));
                //Asteroid on Path
                if (nextNode.isAsteroid)
                {
                    if (AllUnits.Contains(unitMoving))
                    {
                        var minedAmount = nextNode.MineCredits(unitMoving, false);
                    }
                    if (!nextNode.isAsteroid)
                    {
                        yield return StartCoroutine(MoveDirectly(unitMoving, nextNode.transform.position, Constants.MovementSpeed/2));
                        CheckDestroyAsteroidModules(unitMoving);
                    }
                    isValidHex = !nextNode.isAsteroid;
                }
                //Unit on Path
                if (nextNode.unitOnPath != null)
                {
                    isValidHex = false;
                    //if enemy, attack, if you didn't destroy them, stay blocked and move back
                    if (nextNode.unitOnPath.teamId != unitMoving.teamId)
                    {
                        ToggleHPText(true);
                        yield return StartCoroutine(FightEnemyUnit(unitMoving, nextNode));
                        ToggleHPText(false);
                        didFightLastMove = true;
                        if (unitMoving == null || !AllUnits.Contains(unitMoving))
                            break;
                        blockedMovement = nextNode.unitOnPath != null && AllUnits.Contains(nextNode.unitOnPath);
                        isValidHex = nextNode.unitOnPath == null;
                        if(!blockedMovement && isValidHex)
                            yield return StartCoroutine(MoveDirectly(unitMoving, nextNode.transform.position, Constants.MovementSpeed / 2));
                        if (i != path.Count() && !blockedMovement)
                            turnValue.text = beforeText;
                    }
                    //if ally reapir, not yourself
                    else if (nextNode.unitOnPath.teamId == unitMoving.teamId && nextNode.unitOnPath.unitGuid != unitMoving.unitGuid)
                    {
                        nextNode.unitOnPath.RegenHP(1);
                    } 
                }
                if(isValidHex)
                    unitMoving.currentPathNode = nextNode;
                if (!nextNode.isAsteroid && nextNode.ownedById == -1)
                    unitMoving.currentPathNode.SetNodeColor(unitMoving.stationId);
                if (blockedMovement || (!isValidHex && i == path.Count()))
                {
                    var pathBack = GridManager.i.FindPath(nextNode, unitMoving.currentPathNode, null, false);
                    var lastNode = nextNode;
                    foreach (var p in pathBack)
                    {
                        yield return StartCoroutine(MoveUnit(unitMoving, lastNode, p, false));
                        lastNode = p;
                    }
                    unitMoving.transform.position = unitMoving.currentPathNode.transform.position;
                    break;
                }
                currentNode = nextNode;
            //}
            //else
            //{
            //    turnValue.text = "Next hex was not a neighbor or out of range, movement cancelled";
            //    Debug.Log($"Current: {currentNode.coordsText}, Next: {nextNode.coordsText}");
            //}
        }
        if (unitMoving != null && AllUnits.Contains(unitMoving))
        {
            unitMoving.transform.position = unitMoving.currentPathNode.transform.position;
            unitMoving.currentPathNode.unitOnPath = unitMoving;
            if(GridManager.i.GetNeighbors(unitMoving.currentPathNode).Any(x => x.ownedById == unitMoving.stationId))
                unitMoving.currentPathNode.SetNodeColor(unitMoving.stationId);
        }
        if (!didFightLastMove)
        {
            yield return StartCoroutine(WaitforSecondsOrTap(1));
        }
    }
    IEnumerator MoveUnit(Unit unitMoving, PathNode currentNode, PathNode nextNode, bool checkPath)
    {
        // Determine the direction and detect if wrap-around is needed
        Direction direction;
        var toRot = GetDirection(unitMoving, currentNode, nextNode);
        float elapsedTime = 0f;
        float totalTime = Constants.MovementSpeed;
        Vector3 startPos = unitMoving.transform.position;
        Vector3 endPos = nextNode.transform.position;
        float HexSize = .5f;
        var didTeleport = false;
        bool somethingOnPath = checkPath && (nextNode.isAsteroid || (nextNode.unitOnPath != null && nextNode.unitOnPath.teamId != unitMoving.teamId));
        // Check for wrap-around on the X-axis
        if (Mathf.Abs(currentNode.coords.x - nextNode.coords.x) > 1 && currentNode.coords.y == nextNode.coords.y)
        {
            didTeleport = true;
            direction = (startPos.x < endPos.x) ? Direction.Left : Direction.Right;
            toRot = GetDirection(unitMoving, currentNode, nextNode, direction);
            Vector3 intermediatePosX = new Vector3(
                (startPos.x < endPos.x) ? startPos.x - HexSize : startPos.x + HexSize,
                startPos.y,
                startPos.z);

            // Move to intermediate position
            totalTime = Constants.MovementSpeed / 2;
            while (elapsedTime <= totalTime && !tapped)
            {
                unitMoving.unitImage.rotation = Quaternion.Lerp(unitMoving.unitImage.rotation, toRot, elapsedTime / totalTime);
                unitMoving.transform.position = Vector3.Lerp(startPos, intermediatePosX, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Teleport to the opposite edge
            unitMoving.transform.position = new Vector3(
                (startPos.x < endPos.x) ? endPos.x + HexSize : endPos.x - HexSize,
                endPos.y,
                endPos.z);
        }
        // Check for wrap-around on the Y-axis odd
        else if (Mathf.Abs(currentNode.coords.y - nextNode.coords.y) > 1)
        {
            didTeleport = true;
            if (startPos.x < endPos.x)
            {
                direction = (startPos.y < endPos.y) ? Direction.BottomRight : Direction.TopRight;
            }
            else
            {
                direction = (startPos.y < endPos.y) ? Direction.BottomLeft : Direction.TopLeft;
            }
            toRot = GetDirection(unitMoving, currentNode, nextNode, direction);
            Vector3 intermediatePosY = new Vector3(
                (startPos.x < endPos.x) ? startPos.x + (HexSize / 2) : startPos.x - (HexSize / 2),
                (startPos.y < endPos.y) ? startPos.y - HexSize : startPos.y + HexSize,
                startPos.z);
            // Move to intermediate position
            totalTime = Constants.MovementSpeed / 2;
            while (elapsedTime <= totalTime && !tapped)
            {
                unitMoving.unitImage.rotation = Quaternion.Lerp(unitMoving.unitImage.rotation, toRot, elapsedTime / totalTime);
                unitMoving.transform.position = Vector3.Lerp(startPos, intermediatePosY, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            // Teleport to the opposite edge
            unitMoving.transform.position = new Vector3(
                (startPos.x < endPos.x) ? endPos.x - (HexSize / 2) : endPos.x + (HexSize / 2),
                (startPos.y < endPos.y) ? endPos.y + HexSize : endPos.y - HexSize,
                endPos.z);
        }
        // Check for wrap-around on the Y-axis even
        else if (Mathf.Abs(currentNode.coords.x - nextNode.coords.x) > 1)
        {
            didTeleport = true;
            if (startPos.x < endPos.x)
            {
                direction = (startPos.y < endPos.y) ? Direction.TopLeft : Direction.BottomLeft;
            }
            else
            {
                direction = (startPos.y < endPos.y) ? Direction.TopRight : Direction.BottomRight;
            }
            toRot = GetDirection(unitMoving, currentNode, nextNode, direction);
            Vector3 intermediatePosY = new Vector3(
                (startPos.x < endPos.x) ? startPos.x - (HexSize / 2) : startPos.x + (HexSize / 2),
                (startPos.y < endPos.y) ? startPos.y + HexSize : startPos.y - HexSize,
                startPos.z);
            // Move to intermediate position
            totalTime = Constants.MovementSpeed / 2;
            while (elapsedTime <= totalTime && !tapped)
            {
                unitMoving.unitImage.rotation = Quaternion.Lerp(unitMoving.unitImage.rotation, toRot, elapsedTime / totalTime);
                unitMoving.transform.position = Vector3.Lerp(startPos, intermediatePosY, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            // Teleport to the opposite edge
            unitMoving.transform.position = new Vector3(
                (startPos.x < endPos.x) ? endPos.x + (HexSize / 2) : endPos.x - (HexSize / 2),
                (startPos.y < endPos.y) ? endPos.y - HexSize : endPos.y + HexSize,
                endPos.z);
        }
        else if (somethingOnPath)
        {
            totalTime = Constants.MovementSpeed / 2;
            endPos = (startPos + endPos) / 2;
        }
        if ((didTeleport && !somethingOnPath) || somethingOnPath || !didTeleport)
        {
            yield return StartCoroutine(MoveDirectly(unitMoving, endPos, totalTime, toRot));
        }
    }

    private IEnumerator MoveDirectly(Unit unitMoving, Vector3 endPos, float totalTime, Quaternion? toRot = null)
    {
        float elapsedTime = 0f;
        var startPos = unitMoving.transform.position;
        // Move to final position
        while (elapsedTime <= totalTime && !tapped)
        {
            if(toRot != null)
                unitMoving.unitImage.rotation = Quaternion.Lerp(unitMoving.unitImage.rotation, (Quaternion)toRot, elapsedTime / totalTime);
            unitMoving.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (toRot != null)
            unitMoving.unitImage.rotation = (Quaternion)toRot;
        unitMoving.transform.position = endPos;
    }

    internal IEnumerator FightEnemyUnit(Unit unitMoving, PathNode node)
    {
        var unitOnPath = node.unitOnPath;
        Debug.Log($"{unitMoving.unitName} is attacking {unitOnPath.unitName}");
        CheckEnterCombatModules(unitMoving);
        CheckEnterCombatModules(unitOnPath);
        //unitMoving.selectIcon.SetActive(true);
        //unitOnPath.inCombatIcon.SetActive(true);
        var supportingFleets = GridManager.i.GetNeighbors(node).Select(x => x.unitOnPath).Where(x => x != null && x.unitGuid != unitOnPath.unitGuid);
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
        unitOnPath.inCombatIcon.SetActive(false);
        unitMoving.selectIcon.SetActive(false);
        if (unitMoving.HP <= 0)
        {
            Debug.Log($"{unitOnPath.unitName} destroyed {unitMoving.unitName}");
            if (unitMoving is Station)
            {
                var station = (unitMoving as Station);
                while (station.fleets.Count > 0) { AllUnits.Remove(station.fleets[0]); Destroy(station.fleets[0].gameObject); station.fleets.RemoveAt(0); }
                Winner = unitOnPath.stationId;
            }
            else if (unitMoving is Fleet)
            {
                Stations[unitMoving.stationId].fleets.Remove(unitMoving as Fleet);
            }
            AllUnits.Remove(unitMoving);
            unitMoving.currentPathNode.unitOnPath = null;
            Destroy(unitMoving.gameObject);
        }
        if (unitOnPath.HP <= 0)
        {
            Debug.Log($"{unitMoving.unitName} destroyed {unitOnPath.unitName}");
            if (unitOnPath is Station)
            {
                var station = (unitOnPath as Station);
                while (station.fleets.Count > 0) { AllUnits.Remove(station.fleets[0]); Destroy(station.fleets[0].gameObject); station.fleets.RemoveAt(0); }
                Winner = unitMoving.stationId;
            }
            else if (unitOnPath is Fleet)
            {
                Stations[unitOnPath.stationId].fleets.Remove(unitOnPath as Fleet);
            }
            if (unitMoving is Fleet && unitOnPath.moduleEffects.Contains(ModuleEffect.SelfDestruct))
            {
                Stations[unitMoving.stationId].fleets.Remove(unitMoving as Fleet);
                AllUnits.Remove(unitMoving);
                unitMoving.currentPathNode.unitOnPath = null;
                Destroy(unitMoving.gameObject);
            }
            AllUnits.Remove(unitOnPath);
            node.unitOnPath = null;
            Destroy(unitOnPath.gameObject); //they are dead
        }
        else
        {
            Debug.Log($"{unitMoving.unitName} movement was blocked by {unitOnPath.unitName}");
        }
    }
    internal IEnumerator FloatingTextAnimation(string text, Transform spawnTransform, Unit unit, bool stagger = false)
    {
        if(stagger)
            yield return new WaitForSeconds(.4f);
        var floatingObj = Instantiate(floatingTextPrefab, spawnTransform);
        var floatingTextObj = floatingObj.transform.Find("FloatingText");
        floatingTextObj.GetComponent<TextMeshPro>().text = text;
        floatingTextObj.GetComponent<TextMeshPro>().color = unit != null ? GridManager.i.playerColors[unit.stationId] : Color.white;
        var animationTime = floatingTextObj.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;
        yield return new WaitForSeconds(animationTime);
        Destroy(floatingObj);
    }
    private void CheckEnterCombatModules(Unit unit)
    {
        if (unit.moduleEffects.Contains(ModuleEffect.CombatHeal3))
        {
            unit.RegenHP(3);
        }
        if (unit.moduleEffects.Contains(ModuleEffect.CombatKinetic2))
        {
            unit.kineticPower += 2;
        }  
        if (unit.moduleEffects.Contains(ModuleEffect.CombatThermal2))
        {
            unit.thermalPower += 2;
        }
        if (unit.moduleEffects.Contains(ModuleEffect.CombatExplosive2))
        {
            unit.explosivePower += 2;
        }
    }
    private void CheckDestroyAsteroidModules(Unit unit)
    {
        if (unit.moduleEffects.Contains(ModuleEffect.AsteroidHP5))
        {
            unit.IncreaseMaxHP(5);
        }
        if (unit.moduleEffects.Contains(ModuleEffect.AsteroidCredits3))
        {
            Stations[unit.stationId].GainCredits(3, unit,false,false);
        }
    }

    private Quaternion GetDirection(Unit unit, PathNode fromNode, PathNode toNode, Direction? direction = null)
    {
        if (direction == null)
        {
            int x = toNode.coords.x - fromNode.coords.x;
            int y = toNode.coords.y - fromNode.coords.y;
            var offSetCoords = fromNode.offSet.FirstOrDefault(c => c.x == x && c.y == y);
            unit.facing = (Direction)Array.IndexOf(fromNode.offSet, offSetCoords);
        }
        else
        {
            unit.facing = (Direction)direction;
        }
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
        string power1Text = s1.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s1.explosivePower + s1sExplosive}";
        string power2Text = s2.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s2.explosivePower + s2sExplosive}";

        if (type == AttackType.Kinetic)
        {
            s1Dmg = (s2.kineticPower+s2sKinetic) - (s1.kineticPower+s1sKinetic);
            s2Dmg = (s1.kineticPower+s1sKinetic) - (s2.kineticPower+s2sKinetic);
            s1Amr = s1.kineticDamageModifier;
            s2Amr = s2.kineticDamageModifier;
            support1Text = (s1sKinetic > 0 ? $"({s1.kineticPower} base +{s1sKinetic} from support)" : "");
            support2Text = (s2sKinetic > 0 ? $"({s2.kineticPower} base +{s2sKinetic} from support)" : "");
            power1Text = s1.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s1.kineticPower + s1sKinetic}";
            power2Text = s2.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s2.kineticPower + s2sKinetic}";
        }
        else if (type == AttackType.Thermal)
        {
            s1Dmg = (s2.thermalPower+s2sThermal) - (s1.thermalPower+s1sThermal);
            s2Dmg = (s1.thermalPower+s1sThermal) - (s2.thermalPower+s2sThermal);
            s1Amr = s1.thermalDamageModifier;
            s2Amr = s2.thermalDamageModifier;
            support1Text = (s1sThermal > 0 ? $"({s1.thermalPower} base +{s1sThermal} from support)" : "");
            support2Text = (s2sThermal > 0 ? $"({s2.thermalPower} base +{s2sThermal} from support)" : "");
            power1Text = s1.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s1.thermalPower + s1sThermal}";
            power2Text = s2.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s2.thermalPower + s2sThermal}";
        }
        var returnText = "";
        if (s1Dmg > 0)
        {
            var damage = Mathf.Max(s1Dmg - s1Amr, 0);
            s1.TakeDamage(damage, s2.moduleEffects.Contains(ModuleEffect.ReduceMaxHp));
            string hpText = s1.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s1.HP}";
            StartCoroutine(FloatingTextAnimation($"-{damage} HP", s1.transform, s1));
            returnText += $"<u><b>{s1.color} took {damage} damage</u></b> and has {hpText} HP left.";
            if (s1Amr != 0 && !s1.moduleEffects.Contains(ModuleEffect.HiddenStats))
            {
                var modifierSymbol = s1Amr > 0 ? "-" : "+";
                returnText += $"\n({s1Dmg} base damage {modifierSymbol}{Mathf.Abs(s1Amr)} from modules)";
            }
        }
        else if (s2Dmg > 0)
        {
            var damage = Mathf.Max(s2Dmg - s2Amr, 0);
            s2.TakeDamage(damage, s1.moduleEffects.Contains(ModuleEffect.ReduceMaxHp));
            string hpText = s2.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s2.HP}";
            StartCoroutine(FloatingTextAnimation($"-{damage} HP", s2.transform, s2));
            returnText += $"<u><b>{s2.color} took {damage} damage</u></b> and has {hpText} HP left.";
            if (s2Amr != 0 && !s2.moduleEffects.Contains(ModuleEffect.HiddenStats))
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
            turnArchive.Add(new Tuple<string, string>($"    Combat: Kinetic Phase", returnText));

        }
        else if (type is AttackType.Thermal)
        {
            phaseText.text += $"Kinetic <b>Thermal</b> Explosive";
            turnArchive.Add(new Tuple<string, string>($"    Combat: Thermal Phase ", returnText));

        }
        else
        {
            phaseText.text += $"Kinetic Thermal <b>Explosive</b>";
            turnArchive.Add(new Tuple<string, string>($"    Combat: Explosive Phase", returnText));

        }
        return returnText;
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
        while (Globals.GameMatch.GameTurns.Count() <= TurnNumber || Globals.GameMatch.GameTurns[TurnNumber].Players.Any(x=>x == null));
    }

    private void CheckForTurns(GameTurn turns)
    {
        if (turns != null)
        {
            if (!Globals.GameMatch.GameTurns.Any(x => x.TurnNumber == TurnNumber))
            {
                Globals.GameMatch.GameTurns.Add(turns);
            }
            else
            {
                Globals.GameMatch.GameTurns[TurnNumber] = turns;
            }
        }
    }
    public void ShowTurnArchive(bool active)
    {
        if (!isEndingTurn)
        {
            turnArchivePanel.SetActive(active);
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
                    QueueAction(new Action(ActionType.GainCredit));
                }
                //Remove effects for game save
                lastSubmittedTurn = MyStation.actions;
                for (int i = lastSubmittedTurn.Count() - 1; i >= 0; i--)
                {
                    PerformUpdates(lastSubmittedTurn[i], Constants.Remove, true);
                }
                List<int> actionOrders = new List<int>();
                for (int i = 0; i <= Stations.Count * 5; i += Stations.Count)
                {
                    for (int j = 0; j < Stations.Count; j++)
                    {
                        int k = (TurnNumber-1 + j) % Stations.Count;
                        if (k == Globals.localStationIndex)
                            actionOrders.Add(i+j+1);
                    }
                }
                GameTurn gameTurn = new GameTurn()
                {
                    GameGuid = Globals.GameMatch.GameGuid,
                    TurnNumber = TurnNumber,
                    ModulesForMarket = Globals.GameMatch.GameTurns.LastOrDefault().ModulesForMarket,
                    MarketModules = Globals.GameMatch.GameTurns.LastOrDefault().MarketModules,
                    AllModules = AllModules.Select(x => x.ToServerModule()).ToList(),
                    AllNodes = GridManager.i.AllNodes.Select(x => x.ToServerNode()).ToList(),
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
                    Technology = MyStation.technology.Select(x => x.ToServerTechnology()).ToList(),
                    Station = MyStation.ToServerUnit(),
                    Fleets = MyStation.fleets.Select(x => x.ToServerUnit()).ToList(),
                    ModulesGuids = MyStation.modules.Select(x => x.moduleGuid).ToList(),
                    Credits = MyStation.credits,
                    BonusKinetic = MyStation.bonusKinetic,
                    BonusThermal = MyStation.bonusThermal,
                    BonusExplosive = MyStation.bonusExplosive,
                    BonusMining = MyStation.bonusMining,
                    Score = MyStation.score,
                    FleetCount = MyStation.fleetCount,
                    MaxActions = MyStation.maxActions
                };
                //add effects back in case player wants to resubmit their turn
                for (int i = 0; i < lastSubmittedTurn.Count(); i++)
                {
                    PerformUpdates(lastSubmittedTurn[i], Constants.Create, true);
                }
                StartCoroutine(SubmitEndTurn(gameTurn));
            }
            else
            {
                ShowAreYouSurePanel(true);
            }
        }
    }

    private IEnumerator SubmitEndTurn(GameTurn gameTurn)
    {
        customAlertText.text = "Submitting Turn.";
        customAlertPanel.SetActive(true);
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameTurn);
        yield return StartCoroutine(sql.PostRoutine<bool>($"Game/EndTurn", stringToPost, SubmitResponse));
        if (SubmittedTurn)
        {
            yield return StartCoroutine(CheckEndTurn());
        }
        else
        {
            customAlertText.text = $"Your turn was NOT submitted successfully. \n\n Please try again.";
            customAlertPanel.SetActive(true);
        }
    }

    private IEnumerator CheckEndTurn()
    {
        SubmittedTurn = false;
        endTurnButton.sprite = endTurnButtonPressed;
        if (!isWaitingForTurns)
        {
            yield return StartCoroutine(GetTurnsFromServer());
            yield return StartCoroutine(DoEndTurn());
        }
        else
        {
            var plural = Globals.GameMatch.MaxPlayers > 2 ? "s" : "";
            customAlertText.text = $"Your new turn was submitted, overriding your previous one.";
            customAlertPanel.SetActive(true);
        }
    }

    private IEnumerator DoEndTurn()
    {
        customAlertPanel.SetActive(false);
        infoToggle = true;
        ToggleAll();
        isEndingTurn = true;
        turnValue.text = $"Turn #{TurnNumber} Complete!";
        turnLabel.SetActive(true);
        yield return StartCoroutine(WaitforSecondsOrTap(1));
        isWaitingForTurns = false;
        var turnFromServer = Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == TurnNumber)?.Players;
        if (turnFromServer != null)
        {
            //Remove effects from queued actions so we can reapply them with visuals
            for (int i = lastSubmittedTurn.Count() - 1; i >= 0; i--)
            {
                PerformUpdates(lastSubmittedTurn[i], Constants.Remove, true);
            }
            ClearModules();
            ToggleMineralText(true);
            ClearTurnArchiveObjects();
            var serverActions = turnFromServer.SelectMany(x => x.Actions).OrderBy(x => x.ActionOrder);
            foreach (var serverAction in serverActions)
            {
                yield return StartCoroutine(PerformAction(new Action(serverAction)));
                yield return new WaitForSeconds(.1f);
            }
            foreach (var archive in turnArchive)
            {
                var turnArchiveObject = Instantiate(turnArchivePrefab, TurnArchiveList);
                turnArchiveObject.GetComponentInChildren<TextMeshProUGUI>().text = archive.Item1;
                var archivePopUpText = archive.Item2;
                turnArchiveObject.GetComponent<Button>().onClick.AddListener(() => ShowCustomAlertPanel(archivePopUpText));
                TurnArchiveObjects.Add(turnArchiveObject);
            }
            foreach (var asteroid in GridManager.i.AllNodes)
            {
                asteroid.ReginCredits();
            }
            yield return StartCoroutine(sql.GetRoutine<int>($"Game/EndGame?gameGuid={Globals.GameMatch.GameGuid}&winner={Winner}", GetWinner));
        }
    }

    private void PerformUpdates(Action action, int modifier, bool queued = false)
    {
        var station = Stations[action.selectedUnit.stationId];
        station.GainCredits(-1 * action.costOfAction * modifier, station, queued);
        if (action.actionType == ActionType.AttachModule)
        {
            Unit UnitToRemoveFrom;
            List<Module> inventoryRemoveFrom;
            if (action._parentGuid == null)
            {
                UnitToRemoveFrom = AllUnits.FirstOrDefault(x => x.attachedModules.Any(x => x.moduleGuid == action.selectedModule.moduleGuid));
                if (UnitToRemoveFrom != null)
                {
                    inventoryRemoveFrom = UnitToRemoveFrom.attachedModules;
                }
                else
                {
                    UnitToRemoveFrom = Stations[action.selectedUnit.stationId];
                    inventoryRemoveFrom = Stations[action.selectedUnit.stationId].modules;
                    action._statonInventory = true;
                }
            }
            else
            {
                UnitToRemoveFrom = AllUnits.FirstOrDefault(x => x.unitGuid == action._parentGuid);
                if (action._statonInventory)
                {
                    inventoryRemoveFrom = ((Station)UnitToRemoveFrom).modules;
                }
                else
                {
                    inventoryRemoveFrom = UnitToRemoveFrom.attachedModules;
                }

            }
            action.selectedUnit.EditModule(action.selectedModule.moduleId, modifier);
            if (!action._statonInventory)
                UnitToRemoveFrom.EditModule(action.selectedModule.moduleId, modifier * -1);
            if (modifier == Constants.Create)
            {
                action._parentGuid = UnitToRemoveFrom.unitGuid;
                action.selectedUnit.attachedModules.Add(action.selectedModule);
                inventoryRemoveFrom.Remove(inventoryRemoveFrom.FirstOrDefault(x => x.moduleGuid == action.selectedModule.moduleGuid));
            }
            else
            {
                inventoryRemoveFrom.Add(action.selectedModule);
                action.selectedUnit.attachedModules.Remove(action.selectedUnit.attachedModules.FirstOrDefault(x => x.moduleGuid == action.selectedModule.moduleGuid));
                action._statonInventory = false;
                action._parentGuid = null;
            }
        }
        else if (action.actionType == ActionType.SwapModule)
        {
            Module dettachedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.generatedGuid);
            Unit unitToRemoveFrom = AllUnits.FirstOrDefault(x => x.attachedModules.Any(x => x.moduleGuid == (modifier == Constants.Create ? action.selectedModule.moduleGuid : action.generatedGuid)));
            List<Module> inventoryRemoveFrom;
            if (unitToRemoveFrom == null)
            {
                inventoryRemoveFrom = Stations[action.selectedUnit.stationId].modules;
            }
            else
            {
                inventoryRemoveFrom = unitToRemoveFrom?.attachedModules;
            }
            action.selectedUnit.EditModule(action.selectedModule.moduleId, modifier);
            action.selectedUnit.EditModule(dettachedModule.moduleId, modifier * -1);
            if (unitToRemoveFrom != null)
            {
                unitToRemoveFrom.EditModule(action.selectedModule.moduleId, modifier * -1);
                unitToRemoveFrom.EditModule(dettachedModule.moduleId, modifier);
            }
            if (modifier == Constants.Create)
            {              
                //attach new
                action.selectedUnit.attachedModules.Add(action.selectedModule);
                inventoryRemoveFrom.Remove(inventoryRemoveFrom.FirstOrDefault(x => x.moduleGuid == action.selectedModule.moduleGuid));
                //dettach old
                inventoryRemoveFrom.Add(dettachedModule);
                action.selectedUnit.attachedModules.Remove(action.selectedUnit.attachedModules.FirstOrDefault(x => x.moduleGuid == dettachedModule.moduleGuid));
            }
            else
            {
                //attach old
                action.selectedUnit.attachedModules.Add(dettachedModule);
                inventoryRemoveFrom.Remove(inventoryRemoveFrom.FirstOrDefault(x => x.moduleGuid == dettachedModule.moduleGuid));
                //deattach new
                inventoryRemoveFrom.Add(action.selectedModule);
                action.selectedUnit.attachedModules.Remove(action.selectedUnit.attachedModules.FirstOrDefault(x => x.moduleGuid == action.selectedModule.moduleGuid));
            }
        }
        else if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine)
        {
            if (modifier == Constants.Remove)
            {
                action.selectedUnit.ClearMinedPath(action.selectedPath);
                action.selectedUnit.resetMovementRange();
            }
            else
            {
                action.selectedUnit.AddMinedPath(action.selectedPath);
            }
        }
        else if (TechActions.Contains(action.actionType)) //Research Actions
        {
            station.technology[(int)action.actionType - Constants.MinTech].Research(station, modifier);
        }
        else if (action.actionType == ActionType.UpgradeFleet || action.actionType == ActionType.UpgradeStation)
        {
            LevelUpUnit(action.selectedUnit, modifier);
        }
        else if (action.actionType == ActionType.RepairFleet)
        {
            action.selectedUnit.RegenHP(3 * modifier, queued);
        }
    }

    private void SubmitResponse(bool response)
    {
        SubmittedTurn = response;
    }

    public void ExitGame(int exitOption)
    {
        if (exitOption == 0)
        {
            if(Winner == -1)
                exitPanel.SetActive(true);
            else
                SceneManager.LoadScene((int)Scene.Lobby);
        }
        else if (exitOption == 1)
        {
            SceneManager.LoadScene((int)Scene.Lobby);
        }
        else if (exitOption == 2)
        {
            forfietPanel.SetActive(true);
        }
        else if (exitOption == 3)
        {
            StartCoroutine(sql.GetRoutine<int>($"Game/EndGame?gameGuid={Globals.GameMatch.GameGuid}&winner={Stations.LastOrDefault(x => x.teamId != MyStation.teamId).stationId}"));
            EndTurn(true);
            SceneManager.LoadScene((int)Scene.Lobby);
        }
        else if (exitOption == 4)
        {
            exitPanel.SetActive(false);
            forfietPanel.SetActive(false);
        }
    }

    private void GetWinner(int _winner)
    {
        Winner = _winner;
        if (Winner == -1)
        {
            Winner = GridManager.i.CheckForWin();
        }
        if (Winner != -1)
        {
            turnTapText.SetActive(false);
            ToggleHPText(true);
            turnValue.text = $"{Stations[Winner].color} player won after {TurnNumber} turns";
            Debug.Log($"{Stations[Winner].color} player won after {TurnNumber} turns");
            //Add EOG Stats
        }
        else
        {
            previousTurnButton.interactable = true;
            StartTurn();
        }
    }

    private IEnumerator PerformAction(Action action)
    {
        tapped = false;
        var currentUnit = action.selectedUnit;
        turnValue.text = $"{currentUnit.unitName}:\n";
        if (currentUnit != null && AllUnits.Contains(currentUnit))
        {
            var currentStation = Stations[currentUnit.stationId];
            action.costOfAction = GetCostOfAction(action.actionType, currentUnit, false, action.selectedModule);
            if (currentStation.credits >= action.costOfAction)
            {
                if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine || action.actionType == ActionType.MineAsteroid)
                {
                    isMoving = true;
                    if (currentUnit != null && action.selectedPath != null && action.selectedPath.Count > 0 && action.selectedPath.Count <= currentUnit.getMaxMovementRange())
                    {
                        turnTapText.SetActive(false);
                        turnValue.text += $"{GetDescription(action.actionType)}";
                        turnArchive.Add(new Tuple<string, string>($"Action {action.actionOrder}({currentUnit.color}): {GetDescription(action.actionType)}", turnValue.text));
                        Debug.Log("Moving to position: " + currentUnit.currentPathNode.transform.position);
                        yield return StartCoroutine(MoveOnPath(currentUnit, action.selectedPath));
                    }
                    else
                    {
                        turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                        turnArchive.Add(new Tuple<string, string>($"Action {action.actionOrder}({currentUnit.color}): {GetDescription(action.actionType)}", turnValue.text));
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                    }
                    isMoving = false;
                }
                else if (action.actionType == ActionType.CreateFleet)
                {
                    if (IsUnderMaxFleets(currentStation, false))
                    {
                        turnValue.text += $"{GetDescription(action.actionType)}";
                        currentStation.GainCredits(-1 * action.costOfAction, currentStation);
                        yield return StartCoroutine(GridManager.i.CreateFleet(currentStation, (Guid)action.generatedGuid, false));
                    }
                    else
                    {
                        turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                        Debug.Log($"{currentUnit.unitName} not eligible for {GetDescription(action.actionType)}");
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                    }
                }
                else if (action.actionType == ActionType.UpgradeFleet || action.actionType == ActionType.UpgradeStation)
                {
                    if (CanLevelUp(currentUnit, action.actionType, false))
                    {
                        turnValue.text += $"{GetDescription(action.actionType)}";
                        StartCoroutine(FloatingTextAnimation($"Upgraded", currentUnit.transform, currentUnit)); //floater6
                        PerformUpdates(action, Constants.Create);
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
                    if (action.selectedModule != null)
                    {
                        var wonModule = new Module(action.selectedModule.moduleId, action.selectedModule.moduleGuid);
                        turnValue.text += $"Won bid\n\nPaid {action.selectedModule.currentBid} credits for:\n{wonModule.effectText}";
                        StartCoroutine(FloatingTextAnimation($"Won Bid", currentUnit.transform, currentUnit)); //floater5
                        PerformUpdates(action, Constants.Create);
                        currentStation.modules.Add(wonModule);
                    }
                    else
                    {
                        StartCoroutine(FloatingTextAnimation($"Lost Bid", currentUnit.transform, currentUnit)); //floater5
                        turnValue.text += "Lost bid\nGained 1 credit.";
                        currentStation.GainCredits(1, currentStation);
                    }
                    currentUnit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    currentUnit.selectIcon.SetActive(false);
                }
                else if (action.actionType == ActionType.AttachModule)
                {
                    if (currentUnit.attachedModules.Count <= currentUnit.maxAttachedModules)
                    {
                        Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModule?.moduleGuid);
                        if (selectedModule != null)
                        {
                            turnValue.text += $"{GetDescription(action.actionType)}.\n\nModule effect:\n";
                            if (currentUnit.moduleEffects.Contains(ModuleEffect.HiddenStats))
                            {
                                turnValue.text += $"???";
                            }
                            else
                            {
                                turnValue.text += $"{selectedModule.effectText}";
                            }
                            StartCoroutine(FloatingTextAnimation($"+Module", currentUnit.transform, currentUnit)); //floater5
                            PerformUpdates(action, Constants.Create);
                        }
                        else
                        {
                            turnValue.text += $"Could not perform {GetDescription(action.actionType)}, module not available";
                        }
                    }
                    else
                    {
                        turnValue.text += $"Could not perform {GetDescription(action.actionType)}, max attached modules";
                    }
                    currentUnit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    currentUnit.selectIcon.SetActive(false);
                }
                else if (action.actionType == ActionType.SwapModule)
                {
                    if (currentUnit.attachedModules.Count > 0)
                    {
                        Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModule?.moduleGuid);
                        Module dettachedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.generatedGuid);
                        if (selectedModule != null && dettachedModule != null)
                        {
                            turnValue.text += $"{GetDescription(action.actionType)}.\n\nModule effect:\n";
                            if (currentUnit.moduleEffects.Contains(ModuleEffect.HiddenStats))
                            {
                                turnValue.text += $"???";
                            }
                            else
                            {
                                turnValue.text += $"{selectedModule.effectText}";
                            }
                            StartCoroutine(FloatingTextAnimation($"Module Swap", currentUnit.transform, currentUnit)); //floater4
                            PerformUpdates(action, Constants.Create);
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
                    turnValue.text += $"{GetDescription(action.actionType)}";
                    PerformUpdates(action, Constants.Create);
                    currentUnit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    currentUnit.selectIcon.SetActive(false);
                }
                else if (action.actionType == ActionType.RepairFleet)
                {
                    turnValue.text += $"{GetDescription(action.actionType)}\n\n";
                    PerformUpdates(action, Constants.Create);
                    turnValue.text += $"New HP/Max: {Mathf.Min(currentUnit.maxHP, currentUnit.HP)}/{currentUnit.maxHP}";
                    currentUnit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    currentUnit.selectIcon.SetActive(false);
                }
                else if (TechActions.Contains(action.actionType))
                {
                    var tech = currentStation.technology[(int)action.actionType - Constants.MinTech];
                    turnValue.text += $"{GetDescription(action.actionType)}\n\n{tech.effectText} ({tech.currentAmount + 1}/{tech.neededAmount})";
                    StartCoroutine(FloatingTextAnimation($"+Tech", currentUnit.transform, currentUnit)); //floater3
                    PerformUpdates(action, Constants.Create);
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
        else
        {
            turnValue.text = $"The unit that queued {GetDescription(action.actionType)} was destroyed";
            yield return StartCoroutine(WaitforSecondsOrTap(1));
        }
        if (!(action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine || action.actionType == ActionType.MineAsteroid))
            turnArchive.Add(new Tuple<string, string>($"Action {action.actionOrder}({currentUnit.color}): {GetDescription(action.actionType)}", turnValue.text));

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
            yield return new WaitForSeconds(.1f);
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


    private void StartTurn()
    {
        UpdateAuctionModules(TurnNumber);
        TurnNumber++;
        endTurnButton.sprite = endTurnButtonNotPressed;
        foreach (var station in Stations)
        {
            station.AOERegen(1);
            station.actions.Clear();
            if (station.score >= Convert.ToInt32(Math.Floor(GridManager.i.scoreToWin * (.75))))
            {
                station.maxActions = 5;
            }
            else if (station.score >= Convert.ToInt32(Math.Floor(GridManager.i.scoreToWin * (.5))))
            {
                station.maxActions = 4;
            }
            else
            {
                station.maxActions = 3;
            }
        }
        foreach (var unit in AllUnits)
        {
            if (!unit.hasMoved)
            {
                unit.RegenHP(1);
            }
            Stations[unit.stationId].GainCredits(unit.globalCreditGain,unit,TurnNumber == 1);
            unit.hasMoved = false;
            unit.resetMining();
            unit.resetMovementRange();
            unit.HP = Mathf.Min(unit.maxHP, unit.HP);
        }
        turnLabel.SetActive(false);
        ClearActionBar();
        GridManager.i.GetScores();
        for (int i = 0; i < Constants.MaxPlayers; i++)
        {
            var orderObj = turnOrder.Find($"TurnOrder{i}").GetComponent<Image>();
            orderObj.sprite = unReadyCircle;
            if (Stations.Count > i)
                orderObj.transform.Find($"Color").GetComponent<Image>().color = GridManager.i.playerColors[(TurnNumber - 1 + i) % Stations.Count];
            else
                orderObj.gameObject.SetActive(false);
        }
        infoToggle = false;
        ViewModuleMarket(false);
        ViewTechnology(false);
        ResetAfterSelection();
        isEndingTurn = false;
        Debug.Log($"New Turn {TurnNumber} Starting");
    }
#endregion
    private void LevelUpUnit(Unit unit, int modifier)
    {
        unit.level += modifier;
        unit.maxAttachedModules += modifier;
        unit.maxAttachedModules = Mathf.Min(unit.maxAttachedModules, Constants.MaxModules);
        unit.maxAttachedModules = Mathf.Max(unit.maxAttachedModules, Constants.MinModules);
        if (modifier == 1)
        {
            if (unit.level % 2 == 0)
            {
                unit.maxMovement += modifier;
                unit.movementLeft += modifier;
            }
            if (unit.level % 2 != 0)
            {
                unit.IncreaseMaxMining(2 * modifier);
            }
        }
        else
        {
            if (unit.level % 2 == 0)
            {
                unit.IncreaseMaxMining(2 * modifier);
            }
            if (unit.level % 2 != 0)
            {
                unit.maxMovement += modifier;
                unit.movementLeft += modifier;
            }
        }
        unit.IncreaseMaxHP(3 * modifier);
        unit.kineticPower += modifier;
        unit.explosivePower += modifier;
        unit.thermalPower += modifier;
        var spriteRenderer = unit.unitImage.GetComponent<SpriteRenderer>();
        if (unit is Station)
        {
            //unit.globalCreditGain += modifier;
            if (unit.level == 1)
            {
                spriteRenderer.sprite = GridManager.i.stationlvl1;
            }
            else if (unit.level == 2)
            {
                spriteRenderer.sprite = GridManager.i.stationlvl2;
            }
            else if (unit.level == 3)
            {
                spriteRenderer.sprite = GridManager.i.stationlvl3;
            }
            else
            {
                spriteRenderer.sprite = GridManager.i.stationlvl4;
            }
        }
        else
        {
            if (unit.level == 1)
            {
                spriteRenderer.sprite = GridManager.i.fleetlvl1;
            }
            else if (unit.level == 2)
            {
                spriteRenderer.sprite = GridManager.i.fleetlvl2;
            }
            else if (unit.level == 3)
            {
                spriteRenderer.sprite = GridManager.i.fleetlvl3;
            }
            else
            {
                spriteRenderer.sprite = GridManager.i.fleetlvl4;
            }
        }
    }
    public void RepairFleet()
    {
        if (SelectedUnit != null)
        {
            QueueAction(new Action(ActionType.RepairFleet, SelectedUnit));
        }
    }
    private void SetModuleGrid()
    {
        ClearModules();
        foreach (var module in MyStation.modules)
        {
            var moduleObject = Instantiate(modulePrefab, ModuleGrid);
            moduleObject.GetComponentInChildren<Button>().onClick.AddListener(() => SetModuleInfo(module));
            moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
            moduleObject.transform.Find("Queued").gameObject.SetActive(false);
            currentModules.Add(moduleObject);
        }
    }

    private void SetModuleInfo(Module module)
    {
        moduleInfoValue.text = module.effectText;
        moduleInfoIcon.sprite = module.icon;
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
    private void ClearTurnArchiveObjects()
    {
        while (TurnArchiveObjects.Count > 0) { Destroy(TurnArchiveObjects[0].gameObject); TurnArchiveObjects.RemoveAt(0); }
        turnArchive.Clear();
    }
    private void ClearTechnologyObjects()
    {
        while (TechnologyObjects.Count > 0) { Destroy(TechnologyObjects[0].gameObject); TechnologyObjects.RemoveAt(0); }
    }

    private void ClearActionBar()
    {
        for (int i = 0; i < Constants.MaxActions; i++)
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
    private void SetModuleBar(Unit unit)
    {
        for (int i = 0; i < Constants.MaxModules; i++)
        {
            UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
            UnitModuleBar.Find($"Module{i}/Remove").GetComponent<Button>().onClick.RemoveAllListeners();
            if (i < unit.maxAttachedModules)
            {
                if (i < unit.attachedModules.Count)
                {
                   
                    UnitModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(unit.stationId == MyStation.stationId && !MyStation.actions.Any(x=>x.selectedModule?.moduleGuid == unit.attachedModules[i]?.moduleGuid || x.generatedGuid == unit.attachedModules[i]?.moduleGuid));
                    if (unit.teamId != MyStation.teamId && unit.moduleEffects.Contains(ModuleEffect.HiddenStats) && unit.attachedModules[i].moduleId != Constants.SpyModule)
                    {
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = lockModuleBar;
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() => ShowCustomAlertPanel("This units modules are hidden."));
                    }
                    else
                    {
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = unit.attachedModules[i].icon;
                        var module = unit.attachedModules[i];
                        UnitModuleBar.Find($"Module{i}/Remove").GetComponent<Button>().onClick.AddListener(() => ModifyModule(ActionType.SwapModule, module.moduleGuid));
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() => SetModuleInfo(module));
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
