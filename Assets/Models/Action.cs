using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Action : MonoBehaviour
{
    public string ActionType;
    public List<PathNode> movement;
    public Ship selectedShip;
    public Action(string actionType, List<PathNode> movement, Ship selectedShip)
    {
        this.ActionType = actionType;
        this.movement = movement;
        this.selectedShip = selectedShip;
    }
}