using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public ActionType actionType;
    public Structure selectedStructure;
    public Action(ActionType actionType, Structure selectedShip)
    {
        this.actionType = actionType;
        this.selectedStructure = selectedShip;
    }
}