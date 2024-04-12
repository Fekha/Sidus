using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public string actionType;
    public Ship selectedShip;
    public Action(string actionType, Ship selectedShip)
    {
        this.actionType = actionType;
        this.selectedShip = selectedShip;
    }
}