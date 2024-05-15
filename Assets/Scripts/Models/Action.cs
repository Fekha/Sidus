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
    public Guid? selectedModuleGuid;
    public List<PathNode> selectedPath;
    public int generatedModuleId;
    public Guid? generatedGuid;
    internal int costOfAction;

    public Action(ActionType _actionType, Unit _selectedFleet, int _cost, Guid? _selectedModuleGuid = null, List<PathNode> _selectedPath = null, Guid? _generatedGuid = null)
    {
        actionType = _actionType;
        selectedUnit = _selectedFleet;
        costOfAction = _cost;
        selectedModuleGuid = _selectedModuleGuid;
        selectedPath = _selectedPath ?? new List<PathNode>();
        generatedGuid = _generatedGuid;
    }
    public Action(ServerAction _action)
    {
        if (_action is object)
        {
            actionType = (ActionType)_action.ActionTypeId;
            selectedUnit = GameManager.i.AllUnits.FirstOrDefault(x => x.unitGuid == _action.SelectedUnitGuid);
            selectedModuleGuid = _action.SelectedModuleGuid;
            selectedPath = _action.SelectedCoords.Select(x=> GridManager.i.grid[x.X,x.Y]).ToList();
            generatedModuleId = (int)_action.GeneratedModuleId;
            generatedGuid = (Guid)_action.GeneratedGuid;
        }
    }
}