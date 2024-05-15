using StartaneousAPI.ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public ActionType actionType;
    public Unit selectedUnit;
    public List<Guid> selectedModulesGuids;
    public List<PathNode> selectedPath;
    public int generatedModuleId;
    public Guid generatedGuid;
    internal int costOfAction;

    public Action(ActionType _actionType, Unit _selectedFleet, int _cost, List<Guid> _selectedModules = null, List<PathNode> _selectedPath = null)
    {
        actionType = _actionType;
        selectedUnit = _selectedFleet;
        costOfAction = _cost;
        selectedModulesGuids = _selectedModules ?? new List<Guid>();
        selectedPath = _selectedPath ?? new List<PathNode>();
    }
    public Action(ServerAction _action)
    {
        if (_action is object)
        {
            actionType = (ActionType)_action.ActionTypeId;
            selectedUnit = GameManager.i.AllUnits.FirstOrDefault(x => x.unitGuid == _action.SelectedUnitGuid);
            selectedModulesGuids = _action.SelectedModulesGuids;
            selectedPath = _action.SelectedCoords.Select(x=> GridManager.i.grid[x.X,x.Y]).ToList();
            generatedModuleId = (int)_action.GeneratedModuleId;
            generatedGuid = (Guid)_action.GeneratedGuid;
        }
    }
}