using NUnit.Framework.Internal;
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
    public GameObject selectModulePanel;

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
    private Unit SelectedStructure;
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
    //private TextMeshProUGUI kineticArmorValue;
    //private TextMeshProUGUI thermalArmorValue;
    //private TextMeshProUGUI explosiveArmorValue;
    private TextMeshProUGUI turnValue;
    private TextMeshProUGUI moduleInfoValue;
    private TextMeshProUGUI fightText;
    public TextMeshProUGUI ScoreToWinText;
    public TextMeshProUGUI TurnOrderText;
    public TextMeshProUGUI ColorText;
    public TextMeshProUGUI CreditText;
    public TextMeshProUGUI alertText;
 
    internal List<Unit> AllStructures = new List<Unit>();
    internal List<Module> AllModules = new List<Module>();
    internal int winner = -1;
    internal Station MyStation {get {return Stations[Globals.localStationIndex];}}
    public GameObject turnLabel;
    private SqlManager sql;
    private int TurnNumber = 0;
    private Turn[]? TurnsFromServer;
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
    public void ViewFightPanel(bool active)
    {
        fightPanel.SetActive(active);
    }
    
    public void ShowAlertPanel(int unlock)
    {
        alertPanel.SetActive(true);
        alertText.text = $"This action slot will unlock once you own {Convert.ToInt32(Math.Floor(GridManager.i.scoreToWin * (.25 * unlock)))} hexes.";
    }
    public void HideAlertPanel()
    {
        alertPanel.SetActive(false);
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
        fightText = fightPanel.transform.Find("Panel/FightText").GetComponent<TextMeshProUGUI>();
        alertText = alertPanel.transform.Find("Background/AlertText").GetComponent<TextMeshProUGUI>();
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
                        if (targetNode != null && SelectedStructure != null && !targetNode.isAsteroid && currentMovementRange.Select(x => x.currentPathNode).Contains(targetNode) && !MyStation.actions.Any(x => x.actionType == ActionType.MoveStructure && x.selectedStructure?.structureGuid == SelectedStructure.structureGuid))
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
                                ResetAfterSelection();
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
        if (SelectedStructure != null 
            && MyStation.modules.Count > 0 
            && (SelectedStructure.attachedModules.Count+MyStation.actions.Count(x=>x.actionType == ActionType.AttachModule)) < SelectedStructure.maxAttachedModules 
            && MyStation.stationId == SelectedStructure.stationId)
        {
            ClearSelectableModules();
            List<Guid> assignedModules = MyStation.actions.SelectMany(x => x.selectedModulesIds).ToList();
            var availableModules = MyStation.modules.Where(x => !assignedModules.Contains(x.moduleGuid)).ToList();
            if(availableModules.Count > 0){
                foreach (var module in availableModules)
                {
                    var selectedStructure = SelectedStructure;
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

    public void SetTextValues(Unit structure)
    {
        nameValue.text = structure.structureName;
        levelValue.text = structure.level.ToString();
        hpValue.text = structure.hp + "/" + structure.maxHp;
        rangeValue.text = structure.maxRange.ToString();
        kineticAttackValue.text = structure.kineticAttack.ToString();
        thermalAttackValue.text = structure.thermalAttack.ToString();
        explosiveAttackValue.text = structure.explosiveAttack.ToString();
        miningValue.text = structure.mining.ToString();
        //kineticArmorValue.text = structure.kineticAttack.ToString();
        //thermalArmorValue.text = structure.thermalAttack.ToString();
        //explosiveArmorValue.text = structure.explosiveAttack.ToString();
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
            mineButton.gameObject.SetActive(true);
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
        return $"(Costs {cost} Credit{plural})";
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
    private void UpdateGenerateModuleCostText()
    {
        var actionCost = GetCostOfAction(ActionType.GenerateModule, MyStation, false);
        generateModuleCost.text = GetCostText(actionCost);
        generateModuleButton.interactable = MyStation.credits >= actionCost;
    }
    public void UpgradeStructure()
    {
        ActionType actionType = SelectedStructure is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet;
        if (CanQueueUpgrade(SelectedStructure, actionType) && HasGameStarted()){
            QueueAction(actionType);
        }
    }
    private bool CanQueueUpgrade(Unit structure, ActionType actionType)
    {
        return CanLevelUp(structure, actionType, true) && MyStation.credits >= GetCostOfAction(actionType, structure, true);
    }

    private bool CanLevelUp(Unit structure, ActionType actionType, bool countQueue)
    {
        var countingQueue = countQueue ? Stations[structure.stationId].actions.Where(x => x.selectedStructure.structureGuid == structure.structureGuid && x.actionType == actionType).Count() : 0;
        var nextLevel = (structure.level + countingQueue);
        if (actionType == ActionType.UpgradeFleet)
            return nextLevel < Stations[structure.stationId].level;
        else
            return nextLevel < 6;
    }

    private bool CanQueueBuildFleet(Station station)
    {
        return CanBuildAdditonalFleet(station) && station.credits >= GetCostOfAction(ActionType.CreateFleet, station, true);
    }
    private bool CanBuildAdditonalFleet(Station station)
    {
        return station.fleets.Count + station.actions.Where(x => x.actionType == ActionType.CreateFleet).Count() < station.maxFleets;
    }
    private int GetCostOfAction(ActionType actionType, Unit structure, bool countQueue)
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
        else if (actionType == ActionType.GenerateModule)
        {
            return 2;
        }
        else
        {
            return 0;
        }
    }

    public void GenerateModule()
    {
        if (HasGameStarted() && MyStation.credits >= GetCostOfAction(ActionType.GenerateModule, MyStation, false))
        {
            QueueAction(ActionType.GenerateModule);
        }
    }
    
    public void MineAsteroid()
    {
        QueueAction(ActionType.MineAsteroid);
    }

    private void QueueAction(ActionType actionType, List<Guid> selectedModules = null, Unit _structure = null)
    {
        if (MyStation.actions.Count < MyStation.maxActions)
        {
            var structure = _structure ?? SelectedStructure ?? MyStation;
            var costOfAction = GetCostOfAction(actionType, structure, true);
            if (costOfAction <= MyStation.credits)
            {
                Debug.Log($"{MyStation.structureName} queuing up action {actionType}");
                AddActionBarImage(actionType, MyStation.actions.Count());
                MyStation.actions.Add(new Action(actionType, structure, selectedModules, SelectedPath));
                ResetAfterSelection();
            }
            else
            {
                Debug.Log($"You can't afford to queue {actionType} due to pending actions or general lackthere of credits");
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
        ResetAfterSelection();
    }

    private void ResetAfterSelection()
    {
        UpdateGenerateModuleCostText();
        UpdateFleetCostText();
        UpdateCreditTotal();
        SetModuleGrid();
        ClearSelection();
    }

    private void UpdateCreditTotal()
    {
        CreditText.text = $"{MyStation.credits} Credits";
    }

    private IEnumerator MoveOnPath(Unit structure, List<PathNode> path)
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
                blockedMovement = true;
                var structureOnPath = node.structureOnPath;
                //if enemy, attack instead of move, then move if you destroy them
                if (structureOnPath.stationId != structure.stationId)
                {
                    Debug.Log($"{structure.structureName} is attacking {structureOnPath.structureName}");
                    structureOnPath.transform.Find("InCombat").gameObject.SetActive(true);
                    var supportingFleets = GridManager.i.GetNeighbors(node).Select(x => x.structureOnPath).Where(x => x != null && x.structureGuid != structureOnPath.structureGuid);
                    int s1sKinetic = 0;
                    int s2sKinetic = 0;
                    int s1sThermal = 0;
                    int s2sThermal = 0;
                    int s1sExplosive = 0;
                    int s2sExplosive = 0;
                    foreach (var supportFleet in supportingFleets)
                    {
                        if (supportFleet.stationId == structure.stationId)
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
                    string beforeStats = $"Pre-fight stats: \n{structure.structureName}: {structure.hp} HP." +
                        $"\n Stats: {structure.kineticAttack}{s1sKineticText} Kinetic, {structure.thermalAttack}{s1sThermalText} Thermal, {structure.explosiveAttack}{s1sExplosiveText} Explosive." +
                        $"\n\n {structureOnPath.structureName}: {structureOnPath.hp} HP." +
                        $"\n Stats: {structureOnPath.kineticAttack}{s2sKineticText} Kinetic, {structureOnPath.thermalAttack}{s2sThermalText} Thermal, {structureOnPath.explosiveAttack}{s2sExplosiveText} Explosive.";

                    string duringFightText = "";
                    for (int attackType = 0; attackType <= (int)AttackType.Explosive; attackType++)
                    {
                        if (structure.hp > 0 && structureOnPath.hp > 0)
                            duringFightText += Fight(structure, structureOnPath, (AttackType)attackType, s1sKinetic, s1sThermal, s1sExplosive, s2sKinetic, s2sThermal, s2sExplosive);
                    }
                    fightText.text = $"{beforeStats}\n\n{duringFightText}\nPost-fight stats: \n{structure.structureName}: HP {structure.hp}\n{structureOnPath.structureName}: HP {structureOnPath.hp}";
                    ViewFightPanel(true);
                    while (fightPanel.activeInHierarchy)
                    {
                        yield return new WaitForSeconds(.5f);
                    }
                    structureOnPath.transform.Find("InCombat").gameObject.SetActive(false);
                    if (structure.hp <= 0)
                    {
                        Debug.Log($"{structureOnPath.structureName} destroyed {structure.structureName}");
                        if (structureOnPath is Fleet)
                        {
                            if (structureOnPath.hp > 0)
                            {
                                LevelUpUnit(structureOnPath as Fleet);
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
                    }
                    if (structureOnPath.hp > 0)
                    {
                        //even if allied don't move through, don't feel like doing recursive checks right now
                        Debug.Log($"{structure.structureName} movement was blocked by {structureOnPath.structureName}");
                    }
                    else
                    {
                        Debug.Log($"{structure.structureName} destroyed {structureOnPath.structureName}");
                        Destroy(structureOnPath.gameObject); //they are dead
                        if (structure is Fleet)
                            LevelUpUnit(structure as Fleet);
                        if (structureOnPath is Station)
                        {
                            (structureOnPath as Station).defeated = true;
                        }
                        else if (structureOnPath is Fleet)
                        {
                            Stations[structureOnPath.stationId].fleets.Remove(structureOnPath as Fleet);
                        }
                        blockedMovement = false;
                    }
                }
                else //if ally, and theres room after, move through.
                {
                    if (path.Count != i)
                    {
                        for (int j = i; j < path.Count; j++)
                        {
                            if (path[j].structureOnPath == null)
                            {
                                blockedMovement = false;
                            }
                        }
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
            if (blockedMovement)
            {
                break;
            }
            yield return new WaitForSeconds(.25f);
        }
        //character.movementRange -= path.Count();
        //HighlightRangeOfMovement(fleet.currentPathNode, fleet.getMovementRange());
        structure.transform.position = structure.currentPathNode.transform.position;
        structure.currentPathNode.structureOnPath = structure;
    }

    private string Fight(Unit s1, Unit s2, AttackType type, int s1sKinetic, int s1sThermal, int s1sExplosive, int s2sKinetic, int s2sThermal, int s2sExplosive)
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
            return $"{type.ToString()} Phase: {s1.structureName} lost, base damage is {s1Dmg}, after modules applied they lost {damage} HP \n";
        }
        else if (s2Dmg > 0)
        {
            var damage = Mathf.Max(s2Dmg - s2Amr, 0);
            s2.hp -= damage;
            return $"{type.ToString()} Phase: {s2.structureName} lost, base damage is {s1Dmg}, after modules applied they lost {damage} HP \n";
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
            range.transform.SetParent(highlightParent);
            var rangeComponent = range.AddComponent<Node>();
            rangeComponent.Initialize(node);
            currentMovementRange.Add(rangeComponent);
        }
    }
    
    public void EndTurn()
    {
        if (!isEndingTurn && HasGameStarted() && MyStation.actions.Count == MyStation.maxActions)
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
        foreach (var asteroid in GridManager.i.asteroids)
        {
            asteroid.ReginCredits();
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
            var actionCost = GetCostOfAction(action.actionType, action.selectedStructure, false);
            if (currentStation.credits >= actionCost)
            {
                if (action.actionType == ActionType.MoveStructure)
                {
                    isMoving = true;
                    if (currentStructure != null && action.selectedPath != null && action.selectedPath.Count > 0 && action.selectedPath.Count <= currentStructure.getMaxMovementRange())
                    {
                        Debug.Log("Moving to position: " + currentStructure.currentPathNode.transform.position);
                        yield return StartCoroutine(MoveOnPath(currentStructure, action.selectedPath));
                    }
                    else
                    {
                        Debug.Log("Cannot move to position: " + action.selectedPath?.Last()?.transform?.position + ". Out of range or no longer exists.");
                    }
                    yield return new WaitForSeconds(.1f);
                    isMoving = false;
                    currentStructure.resetMovementRange(); //We actually don't currently use range, regardless afraid to comment out        
                }
                else if (action.actionType == ActionType.CreateFleet)
                {
                    if (currentStation.fleets.Count < currentStation.maxFleets)
                    {
                        currentStation.credits -= actionCost;
                        yield return StartCoroutine(GridManager.i.CreateFleet(currentStation, action.generatedGuid));
                    }
                    else
                    {
                        Debug.Log($"{currentStructure.structureName} not eligible for {action.actionType}");
                    }
                    yield return new WaitForSeconds(1f);
                }
                else if (action.actionType == ActionType.UpgradeFleet)
                {
                    if (CanLevelUp(currentStructure, action.actionType, false))
                    {
                        currentStation.credits -= actionCost;
                        LevelUpUnit(currentStructure as Fleet);
                    }
                    else
                    {
                        Debug.Log($"{currentStructure.structureName} not eligible for {action.actionType}");
                    }
                    yield return new WaitForSeconds(1f);
                }
                else if (action.actionType == ActionType.UpgradeStation)
                {
                    if (CanLevelUp(currentStructure, action.actionType, false))
                    {
                        currentStation.credits -= actionCost;
                        LevelUpUnit(currentStructure);
                    }
                    else
                    {
                        Debug.Log($"{currentStructure.structureName} not eligible for {action.actionType}");
                    }
                    yield return new WaitForSeconds(1f);
                }
                else if (action.actionType == ActionType.GenerateModule)
                {
                    currentStation.credits -= actionCost;
                    currentStation.modules.Add(new Module(action.generatedModuleId, action.generatedGuid));
                    yield return new WaitForSeconds(1f);
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
                    yield return new WaitForSeconds(1f);
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
                        else
                        {
                            Debug.Log($"Module not available");
                        }
                    }
                    else
                    {
                        Debug.Log($"Module not available");
                    }
                    yield return new WaitForSeconds(1f);
                }
                else if (action.actionType == ActionType.MineAsteroid)
                {
                    var asteroidToMine = GridManager.i.GetNeighbors(currentStructure.currentPathNode).FirstOrDefault(x => x.isAsteroid);
                    if (asteroidToMine != null)
                    {
                        currentStation.credits += asteroidToMine.MineCredits(currentStructure.mining);
                        asteroidToMine.transform.Find("Mine").gameObject.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        asteroidToMine.transform.Find("Mine").gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                Debug.Log($"Broke ass {currentStructure.color} bitch couldn't afford {action.actionType}");
            }
        }
    }

    private void LevelUpUnit(Unit structure)
    {
        if (CanLevelUp(structure, structure is Station ? ActionType.UpgradeStation : ActionType.UpgradeFleet, false)){
            structure.level++;
            structure.maxAttachedModules++;
            structure.maxHp++;
            structure.hp++;
            structure.kineticAttack++;
            structure.explosiveAttack++;
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
        GridManager.i.GetScores();
        ScoreToWinText.text = $"Hexes to win: {MyStation.score}/{GridManager.i.scoreToWin}";
        TurnOrderText.text = $"Turn Order: ";
        for (int i = 0; i < Stations.Count; i++)
        {
            TurnOrderText.text += Stations[(TurnNumber+i) % Stations.Count].color;
            TurnOrderText.text += i == Stations.Count - 1 ? "." : ", ";
        }
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
    
    private void SetModuleBar(Unit structure)
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
