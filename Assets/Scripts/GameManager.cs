using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    private Transform highlightParent;
    private Transform canvas;
    public GameObject turnArchivePrefab;
    public GameObject floatingTextPrefab;
    public GameObject selectPrefab;
    public GameObject pathPrefab;
    public GameObject deployPrefab;
    public GameObject movementRangePrefab;
    public GameObject movementMinePrefab;
    public GameObject turnOrderPrefab;
    public GameObject modulePrefab;
    public GameObject auctionPrefab;
    public GameObject techPrefab;
    public GameObject moduleInfoPanel;
    public GameObject actionPanel;
    public GameObject infoPanel;
    public GameObject unlockPanel;
    public GameObject customAlertPanel;
    public GameObject submitTurnPanel;
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
    public Sprite[] emptyModuleBar;
    public Sprite unReadyCircle;
    public Sprite readyCircle;

    private PathNode SelectedNode;
    private Unit SelectedUnit;
    private List<Node> currentMovementRange = new List<Node>();
    private List<Node> currentDeployRange = new List<Node>();
    private List<GameObject> currentModules = new List<GameObject>();
    private List<GameObject> currentModulesForSelection = new List<GameObject>();
    internal List<Station> Stations = new List<Station>();

    private bool tapped = false;
    private bool skipTurn = false;
    private bool isMoving = false;
    internal bool isEndingTurn = false;
    private Button upgradeButton;
    private Button unlockButton;
    public Button createFleetButton;
    public Button deployBombButton;
    public Button generateModuleButton;
    public Button previousTurnButton;
    public GameObject hamburgerButton;
    public Image endTurnButton;
    public Sprite endTurnButtonNotPressed;
    public Sprite endTurnButtonPressed;

    private TextMeshProUGUI createFleetCost;
    private TextMeshProUGUI upgradeCost;
    private TextMeshProUGUI deployCost;
    private TextMeshProUGUI nameValue;
    private TextMeshProUGUI playerNameValue;
    private TextMeshProUGUI levelValue;
    private TextMeshProUGUI HPValue;
    private TextMeshProUGUI miningValue;
    private TextMeshProUGUI rangeValue;
    private TextMeshProUGUI KineticValueText;
    private TextMeshProUGUI ThermalValueText;
    private TextMeshProUGUI ExplosiveValueText;
    private TextMeshProUGUI KineticDeployValueText;
    private TextMeshProUGUI ThermalDeployValueText;
    private TextMeshProUGUI ExplosiveDeployValueText;
    private TextMeshProUGUI KineticTakenValueText;
    private TextMeshProUGUI ThermalTakenValueText;
    private TextMeshProUGUI ExplosiveTakenValueText;
    private TextMeshProUGUI ModuleEffectText;
    private TextMeshProUGUI DeployRangeValueText;
    internal TextMeshProUGUI turnValue;
    private TextMeshProUGUI phaseText;
    private TextMeshProUGUI moduleInfoValue;
    private TextMeshProUGUI unlockText;
    private TextMeshProUGUI unlockCostText;
    private TextMeshProUGUI customAlertText;
    public TextMeshProUGUI ColorText;
    public TextMeshProUGUI MatchName;
    public TextMeshProUGUI TurnNumberText;

    public Image Audio;
    public GameObject nextActionButton;
    public GameObject skipActionsButton;
    private Image moduleInfoIcon;
    public AudioSource turnDing;
    internal List<ActionType> TechActions = new List<ActionType>();
    internal List<Unit> AllUnits = new List<Unit>();
    internal List<Module> AllModules = new List<Module>();
    internal List<Module> AuctionModules = new List<Module>();
    internal List<GameObject> AuctionObjects = new List<GameObject>();
    internal List<GameObject> TechnologyObjects = new List<GameObject>();
    internal List<GameObject> TurnArchiveObjects = new List<GameObject>();
    internal List<GameObject> TurnOrderObjects = new List<GameObject>();
    internal Guid Winner;
    private bool lastToSubmitTurn = false;
    internal Station MyStation {get {return Stations.FirstOrDefault(x=>x.playerGuid == Globals.Account.PlayerGuid);}}
    public GameObject turnLabel;
    private SqlManager sql;
    internal int TurnNumber = 0;
    private bool audioToggleOn = true;
    private int helpPageNumber = 0;
    private List<Action> lastSubmittedTurn = new List<Action>();
    List<Tuple<string,string>> turnArchive = new List<Tuple<string, string>>();
    public GameObject clientOutOfSyncPanel;
    private bool isClientOutOfSync = false;
    internal List<ModuleStats> AllModulesStats;

    //Got game icons from https://game-icons.net/
    private void Awake()
    {
        i = this;
        sql = new SqlManager();
#if UNITY_EDITOR
        if(!Globals.HasBeenToLobby)
            SceneManager.LoadScene((int)Scene.Login);
#endif
        loadingPanel.SetActive(true);
        audioToggleOn = PlayerPrefs.GetString("AudioOn") != "False";
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
        MatchName.text = $"{Globals.GameMatch.GameGuid.ToString().Substring(0, 6)}";
        TurnNumberText.text = $"Turn #{TurnNumber.ToString()}";
        ColorText.text = $"{Globals.Account.Username.Split(' ')[0]}";
        UpdateColors((int)MyStation.playerColor);
        for (int i = Constants.MinTech; i <= Constants.MaxTech; i++)
        {
            TechActions.Add((ActionType)i);
            var techObject = Instantiate(techPrefab, technologyPanel.transform.Find("TechBar"));
            TechnologyObjects.Add(techObject);
        }
        loadingPanel.SetActive(false);
        if (TurnNumber == 0)
        {
            StartTurn();
        }
        else
        {
            BuildTurnObjects();
            previousTurnButton.interactable = true;
            yield return StartCoroutine(DoEndTurn());
        }
        if (Winner == Guid.Empty)
        {
            if (Globals.GameMatch.GameTurns.LastOrDefault(x => x.TurnNumber == TurnNumber)?.Players.FirstOrDefault(x => x.PlayerGuid == Globals.Account.PlayerGuid)?.Actions != null)
            {
                foreach (var serverAction in Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == TurnNumber)?.Players.FirstOrDefault(x => x.PlayerGuid == Globals.Account.PlayerGuid)?.Actions)
                {
                    QueueAction(new Action(serverAction), true);
                }
                lastSubmittedTurn = MyStation.actions;
            }
        }
        //Make shift update loop that I can control the speed of
        StartCoroutine(GetIsTurnReady());
    }

    private void UpdateColors(int playerId)
    {
        Color playerColor = GridManager.i.playerColors[playerId];
        Color uiColor = GridManager.i.uiColors[playerId];
        ColorText.color = playerColor;
        canvas.Find("UnlockInfoPanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("CustomAlertPanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("AsteroidPanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("ExitPanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("ForfeitPanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("SubmitTurnPanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("AreYouSurePanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("HelpPanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("TurnLabel").GetComponent<Image>().color = uiColor;
        canvas.Find("ActionPanel/ModulePanel").GetComponent<Image>().color = uiColor;
        canvas.Find("UnitPanel/Background").GetComponent<Image>().color = uiColor;
        canvas.Find("MarketPanel/MarketBar").GetComponent<Image>().color = uiColor;
        canvas.Find("Technology/TechBar").GetComponent<Image>().color = uiColor;
        canvas.Find("SelectModulePanel/SelectedModuleGrid").GetComponent<Image>().color = uiColor;
        canvas.Find("ModuleInfoPanel/Background/Foreground").GetComponent<Image>().color = uiColor;
        canvas.Find("TurnArchivePanel/Background").GetComponent<Image>().color = uiColor;
    }

    private IEnumerator GetIsTurnReady()
    {
        while (Winner == Guid.Empty)
        {
            int i = 0; 
            while (!isEndingTurn && !isClientOutOfSync && TurnOrderObjects.Count >= Globals.GameMatch.MaxPlayers)
            {
                yield return StartCoroutine(sql.GetRoutine<GameTurn>($"Game/GetTurns?gameGuid={Globals.GameMatch.GameGuid}&turnNumber={TurnNumber}&searchType={(int)SearchType.gameSearch}&startPlayers={Globals.GameMatch.GameTurns.FirstOrDefault(x=>x.TurnNumber == TurnNumber)?.Players?.Count() ?? 0}&clientVersion={Constants.ClientVersion}", UpdateGameTurnStatus));
                if (Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == TurnNumber)?.TurnIsOver ?? false)
                {
                    if(audioToggleOn && !lastToSubmitTurn)
                        turnDing.Play();
                    yield return StartCoroutine(DoEndTurn());
                }
                yield return new WaitForSeconds(.5f);
                i++;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateGameTurnStatus(GameTurn? turn, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            isClientOutOfSync = true;
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
        else if (!isEndingTurn)
        {
            UpdateCurrentTurn(turn);
            if (TurnOrderObjects.Count >= Globals.GameMatch.MaxPlayers)
            {
                for (int i = 0; i < Stations.Count; i++)
                {
                    var readyState = TurnOrderObjects[i]?.transform?.Find("ReadyState")?.gameObject;
                    if (readyState != null)
                    {
                        if (Globals.GameMatch.MaxPlayers > 1 && turn != null)
                        {
                            readyState.SetActive(turn.Players.FirstOrDefault(x => x.PlayerGuid == Stations[(TurnNumber - 1 + i) % Stations.Count].playerGuid) != null);
                        }
                        else if (Globals.GameMatch.MaxPlayers == 1)
                        {
                            readyState.SetActive(Stations[(TurnNumber - 1 + i) % Stations.Count].playerGuid != Globals.Account.PlayerGuid);
                        }
                    }
                }
            }
        }
    }

    private void FindUI()
    {
        highlightParent = GameObject.Find("Highlights").transform;
        canvas = GameObject.Find("Canvas").transform;
        nameValue = infoPanel.transform.Find("NameValue").GetComponent<TextMeshProUGUI>();
        playerNameValue = infoPanel.transform.Find("PlayerNameValue").GetComponent<TextMeshProUGUI>();
        levelValue = infoPanel.transform.Find("LevelValue").GetComponent<TextMeshProUGUI>();
        HPValue = infoPanel.transform.Find("S/V/C/HPValue").GetComponent<TextMeshProUGUI>();
        miningValue = infoPanel.transform.Find("S/V/C/MiningValue").GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("S/V/C/MovementValue").GetComponent<TextMeshProUGUI>();        
        KineticValueText = infoPanel.transform.Find("S/V/C/KineticValue").GetComponent<TextMeshProUGUI>();
        ThermalValueText = infoPanel.transform.Find("S/V/C/ThermalValue").GetComponent<TextMeshProUGUI>();
        ExplosiveValueText = infoPanel.transform.Find("S/V/C/ExplosiveValue").GetComponent<TextMeshProUGUI>();
        KineticDeployValueText = infoPanel.transform.Find("S/V/C/KineticDeployValue").GetComponent<TextMeshProUGUI>();
        ThermalDeployValueText = infoPanel.transform.Find("S/V/C/ThermalDeployValue").GetComponent<TextMeshProUGUI>();
        ExplosiveDeployValueText = infoPanel.transform.Find("S/V/C/ExplosiveDeployValue").GetComponent<TextMeshProUGUI>();
        KineticTakenValueText = infoPanel.transform.Find("S/V/C/KineticTakenValue").GetComponent<TextMeshProUGUI>();
        ThermalTakenValueText = infoPanel.transform.Find("S/V/C/ThermalTakenValue").GetComponent<TextMeshProUGUI>();
        ExplosiveTakenValueText = infoPanel.transform.Find("S/V/C/ExplosiveTakenValue").GetComponent<TextMeshProUGUI>();
        ModuleEffectText = infoPanel.transform.Find("S/V/C/ModuleInformation").GetComponent<TextMeshProUGUI>();
        DeployRangeValueText = infoPanel.transform.Find("S/V/C/DeployRangeValue").GetComponent<TextMeshProUGUI>();
        turnValue = turnLabel.transform.Find("TurnValue").GetComponent<TextMeshProUGUI>();
        phaseText = turnLabel.transform.Find("PhaseText").GetComponent<TextMeshProUGUI>();
        moduleInfoValue = moduleInfoPanel.transform.Find("ModuleInfoText").GetComponent<TextMeshProUGUI>();
        moduleInfoIcon = moduleInfoPanel.transform.Find("Image").GetComponent<Image>();
        createFleetCost = createFleetButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        upgradeButton = infoPanel.transform.Find("UpgradeButton").GetComponent<Button>();
        upgradeCost = upgradeButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        deployCost = deployBombButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        unlockText = unlockPanel.transform.Find("Background/AlertText").GetComponent<TextMeshProUGUI>();
        unlockButton = unlockPanel.transform.Find("Background/UnlockButton").GetComponent<Button>();
        unlockCostText = unlockButton.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        customAlertText = customAlertPanel.transform.Find("Background/AlertText").GetComponent<TextMeshProUGUI>();
        Audio.sprite = audioToggleOn ? Resources.Load<Sprite>("Sprites/UI/soundOff") : Resources.Load<Sprite>("Sprites/UI/soundOn");
    }
    public void SetUnitTextValues(Unit unit)
    {
        ToggleHPText(true);
        if (unit.unitType == UnitType.Bomb)
        {
            var alertString = $"{GetPlayerName(unit.playerGuid)}'s Bomb\n\nThis bomb has {unit.explosivePower} Explosive Power.\n\nMoving into this bomb will ";
            if (unit.teamId == MyStation.teamId)
            {
                alertString += "disarm it.";
            }
            else
            {
                alertString += "cause it to explode. One round of Explosive combat will take place, if you take damage you will also lose 1 movement.";
            }
            ShowCustomAlertPanel(alertString);
        }
        if (unit.unitType == UnitType.Station || unit.unitType == UnitType.Bomber)
        {
            nameValue.text = unit.unitName;
            playerNameValue.color = GridManager.i.playerColors[(int)unit.playerColor];
            playerNameValue.text = GetPlayerName(unit.playerGuid);
            levelValue.text = unit.level.ToString();
            if (unit.teamId != MyStation.teamId && unit.moduleEffects.Contains(ModuleEffect.HiddenStats))
            {
                HPValue.text = "?/?";
                rangeValue.text = "?/?";
                KineticValueText.text = "?";
                ThermalValueText.text = "?";
                ExplosiveValueText.text = "?";
                ModuleEffectText.text = "?";
                miningValue.text = "?";
                KineticDeployValueText.text = "?";
                ThermalDeployValueText.text = "?";
                ExplosiveDeployValueText.text = "?";
                KineticTakenValueText.text = "?";
                ThermalTakenValueText.text = "?";
                ExplosiveTakenValueText.text = "?";
                DeployRangeValueText.text = "?";
            }
            else
            {
                HPValue.text = Mathf.Min(unit.maxHP, unit.HP) + "/" + unit.maxHP;
                rangeValue.text = $"{unit.movementLeft}/{unit.maxMovement}";
                DeployRangeValueText.text = unit.deployRange.ToString();
                ModuleEffectText.text = "";
                for (int i = 0; i < unit.attachedModules.Count; i++)
                {
                    ModuleEffectText.text += $"{unit.attachedModules[i].effectText.Replace("\n", ", ").Replace(":,", ":")}\n";
                }
                if(String.IsNullOrEmpty(ModuleEffectText.text))
                    ModuleEffectText.text = "No Modules Installed\n";
                ModuleEffectText.text.Substring(0, ModuleEffectText.text.Length - 1);
                miningValue.text = unit.maxMining.ToString();
                var kineticPower = $"{unit.kineticPower}";
                var thermalPower = $"{unit.thermalPower}";
                var explosivePower = $"{unit.explosivePower}";
                unit.SetSupportValues(true);
                kineticPower += unit.kineticSupportPower > 0 ? $" (+{unit.kineticSupportPower})" : "";
                thermalPower += unit.thermalSupportPower > 0 ? $" (+{unit.thermalSupportPower})" : "";
                explosivePower += unit.explosiveSupportPower > 0 ? $" (+{unit.explosiveSupportPower})" : "";
                KineticValueText.text = $"{kineticPower}";
                ThermalValueText.text = $"{thermalPower}";
                ExplosiveValueText.text = $"{explosivePower}";
                KineticDeployValueText.text = $"{unit.kineticDeployPower}";
                ThermalDeployValueText.text = $"{unit.thermalDeployPower}";
                ExplosiveDeployValueText.text = $"{unit.explosiveDeployPower}";
                var symbol = unit.kineticDamageTaken > 0 ? "+" : "";
                KineticTakenValueText.text = $"{symbol}{unit.kineticDamageTaken}";
                symbol = unit.thermalDamageTaken > 0 ? "+" : "";
                ThermalTakenValueText.text = $"{symbol}{unit.thermalDamageTaken}";
                symbol = unit.explosiveDamageTaken > 0 ? "+" : "";
                ExplosiveTakenValueText.text = $"{symbol}{unit.explosiveDamageTaken}";
            }
            infoPanel.transform.Find("Background").GetComponent<Image>().color = GridManager.i.uiColors[(int)unit.playerColor];
            var actionType = unit is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
            upgradeButton.gameObject.SetActive(false);
            createFleetButton.gameObject.SetActive(false);
            deployBombButton.gameObject.SetActive(false);
            upgradeButton.interactable = false;
            deployBombButton.interactable = false;
            if (unit.playerGuid == MyStation.playerGuid)
            {
                if (unit.level < Constants.MaxUnitLevel)
                {
                    upgradeButton.interactable = true;
                    if (CanLevelUp(unit, actionType, true))
                    {
                        var cost = GetCostOfAction(actionType, unit, true);
                        upgradeCost.text = GetCostText(cost);
                        upgradeButton.interactable = MyStation.credits >= cost;
                    }
                    else
                    {
                        upgradeCost.text = "(Research Required)";
                    }
                }
                else
                {
                    upgradeCost.text = "(Max Level)";
                }
                upgradeButton.gameObject.SetActive(true);
                if (unit is Station)
                {
                    createFleetButton.gameObject.SetActive(true);
                }
                else
                {
                    deployBombButton.gameObject.SetActive(true);
                    if (unit.movementLeft > 0)
                    {
                        deployBombButton.interactable = true;
                        deployCost.text = "(Costs 1 Movement)";
                    }
                    else
                    {
                        deployCost.text = "(Movement Required)";
                    }
                }
                ToggleMineralText(true);
            }
            SetModuleBar(unit);
            infoPanel.gameObject.SetActive(true);
        }
    }

    private string GetPlayerName(Guid playerGuid)
    {
        return Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == 0)?.Players?.FirstOrDefault(x => x.PlayerGuid == playerGuid)?.PlayerName.Split(' ')[0] ?? "CPU";
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            tapped = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);
            if (!isEndingTurn && hit.collider != null && !isMoving)
            {
                PathNode targetNode = hit.collider.GetComponent<PathNode>();
                if (targetNode != null && !EventSystem.current.IsPointerOverGameObject())
                {
                    GridManager.i.CloseBurger();
                    moduleMarket.SetActive(false); 
                    technologyPanel.SetActive(false); 
                    Unit targetUnit = null;
                    //Check if you clicked on unit
                    if (targetNode.unitOnPath != null)
                    {
                        targetUnit = targetNode.unitOnPath;
                    }
                    //Original click on unit
                    if (SelectedUnit == null && targetUnit != null)
                    {
                        SelectedUnit = targetUnit;
                        SetUnitTextValues(SelectedUnit);
                        if (SelectedUnit.playerGuid == MyStation.playerGuid && HasQueuedMovement(SelectedUnit))
                        {
                            HighlightQueuedMovement(MyStation.actions.FirstOrDefault(x => (x.actionType == ActionType.MoveUnit || x.actionType == ActionType.MoveAndMine) && x.selectedUnit.unitGuid == SelectedUnit.unitGuid));
                        }
                        else
                        {
                            Debug.Log($"{SelectedUnit.unitName} Selected.");
                            if (SelectedUnit.unitType != UnitType.Bomb)
                                HighlightMovementRange(SelectedUnit.currentPathNode, SelectedUnit);
                        }
                    }
                    //Double click confirm to movement
                    else if (SelectedUnit != null && SelectedNode != null && targetNode == SelectedNode && SelectedPath != null && SelectedUnit.playerGuid == MyStation.playerGuid && SelectedUnit.unitType != UnitType.Bomb && !HasQueuedMovement(SelectedUnit))
                    {
                        QueueAction(new Action(ActionType.MoveUnit, SelectedUnit, null, SelectedPath));
                    }
                    //Create Deploy
                    else if (SelectedUnit != null && targetNode != null && SelectedUnit.playerGuid == MyStation.playerGuid && currentDeployRange.Select(x => x.currentPathNode).Contains(targetNode))
                    {
                        QueueAction(new Action(SelectedUnit is Station ? ActionType.DeployFleet : ActionType.DeployBomb, SelectedUnit, null, new List<PathNode>() { targetNode }, Guid.NewGuid()));
                    }
                    //Create Movement
                    else if (SelectedUnit != null && targetNode != null && SelectedUnit.playerGuid == MyStation.playerGuid && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode) && !HasQueuedMovement(SelectedUnit))
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
                        HighlightMovementRange(SelectedNode, SelectedUnit);
                        if (SelectedPath.Last().unitOnPath != null && SelectedPath.Last().unitOnPath.teamId != SelectedUnit.teamId)
                        {
                            var unitOnPath = SelectedPath.Last().unitOnPath;
                            unitOnPath.SetSupportValues();
                            var unitKineticDmg = SelectedUnit.GetDamage(unitOnPath, AttackType.Kinetic);
                            var unitThermalDmg = SelectedUnit.GetDamage(unitOnPath, AttackType.Thermal);
                            var unitExplosiveDmg = SelectedUnit.GetDamage(unitOnPath, AttackType.Explosive);
                            var kineticString = "\nKinetic Phase:\n" + (unitKineticDmg > 0 ? $"<b>{SelectedUnit.playerColor} loses <u>{unitKineticDmg+SelectedUnit.kineticDamageTaken} HP</u></b>" : unitKineticDmg < 0 ? $"<b>{unitOnPath.playerColor} loses <u>{Mathf.Abs(unitKineticDmg)+ unitOnPath.kineticDamageTaken} HP</u></b>" : "<b><u>No damage taken.</u></b>");
                            var thermalString = "\nThermal Phase:\n" + (unitThermalDmg > 0 ? $"<b>{SelectedUnit.playerColor} loses <u>{unitThermalDmg + SelectedUnit.thermalDamageTaken} HP</u></b>" : unitThermalDmg < 0 ? $"<b>{unitOnPath.playerColor} loses <u>{Mathf.Abs(unitThermalDmg) + unitOnPath.thermalDamageTaken} HP</u></b>" : "<b><u>No damage taken.</u></b>");
                            var explosiveString = "\nExplosive Phase:\n" + (unitExplosiveDmg > 0 ? $"<b>{SelectedUnit.playerColor} loses <u>{unitExplosiveDmg + SelectedUnit.explosiveDamageTaken} HP</u></b>" : unitExplosiveDmg < 0 ? $"<b>{unitOnPath.playerColor} loses <u>{Mathf.Abs(unitExplosiveDmg) + unitOnPath.explosiveDamageTaken} HP</u></b>" : "<b><u>No damage taken.</u></b>");
                            if (unitOnPath.unitType == UnitType.Bomb || SelectedUnit.unitType == UnitType.Bomb)
                            {
                                kineticString = "";
                                thermalString = "";
                                if (unitExplosiveDmg != 0)
                                    explosiveString += " and 1 movement.";
                            }
                            ShowCustomAlertPanel($"Predicted combat outcome:\n{kineticString}{thermalString}{explosiveString}");
                        }
                    }
                    //Clicked on invalid tile, clear
                    else
                    {
                        DeselectUnitIcons();
                        if (SelectedUnit == null)
                        {
                            if (targetUnit != null)
                            {
                                targetUnit.selectIcon.SetActive(false);
                                SetUnitTextValues(targetUnit);
                                if(targetUnit.unitType != UnitType.Bomb)
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
                            if (SelectedUnit != null && (SelectedPath?.Count ?? 0) > 0 && !HasQueuedMovement(SelectedUnit))
                                ShowCustomAlertPanel("Movement cancelled. \n\n Double click selected hex to confirm movement.");
                            DeselectMovement();
                        }
                    }
                }
                else
                {
                    if (SelectedUnit != null && (SelectedPath?.Count ?? 0) > 0 && !HasQueuedMovement(SelectedUnit))
                        ShowCustomAlertPanel("Movement cancelled. \n\n Double click selected hex to confirm movement.");
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
        moduleMarket.SetActive(false);
        technologyPanel.SetActive(false);
        DeselectUnitIcons();
        var action = MyStation.actions[i];
        action.selectedUnit.selectIcon.SetActive(true);
        if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine)
        {
            HighlightQueuedMovement(action);
        }
        else if (action.actionType == ActionType.UpgradeFleet || action.actionType == ActionType.UpgradeStation)
        {
            var extraBonus = action.selectedUnit.level == 2 ? "+2 Mining Power" : action.selectedUnit.level == 3 ? "+1 Max Movement" : "+1 Deploy Range";
            var message = $"{action.selectedUnit.unitName} will gain:\n+3 Max HP.\n+2 Kinetic, Thermal, and Explosive Power.\n{extraBonus}.";
            ShowCustomAlertPanel(message);
        }
        else if (action.actionType == ActionType.BidOnModule || action.actionType == ActionType.SwapModule || action.actionType == ActionType.AttachModule)
        {
            canvas.Find("ModuleInfoPanel/Background/Foreground").GetComponent<Image>().color = GridManager.i.uiColors[(int)MyStation.playerColor];
            SetModuleInfo(AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModuleGuid));
        }
        else if (TechActions.Contains(action.actionType))
        {
            var tech = MyStation.technology[(int)action.actionType - Constants.MinTech];
            ShowCustomAlertPanel(tech.effectText + "." + tech.currentEffectText);
        }
        else if(action.actionType == ActionType.DeployFleet || action.actionType == ActionType.DeployBomb)
        {
            HighlightQueuedMovement(action, true);
        }
        else 
        {
            SetUnitTextValues(action.selectedUnit);
            if (action.selectedUnit.unitType != UnitType.Bomb)
                ViewUnitInformation(true);
        }
    }
    private void HighlightQueuedMovement(Action movementAction, bool deploy = false)
    {
        SelectedUnit = movementAction.selectedUnit;
        SetUnitTextValues(SelectedUnit);
        ViewUnitInformation(true);
        DrawPath(movementAction.selectedPath, deploy);
        Debug.Log($"{SelectedUnit.unitName} already has a pending movement action.");
    }

    private void DrawPath(List<PathNode> selectedPath, bool deploy = false)
    {
        ClearMovementPath();
        foreach (var node in selectedPath)
        {
            currentPathObjects.Add(Instantiate(deploy ? deployPrefab : node == selectedPath.Last() ? selectPrefab : pathPrefab, node.transform.position, Quaternion.identity));
        }
    }

    internal void DeselectMovement()
    {
        Debug.Log($"Invalid tile selected, reseting path and selection.");
        ResetSelectedUnitMovement();
        ResetAfterSelection();
    }

    private void ResetSelectedUnitMovement()
    {
        if (SelectedUnit != null && SelectedUnit.unitType != UnitType.Bomb)
        {
            Debug.Log($"Resetting Movement");
            SelectedUnit.subtractMovement(-1 * (SelectedPath?.Count ?? 0));
            SelectedUnit.ClearMinedPath(SelectedPath);
            SelectedUnit.selectIcon.SetActive(false);
            ClearMovementPath();
        }
    }

    public void ViewTechnology()
    {
        technologyPanel.SetActive(true);
        DeselectMovement();
        ClearTechnologyObjects();
        for (int j = 0; j < MyStation.technology.Count-Constants.ShadowTechAmount; j++)
        {
            int i = j;
            var techLevel = MyStation.technology[i].level;
            if (i == (int)TechnologyType.ResearchKinetic)
            {
                techLevel = MyStation.technology[(int)TechnologyType.ResearchKinetic].level 
                    + MyStation.technology[(int)TechnologyType.ResearchThermal].level 
                    + MyStation.technology[(int)TechnologyType.ResearchExplosive].level;
                if (MyStation.technology[(int)TechnologyType.ResearchKinetic].level > MyStation.technology[(int)TechnologyType.ResearchThermal].level)
                {
                    i = (int)TechnologyType.ResearchThermal;
                }else if(MyStation.technology[(int)TechnologyType.ResearchKinetic].level > MyStation.technology[(int)TechnologyType.ResearchExplosive].level)
                {
                    i = (int)TechnologyType.ResearchExplosive;
                }
            }
            var tech = MyStation.technology[i];
            var canQueue = tech.GetCanQueueTech();
            var techObject = TechnologyObjects[i];
            techObject.transform.SetParent(technologyPanel.transform.Find("TechBar"));
            var infoText = (canQueue ? "" : tech.requirementText) + tech.effectText +"."+ tech.currentEffectText;
            techObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => ShowCustomAlertPanel(infoText));
            techObject.transform.Find("Level").GetComponent<TextMeshProUGUI>().text = $"{techLevel}";
            var researchButton = techObject.transform.Find("Research").GetComponent<Button>();
            researchButton.interactable = canQueue;
            researchButton.onClick.RemoveAllListeners();
            researchButton.onClick.AddListener(() => Research(i));
            techObject.transform.Find("Queued").gameObject.SetActive(!canQueue);
            techObject.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Actions/{i+Constants.MinTech}");
        }
    }

    private void Research(int i)
    {
        QueueAction(new Action((ActionType)i+Constants.MinTech));
        if (MyStation.actions.Count == MyStation.maxActions)
        {
            technologyPanel.SetActive(false);
        }
        else
        {
            ViewTechnology();
        }
    }

    public void ViewModuleMarket()
    {
        moduleMarket.SetActive(true);
        canvas.Find("ModuleInfoPanel/Background/Foreground").GetComponent<Image>().color = GridManager.i.uiColors[4];
        DeselectMovement();
        ClearAuctionObjects();
        for(int j = 0; j < AuctionModules.Count; j++)
        {
            int i = j;
            var module = AuctionModules[i];
            var hasQueued = MyStation.actions.Any(x => x.actionType == ActionType.BidOnModule && x.selectedModuleGuid == module.moduleGuid);
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
            bidButton.onClick.AddListener(() => BidOn(i, module.currentBid));
            moduleObject.transform.Find("Bid/BidText").GetComponent<TextMeshProUGUI>().text = $"Bid {module.currentBid}";
            moduleObject.transform.Find("Queued").gameObject.SetActive(hasQueued || MyStation.credits < module.currentBid);
            moduleObject.transform.Find("TurnTimer").GetComponent<TextMeshProUGUI>().text = module.turnsLeftOnMarket > 1 ? $"{module.turnsLeftOnMarket} Turns Left" : "Last Turn!!!";
            moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
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

    private void BidOn(int index, int currentBid)
    {
        moduleMarket.SetActive(false);
        QueueAction(new Action(ActionType.BidOnModule, null, AuctionModules[index].moduleGuid,null,null,currentBid));
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
            GridManager.i.CloseBurger();
            helpPanel.SetActive(active);
        }
    }

    public void ShowUnlockPanel(int unlock)
    {
        unlockCostText.text = $"(Costs {unlock} Credits)";
        if (unlock > 0)
        {
            unlockText.text = $"This action slot will cost 3 less to unlock each turn.";
            unlockButton.interactable = unlock <= MyStation.credits;
        }
        else
        {
            unlockText.text = $"This action slot will unlock automatically next turn!";
            unlockButton.interactable = false;
        }
        unlockButton.onClick.RemoveAllListeners();
        unlockButton.onClick.AddListener(() => UnlockActionButton());
        unlockPanel.SetActive(true);
    }
    public void ShowCustomAlertPanel(string message)
    {
        customAlertText.text = message;
        customAlertPanel.SetActive(true);
        Debug.Log(message);
    }
    public void HideAlertPanel()
    {
        unlockPanel.SetActive(false);
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
    private void UpdateModulesFromServer(int i)
    {
        ClearAuctionObjects();
        AuctionModules.Clear();
        AllModules = Globals.GameMatch.GameTurns.FirstOrDefault(x=>x.TurnNumber == i).AllModules.Select(x => new Module(x)).OrderBy(x=>x.turnsLeftOnMarket).ToList();
        var moduleGuids = Globals.GameMatch.GameTurns.FirstOrDefault(x=>x.TurnNumber == i).MarketModuleGuids.Split(",").Select(x=>new Guid(x));
        AuctionModules.AddRange(moduleGuids.Select(x => AllModules.FirstOrDefault(y=>y.moduleGuid == x)).OrderBy(x=>x.turnsLeftOnMarket));
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
        else if (MyStation.playerGuid != SelectedUnit.playerGuid)
        {
            ShowCustomAlertPanel("This is not your unit!");
        } 
        else 
        {
            var selectedUnit = SelectedUnit;
            DeselectMovement();
            ClearSelectableModules();
            List<Module> availableModules = new List<Module>(MyStation.modules);
            List<Module> availableModulesFromUnits = MyStation.fleets.Where(x => x.unitGuid != selectedUnit.unitGuid).SelectMany(x => x.attachedModules).ToList();
            if (selectedUnit.unitGuid != MyStation.unitGuid)
                availableModulesFromUnits.AddRange(MyStation.attachedModules);
            availableModules.AddRange(availableModulesFromUnits);
            List<Guid?> assignedModules = MyStation.actions.Select(x => x.selectedModuleGuid).ToList();
            availableModules = availableModules.Where(x => !assignedModules.Contains(x.moduleGuid)).ToList();
            if (availableModules.Count > 0)
            {
                foreach (var module in availableModules)
                {
                    var moduleObject = Instantiate(modulePrefab, SelectedModuleGrid);
                    moduleObject.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => SetSelectedModule(module, selectedUnit, actionType, dettachGuid));
                    moduleObject.transform.Find("Image").GetComponent<Image>().sprite = module.icon;
                    moduleObject.transform.Find("Queued").gameObject.SetActive(false);
                    moduleObject.transform.Find("OnUnit").gameObject.SetActive(availableModulesFromUnits.Any(x=>x.moduleGuid == module.moduleGuid));
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
        if (MyStation.actions.Any(x => x.selectedModuleGuid == module.moduleGuid))
        {
            ShowCustomAlertPanel("This action is already queued up.");
        }
        else
        {
            ViewModuleSelection(false);
            QueueAction(new Action(action, unit, module.moduleGuid, null, dettachGuid));
            ClearSelectableModules();
        }
    }
    public void CreateFleet()
    {
        if (IsUnderMaxFleets(MyStation, true))
            HighlightDeployRange(MyStation);
        else
            ViewTechnology();
    }

    public void UpgradeStructure()
    {
        var actionType = SelectedUnit is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        if (CanLevelUp(SelectedUnit, actionType, true)){
            QueueAction(new Action(actionType, SelectedUnit));
        } else {
            ViewTechnology();
        }
    }

    public void CancelAction(int slot)
    {
        CancelAction(slot, false);
    }
    internal void CancelAction(int slot, bool isRequeuing)
    {
        technologyPanel.SetActive(false);
        moduleMarket.SetActive(false);
        var action = MyStation.actions[slot];
        Debug.Log($"{MyStation.unitName} removed action {GetDescription(action.actionType)} from queue");
        if (action != null)
        {
            PerformUpdates(action, Constants.Remove, true);           
            MyStation.actions.RemoveAt(slot);
            if (!isRequeuing)
                ReQueueActions();
            ClearActionBar();
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
        action.selectedUnit = action.selectedUnit ?? SelectedUnit ?? MyStation;
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
        else if (action.actionType == ActionType.DeployFleet && !IsUnderMaxFleets(MyStation, true))
        {
            if (!requeing) ShowCustomAlertPanel("Can not create new bomber, you will hit max fleets for your station level");
        }
        else if ((action.actionType == ActionType.DeployFleet || action.actionType == ActionType.DeployBomb) && !GetNodesForDeploy(action.selectedUnit,true).Contains(action.selectedPath.FirstOrDefault()))
        {
            if (!requeing) ShowCustomAlertPanel("Selected node not within deploy range");
        }
        else if (action.actionType != ActionType.UnlockAction && MyStation.actions.Count >= MyStation.maxActions)
        {
            if (!requeing) ShowCustomAlertPanel($"No action slots available to queue action: {GetDescription(action.actionType)}.");
        }
        else if (TechActions.Contains(action.actionType) && !MyStation.technology[(int)action.actionType - Constants.MinTech].GetCanQueueTech()) {
            if (!requeing) ShowCustomAlertPanel($"Missing required tech to queue action: {GetDescription(action.actionType)}.");
        }
        else
        {
            action.selectedPath = SelectedPath ?? action.selectedPath;
            action.playerGuid = MyStation.playerGuid;
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
            action.costOfAction = GetCostOfAction(action.actionType, action.selectedUnit, true, action.playerBid);
            if (action.costOfAction > MyStation.credits)
            {
                if (!requeing) ShowCustomAlertPanel($"Can not afford to queue action: {GetDescription(action.actionType)}.");
            }
            else
            {
                Debug.Log($"{MyStation.unitName} queuing up action {GetDescription(action.actionType)}");
                MyStation.actions.Add(action);
                AddActionBarImage(action.actionType, MyStation.actions.Count() - 1);
                PerformUpdates(action, Constants.Create, true, requeing);
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
        foreach (var unit in AllUnits.Where(x => x.unitType != UnitType.Bomb))
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
        createFleetButton.interactable = false;
        if (MyStation.fleets.Count < Constants.MaxUnitLevel)
        {
            createFleetButton.interactable = true;
            if (IsUnderMaxFleets(MyStation, true))
            {
                var cost = GetCostOfAction(ActionType.DeployFleet, MyStation, true);
                createFleetCost.text = GetCostText(cost);
                createFleetButton.interactable = (MyStation.credits >= cost);
            }
            else
            {
                createFleetCost.text = "(Research Required)";
            }
        }
        else
        {
            createFleetCost.text = "(Max Level)";
        }
    }

    private bool CanLevelUp(Unit unit, ActionType actionType, bool countQueue)
    {
        var station = GetStationByGuid(unit.playerGuid);
        var currentLevel = unit.level;
        var maxFleetLvl = station.technology[(int)TechnologyType.ResearchFleetLvl].level;
        var maxStationLvl = station.technology[(int)TechnologyType.ResearchStationLvl].level;
        if (countQueue) {
            currentLevel += GetStationByGuid(unit.playerGuid).actions.Count(x => x.selectedUnit.unitGuid == unit.unitGuid && x.actionType == actionType);
            maxFleetLvl = station.technology[(int)TechnologyType.ResearchFleetLvl].level;
            maxStationLvl = station.technology[(int)TechnologyType.ResearchStationLvl].level;
        } 
        if (actionType == ActionType.UpgradeFleet)
            return currentLevel <= maxFleetLvl;
        else
            return currentLevel <= maxStationLvl;
    }

    private bool IsUnderMaxFleets(Station station, bool countQueue)
    {
        var currentFleets = station.fleets.Count;
        var maxNumFleets = station.technology[(int)TechnologyType.ResearchMaxFleets].level + 1;
        if (countQueue)
        {
            currentFleets += station.actions.Count(x => x.actionType == ActionType.DeployFleet);
            maxNumFleets = station.technology[(int)TechnologyType.ResearchMaxFleets].level + 1;
        }
        return currentFleets < maxNumFleets;
    }
    private int GetCostOfAction(ActionType actionType, Unit unit, bool countQueue, int? playerBid = null)
    {
        var station = GetStationByGuid(unit.playerGuid);
        var countingQueue = countQueue ? station.actions.Where(x => x.actionType == actionType && x.selectedUnit.unitGuid == unit.unitGuid).Count() : 0;
        if (actionType == ActionType.DeployFleet)
        {
            if (station.fleets.Count + countingQueue <= 0)
                return 0;
            return (station.fleets.Count + countingQueue) * (7 - (station.fleets.Count + countingQueue)); //6,10,12
        }
        else if (actionType == ActionType.UpgradeFleet)
        {
            return (unit.level + countingQueue) * (9 - (unit.level + countingQueue)); //8,14,18
        }
        else if (actionType == ActionType.UpgradeStation)
        {
            return ((unit.level + 1 + countingQueue) * 6); //12,18,24
        }
        else if (actionType == ActionType.BidOnModule)
        {
            return playerBid ?? 0;
        }
        else if (actionType == ActionType.UnlockAction)
        {
            return GetUnlockCost(station.maxActions + 1);
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

    internal void ResetAfterSelection()
    {
        Debug.Log($"Resetting UI");
        ToggleMineralText(false);
        UpdateCreateFleetCostText();
        SetModuleGrid();
        ClearMovementPath();
        ClearHighlightRange();
        SelectedNode = null;
        SelectedUnit = null;
        SelectedPath = null;
        unlockPanel.SetActive(false);
        ViewAsteroidInformation(false);
        ViewUnitInformation(false);
        ToggleHPText(false);
        DeselectUnitIcons();
    }
    private void UpdateCreditsAndHexesText()
    {
        GridManager.i.GetScores();
        for (int i = 0; i < Stations.Count; i++)
        {
            var orderObj = TurnOrderObjects[i];
            var credits = orderObj.transform.Find("Credits").GetComponent<TextMeshProUGUI>();
            credits.text = Stations[(TurnNumber - 1 + i) % Stations.Count].credits.ToString();
            var hexes = orderObj.transform.Find("HexesOwned").GetComponent<TextMeshProUGUI>();
            hexes.text = Stations[(TurnNumber - 1 + i) % Stations.Count].score.ToString();
        }
    }
    private IEnumerator MoveOnPath(Unit unitMoving, List<PathNode> path)
    {
        int i = 0;
        unitMoving.currentPathNode.unitOnPath = null;
        ClearHighlightRange();
        var beforeText = turnValue.text;
        bool didFightLastMove = false;
        PathNode currentNode = unitMoving.currentPathNode;
        tapped = false;
        foreach (var nextNode in path)
        {
            i++;
            didFightLastMove = false;
            //Check that no one hacked their client to send back bad results, currently not full working and stopping real moves, todo come back to later
            //if (GridManager.i.GetNeighbors(currentNode, currentNode.minerals > unitMoving.miningLeft).Contains(nextNode) &&
            if (unitMoving == null || !AllUnits.Contains(unitMoving) || unitMoving.movementLeft <= 0)
            {
                break;
            }
            unitMoving.subtractMovement(GridManager.i.GetGCost(nextNode));
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
                isValidHex = nextNode.unitOnPath.unitType == UnitType.Bomb;
                //if enemy, attack, if you didn't destroy them, stay blocked and move back
                if (nextNode.unitOnPath.teamId != unitMoving.teamId)
                {
                    ToggleHPText(true);
                    yield return StartCoroutine(DoCombat(unitMoving, nextNode));
                    ToggleHPText(false);
                    didFightLastMove = true;
                    if (unitMoving == null || !AllUnits.Contains(unitMoving))
                        break;
                    blockedMovement = nextNode.unitOnPath != null && AllUnits.Contains(nextNode.unitOnPath);
                        isValidHex = nextNode.unitOnPath == null;
                    if (!blockedMovement && isValidHex)
                        yield return StartCoroutine(MoveDirectly(unitMoving, nextNode.transform.position, Constants.MovementSpeed / 2));
                    if (i != path.Count() && !blockedMovement && Winner == Guid.Empty)
                        turnValue.text = beforeText;
                }
                //if ally reapir, not yourself
                else if (nextNode.unitOnPath.teamId == unitMoving.teamId && nextNode.unitOnPath.unitGuid != unitMoving.unitGuid)
                {
                    if (nextNode.unitOnPath.unitType == UnitType.Bomb)
                    {
                        nextNode.unitOnPath.DestroyUnit();
                    }
                    else
                    {
                        nextNode.unitOnPath.RegenHP(2);
                    }
                }
            }
            if(isValidHex)
                unitMoving.currentPathNode = nextNode;
            if (!nextNode.isAsteroid && nextNode.ownedByGuid == Guid.Empty)
                unitMoving.currentPathNode.SetNodeColor(unitMoving.playerGuid);
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
            UpdateCreditsAndHexesText();
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
            if(GridManager.i.GetNeighbors(unitMoving.currentPathNode).Any(x => x.ownedByGuid == unitMoving.playerGuid))
                unitMoving.currentPathNode.SetNodeColor(unitMoving.playerGuid);
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
        if (Mathf.Abs(currentNode.transform.position.x - nextNode.transform.position.x) > 5 && currentNode.actualCoords.y == nextNode.actualCoords.y)
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
            while (elapsedTime <= totalTime && !tapped && !skipTurn)
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
        else if (Mathf.Abs(currentNode.transform.position.y - nextNode.transform.position.y) > 5)
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
            while (elapsedTime <= totalTime && !tapped && !skipTurn)
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
        else if (Mathf.Abs(currentNode.transform.position.x - nextNode.transform.position.x) > 5)
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
            while (elapsedTime <= totalTime && !tapped && !skipTurn)
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
        while (elapsedTime <= totalTime && !tapped && !skipTurn)
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

    internal IEnumerator DoCombat(Unit unitMoving, PathNode node)
    {
        var unitOnPath = node.unitOnPath;
        Debug.Log($"{unitMoving.unitName} is attacking {unitOnPath.unitName}");
        CheckEnterCombatModules(unitMoving);
        CheckEnterCombatModules(unitOnPath);
        unitMoving.SetSupportValues();
        unitOnPath.SetSupportValues();
        if (unitMoving.unitType == UnitType.Bomb || unitOnPath.unitType == UnitType.Bomb)
        {
            turnValue.text = PrintCombat(unitMoving, unitOnPath, AttackType.Explosive);
            yield return StartCoroutine(WaitforSecondsOrTap(1));
        }
        else
        {
            phaseText.gameObject.SetActive(true);
            for (int attackType = 0; attackType <= (int)AttackType.Explosive; attackType++)
            {
                if (unitMoving.HP > 0 && unitOnPath.HP > 0)
                {
                    turnValue.text = PrintCombat(unitMoving, unitOnPath, (AttackType)attackType);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                }
            }
        }
        phaseText.gameObject.SetActive(false);
        unitMoving.CheckDestruction(unitOnPath);
    }
    internal IEnumerator FloatingTextAnimation(string text, Transform spawnTransform, Unit unit, bool stagger = false)
    {
        if(stagger)
            yield return new WaitForSeconds(.4f);
        var floatingObj = Instantiate(floatingTextPrefab, spawnTransform);
        var floatingTextObj = floatingObj.transform.Find("FloatingText");
        floatingTextObj.GetComponent<TextMeshPro>().text = text;
        floatingTextObj.GetComponent<TextMeshPro>().color = unit != null ? GridManager.i.playerColors[(int)unit.playerColor] : Color.white;
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
        if (unit.moduleEffects.Contains(ModuleEffect.CombatKinetic3))
        {
            unit.kineticPower += 2;
        }  
        if (unit.moduleEffects.Contains(ModuleEffect.CombatThermal3))
        {
            unit.thermalPower += 2;
        }
        if (unit.moduleEffects.Contains(ModuleEffect.CombatExplosive3))
        {
            unit.explosivePower += 2;
        }
    }
    private void CheckDestroyAsteroidModules(Unit unit)
    {
        if (unit.moduleEffects.Contains(ModuleEffect.AsteroidMining2))
        {
            unit.IncreaseMaxMining(2);
        }
        if (unit.moduleEffects.Contains(ModuleEffect.AsteroidCredits5))
        {
            GetStationByGuid(unit.playerGuid).GainCredits(5,unit,false,false);
        }
    }

    private Quaternion GetDirection(Unit unit, PathNode fromNode, PathNode toNode, Direction? direction = null)
    {
        if (direction == null)
        {
            int x = toNode.actualCoords.x - fromNode.actualCoords.x;
            int y = toNode.actualCoords.y - fromNode.actualCoords.y;
            x = x == 9 ? -1: x == -9 ? 1 : x;
            y = y == 9 ? -1: y == -9 ? 1 : y;
            var offSetCoords = fromNode.offSet.FirstOrDefault(c => c.x == x && c.y == y);
            unit.facing = (Direction)Array.IndexOf(fromNode.offSet, offSetCoords);
        }
        else
        {
            unit.facing = (Direction)direction;
        }
        return Quaternion.Euler(unit.transform.rotation.x, unit.transform.rotation.y, 270 - (unit.facing == Direction.TopRight ? -60 : (int)unit.facing * 60));
    }

    private string PrintCombat(Unit s1, Unit s2, AttackType type)
    {
        var eDmg = s1.GetDamage(s2, type);
        int s1Dmg = eDmg > 0 ? eDmg : 0;
        int s2Dmg = eDmg < 0 ? Mathf.Abs(eDmg) : 0;
        int s1DT = s1.explosiveDamageTaken;
        int s2DT = s2.explosiveDamageTaken;
        string support1Text = ( s1.thermalSupportPower > 0 && !s1.moduleEffects.Contains(ModuleEffect.HiddenStats)) ? $"({s1.explosivePower}+{ s1.explosiveSupportPower})" : "";
        string support2Text = ( s2.thermalSupportPower > 0 && !s2.moduleEffects.Contains(ModuleEffect.HiddenStats)) ? $"({s2.explosivePower}+{ s2.explosiveSupportPower})" : "";
        string power1Text = s1.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s1.explosivePower +  s1.explosiveSupportPower}";
        string power2Text = s2.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s2.explosivePower +  s2.explosiveSupportPower}";
        if (type == AttackType.Kinetic)
        {
            s1DT = s1.kineticDamageTaken;
            s2DT = s2.kineticDamageTaken;
            support1Text = ( s1.thermalSupportPower > 0 && !s1.moduleEffects.Contains(ModuleEffect.HiddenStats)) ? $"({s1.kineticPower}+{ s1.kineticSupportPower})" : "";
            support2Text = ( s2.thermalSupportPower > 0 && !s2.moduleEffects.Contains(ModuleEffect.HiddenStats)) ? $"({s2.kineticPower}+{ s2.kineticSupportPower})" : "";
            power1Text = s1.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s1.kineticPower +  s1.kineticSupportPower}";
            power2Text = s2.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s2.kineticPower +  s2.kineticSupportPower}";
        }
        else if (type == AttackType.Thermal)
        {
            s1DT = s1.thermalDamageTaken;
            s2DT = s2.thermalDamageTaken;
            support1Text = ( s1.thermalSupportPower > 0 && !s1.moduleEffects.Contains(ModuleEffect.HiddenStats)) ? $"({s1.thermalPower}+{ s1.thermalSupportPower})" : "";
            support2Text = ( s2.thermalSupportPower > 0 && !s2.moduleEffects.Contains(ModuleEffect.HiddenStats)) ? $"({s2.thermalPower}+{ s2.thermalSupportPower})" : "";
            power1Text = s1.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s1.thermalPower +  s1.thermalSupportPower}";
            power2Text = s2.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s2.thermalPower +  s2.thermalSupportPower}";
        }
        var returnText = "";
        var modifytext = "";
        if (s1.unitType == UnitType.Bomb && s2.unitType == UnitType.Bomb)
        {
            returnText += $"<b>Both bombs blew up <u>destroying each other</u></b>";
        }
        else if (s1Dmg > 0 && s1.unitType == UnitType.Bomb)
        {
            returnText += $"<b>{s2.playerColor.ToString()} <u>resisted the bombs damage</u></b>";
        }
        else if (s1Dmg > 0)
        {
            var damage = Mathf.Max(s1Dmg + s1DT, 0);
            s1.TakeDamage(damage, s2);
            string hpText = s1.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s1.HP}";
            if (s1DT != 0 && !s1.moduleEffects.Contains(ModuleEffect.HiddenStats))
            {
                var modifierSymbol = s1DT > 0 ? "+" : "";
                modifytext = $"({s1Dmg}{modifierSymbol}{s1DT})";
            }
            returnText += $"<b>{s1.playerColor.ToString()} took <u>{damage}{modifytext} damage</u></b>";
            if (s2.unitType == UnitType.Bomb && damage > 0)
            {
                s1.subtractMovement(1);
                returnText += $", lost 1 movement,";
            }
            returnText += $" and has {hpText} HP left.";
        }
        else if (s2Dmg > 0 && s2.unitType == UnitType.Bomb)
        {
            returnText += $"<b>{s1.playerColor.ToString()} <u>resisted the bombs damage</u></b>";
        }
        else if (s2Dmg > 0)
        {
            var damage = Mathf.Max(s2Dmg + s2DT, 0);
            s2.TakeDamage(damage, s1);
            string hpText = s2.moduleEffects.Contains(ModuleEffect.HiddenStats) ? "?" : $"{s2.HP}";
            if (s2DT != 0 && !s2.moduleEffects.Contains(ModuleEffect.HiddenStats))
            {
                var modifierSymbol = s2DT > 0 ? "+" : "";
                modifytext = $"({s2Dmg}{modifierSymbol}{s2DT})";
            }
            returnText += $"<b>{s2.playerColor.ToString()} took <u>{damage}{modifytext} damage</u></b>";
            if (s1.unitType == UnitType.Bomb && damage > 0)
            {
                s2.subtractMovement(1);
                returnText += $", lost 1 movement,";
            }
            returnText += $" and has {hpText} HP left.";
        }
        else
        {
            if (s1.unitType == UnitType.Bomb)
            {
                returnText += $"<b>{s2.playerColor.ToString()} <u>resisted the bombs damage</u></b>";
            }
            else if (s2.unitType == UnitType.Bomb)
            {
                returnText += $"<b>{s1.playerColor.ToString()} <u>resisted the bombs damage</u></b>";
            }
            else
            {
                returnText += $"<u><b>No damage taken.</u></b>";
            }
        }
        returnText += $"\n\n{s1.playerColor.ToString()} had {power1Text}{support1Text} Power.\n{s2.playerColor.ToString()} had {power2Text}{support2Text} Power.\n";
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

    public void HighlightMovementRange(PathNode currentNode, Unit unit)
    {
        ClearHighlightRange();
        List<PathNode> nodesWithinRange = GridManager.i.GetNodesWithinRange(currentNode, unit.movementLeft, unit.miningLeft);
        foreach (PathNode node in nodesWithinRange)
        {
            GameObject range = Instantiate(node.isAsteroid ? movementMinePrefab : movementRangePrefab, node.transform.position, Quaternion.identity);
            range.transform.SetParent(highlightParent);
            var rangeComponent = range.AddComponent<Node>();
            rangeComponent.Initialize(node);
            currentMovementRange.Add(rangeComponent);
        }
    }

    public void HighlightDeployRange(Unit unit)
    {
        ClearHighlightRange();
        List<PathNode> nodesWithinRange = GetNodesForDeploy(unit,true);
        foreach (PathNode node in nodesWithinRange)
        {
            GameObject range = Instantiate(deployPrefab, node.transform.position, Quaternion.identity);
            range.transform.SetParent(highlightParent);
            var rangeComponent = range.AddComponent<Node>();
            rangeComponent.Initialize(node);
            currentDeployRange.Add(rangeComponent);
        }
    }

    internal List<PathNode> GetNodesForDeploy(Unit unit, bool queuing)
    {
        PathNode currentNode = queuing ? MyStation.actions.FirstOrDefault(x => (x.actionType == ActionType.MoveAndMine || x.actionType == ActionType.MoveUnit) && x.selectedUnit.unitGuid == unit.unitGuid)?.selectedPath?.Where(x=>!x.isAsteroid).LastOrDefault() ?? unit.currentPathNode : unit.currentPathNode;
        return GridManager.i.GetNodesWithinRange(currentNode, unit.deployRange, 0).ToList();
    }

    private void UpdateCurrentTurn(GameTurn turn)
    {
        if (turn != null)
        {
            var index = Globals.GameMatch.GameTurns.Select(x => x.TurnNumber).ToList().IndexOf(TurnNumber);
            if (index == -1)
            {
                Globals.GameMatch.GameTurns.Add(turn);
            }
            else
            {
                Globals.GameMatch.GameTurns[index] = turn;
            }
        }
    }
    public void ShowTurnArchive(bool active)
    {
        if (!isEndingTurn)
        {
            turnArchivePanel.SetActive(active);
            GridManager.i.CloseBurger();
        }
    }
    //public void ToggleAll()
    //{
    //    if (!isEndingTurn)
    //    {
    //        infoToggle = !infoToggle;
    //        ToggleMineralText(infoToggle);
    //        ToggleHPText(infoToggle);
    //    }
    //}
#region Complete Turn
    public void EndTurn(bool theyAreSure)
    {
        GridManager.i.CloseBurger();
        if (!isEndingTurn)
        {
            if (theyAreSure || MyStation.actions.Count == MyStation.maxActions)
            {
                ShowAreYouSurePanel(false);
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
                        if (k == Globals.localStationColor)
                            actionOrders.Add(i+j+1);
                    }
                }
                GameTurn gameTurn = new GameTurn()
                {
                    GameGuid = Globals.GameMatch.GameGuid,
                    TurnNumber = TurnNumber,
                    ModulesForMarket = Globals.GameMatch.GameTurns.LastOrDefault().ModulesForMarket,
                    MarketModuleGuids = Globals.GameMatch.GameTurns.LastOrDefault().MarketModuleGuids,
                    AllModules = AllModules.Select(x => x.ToServerModule()).ToList(),
                    AllNodes = GridManager.i.AllNodes.Select(x => x.ToServerNode()).ToList(),
                    Players = new List<GamePlayer>()
                    {
                        new GamePlayer()
                        {
                            GameGuid = Globals.GameMatch.GameGuid,
                            TurnNumber = TurnNumber,
                            PlayerColor = Globals.localStationColor,
                            PlayerGuid = Globals.Account.PlayerGuid,
                            PlayerName = Globals.Account.Username.Split(' ')[0],
                            Actions = MyStation.actions.Select((x, j) =>
                                new ServerAction()
                                {
                                    GameGuid = Globals.GameMatch.GameGuid,
                                    TurnNumber = TurnNumber,
                                    PlayerGuid = Globals.Account.PlayerGuid,
                                    ActionOrder = actionOrders[j],
                                    ActionTypeId = (int)x.actionType,
                                    GeneratedGuid = x.generatedGuid,
                                    XList = String.Join(",", x.selectedPath?.Select(x => x.actualCoords.x)),
                                    YList = String.Join(",", x.selectedPath?.Select(x => x.actualCoords.y)),
                                    SelectedModuleGuid = x.selectedModuleGuid,
                                    SelectedUnitGuid = x.selectedUnit?.unitGuid,
                                    PlayerBid = x.playerBid,
                                }).ToList(),
                            Technology = MyStation.technology.Select(x => x.ToServerTechnology()).ToList(),
                            Units = MyStation.GetServerUnits(),
                            ModulesGuids = String.Join(",", MyStation.modules.Select(x => x.moduleGuid)),
                            Credits = MyStation.credits,
                            BonusKinetic = MyStation.bonusKinetic,
                            BonusThermal = MyStation.bonusThermal,
                            BonusExplosive = MyStation.bonusExplosive,
                            BonusMining = MyStation.bonusMining,
                            Score = MyStation.score,
                            FleetCount = MyStation.fleetCount,
                            MaxActions = MyStation.maxActions
                        }
                    }
                };
                //add effects back in case player wants to resubmit their turn
                for (int i = 0; i < lastSubmittedTurn.Count(); i++)
                {
                    PerformUpdates(lastSubmittedTurn[i], Constants.Create, true, true);
                }
                if ((Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == TurnNumber)?.Players?.Count() ?? 0) == Globals.GameMatch.MaxPlayers - 1)
                    lastToSubmitTurn = true;
                submitTurnPanel.SetActive(true);
                var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameTurn);
                StartCoroutine(sql.PostRoutine<bool>($"Game/EndTurn?clientVersion={Constants.ClientVersion}", stringToPost, SubmitResponse));
            }
            else
            {
                ShowAreYouSurePanel(true);
            }
        }
    }

    private IEnumerator DoEndTurn()
    {
        isEndingTurn = true;
        AllUnits.Where(x => x.unitType != UnitType.Bomb).ToList().ForEach(x => x.trail.enabled = true);
        customAlertPanel.SetActive(false); 
        submitTurnPanel.SetActive(false);
        ToggleMineralText(false);
        ToggleHPText(false);
        turnValue.text = $"Turn #{TurnNumber} Complete!";
        for(int i=0;i<Stations.Count;i++)
        {
            TurnOrderObjects[i].transform.Find("ReadyState").gameObject.SetActive(false);
        }
        turnLabel.SetActive(true);
        yield return StartCoroutine(WaitforSecondsOrTap(1));
        var turnsFromServer = Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == TurnNumber)?.Players;
        if (turnsFromServer != null)
        {
            //Remove effects from queued actions so we can reapply them with visuals
            for (int i = lastSubmittedTurn.Count() - 1; i >= 0; i--)
            {
                PerformUpdates(lastSubmittedTurn[i], Constants.Remove, true);
            }
            ClearModules();
            ToggleMineralText(true);
            ClearTurnArchiveObjects();
            var serverActions = turnsFromServer.SelectMany(x => x.Actions).OrderBy(x => x.ActionOrder);
            foreach (var serverAction in serverActions)
            {
                if (Winner != Guid.Empty){ break; }
                yield return StartCoroutine(PerformAction(new Action(serverAction)));
            }
            if (Winner == Guid.Empty)
            {
                for (int i = 0; i < turnsFromServer.Count(); i++)
                {
                    Stations[i].GainCredits(Constants.MaxActions - turnsFromServer.FirstOrDefault(x=>x.PlayerColor == i).Actions.Count(), Stations[i], false, false);
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
                Winner = GridManager.i.CheckForWin();
            }
            yield return StartCoroutine(sql.GetRoutine<Guid>($"Game/EndGame?gameGuid={Globals.GameMatch.GameGuid}&winner={Winner}&clientVersion={Constants.ClientVersion}", GetWinner));
        }
    }

    private void PerformUpdates(Action action, int modifier, bool queued = false, bool addingBack = false)
    {
        var station = GetStationByGuid(action.selectedUnit.playerGuid);
        station.GainCredits(-1 * action.costOfAction * modifier, station, queued);
        if (action.actionType == ActionType.AttachModule)
        {
            var actionModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModuleGuid);
            Unit UnitToRemoveFrom;
            List<Module> inventoryRemoveFrom;
            if (action._parentGuid == null)
            {
                UnitToRemoveFrom = AllUnits.FirstOrDefault(x => x.attachedModules.Any(x => x.moduleGuid == actionModule.moduleGuid));
                if (UnitToRemoveFrom != null)
                {
                    inventoryRemoveFrom = UnitToRemoveFrom.attachedModules;
                }
                else
                {
                    UnitToRemoveFrom = GetStationByGuid(action.selectedUnit.playerGuid);
                    inventoryRemoveFrom = GetStationByGuid(action.selectedUnit.playerGuid).modules;
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
            actionModule.EditUnitStats(action.selectedUnit, modifier);
            if (!action._statonInventory)
                actionModule.EditUnitStats(UnitToRemoveFrom, modifier * -1);
            if (modifier == Constants.Create)
            {
                action._parentGuid = UnitToRemoveFrom.unitGuid;
                action.selectedUnit.attachedModules.Add(actionModule);
                inventoryRemoveFrom.Remove(inventoryRemoveFrom.FirstOrDefault(x => x.moduleGuid == actionModule.moduleGuid));
            }
            else
            {
                inventoryRemoveFrom.Add(actionModule);
                action.selectedUnit.attachedModules.Remove(action.selectedUnit.attachedModules.FirstOrDefault(x => x.moduleGuid == actionModule.moduleGuid));
                action._statonInventory = false;
                action._parentGuid = null;
            }
        }
        else if (action.actionType == ActionType.SwapModule)
        {
            var actionModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModuleGuid);
            Module dettachedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.generatedGuid);
            Unit unitToRemoveFrom = AllUnits.FirstOrDefault(x => x.attachedModules.Any(x => x.moduleGuid == (modifier == Constants.Create ? actionModule.moduleGuid : action.generatedGuid)));
            List<Module> inventoryRemoveFrom;
            if (unitToRemoveFrom == null)
            {
                inventoryRemoveFrom = GetStationByGuid(action.selectedUnit.playerGuid).modules;
            }
            else
            {
                inventoryRemoveFrom = unitToRemoveFrom?.attachedModules;
            }
            actionModule.EditUnitStats(action.selectedUnit, modifier);
            dettachedModule.EditUnitStats(action.selectedUnit, modifier * -1);
            if (unitToRemoveFrom != null)
            {
                actionModule.EditUnitStats(unitToRemoveFrom, modifier * -1);
                dettachedModule.EditUnitStats(unitToRemoveFrom, modifier);
            }
            if (modifier == Constants.Create)
            {
                //attach new
                action.selectedUnit.attachedModules.Add(actionModule);
                inventoryRemoveFrom.Remove(inventoryRemoveFrom.FirstOrDefault(x => x.moduleGuid == actionModule.moduleGuid));
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
                inventoryRemoveFrom.Add(actionModule);
                action.selectedUnit.attachedModules.Remove(action.selectedUnit.attachedModules.FirstOrDefault(x => x.moduleGuid == actionModule.moduleGuid));
            }
        }
        else if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine)
        {
            if (modifier == Constants.Remove)
            {
                action.selectedUnit.subtractMovement(action.selectedPath.Count * modifier);
                action.selectedUnit.ClearMinedPath(action.selectedPath);
                action.selectedUnit._selectedPath = new List<PathNode>();
            }
            else
            {
                if (addingBack)
                    action.selectedUnit.subtractMovement(action.selectedPath.Count * modifier);
                action.selectedUnit.AddMinedPath(action.selectedPath);
                action.selectedUnit._selectedPath = action.selectedPath;
            }
        }
        else if (action.actionType == ActionType.DeployBomb)
        {
            action.selectedUnit.subtractMovement(modifier);
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
            action.selectedUnit.RegenHP(5 * modifier, queued);
        } else if (action.actionType == ActionType.UnlockAction) 
        {
            station.maxActions += modifier;
            ClearActionBar();
        }
        UpdateCreditsAndHexesText();
    }

    private void SubmitResponse(bool response, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }

        if (response)
        {
            //TurnOrderObjects[(TurnNumber - 1 + (int)MyStation.playerColor) % Stations.Count].transform.Find("ReadyState").gameObject.SetActive(true);
            endTurnButton.sprite = endTurnButtonPressed;
            var plural = Globals.GameMatch.MaxPlayers > 2 ? "s" : "";
            customAlertText.text = $"Your orders were successfully transmitted to your units!";
            customAlertPanel.SetActive(true);
            submitTurnPanel.SetActive(false);
        }
        else
        {
            customAlertText.text = $"Your orders were NOT transmitted to your units.\n\n Please try again.";
            customAlertPanel.SetActive(true);
            submitTurnPanel.SetActive(false);
        }
    }
    public void ExitToLobby()
    {
        PlayerPrefs.SetString("GameGuid", "");
        PlayerPrefs.Save();
        SceneManager.LoadScene((int)Scene.Lobby);
    }
    public void ExitGame(int exitOption)
    {
        if (exitOption == 0)
        {
            if (Winner == Guid.Empty)
                exitPanel.SetActive(true);
            else
                ExitToLobby();
            GridManager.i.CloseBurger();
        }
        else if (exitOption == 1)
        {
            ExitToLobby();
        }
        else if (exitOption == 2)
        {
            forfietPanel.SetActive(true);
        }
        else if (exitOption == 3)
        {
            StartCoroutine(sql.GetRoutine<int>($"Game/EndGame?gameGuid={Globals.GameMatch.GameGuid}&winner={Stations.LastOrDefault(x => x.teamId != MyStation.teamId).playerGuid}&clientVersion={Constants.ClientVersion}"));
            loadingPanel.SetActive(true);
            EndTurn(true);
            ExitToLobby();
        }
        else if (exitOption == 4)
        {
            exitPanel.SetActive(false);
            forfietPanel.SetActive(false);
        }
    }

    private void GetWinner(Guid _winner, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
        else
        {
            Winner = _winner;
            if (Winner != Guid.Empty)
            {
                nextActionButton.SetActive(false);
                skipActionsButton.SetActive(false);
                ToggleHPText(true);
                turnValue.text = $"{GetStationByGuid(Winner).playerColor.ToString()} player won after {TurnNumber} turns";
                Debug.Log($"{GetStationByGuid(Winner).playerColor.ToString()} player won after {TurnNumber} turns");
                //Add EOG Stats
            }
            else
            {
                previousTurnButton.interactable = true;
                StartTurn();
            }
        }
    }

    private IEnumerator PerformAction(Action action)
    {
        tapped = false;
        var unit = action.selectedUnit;
        turnValue.text = $"{unit.unitName}:\n";
        if (unit != null && AllUnits.Contains(unit))
        {
            var currentStation = GetStationByGuid(unit.playerGuid);
            action.costOfAction = GetCostOfAction(action.actionType, unit, false, action.playerBid);
            if (currentStation.credits >= action.costOfAction)
            {
                if (action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine)
                {
                    isMoving = true;
                    if (unit != null && action.selectedPath != null && action.selectedPath.Count > 0 && action.selectedPath.Count <= unit.getMaxMovementRange())
                    {
                        turnValue.text += $"{GetDescription(action.actionType)}";
                        turnArchive.Add(new Tuple<string, string>($"Action {action.actionOrder}({unit.playerColor.ToString()}): {GetDescription(action.actionType)}", turnValue.text));
                        Debug.Log("Moving to position: " + unit.currentPathNode.transform.position);
                        yield return StartCoroutine(MoveOnPath(unit, action.selectedPath));
                    }
                    else
                    {
                        turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                        turnArchive.Add(new Tuple<string, string>($"Action {action.actionOrder}({unit.playerColor.ToString()}): {GetDescription(action.actionType)}", turnValue.text));
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                    }
                    isMoving = false;
                }
                else if (action.actionType == ActionType.DeployFleet)
                {
                    if (IsUnderMaxFleets(currentStation, false))
                    {
                        turnValue.text += $"{GetDescription(action.actionType)}";
                        var newFleet = GridManager.i.Deploy(currentStation, (Guid)action.generatedGuid, action.selectedPath.FirstOrDefault().actualCoords, UnitType.Bomber);
                        if (newFleet != null)
                        {
                            currentStation.GainCredits(-1 * action.costOfAction, currentStation);
                            StartCoroutine(FloatingTextAnimation($"New Fleet", newFleet.transform, newFleet));
                        }
                        else
                        {
                            turnValue.text += "\nNo valid location to spawn bomber.";
                        }
                    }
                    else
                    {
                        turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                        Debug.Log($"{unit.unitName} not eligible for {GetDescription(action.actionType)}");
                    }
                    unit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    unit.selectIcon.SetActive(false);
                }
                else if (action.actionType == ActionType.DeployBomb)
                {
                    if (unit.movementLeft > 0)
                    {
                        turnValue.text += $"{GetDescription(action.actionType)}";
                        var newBomb = GridManager.i.Deploy(unit, (Guid)action.generatedGuid, action.selectedPath.FirstOrDefault().actualCoords, UnitType.Bomb);
                        if (newBomb != null)
                        {
                            PerformUpdates(action, Constants.Create);
                            StartCoroutine(FloatingTextAnimation($"New Bomb", newBomb.transform, newBomb));
                            unit.selectIcon.SetActive(true);
                            yield return StartCoroutine(WaitforSecondsOrTap(1));
                            unit.selectIcon.SetActive(false);
                            if (newBomb.currentPathNode.unitOnPath.unitGuid != newBomb.unitGuid)
                                yield return StartCoroutine(DoCombat(newBomb, newBomb.currentPathNode));
                        }
                        else
                        {
                            turnValue.text += "\nNo valid location to spawn Bomb.";
                            unit.selectIcon.SetActive(true);
                            yield return StartCoroutine(WaitforSecondsOrTap(1));
                            unit.selectIcon.SetActive(false);
                        }
                    }
                    else
                    {
                        turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                        unit.selectIcon.SetActive(true);
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                        unit.selectIcon.SetActive(false);
                    }
                }
                else if (action.actionType == ActionType.UpgradeFleet || action.actionType == ActionType.UpgradeStation)
                {
                    if (CanLevelUp(unit, action.actionType, false))
                    {
                        turnValue.text += $"{GetDescription(action.actionType)}";
                        StartCoroutine(FloatingTextAnimation($"Upgraded", unit.transform, unit));
                        PerformUpdates(action, Constants.Create);
                    }
                    else
                    {
                        turnValue.text += $"Could not perform {GetDescription(action.actionType)}";
                    }
                    unit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    unit.selectIcon.SetActive(false);
                }
                else if (action.actionType == ActionType.BidOnModule)
                {
                    if (action.selectedModuleGuid != null)
                    {
                        var actionModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModuleGuid);
                        var wonModule = new Module(actionModule.moduleId, actionModule.moduleGuid);
                        turnValue.text += $"Won bid\n\nPaid {action.playerBid} credits for:\n{wonModule.effectText}";
                        StartCoroutine(FloatingTextAnimation($"Won Bid", unit.transform, unit));
                        PerformUpdates(action, Constants.Create);
                        currentStation.modules.Add(wonModule);
                    }
                    else
                    {
                        StartCoroutine(FloatingTextAnimation($"Lost Bid", unit.transform, unit));
                        turnValue.text += "Lost bid\nGained 1 credit.";
                        currentStation.GainCredits(1, currentStation);
                    }
                    unit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    unit.selectIcon.SetActive(false);
                }
                else if (action.actionType == ActionType.AttachModule)
                {
                    if (unit.attachedModules.Count <= unit.maxAttachedModules)
                    {
                        Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModuleGuid);
                        if (selectedModule != null)
                        {
                            turnValue.text += $"{GetDescription(action.actionType)}.\n\nModule effect:\n";
                            if (unit.moduleEffects.Contains(ModuleEffect.HiddenStats))
                            {
                                turnValue.text += $"???";
                            }
                            else
                            {
                                turnValue.text += $"{selectedModule.effectText}";
                            }
                            StartCoroutine(FloatingTextAnimation($"+Module", unit.transform, unit));
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
                    unit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    unit.selectIcon.SetActive(false);
                }
                else if (action.actionType == ActionType.SwapModule)
                {
                    if (unit.attachedModules.Count > 0)
                    {
                        Module selectedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.selectedModuleGuid);
                        Module dettachedModule = AllModules.FirstOrDefault(x => x.moduleGuid == action.generatedGuid);
                        if (selectedModule != null && dettachedModule != null)
                        {
                            turnValue.text += $"{GetDescription(action.actionType)}.\n\nModule effect:\n";
                            if (unit.moduleEffects.Contains(ModuleEffect.HiddenStats))
                            {
                                turnValue.text += $"???";
                            }
                            else
                            {
                                turnValue.text += $"{selectedModule.effectText}";
                            }
                            StartCoroutine(FloatingTextAnimation($"Module Swap", unit.transform, unit));
                            PerformUpdates(action, Constants.Create);
                        }
                        else
                        {
                            turnValue.text += $"Could not perform {GetDescription(action.actionType)}, module not available";
                        }
                        unit.selectIcon.SetActive(true);
                        yield return StartCoroutine(WaitforSecondsOrTap(1));
                        unit.selectIcon.SetActive(false);
                    }
                    else
                    {
                        turnValue.text += $"Could not perform {GetDescription(action.actionType)}, module not attached";
                    }
                }
                else if (action.actionType == ActionType.RepairFleet)
                {
                    turnValue.text += $"{GetDescription(action.actionType)}\n\n";
                    PerformUpdates(action, Constants.Create);
                    turnValue.text += $"New HP/Max: {Mathf.Min(unit.maxHP, unit.HP)}/{unit.maxHP}";
                    unit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    unit.selectIcon.SetActive(false);
                }
                else if (TechActions.Contains(action.actionType))
                {
                    var tech = currentStation.technology[(int)action.actionType - Constants.MinTech];
                    turnValue.text += $"{GetDescription(action.actionType)}\n\n{tech.effectText} ({tech.currentAmount + 1}/{tech.neededAmount})";
                    StartCoroutine(FloatingTextAnimation($"+Tech", unit.transform, unit));
                    PerformUpdates(action, Constants.Create);
                    unit.selectIcon.SetActive(true);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
                    unit.selectIcon.SetActive(false);
                }
                else if (action.actionType == ActionType.UnlockAction)
                {
                    turnValue.text += $"{GetDescription(action.actionType)}";
                    StartCoroutine(FloatingTextAnimation($"+Action", unit.transform, unit));
                    PerformUpdates(action, Constants.Create);
                    yield return StartCoroutine(WaitforSecondsOrTap(1));
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
                Debug.Log($"Broke ass {unit.playerColor.ToString()} bitch couldn't afford {GetDescription(action.actionType)}");
                yield return StartCoroutine(WaitforSecondsOrTap(1));
            }
        }
        else
        {
            turnValue.text = $"The unit that queued {GetDescription(action.actionType)} was destroyed";
            yield return StartCoroutine(WaitforSecondsOrTap(1));
        }
        if (!(action.actionType == ActionType.MoveUnit || action.actionType == ActionType.MoveAndMine))
            turnArchive.Add(new Tuple<string, string>($"Action {action.actionOrder}({unit.playerColor.ToString()}): {GetDescription(action.actionType)}", turnValue.text));

    }
    internal IEnumerator WaitforSecondsOrTap(float time)
    {
        float elapsedTime = 0f;
        float totalTime = time;
        nextActionButton.SetActive(true);
        skipActionsButton.SetActive(true);
        tapped = false;
        while (!tapped && !skipTurn)
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
        lastToSubmitTurn = false;
        UpdateModulesFromServer(TurnNumber);
        TurnNumber++;
        skipTurn = false;
        TurnNumberText.text = $"Turn #{TurnNumber.ToString()}";
        endTurnButton.sprite = endTurnButtonNotPressed;
        foreach (var unit in AllUnits)
        {
            if (unit.unitType != UnitType.Bomb)
            {
                if (unit.unitType == UnitType.Station)
                {
                    var station = unit as Station;
                    //station.AOERegen(1);
                    station.actions.Clear();
                    for(int i = 1; i <= Constants.MaxActions; i++)
                    {
                        int unlockCost = GetUnlockCost(i);
                        if (unlockCost < 0)
                        {
                            station.maxActions = Math.Max(station.maxActions, i);
                        }
                    }
                }
                unit.trail.enabled = false;
                var healing = 0;
                if (!unit.hasTakenDamage)
                    healing += 1;
                if(unit.movementLeft > 0)
                    healing += unit.movementLeft * 2;
                unit.RegenHP(healing);
                unit.hasMoved = false;
                unit.hasTakenDamage = false;
                GetStationByGuid(unit.playerGuid).GainCredits(unit.globalCreditGain, unit, TurnNumber == 1);
                unit.resetMining();
                unit.resetMovementRange();
            }
            unit._selectedPath.Clear();
            unit._minedPath.Clear();
            unit.HP = Mathf.Min(unit.maxHP, unit.HP);
        }
        turnLabel.SetActive(false);
        ClearActionBar();
        GridManager.i.GetScores();
        moduleMarket.SetActive(false);
        technologyPanel.SetActive(false);
        BuildTurnObjects();
        ResetAfterSelection();
        ToggleHPText(false);
        DeselectUnitIcons();
        isEndingTurn = false;
        customAlertPanel.SetActive(false);
        if (Globals.GameMatch.MaxPlayers == 1) { UpdateGameTurnStatus(null,""); }
        Debug.Log($"New Turn {TurnNumber} Starting");
    }

    private void BuildTurnObjects()
    {
        ClearTurnOrderObjects();
        for (int i = 0; i < Stations.Count; i++)
        {
            var station = Stations[(TurnNumber - 1 + i) % Stations.Count];
            var playerColor = GridManager.i.playerColors[(int)station.playerColor];
            var orderObj = Instantiate(turnOrderPrefab, turnOrder);
            var readystate = orderObj.transform.Find("ReadyState").GetComponent<Image>();
            readystate.color = playerColor;
            readystate.gameObject.SetActive(Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == TurnNumber)?.Players?.FirstOrDefault(x => x.PlayerGuid == station.playerGuid) != null);
            var credits = orderObj.transform.Find("Credits").GetComponent<TextMeshProUGUI>();
            credits.text = station.credits.ToString();
            credits.color = playerColor;
            var hexes = orderObj.transform.Find("HexesOwned").GetComponent<TextMeshProUGUI>();
            hexes.text = station.score.ToString();
            hexes.color = playerColor;
            //orderObj.transform.Find("HexText").GetComponent<TextMeshProUGUI>().color = playerColor;
            //orderObj.transform.Find("CreditText").GetComponent<TextMeshProUGUI>().color = playerColor;
            TurnOrderObjects.Add(orderObj);
        }
    }
    #endregion
    private void LevelUpUnit(Unit unit, int modifier)
    {
        if (modifier == Constants.Remove)
            unit.level--;
        unit.maxAttachedModules += modifier;
        unit.maxAttachedModules = Mathf.Min(unit.maxAttachedModules, Constants.MaxModules);
        unit.maxAttachedModules = Mathf.Max(unit.maxAttachedModules, Constants.MinModules);
        if (unit.level == 1)
        {
            unit.IncreaseMaxMining(2 * modifier);
        }
        else if (unit.level == 2)
        {
            unit.IncreaseMaxMovement(1 * modifier);
        }
        else if (unit.level == 3)
        {
            unit.deployRange += (1 * modifier);
        }
        unit.IncreaseMaxHP(3 * modifier);
        unit.kineticPower += (2 * modifier);
        unit.explosivePower += (2 * modifier);
        unit.thermalPower += (2 * modifier);
        unit.kineticDeployPower += (1 * modifier);
        unit.thermalDeployPower += (1 * modifier);
        unit.explosiveDeployPower += (1 * modifier);
        if (modifier == Constants.Create)
            unit.level++;
        var spriteRenderer = unit.unitImage.GetComponent<SpriteRenderer>();
        if (unit is Station)
        {
            spriteRenderer.sprite = GridManager.i.stationSprites[unit.level - 1];
            spriteRenderer.color = GridManager.i.playerColors[(int)unit.playerColor];
        }
        else
        {
            spriteRenderer.sprite = GridManager.i.fleetSprites[(int)unit.playerColor, unit.level-1];
        }
        
    }
    public void DeployBomb()
    {
        if (SelectedUnit != null && SelectedUnit.movementLeft > 0)
        {
            if (!HasQueuedMovement(SelectedUnit))
                ResetSelectedUnitMovement();
            HighlightDeployRange(SelectedUnit);
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
            moduleObject.transform.Find("OnUnit").gameObject.SetActive(false);
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
    internal void ClearHighlightRange()
    {
        while(currentMovementRange.Count > 0) { Destroy(currentMovementRange[0].gameObject); currentMovementRange.RemoveAt(0); }
        while (currentDeployRange.Count > 0) { Destroy(currentDeployRange[0].gameObject); currentDeployRange.RemoveAt(0); }
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
       foreach (var child in TechnologyObjects) { child.transform.SetParent(null); }
    }
    private void ClearTurnOrderObjects()
    {
        while (TurnOrderObjects.Count > 0) { Destroy(TurnOrderObjects[0].gameObject); TurnOrderObjects.RemoveAt(0); }
    }

    private void ClearActionBar()
    {
        for (int i = 0; i < Constants.MaxActions; i++)
        {
            ActionBar.Find($"Action{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
            ActionBar.Find($"Action{i}/Remove").gameObject.SetActive(false);
            if (i < MyStation.actions.Count)
                AddActionBarImage(MyStation.actions[i].actionType, i);
            else if (i < MyStation.maxActions)
                ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = emptyModuleBar[i];
            else
            {
                int unlockCost = GetUnlockCost(i + 1);
                ActionBar.Find($"Action{i}/Image").GetComponent<Image>().sprite = lockActionBar;
                ActionBar.Find($"Action{i}/Image").GetComponent<Button>().onClick.AddListener(() => ShowUnlockPanel(unlockCost));
            }
        }
    }
    public int GetUnlockCost(int i)
    {
        if (i == 3) { return 27 - (3 * TurnNumber); }
        if (i == 4) { return 48 - (3 * TurnNumber); }
        if (i == 5) { return 63 - (3 * TurnNumber); }
        return -1;
    }
    private void SetModuleBar(Unit unit)
    {
        canvas.Find("ModuleInfoPanel/Background/Foreground").GetComponent<Image>().color = GridManager.i.uiColors[(int)unit.playerColor];
        for (int i = 0; i < Constants.MaxModules; i++)
        {
            UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.RemoveAllListeners();
            UnitModuleBar.Find($"Module{i}/Remove").GetComponent<Button>().onClick.RemoveAllListeners();
            if (i < unit.maxAttachedModules)
            {
                if (i < unit.attachedModules.Count)
                {
                   
                    UnitModuleBar.Find($"Module{i}/Remove").gameObject.SetActive(unit.playerGuid == MyStation.playerGuid && !MyStation.actions.Any(x=>x.selectedModuleGuid == unit.attachedModules[i]?.moduleGuid || x.generatedGuid == unit.attachedModules[i]?.moduleGuid));
                    if (unit.teamId != MyStation.teamId && unit.moduleEffects.Contains(ModuleEffect.HiddenStats) && !Constants.SpyModules.Contains(unit.attachedModules[i].moduleId))
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
                    if (unit.playerGuid == MyStation.playerGuid)
                    {
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = attachModuleBar;
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Button>().onClick.AddListener(() => ModifyModule(ActionType.AttachModule));
                    }
                    else
                    {
                        UnitModuleBar.Find($"Module{i}/Image").GetComponent<Image>().sprite = emptyModuleBar[i];
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
    public Station GetStationByGuid(Guid playerGuid)
    {
        return Stations.FirstOrDefault(x => x.playerGuid == playerGuid);
    }

    public void SkipTurns()
    {
        skipTurn = true;
    }

    public void ToggleAudio()
    {
        audioToggleOn = !audioToggleOn;
        PlayerPrefs.SetString("AudioOn", $"{audioToggleOn}");
        PlayerPrefs.Save();
        Audio.sprite = audioToggleOn ? Resources.Load<Sprite>("Sprites/UI/soundOff") : Resources.Load<Sprite>("Sprites/UI/soundOn");
    }

    public void UnlockActionButton()
    {
        QueueAction(new Action(ActionType.UnlockAction, null, null));
    }
}
