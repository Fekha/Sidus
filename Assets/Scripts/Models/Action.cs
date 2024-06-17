using Models;
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
        if (_action != null)
        {
            actionType = (ActionType)_action.ActionTypeId;
            selectedUnit = GameManager.i.AllUnits.FirstOrDefault(x => x.unitGuid == _action.SelectedUnitGuid);
            selectedModule = GameManager.i.AllModules.FirstOrDefault(x => x.moduleGuid == _action.SelectedModuleGuid);
            if (!String.IsNullOrEmpty(_action.XList) && !String.IsNullOrEmpty(_action.YList))
            {
                var intXs = _action.XList.Split(",").Select(x => int.Parse(x.ToString())).ToList();
                var intYs = _action.YList.Split(",").Select(x => int.Parse(x.ToString())).ToList();
                selectedPath = intXs.Select((x, i) => GridManager.i.grid[x, intYs[i]]).ToList();
            }
            generatedGuid = _action.GeneratedGuid;
            actionOrder = _action.ActionOrder;
            playerId = _action.PlayerId;
        }
    }
}