using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public ActionTypes actionType;
    public Ship selectedShip;
    public Action(ActionTypes actionType, Ship selectedShip = null)
    {
        this.actionType = actionType;
        this.selectedShip = selectedShip;
    }
}