using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public ActionType actionType;
    public Structure selectedStructure;
    public Action(ActionType actionType, Structure selectedShip = null)
    {
        this.actionType = actionType;
        this.selectedStructure = selectedShip;
    }
}