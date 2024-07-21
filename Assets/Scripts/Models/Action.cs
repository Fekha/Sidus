using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class Action {
    public ActionType actionType;
    public Unit selectedUnit;
    public Guid? selectedModuleGuid;
    public List<PathNode> selectedPath;
    public int generatedModuleId;
    public int? playerBid;
    public Guid? generatedGuid;
    internal int costOfAction;
    internal int actionOrder;
    internal Guid playerGuid;
    internal bool _statonInventory = false;
    internal Guid? _parentGuid = null;

    public Action(ActionType _actionType, Unit _selectedUnit = null, Guid? _selectedModule = null, List<PathNode> _selectedPath = null, Guid? _generatedGuid = null, int? _playerBid = null)
    {
        actionType = _actionType;
        selectedUnit = _selectedUnit;
        selectedModuleGuid = _selectedModule;
        selectedPath = _selectedPath ?? new List<PathNode>();
        generatedGuid = _generatedGuid;
        playerBid = _playerBid;
    }
    public Action(ServerAction _action)
    {
        if (_action != null)
        {
            actionType = (ActionType)_action.ActionTypeId;
            selectedUnit = GameManager.i.AllUnits.FirstOrDefault(x => x.unitGuid == _action.SelectedUnitGuid);
            if (!String.IsNullOrEmpty(_action.XList) && !String.IsNullOrEmpty(_action.YList))
            {
                var intXs = _action.XList.Split(",").Select(x => int.Parse(x.ToString())).ToList();
                var intYs = _action.YList.Split(",").Select(x => int.Parse(x.ToString())).ToList();
                selectedPath = intXs.Select((x, i) => GridManager.i.grid[x, intYs[i]]).ToList();
            }
            else
            {
                selectedPath = new List<PathNode>();
            }
            generatedGuid = _action.GeneratedGuid;
            actionOrder = _action.ActionOrder;
            playerGuid = _action.PlayerGuid;
            selectedModuleGuid = _action.SelectedModuleGuid;
            playerBid = _action.PlayerBid;
        }
    }
}