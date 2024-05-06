using StartaneousAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public ActionType actionType;
    public Unit selectedStructure;
    public List<Guid> selectedModulesIds;
    public List<PathNode> selectedPath;
    public int generatedModuleId;
    public Guid generatedGuid;

    public Action(ActionType _actionType, Unit _selectedFleet, List<Guid> _selectedModules = null, List<PathNode> _selectedPath = null)
    {
        actionType = _actionType;
        selectedStructure = _selectedFleet;
        selectedModulesIds = _selectedModules ?? new List<Guid>();
        selectedPath = _selectedPath ?? new List<PathNode>();
    }
    public Action(ActionIds _action)
    {
        if (_action is object) {
            actionType = (ActionType)_action.actionTypeId;
            selectedStructure = GameManager.i.AllStructures.FirstOrDefault(x => x.structureGuid == _action.selectedStructureId);
            selectedModulesIds = _action.selectedModulesIds;
            selectedPath = GameManager.i.GetPathFromCoords(_action.selectedCoords);
            generatedModuleId = _action.generatedModuleId;
            generatedGuid = _action.generatedGuid;
        }
    }
}