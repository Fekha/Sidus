using StartaneousAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public ActionType actionType;
    public Unit selectedUnit;
    public List<Guid> selectedModulesIds;
    public List<PathNode> selectedPath;
    public int generatedModuleId;
    public Guid generatedGuid;

    public Action(ActionType _actionType, Unit _selectedFleet, List<Guid> _selectedModules = null, List<PathNode> _selectedPath = null)
    {
        actionType = _actionType;
        selectedUnit = _selectedFleet;
        selectedModulesIds = _selectedModules ?? new List<Guid>();
        selectedPath = _selectedPath ?? new List<PathNode>();
    }
    public Action(ActionIds _action)
    {
        if (_action is object) {
            actionType = (ActionType)_action.actionTypeId;
            selectedUnit = GameManager.i.AllUnits.FirstOrDefault(x => x.unitGuid == _action.selectedUnitId);
            selectedModulesIds = _action.selectedModulesIds;
            selectedPath = GameManager.i.GetPathFromCoords(_action.selectedCoords);
            generatedModuleId = _action.generatedModuleId;
            generatedGuid = _action.generatedGuid;
        }
    }
}