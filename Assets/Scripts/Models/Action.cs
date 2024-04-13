using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public ActionType actionType;
    public Ship selectedShip;
    public Action(ActionType actionType, Ship selectedShip = null)
    {
        this.actionType = actionType;
        this.selectedShip = selectedShip;
    }
}