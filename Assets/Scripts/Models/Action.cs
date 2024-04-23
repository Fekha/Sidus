using System.Collections.Generic;
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
}