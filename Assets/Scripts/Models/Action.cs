using StartaneousAPI.ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class Action {
    public ActionType actionType;
    public Unit selectedUnit;
    public Module selectedModule;
    public List<PathNode> selectedPath;
    public int generatedModuleId;
    public Guid? generatedGuid;
    internal int costOfAction;
    internal int actionOrder;
    internal int playerId;
    internal bool _statonInventory = false;
    internal Guid? _parentGuid = null;

    public Action(ActionType _actionType, Unit _selectedUnit = null, Module? _selectedModule = null, int _cost = 0, List<PathNode> _selectedPath = null, Guid? _generatedGuid = null)
    {
        actionType = _actionType;
        selectedUnit = _selectedUnit;
        costOfAction = _cost;
        selectedModule = _selectedModule;
        selectedPath = _selectedPath ?? new List<PathNode>();
        generatedGuid = _generatedGuid;
    }
    public Action(ServerAction _action)
    {
        if (_action is object)
        {
            actionType = (ActionType)_action.ActionTypeId;
            selectedUnit = GameManager.i.AllUnits.FirstOrDefault(x => x.unitGuid == _action.SelectedUnitGuid);
            selectedModule = _action.SelectedModule != null ? new Module(_action.SelectedModule) : null;
            selectedPath = _action.SelectedCoords.Select(x=> GridManager.i.grid[x.X,x.Y]).ToList();
            generatedGuid = _action.GeneratedGuid;
            actionOrder = _action.ActionOrder;
            playerId = _action.PlayerId;
        }
    }
}