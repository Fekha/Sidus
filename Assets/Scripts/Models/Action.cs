using StartaneousAPI.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public ActionType actionType;
    public Structure selectedStructure;
    public List<Module> selectedModules;
    public Action(ActionType _actionType, Structure _selectedShip, List<Module> _cost = null)
    {
        actionType = _actionType;
        selectedStructure = _selectedShip;
        selectedModules = _cost ?? new List<Module>();
    }
    public Action(ActionIds _action)
    {
        if (_action is object) {
            actionType = (ActionType)_action.actionTypeId;
            selectedStructure = GameManager.i.AllStructures.FirstOrDefault(x => x.structureId == _action.selectedStructureId);
            selectedModules = GameManager.i.AllModules.Where(x => _action.selectedModulesIds.Contains(x.id)).ToList();
        }
    }
}